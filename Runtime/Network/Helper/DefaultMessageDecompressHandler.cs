using System.Buffers;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息解压处理
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class DefaultMessageDecompressHandler : IMessageDecompressHandler, IPacketHandler
    {
        /// <summary>
        /// 解压处理
        /// </summary>
        /// <param name="message">消息压缩内容</param>
        /// <returns></returns>
        public byte[] Handler(byte[] message)
        {
            return Decompress(message);
        }

        /// <summary>
        /// 解压数据。
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        static byte[] Decompress(byte[] bytes)
        {
            using (var compressedStream = new MemoryStream(bytes))
            {
                using (var gZipInputStream = new GZipInputStream(compressedStream))
                {
                    using (var decompressedStream = new MemoryStream())
                    {
                        var buffer = ArrayPool<byte>.Shared.Rent(8192);
                        try
                        {
                            int count;
                            while ((count = gZipInputStream.Read(buffer, 0, buffer.Length)) != 0)
                            {
                                decompressedStream.Write(buffer, 0, count);
                            }
                        }
                        finally
                        {
                            ArrayPool<byte>.Shared.Return(buffer);
                        }

                        var array = decompressedStream.ToArray();
                        return array;
                    }
                }
            }
        }
    }
}