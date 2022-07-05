using Extending.Library.Api;
using Extending.Library.Api.Data;
using Extending.Library.Api.Models;
using Extending.Library.Api.Services;
using FluentValidation;
using FluentValidation.Results;

var builder = WebApplication.CreateBuilder(args);

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

app.MapPost("books", async (Book book, IBookService bookService, IValidator<Book> validator) =>
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

    return Results.Created($"/books/{book.Isbn}", book);
});

app.MapGet("/books", async (IBookService bookService, string? searchTerm) =>
{
    var books = searchTerm is not null ? 
        await bookService.SearchByTitleAsync(searchTerm) : 
        await bookService.GetAllAsync();
    return Results.Ok(books);
});

app.MapGet("/books/{isbn}", async (string isbn, IBookService bookService) =>
{
    var book = await bookService.GetByIsbnAsync(isbn);
    return book is not null ? Results.Ok(book) : Results.NotFound();
});

app.MapPut("books/{isbn}", async (string isbn, Book book, IBookService bookService, IValidator<Book> validator) =>
{
    var validationResult = await validator.ValidateAsync(book);
    if (!validationResult.IsValid)
        return Results.BadRequest(validationResult.Errors);

    var isUpdated = await bookService.UpdateAsync(book);

    return isUpdated ? Results.Ok(book) : Results.NotFound();
});

app.MapDelete("books/{isbn}", async (string isbn, IBookService bookService) =>
{ 
    var isDeleted = await bookService.DeleteAsync(isbn);

    return isDeleted ? Results.NoContent() : Results.NotFound();
});

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
