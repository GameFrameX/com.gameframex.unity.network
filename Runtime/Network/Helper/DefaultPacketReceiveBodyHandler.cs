namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息接收内容处理器
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class DefaultPacketReceiveBodyHandler : IPacketReceiveBodyHandler, IPacketHandler
    {
        internal IMessageSerializer ChannelSerializer { get; set; }

        public bool Handler<T>(byte[] source, int messageId, out T messageObject) where T : MessageObject
        {
            var messageType = ProtoMessageIdHandler.GetRespTypeById(messageId);
            messageObject = (T)(ChannelSerializer ?? MessageSerializerRegistry.Global).Deserialize(source, messageType);
            return true;
        }
    }
}