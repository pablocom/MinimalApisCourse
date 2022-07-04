using Books.Api.Dtos;
using Books.Api.Models;
using Books.Api.Persistance;
using Microsoft.EntityFrameworkCore;

namespace Books.Api;

public class BookManagementService : IBookManagementService
{
    private readonly BooksDbContext _dbContext;

    public BookManagementService(BooksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(BookDto dto)
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

        _dbContext.Books.Add(book);
        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveAsync(string isbn)
    {
        var book = await _dbContext.Books.SingleAsync(b => b.Isbn == isbn);
        _dbContext.Books.Remove(book);
        await _dbContext.SaveChangesAsync();
    }

    public async Task EditAsync(BookDto dto)
    {
        var book = await _dbContext.Books.SingleAsync(b => b.Isbn == dto.Isbn);

        book.Author = dto.Author;
        book.Title = dto.Title;
        book.ShortDescription = dto.ShortDescription;
        book.PageCount = dto.PageCount;
        book.ReleaseDate = dto.ReleaseDate;

        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(string isbn)
    {
        return await _dbContext.Books.AnyAsync(b => b.Isbn == isbn);
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        return await _dbContext.Books.ToArrayAsync();
    }

    public async Task<Book> GetByIsbnAsync(string isbn)
    {
        return await _dbContext.Books.SingleAsync(x => x.Isbn == isbn);
    }

    public async Task<IEnumerable<Book>> GetBySearchTermAsync(string searchTerm)
    {
        return await _dbContext.Books
            .Where(b => b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToArrayAsync();
    }
}
