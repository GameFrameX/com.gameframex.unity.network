//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.IO;

namespace GameFrameX.Network.Runtime
{
    public sealed partial class NetworkManager
    {
        public sealed class ReceiveState : IDisposable
        {
            public const int DefaultBufferLength = 1024 * 64;
            private bool _disposed;

            public ReceiveState()
            {
                Stream = new MemoryStream(DefaultBufferLength);
                _disposed = false;
            }

            public MemoryStream Stream { get; private set; }
            public IPacketReceiveHeaderHandler PacketHeader { get; set; }

            public void PrepareForPacketHeader(int packetHeaderLength = 16)
            {
                Reset(packetHeaderLength, null);
            }

            public void PrepareForPacket(IPacketReceiveHeaderHandler packetReceiveHeaderHandler)
            {
                GameFrameworkGuard.NotNull(packetReceiveHeaderHandler, nameof(packetReceiveHeaderHandler));

                Reset(packetReceiveHeaderHandler.PacketLength, packetReceiveHeaderHandler);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (_disposed)
                {
                    return;
                }

                if (disposing)
                {
                    if (Stream != null)
                    {
                        Stream.Dispose();
                        Stream = null;
                    }
                }

                _disposed = true;
            }

            public void Reset(int targetLength, IPacketReceiveHeaderHandler packetHeader)
            {
                if (targetLength < 0)
                {
                    throw new GameFrameworkException("Target length is invalid.");
                }

                Stream.Position = 0L;
                Stream.SetLength(targetLength);
                PacketHeader = packetHeader;
            }
        }
    }
}