using Game.Server.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Data
{
    public class PlayerManager
    {
        public void AddConnectedPlayer(string deviceId, Guid playerId,Guid socketId, WebSocket socket)
        {
            GameDatabase.ConnectedPlayers.TryAdd(deviceId, playerId);
            GameDatabase.Sockets.TryAdd(socketId, socket);
        }

        public bool IsPlayerConnected(string deviceId)
        {
            return GameDatabase.ConnectedPlayers.ContainsKey(deviceId);
        }

        public bool IsPlayerConnected(Guid playerId)
        {
            return GameDatabase.ConnectedPlayers.Values.Any(x=>x == playerId);
        }

        public void AddPlayer(Guid socketId)
        {
            GameDatabase.PlayerStates.TryAdd(socketId, new PlayerState() { PlayerId = Guid.NewGuid()});
        }

        public void RemovePlayer(Guid socketId)
        {
            GameDatabase.PlayerStates.TryGetValue(socketId, out var playerState);
            var deviceId = GameDatabase.ConnectedPlayers.FirstOrDefault(x => x.Value == playerState.PlayerId).Key;
            
            if(!string.IsNullOrEmpty(deviceId))
                GameDatabase.ConnectedPlayers.TryRemove(deviceId, out _);

            GameDatabase.PlayerStates.TryRemove(socketId, out _);
            GameDatabase.Sockets.TryRemove(socketId, out _);
        }

        public WebSocket GetSocket(Guid socketId)
        {
            GameDatabase.Sockets.TryGetValue(socketId, out var socket);
            return socket;
        }

        public Guid GetSocketIdByPlayerId(string playerId)
        {
            foreach (var playerIdFromDict in GameDatabase.ConnectedPlayers.Values)
            {
                if (playerIdFromDict.ToString() == playerId)
                {
                    foreach (var state in GameDatabase.PlayerStates)
                    {
                        if(state.Value.PlayerId == playerIdFromDict)
                            return state.Key;
                    }
                }
            }
            return Guid.Empty;
        }

        public PlayerState GetPlayerState(Guid socketId)
        {
            GameDatabase.PlayerStates.TryGetValue(socketId, out var playerState);
            return playerState;
        }
    }
}
