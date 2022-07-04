using Books.Api;
using Books.Api.Dtos;
using Books.Api.Persistance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BooksDbContext>(x => x.UseSqlite($"Data Source=books.db;"));
builder.Services.AddScoped<IBookManagementService, BookManagementService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/books/{isbn}", async (string isbn, [FromServices] IBookManagementService booksService) =>
{
    if (await booksService.ExistsAsync(isbn))
        return Results.NotFound();

    var book = await booksService.GetByIsbnAsync(isbn);
    return Results.Ok(book);
});

app.MapGet("/books", async ([FromQuery] string searchTerm, [FromServices] IBookManagementService booksService) =>
{
    var books = await booksService.GetBySearchTermAsync(searchTerm);
    return Results.Ok(books);
});

app.MapGet("/books", async ([FromServices] IBookManagementService booksService) =>
{
    var allBooks = await booksService.GetAllAsync();
    return Results.Ok(allBooks);
});

app.MapPost("/books", async (BookDto dto, [FromServices] IBookManagementService booksService) =>
{
    if (await booksService.ExistsAsync(dto.Isbn))
        return Results.NotFound();

    await booksService.AddAsync(dto);

    return Results.Ok();
});

app.MapPut("/books/{isbn}", async (string isbn, BookDto dto, [FromServices] IBookManagementService booksService) =>
{
    if (await booksService.ExistsAsync(isbn))
        return Results.NotFound();

    await booksService.EditAsync(dto);

    return Results.Ok();
});

app.MapDelete("/books/{isbn}", async (string isbn, [FromServices] IBookManagementService booksService) => 
{
    if (await booksService.ExistsAsync(isbn))
        return Results.NotFound();

    await booksService.RemoveAsync(isbn);

    return Results.Ok();
});

app.Run();
