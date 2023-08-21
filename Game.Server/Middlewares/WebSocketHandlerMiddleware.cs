using Game.Server.Core;
using Serilog;
using System.Net;

namespace Game.Server
{
    public class WebSocketHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketHandler> _logger;

        public WebSocketHandlerMiddleware(RequestDelegate next, ILogger<WebSocketHandler> logger)
        {
            _next = next;
            _logger=logger;
        }

        public async Task Invoke(HttpContext context, WebSocketHandler webSocketHandler)
        {
            if (context.Request.Path == "/ws-game")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var socket = await context.WebSockets.AcceptWebSocketAsync();
                    _logger.Log(LogLevel.Information, "Websocket request recieved", socket);
                    await webSocketHandler.HandleWebSocketAsync(socket);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
