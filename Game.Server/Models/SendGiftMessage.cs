namespace Game.Server.Models
{
    public class SendGiftMessage
    {
        public string FriendPlayerId { get; set; }
        public string ResourceType { get; set; }
        public int ResourceValue { get; set; }
    }
}
