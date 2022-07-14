using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Testing.Library.Api.Models;

namespace Testing.Library.Api.IntegrationTests;

public class LibraryEndpointsTests : IClassFixture<WebApplicationFactory<ILibraryApiMarker>>
{
    private readonly WebApplicationFactory<ILibraryApiMarker> _factory;

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

        result.StatusCode.Should().Be(HttpStatusCode.Created);
        createdBook.Should().BeEquivalentTo(book);
        result.Headers.Location!.PathAndQuery.Should().Be($"/books/{book.Isbn}");
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
}
