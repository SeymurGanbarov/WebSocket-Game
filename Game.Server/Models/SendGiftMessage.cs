using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Models
{
    public class SendGiftMessage
    {
        public string FriendPlayerId { get; set; }
        public string ResourceType { get; set; }
        public int ResourceValue { get; set; }
    }
}
