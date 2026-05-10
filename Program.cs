using Microsoft.Extensions.FileProviders;
using OmgtuPortal.Configuration;

DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(AppSettings.Load());

var app = builder.Build();

var distPath = Path.Combine(app.Environment.ContentRootPath, ".frontend", "dist");

if (Directory.Exists(distPath))
{
    var fileProvider = new PhysicalFileProvider(distPath);

    app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = fileProvider });
    app.UseStaticFiles(new StaticFileOptions { FileProvider = fileProvider });
}

app.MapGet("/api/health", () => Results.Ok());

app.Map("/api/{**slug}", () => Results.NotFound());

if (Directory.Exists(distPath))
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(distPath)
    });
}

app.Run();
