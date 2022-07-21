using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Testing.Library.Api.Endpoints.Internal;
using Testing.Library.Api.Models;
using Testing.Library.Api.Services;
using System.Net.Mime;

namespace Testing.Library.Api.Endpoints;

public class BookEndpoints : IEndpoints
{
    private const string Tag = "Books";
    private const string BaseRoute = "books";

    public static void AddServices(IServiceCollection services,  IConfiguration configuration)
    {
        services.AddSingleton<IBookService, BookService>();
    }

    public static void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost(BaseRoute, CreateBookAsync)
            .WithName("CreateBook")
            .Accepts<Book>(MediaTypeNames.Application.Json)
            .Produces<Book>(StatusCodes.Status201Created)
            .Produces<IEnumerable<ValidationFailure>>(StatusCodes.Status400BadRequest)
            .WithTags(Tag);

        app.MapGet(BaseRoute, SearchBooksAsync)
            .WithName("SearchBooks")
            .Produces<IEnumerable<Book>>(StatusCodes.Status200OK)
            .WithTags(Tag);

        app.MapGet($"{BaseRoute}/{{isbn}}", GetBookByIsbnAsync)
            .WithName("GetBook")
            .Accepts<Book>(MediaTypeNames.Application.Json)
            .Produces<Book>(StatusCodes.Status201Created).Produces<IEnumerable<ValidationFailure>>(StatusCodes.Status400BadRequest)
            .WithTags(Tag);

        app.MapPut($"{BaseRoute}/{{isbn}}", EditBookAsync)
            .WithName("UpdateBook")
            .Accepts<Book>(MediaTypeNames.Application.Json)
            .Produces<Book>(StatusCodes.Status200OK).Produces<IEnumerable<ValidationFailure>>(StatusCodes.Status400BadRequest)
            .WithTags(Tag);

        app.MapDelete($"{BaseRoute}/{{isbn}}", DeleteBookAsync)
            .WithName("DeleteBook")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .WithTags(Tag);
    }

    private static async Task<IResult> CreateBookAsync(Book book, IBookService bookService, IValidator<Book> validator,
        LinkGenerator linkGenerator, HttpContext httpContext)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(validationResult.Errors);
        }

        var isCreated = await bookService.CreateAsync(book);
        if (!isCreated)
        {
            return Results.BadRequest(new List<ValidationFailure>()
            {
                new ValidationFailure(nameof(Book.Isbn), "A book with this ISBN-13 already exists")
            });
        }
        var locationUri = linkGenerator.GetUriByName(httpContext, "GetBook", new { isbn = book.Isbn })!;
        return Results.Created(locationUri, book);
    }

    private static async Task<IResult> SearchBooksAsync(IBookService bookService, [FromQuery] string? searchTerm)
    {
        var books = searchTerm is not null ?
            await bookService.SearchByTitleAsync(searchTerm) :
            await bookService.GetAllAsync();
        return Results.Ok(books);
    }

    private static async Task<IResult> GetBookByIsbnAsync(IBookService bookService, string isbn)
    {
        var book = await bookService.GetByIsbnAsync(isbn);
        return book is not null ? Results.Ok(book) : Results.NotFound();
    }

    private static async Task<IResult> EditBookAsync(string isbn, Book book, IBookService bookService, IValidator<Book> validator)
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var isUpdated = await bookService.UpdateAsync(book);

        return isUpdated ? Results.Ok(book) : Results.NotFound();
    }

    private static async Task<IResult> DeleteBookAsync(string isbn, IBookService bookService)
    {
        var isDeleted = await bookService.DeleteAsync(isbn);
        return isDeleted ? Results.NoContent() : Results.NotFound();
    }
}
