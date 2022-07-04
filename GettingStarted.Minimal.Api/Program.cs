using GettingStarted.Minimal.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<PeopleService>();

var app = builder.Build();

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

app.Run();
