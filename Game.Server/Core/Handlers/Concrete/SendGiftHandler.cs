using Game.Server.Data;
using Game.Server.Enums;
using Game.Server.Models;
using Newtonsoft.Json;
using System.Net.WebSockets;

namespace Game.Server.Core
{
    public class SendGiftHandler : BaseHandler, IMessageHandler
    {
        private readonly ILogger<BaseHandler> _logger;
        public SendGiftHandler(ILogger<BaseHandler> logger) : base(logger)
        {
            _logger = logger;
        }
        public async Task HandleAsync(Guid socketId, string messageBody, WebSocket socket, PlayerManager playerManager)
        {
            try
            {
                var parsedBody = JsonConvert.DeserializeObject<SendGiftMessage>(messageBody);

                LogInfo("SendGift Message deserialized", new { socketId, parsedBody });

                if (parsedBody == null || string.IsNullOrEmpty(parsedBody.FriendPlayerId) || string.IsNullOrEmpty(parsedBody.ResourceType) || parsedBody.ResourceValue <= 0)
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Invalid message format.", new { MessageType = MessageType.SendGift.ToString() }));
                    return;
                }

                var sendingPlayerState = playerManager.GetPlayerState(socketId);

                LogInfo("SendGift PlayerState", new { socketId, sendingPlayerState });

                var IsPlayerLogged = playerManager.IsPlayerConnected(sendingPlayerState.PlayerId);
                if (!IsPlayerLogged)
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Authentication Required: Please log in to continue.", new { MessageType = MessageType.SendGift.ToString() }));
                    return;
                }

                var friendSocketId = playerManager.GetSocketIdByPlayerId(parsedBody.FriendPlayerId);
                var friendPlayerState = playerManager.GetPlayerState(friendSocketId);

                LogInfo("SendGift Friend PlayerState", new { socketId, friendPlayerState });


                if (sendingPlayerState == null || friendPlayerState == null)
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Player(s) not found.", new { MessageType = MessageType.SendGift.ToString() }));
                    return;
                }

                if (!Enum.TryParse(parsedBody.ResourceType, out ResourceType resourceType))
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Invalid resource type.", new { MessageType = MessageType.SendGift.ToString() }));
                    return;
                }

                if (sendingPlayerState.Resources.TryGetValue(resourceType, out var senderResourceBalance) &&
                    senderResourceBalance >= parsedBody.ResourceValue)
                {
                    sendingPlayerState.Resources[resourceType] -= parsedBody.ResourceValue;

                    if (friendPlayerState.Resources.TryGetValue(resourceType, out var friendResourceBalance))
                    {
                        friendPlayerState.Resources[resourceType] += parsedBody.ResourceValue;
                    }
                    else
                    {
                        friendPlayerState.Resources.Add(resourceType, parsedBody.ResourceValue);
                    }

                    if (friendSocketId != Guid.Empty)
                    {
                        await SendResponseAsync(playerManager.GetSocket(friendSocketId), OperationResult.Succeed(new { MessageType = MessageType.GiftEvent.ToString(), ResourceType = resourceType.ToString(), parsedBody.ResourceValue }));
                        LogInfo("GiftEvent sent to the friend", new { socketId });
                    }

                    await SendResponseAsync(socket, OperationResult.Succeed(new { MessageType = MessageType.SendGift.ToString(), parsedBody.ResourceType, ResourceValue = sendingPlayerState.Resources[resourceType] }));

                    LogInfo("SendGift Players balance changed", new { socketId, senderBalance = sendingPlayerState.Resources[resourceType], friendBalance = friendPlayerState.Resources[resourceType] });
                }
                else
                {
                    await SendResponseAsync(socket, OperationResult.Failure("Insufficient resources to send gift.", new { MessageType = MessageType.SendGift.ToString() }));
                }
            }
            catch (Exception ex)
            {
                await SendResponseAsync(socket, OperationResult.Failure("Something went wrong.", new { MessageType = MessageType.SendGift.ToString() }));
                LogError($"SendGft handler exception - {socketId}", ex);
            }
        }
    }


}
