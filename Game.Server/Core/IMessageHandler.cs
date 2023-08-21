using Game.Server.Data;
using System.Net.WebSockets;

namespace Game.Server.Core
{
    public interface IMessageHandler
    {
        Task HandleAsync(Guid socketId, string messageBody, WebSocket socket, PlayerManager playerManager);
    }
}
