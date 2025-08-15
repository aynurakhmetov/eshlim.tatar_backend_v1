using Microsoft.EntityFrameworkCore;
using TatarSpecialistFinder.Data;

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

app.Run();
