using Microsoft.Extensions.FileProviders;
using OmgtuPortal.Configuration;
using Scalar.AspNetCore;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(AppSettings.Load());
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs", options =>
    {
        options.Title = "OmgtuPortal API";
    });
}
else
{
    app.Map("/docs/{**slug}", () => Results.NotFound()).ExcludeFromDescription();
    app.MapGet("/docs", () => Results.NotFound()).ExcludeFromDescription();
    app.MapGet("/openapi/{**slug}", () => Results.NotFound()).ExcludeFromDescription();
}

var distPath = Path.Combine(app.Environment.ContentRootPath, ".frontend", "dist");

if (Directory.Exists(distPath))
{
    var fileProvider = new PhysicalFileProvider(distPath);

    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
}

app.MapGet("/api/health", () => Results.Ok())
    .WithDescription("Проверка работоспособности сервиса")
    .Produces(StatusCodes.Status200OK);

app.Map("/api/{**slug}", () => Results.NotFound())
    .WithDescription("Обработка несуществующих API-маршрутов")
    .Produces(StatusCodes.Status404NotFound)
    .ExcludeFromDescription();

if (Directory.Exists(distPath))
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(distPath)
    });
}

app.Run();
