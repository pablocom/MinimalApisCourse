using FluentValidation;
using Structuring.Library.Api.Data;
using Structuring.Library.Api.Endpoints.Internal;

var builder = WebApplication.CreateBuilder();

builder.Configuration.AddJsonFile("appsettings.Local.json", true, true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new SqliteConnectionFactory(builder.Configuration.GetValue<string>("Database:ConnectionString"))
);

builder.Services.AddEndpoints<Program>(builder.Configuration);

builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseEndpoints<Program>();

var databaseInitializer = app.Services.GetRequiredService<DatabaseInitializer>();
await databaseInitializer.InitializeAsync();

app.Run();
