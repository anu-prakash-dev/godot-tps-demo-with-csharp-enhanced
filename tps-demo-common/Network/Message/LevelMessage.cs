
using System.Collections.Generic;
using MessagePack;

namespace GodotTPSSharpCommon.Network.Message
{
    [MessagePackObject]
    public class LevelMessage
    {
        [Key(0)]
        public List<PlayerMessage> Players { get; set; }
    }
}
