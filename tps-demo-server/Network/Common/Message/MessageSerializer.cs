using MessagePack;

namespace GodotTPSSharpCommon.Network.Common.Message
{
    public static class MessageSerializer
    {
        public static byte[] Serialize<T>(T message) => MessagePackSerializer.Serialize(message);
        public static T Deserialize<T>(byte[] message) => MessagePackSerializer.Deserialize<T>(message);
    }
}
