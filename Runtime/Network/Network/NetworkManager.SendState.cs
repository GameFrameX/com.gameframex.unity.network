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
        public sealed class SendState : IDisposable
        {
            private const int DefaultBufferLength = 1024 * 64;
            private bool m_Disposed;
            [UnityEngine.Scripting.Preserve]
            public SendState()
            {
                Stream = new MemoryStream(DefaultBufferLength);
                m_Disposed = false;
            }

            public MemoryStream Stream { get; private set; }

            public void Reset()
            {
                Stream.Position = 0L;
                Stream.SetLength(0L);
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private void Dispose(bool disposing)
            {
                if (m_Disposed)
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

                m_Disposed = true;
            }
        }
    }
}