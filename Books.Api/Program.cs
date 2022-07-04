using Books.Api.Dtos;
using Books.Api.Models;
using Books.Api.Persistance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BooksDbContext>(x => x.UseSqlite($"Data Source=books.db;"));

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/books/{isbn}", async (string isbn, BooksDbContext dbContext) =>
{
    var book = await dbContext.Books.FirstOrDefaultAsync(x => x.Isbn == isbn);
    if (book is null)
        return Results.NotFound();

    return Results.Ok(book);
});

app.MapGet("/books", async ([FromQuery] string searchTerm, [FromServices] BooksDbContext dbContext) =>
{
    var books = await dbContext.Books
        .Where(b => b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToArrayAsync();
    return Results.Ok(books);
});

app.MapGet("/books", async ([FromServices] BooksDbContext dbContext) =>
{
    var allBooks = await dbContext.Books.ToArrayAsync();
    return Results.Ok(allBooks);
});

app.MapPost("/books", async (BookDto dto, [FromServices] BooksDbContext dbContext) =>
{
    var book = new Book
    {
        Isbn = dto.Isbn,
        Author = dto.Author,
        Title = dto.Title,
        ShortDescription = dto.ShortDescription,
        PageCount = dto.PageCount,
        ReleaseDate = dto.ReleaseDate,
    };

    dbContext.Books.Add(book);
    await dbContext.SaveChangesAsync();

    return Results.Ok(book);
});

app.MapPut("/books/{isbn}", async (string isbn, BookDto dto, [FromServices] BooksDbContext dbContext) =>
{
    var book = await dbContext.Books.FirstOrDefaultAsync(b => b.Isbn == isbn);
    if (book is null)
        return Results.NotFound();

    book.Author = dto.Author;
    book.Title = dto.Title;
    book.ShortDescription = dto.ShortDescription;
    book.PageCount = dto.PageCount;
    book.ReleaseDate = dto.ReleaseDate;

    await dbContext.SaveChangesAsync();

    return Results.Ok(book);
});

app.MapDelete("/books/{isbn}", async (string isbn, [FromServices] BooksDbContext dbContext) => 
{
    var book = await dbContext.Books.FirstOrDefaultAsync(b => b.Isbn == isbn);
    if (book is null)
        return Results.NotFound();

    dbContext.Books.Remove(book);
    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
