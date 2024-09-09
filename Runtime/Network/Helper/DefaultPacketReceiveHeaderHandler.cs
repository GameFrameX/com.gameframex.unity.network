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
        /// 消息ID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 消息唯一编号
        /// </summary>
        public int UniqueId { get; private set; }

        /// <summary>
        /// 消息操作类型
        /// </summary>
        public byte OperationType { get; private set; }

        /// <summary>
        /// 压缩标记
        /// </summary>
        public byte ZipFlag { get; private set; }


        /// <summary>
        /// 消息包处理
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public bool Handler(object source)
        {
            byte[] reader = source as byte[];
            if (reader == null)
            {
                return false;
            }

            // packetLength
            int offset = 0;
            var packetLength = reader.ReadUShort(ref offset); //4
            PacketLength = packetLength;
            // operationType
            OperationType = reader.ReadByte(ref offset); //1
            // zipFlag
            ZipFlag = reader.ReadByte(ref offset); //1
            // uniqueId
            UniqueId = reader.ReadInt(ref offset); //4
            // MsgId
            Id = reader.ReadInt(ref offset); //4
            return true;
        }

        /// <summary>
        /// 网络包长度
        /// </summary>
        private const int NetPacketLength = 2;

        /// <summary>
        /// 操作消息类型
        /// </summary>
        private const int OperationTypeLength = 1;

        /// <summary>
        /// 消息压缩标记长度
        /// </summary>
        private const int NetZipFlagLength = 1;

        /// <summary>
        /// 消息码
        /// </summary>
        private const int NetCmdIdLength = 4;

        /// <summary>
        /// 消息编号
        /// </summary>
        private const int NetUniqueIdLength = 4;

        /// <summary>
        /// 包头长度 2 + 1 + 1 + 4 + 4
        /// </summary>
        public ushort PacketHeaderLength { get; } = NetPacketLength + OperationTypeLength + NetZipFlagLength + NetUniqueIdLength + NetCmdIdLength;
    }
}