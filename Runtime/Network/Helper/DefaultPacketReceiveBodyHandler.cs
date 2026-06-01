namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息接收内容处理器
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class DefaultPacketReceiveBodyHandler : IPacketReceiveBodyHandler, IPacketHandler
    {
        /// <summary>
        /// 通道级别的消息序列化器，为空时回退使用全局序列化器。
        /// </summary>
        /// <remarks>
        /// Channel-level message serializer; falls back to the global serializer when null.
        /// </remarks>
        internal IMessageSerializer ChannelSerializer { get; set; }

        public bool Handler<T>(byte[] source, int messageId, out T messageObject) where T : MessageObject
        {
            var messageType = ProtoMessageIdHandler.GetRespTypeById(messageId);
            if (messageType == null)
            {
                messageObject = default(T);
                return false;
            }

            var deserialized = (ChannelSerializer ?? MessageSerializerRegistry.Global).Deserialize(source, messageType);
            messageObject = (T)deserialized;
            return messageObject != null;
        }
    }
}