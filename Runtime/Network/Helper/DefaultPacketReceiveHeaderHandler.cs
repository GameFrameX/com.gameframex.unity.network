using ProtoBuf;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息接收头处理器
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class DefaultPacketReceiveHeaderHandler : IPacketReceiveHeaderHandler, IPacketHandler
    {
        /// <summary>
        /// 包长度
        /// </summary>
        public ushort PacketLength { get; private set; }

        /// <summary>
        /// 消息包头
        /// </summary>
        public INetworkMessageHeader Header { get; private set; }

        /// <summary>
        /// 消息包处理
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool Handler(object source)
        {
            if (!(source is byte[] reader))
            {
                return false;
            }

            m_Offset = 0;
            // 消息总长度
            PacketLength = (ushort)reader.ReadUInt(ref m_Offset); // 4
            // 消息头长度
            PacketHeaderLength = reader.ReadUShort(ref m_Offset); // 2
            // 消息头字节数组
            var messageHeaderData = reader.ReadBytes(ref m_Offset, PacketHeaderLength);
            // 消息对象头
            Header = (INetworkMessageHeader)SerializerHelper.Deserialize(messageHeaderData, typeof(MessageObjectHeader));
            return true;
        }

        private int m_Offset;

        /// <summary>
        /// 固定包头长度 4(总包长度) + 2(包头长度)
        /// </summary>
        public ushort FixedPacketHeaderLength { get; } = 4 + 2;

        public ushort PacketHeaderLength { get; private set; } = 4 + 2 + sizeof(int) + sizeof(int) + sizeof(byte) + sizeof(byte);
    }
}