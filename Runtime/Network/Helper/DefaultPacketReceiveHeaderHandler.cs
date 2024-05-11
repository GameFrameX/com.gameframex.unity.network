using System;
using System.Buffers;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    public sealed class DefaultPacketReceiveHeaderHandler : IPacketReceiveHeaderHandler, IPacketHandler
    {
        /// <summary>
        /// 包长度
        /// </summary>
        public int PacketLength { get; private set; }

        /// <summary>
        /// 消息ID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 消息唯一编号
        /// </summary>
        public long UniqueId { get; private set; }


        public bool Handler(object source)
        {
            byte[] reader = source as byte[];
            if (reader == null)
            {
                return false;
            }

            // packetLength
            int offset = 0;
            int packetLength = reader.ReadInt(ref offset); //4
            PacketLength = packetLength;
            // uniqueId
            long uniqueId = reader.ReadLong(ref offset); //8
            UniqueId = uniqueId;
            // MsgId
            int msgId = reader.ReadInt(ref offset); //4
            Id = msgId;
            return true;
        }

        /// <summary>
        /// 网络包长度
        /// </summary>
        private const int NetPacketLength = 4;

        /// <summary>
        /// 消息码
        /// </summary>
        private const int NetCmdIdLength = 4;

        /// <summary>
        /// 消息编号
        /// </summary>
        private const int NetUniqueIdLength = 8;

        public DefaultPacketReceiveHeaderHandler()
        {
            PacketHeaderLength = NetPacketLength + NetUniqueIdLength + NetCmdIdLength;
        }

        /// <summary>
        /// 包头长度
        /// </summary>
        public int PacketHeaderLength { get; }
    }
}