using Books.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Books.Api.Persistance;

internal class BookMappingConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> entity)
    {
        entity.HasKey(x => x.Isbn);
        entity.Property(x => x.Isbn).ValueGeneratedNever();
    }
}