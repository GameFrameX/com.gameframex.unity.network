using System.IO;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络消息包头接口。
    /// </summary>
    public interface IPacketSendHeaderHandler
    {
        /// <summary>
        /// 消息包头长度
        /// </summary>
        ushort PacketHeaderLength { get; }

        /// <summary>
        /// 获取网络消息包协议编号。
        /// </summary>
        int Id { get; }

        /// <summary>
        /// 获取网络消息包长度。
        /// </summary>
        uint PacketLength { get; }

        /// <summary>
        /// 是否压缩消息内容
        /// </summary>
        bool IsZip { get; }

        /// <summary>
        /// 超过消息的长度超过该值的时候启用压缩.该值 必须在设置压缩器的时候才生效
        /// </summary>
        uint LimitCompressLength { get; }

        /// <summary>
        /// 处理消息
        /// </summary>
        /// <param name="messageObject">消息对象</param>
        /// <param name="messageCompressHandler">压缩消息内容处理器</param>
        /// <param name="destination">缓存流</param>
        /// <param name="messageBodyBuffer">消息序列化完的二进制数组</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool Handler<T>(T messageObject, IMessageCompressHandler messageCompressHandler, MemoryStream destination, out byte[] messageBodyBuffer) where T : MessageObject;
    }
}