using Microsoft.EntityFrameworkCore;
using MiniCrm.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MiniCrmContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MiniCrm")));

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("Frontend");
app.MapControllers();

app.Run();
