using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniCrm.Api.Middlewares;
using MiniCrm.Application.Interfaces;
using MiniCrm.Application.Services;
using MiniCrm.Data;
using MiniCrm.Data.Repositories;
using MiniCrm.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MiniCrmContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("MiniCrm")));

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IGestionRepository, GestionRepository>();
builder.Services.AddScoped<IGestionService, GestionService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Los datos enviados no son válidos."
        });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("Frontend");
app.UseMiddleware<ExceptionMiddleware>();
app.MapControllers();

app.Run();
