using System.Buffers;
using GameFrameX.Runtime;
using ProtoBuf;

namespace GameFrameX.Network.Runtime
{
    public sealed class DefaultPacketReceiveBodyHandler : IPacketReceiveBodyHandler, IPacketHandler
    {
        public bool Handler<T>(object source, int messageId, out T messageObject) where T : MessageObject
        {
            var messageType = ProtoMessageIdHandler.GetRespTypeById(messageId);
            messageObject = (T)SerializerHelper.Deserialize(source as byte[], messageType);
            return true;
        }
    }
}