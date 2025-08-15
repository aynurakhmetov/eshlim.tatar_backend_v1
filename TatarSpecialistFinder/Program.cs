using Microsoft.EntityFrameworkCore;
using Serilog;
using TatarSpecialistFinder.Data;
using TatarSpecialistFinder.Models;

var builder = WebApplication.CreateBuilder(args);

// Гарантируем, что есть папка для БД и логов
Directory.CreateDirectory("App_Data");

// ---------- Serilog (ставим ДО Build) ----------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("App_Data/logs.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
// ----------------------------------------------

// CORS: читаем список разрешённых источников (фронтов)
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Front", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
        // Если понадобятся куки/авторизация через браузер: .AllowCredentials()
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Логи каждого HTTP-запроса
app.UseSerilogRequestLogging();

// В Dev показываем детальные ошибки (помогает ловить 500)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();

    // удобный редирект на Swagger
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

// ВАЖНО: CORS должен идти до маппинга маршрутов
app.UseCors("Front");

// Применяем миграции при старте (создаст таблицы, если их нет)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// Пинг для проверки
app.MapGet("/ping", () => "pong");

// ---------- API ----------

// Создать заявку
app.MapPost("/api/requests", async (CreateRequestDto dto, ApplicationDbContext db) =>
{
    try
    {
        if (string.IsNullOrWhiteSpace(dto.Name) ||
            string.IsNullOrWhiteSpace(dto.Contact) ||
            string.IsNullOrWhiteSpace(dto.WhatNeeded) ||
            string.IsNullOrWhiteSpace(dto.WhereNeeded))
        {
            return Results.BadRequest(new { error = "All fields are required." });
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

        Log.Information("New request created: {Name}, contact: {Contact}, where: {WhereNeeded}",
            request.Name, request.Contact, request.WhereNeeded);

        db.ApplicationRequests.Add(request);
        await db.SaveChangesAsync();

        return Results.Created($"/api/requests/{request.Id}", new { request.Id });
    }
    catch (Exception ex)
    {
        Log.Error(ex, "POST /api/requests failed");
        return app.Environment.IsDevelopment()
            ? Results.Problem(title: "Save failed", detail: ex.ToString(), statusCode: 500)
            : Results.StatusCode(500);
    }
});

// Список заявок
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

    Log.Information("Returned {Count} requests", items.Count);
    return Results.Ok(items);
});

// Одна заявка
app.MapGet("/api/requests/{id:int}", async (int id, ApplicationDbContext db) =>
{
    var x = await db.ApplicationRequests.FindAsync(id);
    if (x is null) return Results.NotFound();

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
