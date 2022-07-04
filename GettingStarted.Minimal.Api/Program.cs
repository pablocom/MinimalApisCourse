using GettingStarted.Minimal.Api;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<PeopleService>();
builder.Services.AddSingleton<GuidGenerator>();

var app = builder.Build();

//app.UseMiddleware<CookiePolicyMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => "Hello from GET");
app.MapPost("/", () => "Hello from POST");

app.MapGet("/ok-result/{age:int}", (int age) => Results.Ok(new 
{ 
    Name = "Pablo Company", 
    Age = age 
}));

app.MapGet("/slow-request", async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(2));

    return Results.Ok(new 
    { 
        Name = "Pablo Company" 
    });
});

app.MapMethods("options-or-head", new[] { "HEAD", "OPTIONS" }, 
    () => "Hello from either OPTIONS or HEAD");

var requestHandler = () => "This response comes from var";
app.MapGet("fromvar", requestHandler);
app.MapGet("fromclass", Example.SomeMethod);

app.MapGet("people/search", (string? searchTerm, PeopleService peopleService) =>
{
    var results = peopleService.Search(searchTerm);
    return Results.Ok(results);
});

app.MapGet("mix/{routeParam}", 
    (
        [FromRoute] string routeParam, 
        [FromQuery(Name = "query")] int queryParam, 
        [FromServices] GuidGenerator guidGenerator,
        [FromHeader(Name = "Accept-Encoding")] string encoding) =>
    {
        return $"{routeParam} {queryParam} {guidGenerator.NewGuid()} Encoding: {encoding}";
    });

app.MapPost("people", (Person person) => Results.Ok(person));
app.MapGet("httpcontext", async context => context.Response.StatusCode = 204);
app.MapGet("claims", (ClaimsPrincipal user) => Results.Ok(user.Identity?.Name));

app.MapGet("cancel", async (CancellationToken ct) =>
{
    await Task.Delay(TimeSpan.FromSeconds(2), ct);
    return Results.Ok();
});

app.MapGet("map-point", (MapPoint point) => Results.Ok(point));
app.MapGet("map-point-frombody", (MapPoint point) => Results.Ok(point));

app.Run();
