using GameFrameX.Runtime;

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
            return ZipHelper.Compress(message);
        }
    }
}