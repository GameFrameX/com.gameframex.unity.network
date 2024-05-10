//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

#if ENABLE_GAME_FRAME_X_WEB_SOCKET
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GameFrameX.Network.Runtime
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// Web Socket 网络频道。
        /// </summary>
        private sealed class WebSocketNetworkChannel : Network.Runtime.NetworkManager.NetworkChannelBase
        {
            private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public WebSocketNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public override void Connect(IPAddress ipAddress, int port, object userData = null)
            {
                base.Connect(ipAddress, port, userData);
                PSocket = new WebSocketNetSocket(ipAddress, port, ReceiveCallback);
                if (PSocket == null)
                {
                    const string errorMessage = "Initialize network channel failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                PNetworkChannelHelper.PrepareForConnecting();
                ConnectAsync(userData);
            }

            public override void Close()
            {
                base.Close();
                _cancellationTokenSource.Cancel();
            }

            private bool IsClose()
            {
                return _cancellationTokenSource.IsCancellationRequested;
            }


            /// <summary>
            /// 处理发送消息对象
            /// </summary>
            /// <param name="messageObject">消息对象</param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            protected override bool ProcessSendMessage(MessageObject messageObject)
            {
                if (IsClose())
                {
                    PActive = false;
                    const string errorMessage = "Network channel is closing.";
                    NetworkChannelError?.Invoke(this, NetworkErrorCode.SocketError, SocketError.Disconnecting, errorMessage);

                    return false;
                }

                bool serializeResult = base.ProcessSendMessage(messageObject);
                if (serializeResult)
                {
                    var webSocketClientNetSocket = (WebSocketNetSocket)PSocket;

                    byte[] buffer = new byte[PSendState.Stream.Length];
                    PSendState.Stream.Seek(0, SeekOrigin.Begin);
                    _ = PSendState.Stream.Read(buffer, 0, buffer.Length);

                    webSocketClientNetSocket.Client.SendAsync(buffer);
                }
                else
                {
                    const string errorMessage = "Serialized packet failure.";
                    throw new InvalidOperationException(errorMessage);
                }

                return true;
            }

            private async void ConnectAsync(object userData)
            {
                try
                {
                    var socketClient = (WebSocketNetSocket)PSocket;
                    await socketClient.ConnectAsync();
                    ConnectCallback(new Network.Runtime.NetworkManager.ConnectState(PSocket, userData));
                }
                catch (Exception exception)
                {
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }


            private void ConnectCallback(Network.Runtime.NetworkManager.ConnectState connectState)
            {
                try
                {
                    var socketUserData = (WebSocketNetSocket)PSocket;
                    if (!socketUserData.IsConnected)
                    {
                        throw new SocketException((int)NetworkErrorCode.ConnectError);
                    }
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ConnectError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                PSentPacketCount = 0;
                PReceivedPacketCount = 0;

                lock (PSendPacketPool)
                {
                    PSendPacketPool.Clear();
                }

                PReceivePacketPool.Clear();

                lock (PHeartBeatState)
                {
                    PHeartBeatState.Reset(true);
                }

                NetworkChannelConnected?.Invoke(this, connectState.UserData);

                PActive = true;
            }

            private void ReceiveCallback(byte[] buffer)
            {
                ProcessReceiveMessage(ref buffer);
            }
        }
    }
}
#endif