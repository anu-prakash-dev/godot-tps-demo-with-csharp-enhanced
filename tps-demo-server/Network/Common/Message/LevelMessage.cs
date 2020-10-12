
using System.Collections.Generic;
using MessagePack;

namespace GodotTPSSharpCommon.Network.Common.Message
{
    [MessagePackObject]
    public class LevelMessage
    {
        [Key(0)]
        public List<PlayerMessage> Players { get; set; }
    }
}
