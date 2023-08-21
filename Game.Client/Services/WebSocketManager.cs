using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game.Client
{
    public class WebSocketManager
    {
        private readonly ClientWebSocket _clientWebSocket;
        private readonly Uri _serverUri;
        private readonly LogManager _logManager;
        private readonly Guid deviceId;
        public WebSocketManager(ClientWebSocket clientWebSocket, Uri serverUri, Guid deviceId, LogManager logManager)
        {
            _clientWebSocket= clientWebSocket;
            _logManager=logManager;
            _serverUri= serverUri;
            this.deviceId= deviceId;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            _logManager.LogInfo("Connecting to the server", new { deviceId });
            await _clientWebSocket.ConnectAsync(_serverUri, cancellationToken);
        }
        public async Task ListenServerAsync(CancellationToken cancellationToken)
        {
            // This worker listening server immediately
            ThreadPool.QueueUserWorkItem(async (x) =>
            {
                _logManager.LogInfo("Listening server", new { deviceId });

                await ReceiveMessageFromServerAsync(cancellationToken);
            });
        }

        private async Task ReceiveMessageFromServerAsync(CancellationToken cancellationToken)
        {
            var responseBuffer = new byte[1024];
            while (true)
            {
                var byteReceived = new ArraySegment<byte>(responseBuffer);
                WebSocketReceiveResult response = await _clientWebSocket.ReceiveAsync(byteReceived, cancellationToken);
                var responseMessage = Encoding.UTF8.GetString(responseBuffer, 0, response.Count);
                if (response.EndOfMessage)
                {
                    _logManager.LogInfo("Message received from server", new { deviceId, responseMessage });
                    Console.WriteLine(responseMessage);
                }

            }
        }

        public async Task SendWebSocketMessageAsync(string message, CancellationToken cancellationToken)
        {
            _logManager.LogInfo("Message sending to the server", new { deviceId, message });
            var bytes = Encoding.UTF8.GetBytes(message);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken);
            Console.WriteLine("Message sent to the server");
            _logManager.LogInfo("Message sent to the server", new { deviceId, message });

        }

        public async Task CloseConnectionAsync()
        {
            await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing connection", CancellationToken.None);
            _logManager.LogInfo("Client closed connection", new { deviceId });

        }

    }
}
