using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Testing.Library.Api.Models;

namespace Testing.Library.Api.IntegrationTests;

public class LibraryEndpointsTests : IClassFixture<WebApplicationFactory<ILibraryApiMarker>>, IAsyncLifetime
{
    private readonly WebApplicationFactory<ILibraryApiMarker> _factory;
    private readonly List<string> _createdIsbns = new();

    public LibraryEndpointsTests(WebApplicationFactory<ILibraryApiMarker> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateBook_CreatesBook_WhenDataIsCorrect()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();

        var result = await httpClient.PostAsJsonAsync("/books", book);
        var createdBook = await result.Content.ReadFromJsonAsync<Book>();
        _createdIsbns.Add(createdBook!.Isbn);

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location!.PathAndQuery.Should().Be($"/books/{book.Isbn}");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenIsbnIsInvalid()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();
        book.Isbn = "INVALID";

        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book!.Isbn);
        var error = (await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>())!.Single();

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("Value was not a valid ISBN-13");
    }

    [Fact]
    public async Task CreateBook_Fails_WhenBookExists()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();

        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book!.Isbn);
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book!.Isbn);
        var error = (await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>())!.Single();

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("A book with this ISBN-13 already exists");
    }

    [Fact]
    public async Task GetBook_ReturnsBook_WhenBookExists()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        var result = await httpClient.GetAsync($"books/{book.Isbn}");
        var existingBook = await result.Content.ReadFromJsonAsync<Book>();

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBook.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task GetBook_ReturnsNotFound_WhenBookDoesNotExists()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();

        var result = await httpClient.GetAsync($"books/{book.Isbn}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllBooks_ReturnsAllBooks_WhenBooksExist()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        var result = await httpClient.GetAsync("books");
        var existingBooks = await result.Content.ReadFromJsonAsync<Book[]>();

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBooks.Should().BeEquivalentTo(new[] { book });
    }

    [Fact]
    public async Task GetAllBooks_ReturnsNoBooks_WhenNoBooksExist()
    {
        var httpClient = _factory.CreateClient();

        var result = await httpClient.GetAsync("books");
        var existingBooks = await result.Content.ReadFromJsonAsync<Book[]>();

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        existingBooks.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchBooks_ReturnsBooks_WhenTitleContainsSearchCriteria()
    {
        var httpClient = _factory.CreateClient();
        var bookToBeFound = BuildBook("Clean coder"); 
        var bookToNotBeFound = BuildBook("To not match criteria");
        await httpClient.PostAsJsonAsync("/books", bookToBeFound);
        await httpClient.PostAsJsonAsync("/books", bookToNotBeFound);
        _createdIsbns.AddRange(new[] { bookToBeFound.Isbn, bookToNotBeFound.Isbn });

        var result = await httpClient.GetAsync("books?searchTerm=oder");
        var matchingBooks = await result.Content.ReadFromJsonAsync<Book[]>();

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        matchingBooks.Should().BeEquivalentTo(new[] { bookToBeFound });
    }

    [Fact]
    public async Task UdpateBook_UpdatesBook_WhenDataIsCorrect()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        book.Author = "New author";
        book.Title = "New title";
        book.PageCount = 1;
        book.ReleaseDate = new DateTime(2022, 10, 25);
        book.ShortDescription = "New description";

        var result = await httpClient.PutAsJsonAsync($"books/{book.Isbn}", book);
        var updatedBook = await result.Content.ReadFromJsonAsync<Book>();

        result.StatusCode.Should().Be(HttpStatusCode.OK);
        updatedBook.Should().BeEquivalentTo(book);
    }

    [Fact]
    public async Task UdpateBook_DoesNotUpdateBook_WhenDataIsInCorrect()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        book.Title = string.Empty;

        var result = await httpClient.PutAsJsonAsync($"books/{book.Isbn}", book);
        var error = (await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>())!.Single();

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be(nameof(Book.Title));
        error.ErrorMessage.Should().Be($"'{nameof(Book.Title)}' must not be empty.");
    }

    [Fact]
    public async Task UdpateBook_ReturnsNotFound_WhenBookDoesNotExist()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();

        var result = await httpClient.PutAsJsonAsync($"books/{book.Isbn}", book);

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBook_DeletesBook_WhenBookExist()
    {
        var httpClient = _factory.CreateClient();
        var book = BuildBook();
        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book.Isbn);

        var result = await httpClient.DeleteAsync($"books/{book.Isbn}");

        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private Book BuildBook(string title = "The Dirty Coder")
    {
        return new Book
        {
            Isbn = GenerateIsbn(),
            Title = title,
            Author = "Pablo Company",
            ShortDescription = "Cool and dirty",
            PageCount = 69,
            ReleaseDate = new DateTime(2024, 1, 1)
        };
    }

    private string GenerateIsbn()
    {
        return $"{Random.Shared.Next(100, 999)}-" +
            $"{Random.Shared.Next(0, 9)}-" +
            $"{Random.Shared.Next(10000, 99999)}-" +
            $"{Random.Shared.Next(100, 999)}-" +
            $"{Random.Shared.Next(0, 9)}";
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        var httpClient = _factory.CreateClient();
        foreach (var isbn in _createdIsbns)
        {
            await httpClient.DeleteAsync($"/books/{isbn}");
        }
    }
}
