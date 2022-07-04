using Books.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Books.Api.Persistance;

public class BooksDbContext : DbContext
{
    public DbSet<Book> Books { get; set; } = default!;

    public BooksDbContext(DbContextOptions<BooksDbContext> options) : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new BookMappingConfiguration());
    }
}
