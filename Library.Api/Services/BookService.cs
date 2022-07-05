using Dapper;
using Library.Api.Data;
using Library.Api.Models;

namespace Library.Api.Services;

public class BookService : IBookService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public BookService(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<bool> CreateAsync(Book book)
    {
        var existingBook = await GetByIsbnAsync(book.Isbn);
        if (existingBook is null)
        {
            return false;
        }

        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var affectedRows = await connection.ExecuteAsync(
            @$"INSERT INTO Books (Isbn, Title, Author, ShortDescription, PageCount, ReleaseDate)
               VALUES (@Isbn, @Title, @Author, @ShortDescription, @PageCount, @ReleaseDate)", 
            book
        );

        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(string isbn)
    {
        var existingBook = await GetByIsbnAsync(isbn);
        if (existingBook is null)
        {
            return false;
        }

        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var affectedRows = await connection.ExecuteAsync(
            @$"DELETE FROM Books WHERE Isbn = @Isbn",
            new { Isbn = isbn }
        );

        return affectedRows > 0;
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Book>(@"SELECT * FROM Books");
    }

    public async Task<Book?> GetByIsbnAsync(string isbn)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.QuerySingleOrDefaultAsync<Book>(@"SELECT * FROM Books WHERE Isbn = @Isbn",new { Isbn = isbn });
    }

    public async Task<IEnumerable<Book>> SearchByTitleAsync(string searchTerm)
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        return await connection.QueryAsync<Book>(@"SELECT * FROM Books WHERE Title LIKE '%' || @SearchTerm || '%'", 
            new { SearchTerm = searchTerm });
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        var existingBook = await GetByIsbnAsync(book.Isbn);
        if (existingBook is null)
        {
            return false;
        }

        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        var affectedRows = await connection.ExecuteAsync(
            @$"Update Books SET Title = @Title, Author = @Author, ShortDescription = @ShortDescription, PageCount = @PageCount, ReleaseDate = @ReleaseDate WHERE Isbn = @Isbn;",
            book
        );

        return affectedRows > 0;
    }
}
