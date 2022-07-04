using Books.Api.Dtos;
using Books.Api.Models;

namespace Books.Api;

public interface IBookManagementService
{
    Task<Book> GetByIsbnAsync(string isbn);
    Task<IEnumerable<Book>> GetAllAsync();
    Task<IEnumerable<Book>> GetBySearchTermAsync(string searchTerm);
    Task AddAsync(BookDto bookDto);
    Task EditAsync(BookDto bookDto);
    Task RemoveAsync(string isbn);
    Task<bool> ExistsAsync(string isbn);
}
