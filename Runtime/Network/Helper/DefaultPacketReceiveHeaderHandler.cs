using System;
using System.Buffers;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    public sealed class DefaultPacketReceiveHeaderHandler : IPacketReceiveHeaderHandler, IPacketHandler
    {
        public int PacketLength { get; private set; }
        public int Id { get; private set; }


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
            // timestamp
            long timestamp = reader.ReadLong(ref offset); //8
            // MsgId
            int msgId = reader.ReadInt(ref offset); //4
            Id = msgId;
            return true;
        }

        /// <summary>
        /// 网络包长度
        /// </summary>
        private const int NetPacketLength = 4;

        // 消息码
        private const int NetCmdIdLength = 4;

        // 消息时间戳
        private const int NetTicketLength = 8;

        public DefaultPacketReceiveHeaderHandler()
        {
            PacketHeaderLength = NetPacketLength + NetTicketLength + NetCmdIdLength;
        }

        public int PacketHeaderLength { get; }
    }
}