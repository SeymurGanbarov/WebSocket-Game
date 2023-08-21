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
    public class UpdateResourcesHandler : BaseHandler, IMessageHandler
    {
        private readonly ILogger<BaseHandler> _logger;
        public UpdateResourcesHandler(ILogger<BaseHandler> logger) : base(logger)
        {
            _logger=logger;
        }
        public async Task HandleAsync(Guid socketId, string messageBody, WebSocket socket, PlayerManager playerManager)
        {
            try
            {
                var parsedBody = JsonConvert.DeserializeObject<UpdateResourcesMessage>(messageBody);

                LogInfo("UpdateResources Message deserialized", new { socketId, parsedBody });


                if (parsedBody == null || string.IsNullOrEmpty(parsedBody.ResourceType) || parsedBody.ResourceValue <= 0)
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Invalid message format.", new { MessageType = MessageType.UpdateResources.ToString() }));
                    return;
                }

                if (!Enum.TryParse(parsedBody.ResourceType, out ResourceType resourceType))
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Invalid resource type.", new { MessageType = MessageType.UpdateResources.ToString() }));
                    return;
                }

                var playerState = playerManager.GetPlayerState(socketId);

                LogInfo("UpdateResources PlayerState", new { socketId, playerState });


                if (playerState == null)
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Player not found.", new { MessageType = MessageType.UpdateResources.ToString() }));
                    return;
                }

                var IsPlayerLogged = playerManager.IsPlayerConnected(playerState.PlayerId);
                if (!IsPlayerLogged)
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Authentication Required: Please log in to continue.", new { MessageType = MessageType.UpdateResources.ToString() }));
                    return;
                }

                LogInfo("UpdateResources Player balance", new { socketId, playerState });
                playerState.Resources.TryGetValue(resourceType, out var currentBalance);
                var newBalance = currentBalance + parsedBody.ResourceValue;
                playerState.Resources[resourceType] = newBalance;

                await SendResponseAsync(socket, OperationResult.Succeed(new { MessageType = MessageType.UpdateResources.ToString().ToString(), ResourceType = parsedBody.ResourceType, ResourceValue = newBalance }));

                LogInfo("UpdateResources Player balance changed", new { socketId, playerState });

            }
            catch (Exception ex)
            {
                await SendResponseAsync(socket, OperationResult.Failure("Something went wrong.", new { MessageType = MessageType.UpdateResources.ToString() }));
                LogError("UpdateResources handler exception", ex);
            }
        }
    }
}