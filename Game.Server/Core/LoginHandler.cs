using Game.Server.Data;
using Game.Server.Enums;
using Game.Server.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Core
{
    public class LoginHandler : BaseHandler, IMessageHandler
    {
        private readonly ILogger<BaseHandler> _logger;
        public LoginHandler(ILogger<BaseHandler> logger) : base(logger)
        {
            _logger=logger;
        }
        public async Task HandleAsync(Guid socketId, string messageBody, WebSocket socket, PlayerManager playerManager)
        {
            try
            {
                var parsedBody = JsonConvert.DeserializeObject<LoginMessage>(messageBody);

                LogInfo("Login Message deserialized", new {socketId, parsedBody});

                if (parsedBody == null || string.IsNullOrEmpty(parsedBody.DeviceId))
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Invalid message format.", new {MessageType = MessageType.Login.ToString()}));
                    return;
                }

                if (playerManager.IsPlayerConnected(parsedBody.DeviceId))
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Player already connected.", new { MessageType = MessageType.Login.ToString() }));
                    return;
                }
                else
                {
                    var playerId = GameDatabase.PlayerStates[socketId]?.PlayerId;
                    if (!playerId.HasValue || playerId == Guid.Empty)
                    {
                        await SendResponseAsync(socket, OperationResult.Failure("Player id not found.", new { MessageType = MessageType.Login.ToString() }));
                        return;
                    }


                    playerManager.AddConnectedPlayer(parsedBody.DeviceId, playerId.Value, socketId, socket);

                    LogInfo("Login Player added to db", new { socketId, playerId, parsedBody.DeviceId });


                    await SendResponseAsync(socket,  OperationResult.Succeed(new { MessageType = MessageType.Login.ToString(), PlayerId = playerId }));

                    LogInfo("Login handler successfully executed", new {socketId,playerId, parsedBody.DeviceId });
                }
            }
            catch (Exception ex)
            {
                await SendResponseAsync(socket, OperationResult.Failure("Something went wrong.", new { MessageType = MessageType.Login.ToString() }));
                LogError($"Login handler exception-{socketId}", ex);
            }
        }
    }
}