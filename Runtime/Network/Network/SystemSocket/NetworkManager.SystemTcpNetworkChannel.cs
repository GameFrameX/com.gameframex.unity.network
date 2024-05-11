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
            private IPEndPoint m_ConnectedEndPoint = null;
            private SystemNetSocket PSystemNetSocket = null;

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            public SystemTcpNetworkChannel(string name, INetworkChannelHelper networkChannelHelper)
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
                m_ConnectedEndPoint = new IPEndPoint(ipAddress, port);
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
                    PSystemNetSocket.BeginReceive(PReceiveState.Stream.GetBuffer(), (int)PReceiveState.Stream.Position, (int)(PReceiveState.Stream.Length - PReceiveState.Stream.Position), SocketFlags.None, ReceiveCallback, PSystemNetSocket);
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
                var buffer = new byte[PacketReceiveHeaderHandler.PacketHeaderLength];
                _ = PReceiveState.Stream.Read(buffer, 0, PacketReceiveHeaderHandler.PacketHeaderLength);
                var processSuccess = PNetworkChannelHelper.DeserializePacketHeader(buffer);
                PReceiveState.Reset(PacketReceiveHeaderHandler.PacketLength - PacketReceiveHeaderHandler.PacketHeaderLength, PacketReceiveHeaderHandler);
                return processSuccess;
            }

            /// <summary>
            /// 解析消息内容
            /// </summary>
            /// <returns></returns>
            private bool ProcessPackBody()
            {
                var buffer = new byte[PReceiveState.PacketHeader.PacketLength - PReceiveState.PacketHeader.PacketHeaderLength];
                _ = PReceiveState.Stream.Read(buffer, 0, PReceiveState.PacketHeader.PacketLength - PReceiveState.PacketHeader.PacketHeaderLength);
                var processSuccess = PNetworkChannelHelper.DeserializePacketBody(buffer, PacketReceiveHeaderHandler.Id, out var messageObject);
                if (processSuccess)
                {
                    messageObject.SetUpdateUniqueId(PacketReceiveHeaderHandler.UniqueId);
                }

                Log.Debug($"收到消息 ID:[{PacketReceiveHeaderHandler.Id},{messageObject.UniqueId}] ==>消息类型:{messageObject.GetType()} 消息内容:{Utility.Json.ToJson(messageObject)}");

                bool replySuccess = PRpcState.Reply(messageObject);
                if (!replySuccess)
                {
                    PacketBase packetBase = ReferencePool.Acquire<PacketBase>();
                    packetBase.MessageObject = messageObject;
                    packetBase.MessageId = PacketReceiveHeaderHandler.Id;
                    PReceivePacketPool.Fire(this, packetBase);
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

                PReceivePacketPool.Clear();

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