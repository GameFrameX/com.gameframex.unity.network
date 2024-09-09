using ProtoBuf;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息接收内容处理器
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class DefaultPacketReceiveBodyHandler : IPacketReceiveBodyHandler, IPacketHandler
    {
        public bool Handler<T>(byte[] source, int messageId, out T messageObject) where T : MessageObject
        {
            var messageType = ProtoMessageIdHandler.GetRespTypeById(messageId);
            messageObject = (T)SerializerHelper.Deserialize(source, messageType);
            return true;
        }
    }
}