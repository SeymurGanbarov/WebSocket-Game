using Game.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Models
{
    public class SocketMessage
    {
        public MessageType MessageType { get; set; }
        public string MessageBody { get; set; }
    }
}
