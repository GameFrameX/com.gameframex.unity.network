using System.Buffers;
using System.IO;
using ProtoBuf;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息发送头部处理器
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class DefaultPacketSendHeaderHandler : IPacketSendHeaderHandler, IPacketHandler
    {
        /// <summary>
        /// 网络包长度
        /// </summary>
        private const int NetPacketLength = 2;

        /// <summary>
        /// 消息码
        /// </summary>
        private const int NetCmdIdLength = 4;

        /// <summary>
        /// 消息操作类型长度
        /// </summary>
        private const int NetOperationTypeLength = 1;

        /// <summary>
        /// 消息压缩标记长度
        /// </summary>
        private const int NetZipFlagLength = 1;

        /// <summary>
        /// 消息编号
        /// </summary>
        private const int NetUniqueIdLength = 4;


        public DefaultPacketSendHeaderHandler()
        {
            // 2 + 1 + 1 + 4 + 4
            PacketHeaderLength = 6;
        }

        /// <summary>
        /// 消息包头长度
        /// </summary>
        public ushort PacketHeaderLength { get; }

        /// <summary>
        /// 获取网络消息包协议编号。
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        public ushort PacketLength { get; private set; }

        /// <summary>
        /// 是否压缩消息内容
        /// </summary>
        public bool IsZip { get; private set; }

        /// <summary>
        /// 超过消息的长度超过该值的时候启用压缩.该值 必须在设置压缩器的时候才生效,默认100
        /// </summary>
        public virtual uint LimitCompressLength { get; } = 100;

        private int m_Offset;

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="messageObject">消息对象</param>
        /// <param name="messageCompressHandler">压缩消息内容处理器</param>
        /// <param name="destination">缓存流</param>
        /// <param name="messageBodyBuffer">消息序列化完的二进制数组</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Handler<T>(T messageObject, IMessageCompressHandler messageCompressHandler, MemoryStream destination, out byte[] messageBodyBuffer) where T : MessageObject
        {
            m_Offset = 0;
            var messageType = messageObject.GetType();
            Id = ProtoMessageIdHandler.GetReqMessageIdByType(messageType);
            messageBodyBuffer = SerializerHelper.Serialize(messageObject);

            var messageObjectHeader = new MessageObjectHeader
            {
                MessageId = Id,
                UniqueId = messageObject.UniqueId,
                OperationType = (byte)(ProtoMessageIdHandler.IsHeartbeat(messageType) ? 1 : 4),
                ZipFlag = 0
            };

            if (messageCompressHandler != null && messageBodyBuffer.Length > LimitCompressLength)
            {
                IsZip = true;
                messageBodyBuffer = messageCompressHandler.Handler(messageBodyBuffer);
                messageObjectHeader.ZipFlag = 1;
            }
            else
            {
                IsZip = false;
                messageObjectHeader.ZipFlag = 0;
            }

            var messageHeaderBuffer = SerializerHelper.Serialize(messageObjectHeader);
            PacketLength = (ushort)(messageHeaderBuffer.Length + messageBodyBuffer.Length);

            var totalCount = PacketHeaderLength + messageHeaderBuffer.Length;
            var cachedByte = ArrayPool<byte>.Shared.Rent(totalCount);
            cachedByte.WriteUInt((uint)totalCount, ref m_Offset);
            cachedByte.WriteUShort((ushort)messageHeaderBuffer.Length, ref m_Offset);
            cachedByte.WriteBytesWithoutLength(messageHeaderBuffer, ref m_Offset);
            destination.Write(cachedByte, 0, totalCount);
            ArrayPool<byte>.Shared.Return(cachedByte);
            return true;
        }
    }
}