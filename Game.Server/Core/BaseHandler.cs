using Newtonsoft.Json;
using Serilog;
using System.Net.WebSockets;
using System.Text;

namespace Game.Server.Core
{
    public abstract class BaseHandler
    {
        private readonly ILogger<BaseHandler> _logger;
        public BaseHandler(ILogger<BaseHandler> logger)
        {
            _logger=logger;
        }

        public void LogInfo(string message, object data)
        {
            var dataAsJson = JsonConvert.SerializeObject(data);
            _logger.LogInformation($"{message} : {dataAsJson}");
        }

        public void LogError(string message, Exception ex)
        {
            _logger.LogError(message, ex);
        }
        public async Task SendResponseAsync(WebSocket socket, OperationResult result)
        {
            var resultAsJson = JsonConvert.SerializeObject(result);
            await socket.SendAsync(Encoding.UTF8.GetBytes(resultAsJson), WebSocketMessageType.Text, true, CancellationToken.None);

            LogInfo("Message sent to the client", new { Data = resultAsJson });
        }
    }
}
