using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息压缩处理
    /// </summary>
    [UnityEngine.Scripting.Preserve]
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
        /// <param name="inputBytes"></param>
        /// <returns></returns>
        static byte[] Compress(byte[] inputBytes)
        {
            using (var compressStream = new MemoryStream())
            {
                using (var gZipOutputStream = new GZipOutputStream(compressStream))
                {
                    gZipOutputStream.Write(inputBytes, 0, inputBytes.Length);
                    var press = compressStream.ToArray();
                    return press;
                }
            }
        }
    }
}