using System;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 消息序列化器接口，提供消息对象与字节数组之间的转换能力。
    /// </summary>
    /// <remarks>
    /// Message serializer interface that provides conversion between message objects and byte arrays.
    /// </remarks>
    /// <typeparam name="T">消息对象的类型 / Type of the message object</typeparam>
    public interface IMessageSerializer
    {
        /// <summary>
        /// 将消息对象序列化为字节数组。
        /// </summary>
        /// <remarks>
        /// Serializes a message object to a byte array.
        /// </remarks>
        /// <param name="message">要序列化的消息对象 / The message object to serialize</param>
        /// <returns>序列化后的字节数组 / The serialized byte array</returns>
        byte[] Serialize<T>(T message) where T : MessageObject;

        /// <summary>
        /// 将字节数组反序列化为指定类型的消息对象。
        /// </summary>
        /// <remarks>
        /// Deserializes a byte array into a message object of the specified type.
        /// </remarks>
        /// <param name="data">要反序列化的字节数组 / The byte array to deserialize</param>
        /// <param name="targetType">目标消息类型 / The target message type</param>
        /// <returns>反序列化后的消息对象 / The deserialized message object</returns>
        object Deserialize(byte[] data, Type targetType);
    }
}
