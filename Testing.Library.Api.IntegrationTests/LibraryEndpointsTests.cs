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
        var book = GenerateBook();

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
        var book = GenerateBook();
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
        var book = GenerateBook();

        await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book!.Isbn);
        var result = await httpClient.PostAsJsonAsync("/books", book);
        _createdIsbns.Add(book!.Isbn);
        var error = (await result.Content.ReadFromJsonAsync<IEnumerable<ValidationError>>())!.Single();

        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        error.PropertyName.Should().Be("Isbn");
        error.ErrorMessage.Should().Be("A book with this ISBN-13 already exists");
    }

    private Book GenerateBook(string title = "The Dirty Coder")
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
