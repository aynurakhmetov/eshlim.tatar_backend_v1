using Microsoft.EntityFrameworkCore;
using TatarSpecialistFinder.Data;
using TatarSpecialistFinder.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer(); // даёт описания эндпоинтов
builder.Services.AddSwaggerGen();            // генерит Swagger/OpenAPI
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/ping", () => "pong"); // тестовый эндпоинт

app.MapPost("/api/requests", async (CreateRequestDto dto, ApplicationDbContext db) =>
{
    // Простая валидация
    if (string.IsNullOrWhiteSpace(dto.Name) ||
        string.IsNullOrWhiteSpace(dto.Contact) ||
        string.IsNullOrWhiteSpace(dto.WhatNeeded) ||
        string.IsNullOrWhiteSpace(dto.WhereNeeded))
    {
        return Results.BadRequest("All fields are required.");
    }

    var request = new ApplicationRequest
    {
        Name = dto.Name,
        Contact = dto.Contact,
        WhatNeeded = dto.WhatNeeded,
        WhereNeeded = dto.WhereNeeded,
        CreatedAt = DateTime.UtcNow,
        Status = RequestStatus.New
    };

    db.ApplicationRequests.Add(request);
    await db.SaveChangesAsync();

    return Results.Created($"/api/requests/{request.Id}", new { request.Id });
});


app.MapGet("/api/requests", async (ApplicationDbContext db) =>
{
    var items = await db.ApplicationRequests
        .OrderByDescending(x => x.Id)
        .Select(x => new RequestDto
        {
            Id = x.Id,
            Name = x.Name,
            Contact = x.Contact,
            WhatNeeded = x.WhatNeeded,
            WhereNeeded = x.WhereNeeded,
            CreatedAt = x.CreatedAt,
            Status = x.Status.ToString()
        })
        .ToListAsync();

    return Results.Ok(items);
});

app.MapGet("/api/requests/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var x = await db.ApplicationRequests.FindAsync(id);
    if (x == null) return Results.NotFound();

    var dto = new RequestDto
    {
        Id = x.Id,
        Name = x.Name,
        Contact = x.Contact,
        WhatNeeded = x.WhatNeeded,
        WhereNeeded = x.WhereNeeded,
        CreatedAt = x.CreatedAt,
        Status = x.Status.ToString()
    };

    return Results.Ok(dto);
});

app.Run();
