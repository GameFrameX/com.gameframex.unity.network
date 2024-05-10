using System.IO;

namespace GameFrameX.Network.Runtime
{
    public sealed class DefaultPacketSendBodyHandler : IPacketSendBodyHandler, IPacketHandler
    {
        public bool Handler(byte[] messageBodyBuffer, MemoryStream destination)
        {
            destination.Write(messageBodyBuffer, 0, messageBodyBuffer.Length);
            return true;
        }
    }
}