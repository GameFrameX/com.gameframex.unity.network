using System;

namespace GameFrameX.Network.Runtime
{
    public class DefaultMessageSerializer : IMessageSerializer
    {
        public static readonly DefaultMessageSerializer Instance = new DefaultMessageSerializer();

        public byte[] Serialize<T>(T message) where T : MessageObject
        {
            throw new InvalidOperationException(
                "No IMessageSerializer registered. " +
                "Call MessageSerializerRegistry.RegisterGlobal() or provide a channel-level serializer.");
        }

        public object Deserialize(byte[] data, Type targetType)
        {
            throw new InvalidOperationException(
                "No IMessageSerializer registered. " +
                "Call MessageSerializerRegistry.RegisterGlobal() or provide a channel-level serializer.");
        }
    }
}
