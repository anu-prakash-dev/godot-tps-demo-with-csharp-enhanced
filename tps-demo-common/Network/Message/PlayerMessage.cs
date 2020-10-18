
using MessagePack;

namespace GodotTPSSharpCommon.Network.Message
{
    [MessagePackObject]
    public class PlayerMessage
    {
        [Key(0)]
        public int PlayerId { get; set; }
        [Key(1)]
        public float OriginX { get; set; }
        [Key(2)]
        public float OriginY { get; set; }
        [Key(3)]
        public float OriginZ { get; set; }
        [Key(4)]
        public float BasisXX { get; set; }
        [Key(5)]
        public float BasisXY { get; set; }
        [Key(6)]
        public float BasisXZ { get; set; }
        [Key(7)]
        public float BasisYX { get; set; }
        [Key(8)]
        public float BasisYY { get; set; }
        [Key(9)]
        public float BasisYZ { get; set; }
        [Key(10)]
        public float BasisZX { get; set; }
        [Key(11)]
        public float BasisZY { get; set; }
        [Key(12)]
        public float BasisZZ { get; set; }
    }
}
