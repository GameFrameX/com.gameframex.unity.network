//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络消息包头接口。
    /// </summary>
    public interface IPacketReceiveHeaderHandler
    {
        /// <summary>
        /// 消息包头长度
        /// </summary>
        ushort PacketHeaderLength { get; }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        ushort PacketLength { get; }

        /// <summary>
        /// 消息包头
        /// </summary>
        INetworkMessageHeader Header { get; }

        /// <summary>
        /// 消息包处理
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        bool Handler(object source);
    }
}