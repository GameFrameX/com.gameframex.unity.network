using System;

namespace GameFrameX.Network.Runtime
{
    public interface IMessageSerializer
    {
        byte[] Serialize<T>(T message) where T : MessageObject;

        object Deserialize(byte[] data, Type targetType);
    }
}
