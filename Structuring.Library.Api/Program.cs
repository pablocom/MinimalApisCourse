using Structuring.Library.Api;
using Structuring.Library.Api.Auth;
using Structuring.Library.Api.Data;
using Structuring.Library.Api.Models;
using Structuring.Library.Api.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ApplicationName = "Structuring.Library.Api",
});

builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.IncludeFields = true;
});

builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

builder.Services.AddAuthentication(ApiKeySchemeConstants.SchemeName)
    .AddScheme<ApiKeyAuthSchemeOptions, ApiKeyAuthHandler>(ApiKeySchemeConstants.SchemeName, _ => { });
builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(builder.Configuration.GetValue<string>("Database:ConnectionString"))
);
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthorization();

app.MapPost("/books",
    // [Authorize(AuthenticationSchemes = ApiKeySchemeConstants.SchemeName)]
        async (
            Book book, 
            IBookService bookService, 
            IValidator<Book> validator, 
            LinkGenerator linkGenerator, 
            HttpContext httpContext) =>
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
        //return Results.CreatedAtRoute("GetBook", new { isbn = book.Isbn }, book);
    })
    .WithName("CreateBook")
    .Accepts<Book>(MediaTypeNames.Application.Json)
    .Produces<Book>(StatusCodes.Status201Created)
    .Produces<IEnumerable<ValidationFailure>>(StatusCodes.Status400BadRequest)
    .WithTags("Books");

app.MapGet("/books", async (IBookService bookService, string? searchTerm) =>
    {
        var books = searchTerm is not null ? 
            await bookService.SearchByTitleAsync(searchTerm) : 
            await bookService.GetAllAsync();
        return Results.Ok(books);
    })
    .WithName("GetBooks")
    .Produces<IEnumerable<Book>>(StatusCodes.Status200OK)
    .WithTags("Books");

app.MapGet("/books/{isbn}", async (string isbn, IBookService bookService) =>
    {
        var book = await bookService.GetByIsbnAsync(isbn);
        return book is not null ? Results.Ok(book) : Results.NotFound();
    })
    .WithName("GetBook")
    .Accepts<Book>(MediaTypeNames.Application.Json)
    .Produces<Book>(StatusCodes.Status201Created)
    .Produces<IEnumerable<ValidationFailure>>(StatusCodes.Status400BadRequest)
    .WithTags("Books");

app.MapPut("/books/{isbn}", async (string isbn, Book book, IBookService bookService, IValidator<Book> validator) =>
    {
        var validationResult = await validator.ValidateAsync(book);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.Errors);

        var isUpdated = await bookService.UpdateAsync(book);

        return isUpdated ? Results.Ok(book) : Results.NotFound();
    })
    .WithName("UpdateBook")
    .Accepts<Book>(MediaTypeNames.Application.Json)
    .Produces<Book>(StatusCodes.Status200OK)
    .Produces<IEnumerable<ValidationFailure>>(StatusCodes.Status400BadRequest)
    .WithTags("Books");

app.MapDelete("/books/{isbn}", async (string isbn, IBookService bookService) =>
    { 
        var isDeleted = await bookService.DeleteAsync(isbn);

        return isDeleted ? Results.NoContent() : Results.NotFound();
    })
    .WithName("DeleteBook")
    .Produces(StatusCodes.Status204NoContent)
    .Produces(StatusCodes.Status404NotFound)
    .WithTags("Books");

app.MapGet("status", () =>
{
    return Results.Extensions.Html(@"<!doctype html>
<html>
    <head><title>Status page</title></head>
    <body>
        <h1>Status</h1>
        <p>Server working fine. </p>
    </body>
</html>
");
});

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
