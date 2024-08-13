using System.IO;
using System.IO.Compression;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息压缩处理
    /// </summary>
    public sealed class DefaultMessageCompressHandler : IMessageCompressHandler, IPacketHandler
    {
        /// <summary>
        /// 压缩处理
        /// </summary>
        /// <param name="message">消息未压缩内容</param>
        /// <returns></returns>
        public byte[] Handler(byte[] message)
        {
            return Compress(message);
        }

        /// <summary>
        /// 压缩数据。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        static byte[] Compress(byte[] bytes)
        {
            var uncompressed = new MemoryStream(bytes);
            var compressed = new MemoryStream();
            var deflateStream = new DeflateStream(compressed, CompressionMode.Compress);
            uncompressed.CopyTo(deflateStream);
            deflateStream.Close();
            return compressed.ToArray();
        }
    }
}