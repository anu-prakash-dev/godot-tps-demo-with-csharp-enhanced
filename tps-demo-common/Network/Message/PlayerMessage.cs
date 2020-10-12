
using MessagePack;

namespace GodotTPSSharpCommon.Network.Message
{
    [MessagePackObject]
    public class PlayerMessage
    {
        [Key(0)]
        public int PlayerId { get; set; }
    }
}
