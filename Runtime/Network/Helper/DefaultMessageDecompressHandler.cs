using System.IO;
using System.IO.Compression;

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
            using (var compressed = new MemoryStream(bytes))
            {
                using (var decompressed = new MemoryStream())
                {
                    // 注意： 这里第一个参数同样是填写压缩的数据，但是这次是作为输入的数据
                    using (var gZipStream = new GZipStream(compressed, CompressionMode.Decompress))
                    {
                        gZipStream.CopyTo(decompressed);
                    }

                    var result = decompressed.ToArray();
                    return result;
                }
            }
        }
    }
}