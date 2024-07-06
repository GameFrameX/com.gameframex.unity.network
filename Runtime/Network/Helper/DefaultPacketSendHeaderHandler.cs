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
            // 4 + 4 + 4 + 4 
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


        int m_Count = 0;
        private int m_Offset = 0;
        private readonly byte[] m_CachedByte;

        public bool Handler<T>(T messageObject, MemoryStream destination, out byte[] messageBodyBuffer) where T : MessageObject
        {
            m_Offset = 0;
            messageBodyBuffer = SerializerHelper.Serialize(messageObject);
            var messageType = messageObject.GetType();
            Id = ProtoMessageIdHandler.GetReqMessageIdByType(messageType);
            var messageLength = messageBodyBuffer.Length;
            PacketLength = (ushort)(PacketHeaderLength + messageLength);
            // 数据包总大小
            m_CachedByte.WriteUShort(PacketLength, ref m_Offset);
            // 消息操作类型
            m_CachedByte.WriteByte((byte)(ProtoMessageIdHandler.IsHeartbeat(messageType) ? 1 : 4), ref m_Offset);
            // 消息压缩标记
            m_CachedByte.WriteByte(0, ref m_Offset);
            // 消息编号
            m_CachedByte.WriteInt(messageObject.UniqueId, ref m_Offset);
            // 消息ID
            m_CachedByte.WriteInt(Id, ref m_Offset);
            destination.Write(m_CachedByte, 0, PacketHeaderLength);
            return true;
        }
    }
}