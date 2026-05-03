using dotenv.net;
using Microsoft.Extensions.FileProviders;
using OmgtuPortal;
using Scalar.AspNetCore;

DotEnv.Load();

var appSettings = AppSettings.FromEnvironment();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(appSettings);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("/docs");
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, ".frontend", "dist")),
    RequestPath = ""
});

app.MapGet("/api/health", () => Results.Ok(new { Status = "Healthy" }))
    .Produces<object>(StatusCodes.Status200OK);

app.MapFallback(async context =>
{
    if (context.Request.Path.StartsWithSegments("/api"))
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsJsonAsync(new { Error = "Not Found" });
        return;
    }

    var indexPath = Path.Combine(builder.Environment.ContentRootPath, ".frontend", "dist", "index.html");
    context.Response.ContentType = "text/html";
    await context.Response.SendFileAsync(indexPath);
});

app.Run();
