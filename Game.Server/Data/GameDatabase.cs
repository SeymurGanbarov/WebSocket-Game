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
    public static class GameDatabase
    {
        public static ConcurrentDictionary<string, Guid> ConnectedPlayers { get; set; }
        public static ConcurrentDictionary<Guid, PlayerState> PlayerStates { get; set; }
        public static ConcurrentDictionary<Guid, WebSocket> Sockets { get; set; }
        
        static GameDatabase()
        {
            ConnectedPlayers= new ConcurrentDictionary<string, Guid>();
            PlayerStates= new ConcurrentDictionary<Guid, PlayerState>();
            Sockets= new ConcurrentDictionary<Guid, WebSocket>();
        }
    }
}
