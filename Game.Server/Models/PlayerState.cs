using Game.Server.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Server.Models
{
    public class PlayerState
    {
        public Guid PlayerId { get; set; }
        public Dictionary<ResourceType, int> Resources { get; } = new Dictionary<ResourceType, int>();
    }
}
