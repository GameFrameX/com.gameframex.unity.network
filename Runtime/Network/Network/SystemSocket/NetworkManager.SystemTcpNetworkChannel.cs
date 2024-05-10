//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// TCP 网络频道。
        /// </summary>
        private sealed class SystemTcpNetworkChannel : NetworkChannelBase
        {
            private ConnectState m_ConnectState = null;
            private SocketAsyncEventArgs _receiveEventArgs = new SocketAsyncEventArgs();
            private SocketAsyncEventArgs _sendEventArgs = new SocketAsyncEventArgs();
            private const int BufferSize = 8192;
            private byte[] _incompletePacket;
            private IPEndPoint connectedEndPoint = null;
            private SystemNetSocket PSystemNetSocket = null;
            private readonly byte[] _receiveBuffer = new byte[BufferSize];

            private bool isSending = false;
            private bool isReceiving = false;

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public SystemTcpNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
                : base(name, networkChannelHelper)
            {
                // 初始化接收用的 SocketAsyncEventArgs
                _receiveEventArgs.SetBuffer(_receiveBuffer, 0, _receiveBuffer.Length);
                _receiveEventArgs.Completed += Completed;

                // 初始化发送用的 SocketAsyncEventArgs
                _sendEventArgs.Completed += Completed;
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            public override void Connect(IPAddress ipAddress, int port, object userData = null)
            {
                connectedEndPoint = new IPEndPoint(ipAddress, port);
                base.Connect(ipAddress, port, userData);
                PSystemNetSocket = new SystemNetSocket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                PSocket = PSystemNetSocket;
                if (PSocket == null)
                {
                    string errorMessage = "Initialize network channel failure.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                PNetworkChannelHelper.PrepareForConnecting();
                m_ConnectState = new ConnectState(PSystemNetSocket, userData);
                ConnectAsync();
            }

            /*protected override void ProcessReceive()
            {
                base.ProcessReceive();
                if (isReceiving)
                {
                    return;
                }

                ReceiveAsync();
            }*/

            protected override bool ProcessSend()
            {
                if (isSending)
                {
                    return false;
                }

                if (base.ProcessSend())
                {
                    // SendAsync();
                    return true;
                }

                return false;
            }

            private void Completed(object sender, SocketAsyncEventArgs e)
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Receive:
                        isReceiving = false;
                        ProcessReceive(e);
                        break;
                    case SocketAsyncOperation.Send:
                        isSending = false;
                        ProcessSend(e);
                        break;
                    default:
                        throw new Exception("Invalid operation completed");
                }
            }

            private void ReceiveAsync()
            {
                if (isReceiving)
                {
                    return;
                }

                isReceiving = true;
                bool willRaiseEvent = PSystemNetSocket.Socket.ReceiveAsync(_receiveEventArgs);
                if (!willRaiseEvent)
                {
                    ProcessReceive(_receiveEventArgs);
                }
                else
                {
                    isReceiving = false;
                }
            }

            private void ProcessReceive(SocketAsyncEventArgs e)
            {
                isReceiving = false;
                if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
                {
                    PReceiveState.Stream.Write(e.Buffer, e.Offset, e.BytesTransferred);
                    var buffer = PReceiveState.Stream.GetBuffer();
                    ProcessReceiveMessage(ref buffer);
                    // We're ready to receive more data
                    ReceiveAsync();
                }
                else
                {
                    Close();
                }
            }

            #region Sender

            protected override bool ProcessSendMessage(MessageObject messageObject)
            {
                if (PActive == false)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SocketError, SocketError.Disconnecting, "Network channel is closing.");
                    }

                    return false;
                }

                bool serializeResult = base.ProcessSendMessage(messageObject);
                if (serializeResult)
                {
                    SendAsync();
                }
                else
                {
                    const string errorMessage = "Serialized packet failure.";
                    throw new InvalidOperationException(errorMessage);
                }

                return true;
            }

            private void ProcessSend(SocketAsyncEventArgs e)
            {
                if (e.SocketError == SocketError.Success)
                {
                    // 数据发送成功
                    Log.Info("发送成功");
                    isSending = false;
                }
                else
                {
                    // 处理错误或断开连接
                    Close();
                }
            }

            /// <summary>
            /// 实际发送异步数据
            /// </summary>
            private void SendAsync()
            {
                try
                {
                    // Log.Info("SendAsync");
                    isSending = true;
                    _sendEventArgs.SetBuffer(PSendState.Stream.GetBuffer(), 0, (int)PSendState.Stream.Length);
                    // 发送数据
                    bool willRaiseEvent = PSystemNetSocket.Socket.SendAsync(_sendEventArgs);
                    if (!willRaiseEvent)
                    {
                        // 发送数据完成
                        PSentPacketCount++;
                        PSendState.Reset();
                        isSending = false;
                        ProcessSend(_sendEventArgs);
                    }
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException != null ? socketException.SocketErrorCode : SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            #endregion


            #region Connect

            private void ConnectAsync()
            {
                try
                {
                    ((SystemNetSocket)PSocket).Socket.Connect(connectedEndPoint);

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

                    NetworkChannelConnected?.Invoke(this, m_ConnectState.UserData);

                    PActive = true;
                    ReceiveAsync();
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

            #endregion
        }
    }
}