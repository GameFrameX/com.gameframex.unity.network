using System;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息序列化器，作为未注册时的兜底实现，调用时抛出异常以提示用户注册实际的序列化器。
    /// </summary>
    /// <remarks>
    /// Default message serializer that serves as a fallback when no serializer is registered.
    /// Throws an exception to remind the user to register an actual serializer.
    /// </remarks>
    public class DefaultMessageSerializer : IMessageSerializer
    {
        /// <summary>
        /// 默认兜底实例。
        /// </summary>
        /// <remarks>
        /// Default fallback instance.
        /// </remarks>
        public static readonly DefaultMessageSerializer Instance = new DefaultMessageSerializer();

        private DefaultMessageSerializer() { }

        /// <summary>
        /// 序列化消息对象，未注册实际序列化器时始终抛出异常。
        /// </summary>
        /// <remarks>
        /// Serializes a message object; always throws when no actual serializer is registered.
        /// </remarks>
        /// <param name="message">要序列化的消息对象 / The message object to serialize</param>
        /// <returns>此方法不会返回 / This method never returns</returns>
        /// <exception cref="InvalidOperationException">未注册消息序列化器时抛出 / Thrown when no message serializer is registered</exception>
        public byte[] Serialize<T>(T message) where T : MessageObject
        {
            throw new InvalidOperationException(
                "No IMessageSerializer registered. " +
                "Call MessageSerializerRegistry.RegisterGlobal() or provide a channel-level serializer.");
        }

        /// <summary>
        /// 反序列化消息对象，未注册实际序列化器时始终抛出异常。
        /// </summary>
        /// <remarks>
        /// Deserializes a message object; always throws when no actual serializer is registered.
        /// </remarks>
        /// <param name="data">要反序列化的字节数组 / The byte array to deserialize</param>
        /// <param name="targetType">目标消息类型 / The target message type</param>
        /// <returns>此方法不会返回 / This method never returns</returns>
        /// <exception cref="InvalidOperationException">未注册消息序列化器时抛出 / Thrown when no message serializer is registered</exception>
        public object Deserialize(byte[] data, Type targetType)
        {
            throw new InvalidOperationException(
                "No IMessageSerializer registered. " +
                "Call MessageSerializerRegistry.RegisterGlobal() or provide a channel-level serializer.");
        }
    }
}
