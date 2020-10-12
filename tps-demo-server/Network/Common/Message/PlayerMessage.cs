
using MessagePack;

namespace GodotTPSSharpCommon.Network.Common.Message
{
    [MessagePackObject]
    public class PlayerMessage
    {
        [Key(0)]
        public int PlayerId { get; set; }
    }
}
