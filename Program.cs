using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "SongListAPI";
    config.Title = "SongListAPI v1";
    config.Version = "v1";
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SongModelDb>(options =>
    options.UseMySql(connectionString, new MySqlServerVersion(new Version(11, 5, 2)))); // MariaDB のバージョン

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

var app = builder.Build();
app.UseCors("AllowAllOrigins");
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "SongListAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/", () => "Hello World!");

app.MapGet("/songs", async (SongModelDb db) =>
{
    var result = await db.Songs.Where(s => s.Arribalflg == 0).ToListAsync();
    return Results.Json(result);
});

/*
app.MapGet("/songs/alive", async (SongModelDb db) =>
    await db.Songs.Where(s => s.arribalflg == 0).ToListAsync());
*/
app.MapPost("/songs", async (SongModel song, SongModelDb db) =>
{
    db.Songs.Add(song);
    await db.SaveChangesAsync();
    return Results.Created($"/songs/{song.Id}", song);
});

app.MapPut("/songs/{id}", async (int id, SongModel input, SongModelDb db) =>
{
    var song = await db.Songs.FindAsync(id);

    if (song is null) return Results.NotFound();

    song.Name = input.Name;
    song.Arribalflg = input.Arribalflg;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();
