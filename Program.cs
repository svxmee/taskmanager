using Microsoft.EntityFrameworkCore;
using TaskManager.Classification;
using TaskManager.Data;
using TaskManager.Models;
using TaskManager.Services;
DotNetEnv.Env.Load();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<AiTaskClassifier>();
builder.Services.AddScoped<KeywordTaskClassifier>();
builder.Services.AddScoped<ITaskClassifierFactory, TaskClassifierFactory>();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();

