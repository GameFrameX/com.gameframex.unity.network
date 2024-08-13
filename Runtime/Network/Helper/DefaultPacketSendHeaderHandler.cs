using System.IO;
using ProtoBuf;

namespace GameFrameX.Network.Runtime
{
    public sealed class DefaultPacketSendHeaderHandler : IPacketSendHeaderHandler, IPacketHandler
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
            // 4 + 1 + 1 + 4 + 4 + 4
            PacketHeaderLength = NetPacketLength + NetOperationTypeLength + NetZipFlagLength + NetUniqueIdLength + NetCmdIdLength;
            m_CachedByte = new byte[PacketHeaderLength];
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

        private int m_Offset = 0;
        private readonly byte[] m_CachedByte;

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
            IsZip = messageCompressHandler != null;
            messageBodyBuffer = SerializerHelper.Serialize(messageObject);
            if (IsZip)
            {
                messageBodyBuffer = messageCompressHandler.Handler(messageBodyBuffer);
            }

            var messageLength = messageBodyBuffer.Length;
            PacketLength = (ushort)(PacketHeaderLength + messageLength);
            // 数据包总大小
            m_CachedByte.WriteUShort(PacketLength, ref m_Offset);
            // 消息操作类型
            m_CachedByte.WriteByte((byte)(ProtoMessageIdHandler.IsHeartbeat(messageType) ? 1 : 4), ref m_Offset);
            // 消息压缩标记
            m_CachedByte.WriteByte((byte)(IsZip ? 1 : 0), ref m_Offset);
            // 消息编号
            m_CachedByte.WriteInt(messageObject.UniqueId, ref m_Offset);
            // 消息ID
            m_CachedByte.WriteInt(Id, ref m_Offset);
            destination.Write(m_CachedByte, 0, PacketHeaderLength);
            return true;
        }
    }
}