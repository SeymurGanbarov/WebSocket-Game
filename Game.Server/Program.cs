using Game.Server;
using Game.Server.Core;
using Game.Server.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSingleton<PlayerManager>();
builder.Services.AddSingleton<WebSocketHandler>();

var logFilePath = GetLogFilePath();

var logger  = new LoggerConfiguration()
.WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
.CreateLogger();

builder.Logging.AddSerilog(logger);

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseRouting();
app.UseWebSockets();
app.UseMiddleware<WebSocketHandlerMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapGet("/", async context =>
    {
        await context.Response.WriteAsync("Game websocket server");
    });
});

app.Run();


string GetLogFilePath()
{
    var directory = new DirectoryInfo(
         Directory.GetCurrentDirectory());
    while (directory != null && !directory.GetFiles("*.sln").Any())
    {
        directory = directory.Parent;
    }
    return Path.Combine(directory.FullName, "Logs", "GameServer-Logs", "Log-.log");
}