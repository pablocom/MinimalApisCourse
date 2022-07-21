using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testing.Library.Api.Data;

namespace Testing.Library.Api.EndToEndTests;

public class LibraryApiFactory : WebApplicationFactory<ILibraryApiMarker>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(ConfigureInMemoryDatabase);
    }

    private static void ConfigureInMemoryDatabase(IServiceCollection services)
    {
        services.RemoveAll(typeof(IDbConnectionFactory));
        services.AddSingleton((Func<IServiceProvider, IDbConnectionFactory>)(_ =>
        {
            var inMemorySqliteFactory = new SqliteConnectionFactory("DataSource=file:inmem?mode=memory&cache=shared");
            return inMemorySqliteFactory;
        }));
    }
}
