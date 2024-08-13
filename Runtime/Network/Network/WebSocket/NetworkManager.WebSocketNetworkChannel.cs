﻿//------------------------------------------------------------
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
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// Web Socket 网络频道。
        /// </summary>
        private sealed class WebSocketNetworkChannel : NetworkChannelBase
        {
            private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

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
            /// <param name="isSsl">是否是加密</param>
            public override void Connect(IPAddress ipAddress, int port, object userData = null, bool isSsl = false)
            {
                base.Connect(ipAddress, port, userData, isSsl);
                PSocket = new WebSocketNetSocket(ipAddress, port, isSsl, ReceiveCallback, CloseCallback);
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

            private void CloseCallback(string errorMessage)
            {
                NetworkChannelClosed?.Invoke(this);
                NetworkChannelError?.Invoke(this, NetworkErrorCode.SocketError, SocketError.SocketError, errorMessage);
            }

            public override void Close()
            {
                base.Close();
                m_CancellationTokenSource.Cancel();
            }

            private bool IsClose()
            {
                return !PSocket.IsConnected && m_CancellationTokenSource.IsCancellationRequested;
            }

            protected override bool ProcessSend()
            {
                lock (PSendPacketPool)
                {
                    if (PSendPacketPool.Count <= 0)
                    {
                        return false;
                    }

                    while (PSendPacketPool.Count > 0)
                    {
                        var messageObject = PSendPacketPool.Dequeue();

                        bool serializeResult = false;
                        try
                        {
                            serializeResult = ProcessSendMessage(messageObject);
                            DebugSendLog(messageObject);
                        }
                        catch (Exception exception)
                        {
                            PActive = false;
                            if (NetworkChannelError != null)
                            {
                                SocketException socketException = exception as SocketException;
                                NetworkChannelError(this, NetworkErrorCode.SerializeError, socketException?.SocketErrorCode ?? SocketError.Success, exception.ToString());
                                return false;
                            }

                            throw;
                        }

                        if (!serializeResult)
                        {
                            const string errorMessage = "Serialized packet failure.";
                            if (NetworkChannelError != null)
                            {
                                NetworkChannelError(this, NetworkErrorCode.SerializeError, SocketError.Success, errorMessage);
                                return false;
                            }

                            throw new GameFrameworkException(errorMessage);
                        }

                        PSendState.Reset();
                    }

                    return true;
                }
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
                    ConnectCallback(new ConnectState(PSocket, userData));
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


            private void ConnectCallback(ConnectState connectState)
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

                lock (PHeartBeatState)
                {
                    PHeartBeatState.Reset(true);
                }

                NetworkChannelConnected?.Invoke(this, connectState.UserData);

                PActive = true;
            }

            private void ReceiveCallback(byte[] buffer)
            {
                try
                {
                    lock (PHeartBeatState)
                    {
                        PHeartBeatState.Reset(PResetHeartBeatElapseSecondsWhenReceivePacket);
                    }

                    PReceivedPacketCount++;

                    if (buffer.Length < PacketReceiveHeaderHandler.PacketHeaderLength)
                    {
                        return;
                    }

                    var processSuccess = PNetworkChannelHelper.DeserializePacketHeader(buffer);
                    if (processSuccess)
                    {
                        var bodyLength = PacketReceiveHeaderHandler.PacketLength - PacketReceiveHeaderHandler.PacketHeaderLength;
                        if (buffer.Length < bodyLength)
                        {
                            return;
                        }

                        var body = buffer.ReadBytes(PacketReceiveHeaderHandler.PacketHeaderLength, bodyLength);
                        if (PReceiveState.PacketHeader.ZipFlag != 0)
                        {
                            // 解压
                            GameFrameworkGuard.NotNull(MessageDecompressHandler, nameof(MessageDecompressHandler));
                            body = MessageDecompressHandler.Handler(buffer);
                        }

                        // 反序列化数据
                        processSuccess = PNetworkChannelHelper.DeserializePacketBody(body, PacketReceiveHeaderHandler.Id, out var messageObject);
                        if (processSuccess)
                        {
                            messageObject.SetUpdateUniqueId(PacketReceiveHeaderHandler.UniqueId);
                        }

                        DebugReceiveLog(messageObject);
                        if (!processSuccess)
                        {
                            if (NetworkChannelError != null)
                            {
                                NetworkChannelError(this, NetworkErrorCode.DeserializePacketError, SocketError.Success, "Packet body is invalid.");
                                return;
                            }
                        }

                        var replySuccess = PRpcState.Reply(messageObject);
                        if (!replySuccess)
                        {
                            InvokeMessageHandler(messageObject);
                        }

                        PReceivedPacketCount++;
                    }
                    else
                    {
                        NetworkChannelError?.Invoke(this, NetworkErrorCode.DeserializePacketHeaderError, SocketError.Success, "Packet header is invalid.");
                    }
                }
                catch (Exception e)
                {
                    NetworkChannelError?.Invoke(this, NetworkErrorCode.DeserializePacketError, SocketError.Success, "Packet body is invalid." + e.Message);
                }
            }
        }
    }
}
#endif