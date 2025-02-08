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
        [UnityEngine.Scripting.Preserve]
        private sealed class SystemTcpNetworkChannel : NetworkChannelBase
        {
            private ConnectState m_ConnectState = null;
            private IPEndPoint m_ConnectedEndPoint = null;
            private SystemNetSocket PSystemNetSocket = null;

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            /// <param name="rpcTimeout">RPC超时时间</param>
            public SystemTcpNetworkChannel(string name, INetworkChannelHelper networkChannelHelper, int rpcTimeout) : base(name, networkChannelHelper, rpcTimeout)
            {
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="ipAddress">远程主机的 IP 地址。</param>
            /// <param name="port">远程主机的端口号。</param>
            /// <param name="userData">用户自定义数据。</param>
            /// <param name="isSsl">是否是加密</param>
            public override void Connect(IPAddress ipAddress, int port, object userData = null, bool isSsl = false)
            {
                if (PIsConnecting)
                {
                    return;
                }

                m_ConnectedEndPoint = new IPEndPoint(ipAddress, port);
                base.Connect(ipAddress, port, userData, isSsl);
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

                ConnectAsync(userData);
            }

            protected override bool ProcessSend()
            {
                if (base.ProcessSend())
                {
                    SendAsync();
                    return true;
                }

                return false;
            }

            #region Receive

            private void ReceiveAsync()
            {
                try
                {
                    var position = (int)PReceiveState.Stream.Position;
                    var length = (int)(PReceiveState.Stream.Length - PReceiveState.Stream.Position);
                    PSystemNetSocket.BeginReceive(PReceiveState.Stream.GetBuffer(), position, length, SocketFlags.None, ReceiveCallback, PSystemNetSocket);
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void ReceiveCallback(IAsyncResult asyncResult)
            {
                var systemNetSocket = (SystemNetSocket)asyncResult.AsyncState;
                if (!systemNetSocket.IsConnected)
                {
                    return;
                }

                int bytesReceived = 0;
                try
                {
                    bytesReceived = systemNetSocket.EndReceive(asyncResult);
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.ReceiveError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }

                if (bytesReceived <= 0)
                {
                    Close();
                    return;
                }

                lock (PHeartBeatState)
                {
                    PHeartBeatState.Reset(PResetHeartBeatElapseSecondsWhenReceivePacket);
                }

                PReceiveState.Stream.Position += bytesReceived;
                if (PReceiveState.Stream.Position < PReceiveState.Stream.Length)
                {
                    ReceiveAsync();
                    return;
                }

                PReceiveState.Stream.Position = 0L;
                bool processSuccess;
                if (PReceiveState.PacketHeader != null)
                {
                    processSuccess = ProcessPackBody();
                }
                else
                {
                    processSuccess = ProcessPackHeader();
                    if (PReceiveState.IsEmptyBody)
                    {
                        // 如果是空消息,直接返回
                        ProcessPackBody();
                        ReceiveAsync();
                        return;
                    }
                }

                if (processSuccess)
                {
                    ReceiveAsync();
                    return;
                }
            }

            /// <summary>
            /// 解析消息头
            /// </summary>
            /// <returns></returns>
            private bool ProcessPackHeader()
            {
                var headerLength = PacketReceiveHeaderHandler.PacketHeaderLength;
                var buffer = new byte[headerLength];
                _ = PReceiveState.Stream.Read(buffer, 0, headerLength);
                var processSuccess = PNetworkChannelHelper.DeserializePacketHeader(buffer);
                var bodyLength = (int)(PacketReceiveHeaderHandler.PacketLength - PacketReceiveHeaderHandler.PacketHeaderLength);
                PReceiveState.Reset(bodyLength, PacketReceiveHeaderHandler);
                return processSuccess;
            }

            /// <summary>
            /// 解析消息内容
            /// </summary>
            /// <returns></returns>
            private bool ProcessPackBody()
            {
                var bodyLength = (int)(PReceiveState.PacketHeader.PacketLength - PReceiveState.PacketHeader.PacketHeaderLength);
                var buffer = new byte[bodyLength];
                _ = PReceiveState.Stream.Read(buffer, 0, bodyLength);

                if (PReceiveState.PacketHeader.ZipFlag != 0)
                {
                    // 解压
                    GameFrameworkGuard.NotNull(MessageDecompressHandler, nameof(MessageDecompressHandler));
                    buffer = MessageDecompressHandler.Handler(buffer);
                }

                var processSuccess = PNetworkChannelHelper.DeserializePacketBody(buffer, PacketReceiveHeaderHandler.Id, out var messageObject);
                if (processSuccess)
                {
                    messageObject.SetUpdateUniqueId(PacketReceiveHeaderHandler.UniqueId);
                }

                DebugReceiveLog(messageObject);

                var replySuccess = PRpcState.TryReply(messageObject);
                if (!replySuccess)
                {
                    InvokeMessageHandler(messageObject);
                }

                PReceivedPacketCount++;
                PReceiveState.PrepareForPacketHeader();
                return processSuccess;
            }

            #endregion

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
                }
                else
                {
                    const string errorMessage = "Serialized packet failure.";
                    throw new InvalidOperationException(errorMessage);
                }

                return true;
            }


            /// <summary>
            /// 实际发送异步数据
            /// </summary>
            private void SendAsync()
            {
                try
                {
                    PSystemNetSocket.BeginSend(PSendState.Stream.GetBuffer(), (int)PSendState.Stream.Position, (int)(PSendState.Stream.Length - PSendState.Stream.Position), SocketFlags.None, SendCallback, PSystemNetSocket);
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                        return;
                    }

                    throw;
                }
            }

            private void SendCallback(IAsyncResult asyncResult)
            {
                var systemNetSocket = (SystemNetSocket)asyncResult.AsyncState;
                if (!systemNetSocket.IsConnected)
                {
                    return;
                }

                int bytesSent = 0;
                try
                {
                    bytesSent = systemNetSocket.EndSend(asyncResult, out var error);
                }
                catch (Exception exception)
                {
                    PActive = false;
                    if (NetworkChannelError != null)
                    {
                        SocketException socketException = exception as SocketException;
                        NetworkChannelError(this, NetworkErrorCode.SendError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                    }

                    return;
                }

                PSendState.Stream.Position += bytesSent;
                if (PSendState.Stream.Position < PSendState.Stream.Length)
                {
                    SendAsync();
                    return;
                }

                PSentPacketCount++;
                PSendState.Reset();
            }

            #endregion

            #region Connect

            private void ConnectAsync(object userData)
            {
                try
                {
                    PIsConnecting = true;
                    m_ConnectState = new ConnectState(PSystemNetSocket, userData);
                    ((SystemNetSocket)PSocket).BeginConnect(m_ConnectedEndPoint.Address, m_ConnectedEndPoint.Port, ConnectCallback, m_ConnectState);
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

            private void ConnectCallback(IAsyncResult asyncResult)
            {
                PIsConnecting = false;
                var connectState = (ConnectState)asyncResult.AsyncState;
                var systemNetSocket = (SystemNetSocket)connectState.Socket;
                try
                {
                    systemNetSocket.EndConnect(asyncResult);
                }
                catch (ObjectDisposedException)
                {
                    return;
                }
                catch (Exception exception)
                {
                    SocketException socketException = exception as SocketException;
                    NetworkChannelError?.Invoke(this, NetworkErrorCode.ConnectError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                    Close();
                    return;
                }

                PSentPacketCount = 0;
                PReceivedPacketCount = 0;

                lock (PSendPacketPool)
                {
                    PSendPacketPool.Clear();
                }


                lock (PHeartBeatState)
                {
                    PHeartBeatState.Reset(true);
                }

                NetworkChannelConnected?.Invoke(this, m_ConnectState.UserData);
                PActive = true;
                ReceiveAsync();
            }

            #endregion
        }
    }
}