using FocusTrack.ReminderService.Data;
using FocusTrack.ReminderService.Services;
using Hangfire;
using Hangfire.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReminderDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Hangfire services
builder.Services.AddHangfire(config =>
    config.UseSqliteStorage(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddHangfireServer();

// Add RabbitMQ listener and scheduler services
builder.Services.AddSingleton<RabbitMQListener>();
builder.Services.AddSingleton<ReminderScheduler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Start Hangfire dashboard (optional)
app.UseHangfireDashboard();

// Start RabbitMQ listener
var rabbitMqListener = app.Services.GetRequiredService<RabbitMQListener>();
rabbitMqListener.StartListening();

app.Run();
