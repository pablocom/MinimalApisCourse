using System.Reflection;

namespace Structuring.Library.Api.Endpoints.Internal;

public static class EndpointExtensions
{
    public static void AddEndpoints<TAssemblyMarker>(this IServiceCollection services, IConfiguration configuration) 
        => AddEndpoints(services, typeof(TAssemblyMarker), configuration);

    public static void AddEndpoints(this IServiceCollection services, Type assemblyMarker, IConfiguration configuration)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(assemblyMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.AddServices))!
                .Invoke(null, new object[] { services, configuration });
        }
    }

    public static void UseEndpoints<TAssemblyMarker>(this IApplicationBuilder app) 
        => UseEndpoints(app, typeof(TAssemblyMarker));

    public static void UseEndpoints(this IApplicationBuilder app, Type assemblyMarker)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(assemblyMarker);

        foreach (var endpointType in endpointTypes)
        {
            endpointType.GetMethod(nameof(IEndpoints.DefineEndpoints))!
                .Invoke(null, new object[] { app });
        }

    }

    private static IEnumerable<TypeInfo> GetEndpointTypesFromAssemblyContaining(Type assemblyMarker)
    {
        return assemblyMarker.Assembly.DefinedTypes
            .Where(t => !t.IsAbstract && !t.IsInterface && typeof(IEndpoints).IsAssignableFrom(t));
    }
}
