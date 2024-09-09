using System.IO;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认消息发送内容处理器
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class DefaultPacketSendBodyHandler : IPacketSendBodyHandler, IPacketHandler
    {
        public bool Handler(byte[] messageBodyBuffer, MemoryStream destination)
        {
            destination.Write(messageBodyBuffer, 0, messageBodyBuffer.Length);
            return true;
        }
    }
}