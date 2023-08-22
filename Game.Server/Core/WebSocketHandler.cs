using Game.Server.Data;
using Game.Server.Enums;
using Game.Server.Models;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace Game.Server.Core
{
    public class WebSocketHandler : BaseHandler
    {
        private readonly PlayerManager _playerManager;
        private readonly ILogger<BaseHandler> _logger;

        public WebSocketHandler(PlayerManager playerManager, ILogger<BaseHandler> logger) : base(logger)
        {
            _playerManager = playerManager;
            _logger=logger;
        }

        public async Task HandleWebSocketAsync(WebSocket socket)
        {
            try
            {
                var buffer = new byte[1024];
                var socketId = Guid.NewGuid();

                _playerManager.AddPlayer(socketId);

                LogInfo($"Client connected", new { socketId });


                while (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                            LogInfo("Message received from client", new { socketId, message });

                            await ProcessMessageAsync(socketId, socket, message);
                        }
                    }
                    catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {
                        LogInfo("Client disconnected", new { socketId });
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogError("Something went wrong during communication", ex);
                    }

                }

                _playerManager.RemovePlayer(socketId);
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);

                LogInfo("Connection closed.", new { socketId });

            }
            catch (Exception ex)
            {
                LogError("Something went wrong", ex);
            }
        }

        private async Task ProcessMessageAsync(Guid socketId, WebSocket socket, string message)
        {
            IMessageHandler handler = null;

            var socketMessage = JsonConvert.DeserializeObject<SocketMessage>(message);

            LogInfo("Message deserialized", new { socketId, socketMessage });


            switch (socketMessage.MessageType)
            {
                case MessageType.Login:
                    handler = new LoginHandler(_logger);
                    break;
                case MessageType.UpdateResources:
                    handler = new UpdateResourcesHandler(_logger);
                    break;
                case MessageType.SendGift:
                    handler = new SendGiftHandler(_logger);
                    break;
                default:
                    await SendResponseAsync(socket, OperationResult.Failure("Invalid message type", new { MessageType = socketMessage.MessageType.ToString() }));
                    break;
            }

            LogInfo("Handler starting to execute", new { socketId, socketMessage });

            await handler.HandleAsync(socketId, socketMessage.MessageBody, socket, _playerManager);

            LogInfo("Handler executed", new { socketId, socketMessage });


        }
    }


}
