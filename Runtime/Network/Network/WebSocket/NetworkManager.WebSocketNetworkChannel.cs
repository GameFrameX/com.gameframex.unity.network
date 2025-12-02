// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
// 
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
// 
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
// 
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
// 
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

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
        [UnityEngine.Scripting.Preserve]
        private sealed class WebSocketNetworkChannel : NetworkChannelBase
        {
            private readonly CancellationTokenSource m_CancellationTokenSource = new CancellationTokenSource();

            /// <summary>
            /// 初始化网络频道的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            /// <param name="rpcTimeout">RPC超时时间</param>
            public WebSocketNetworkChannel(string name, INetworkChannelHelper networkChannelHelper, int rpcTimeout) : base(name, networkChannelHelper, rpcTimeout)
            {
            }

            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="address">远程主机的地址。</param>
            /// <param name="userData">用户自定义数据。</param>
            public override void Connect(Uri address, object userData = null)
            {
                if (PIsConnecting)
                {
                    return;
                }

                base.Connect(address, userData);
                PSocket = new WebSocketNetSocket(address.ToString(), ReceiveCallback, CloseCallback);
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

            private void CloseCallback(string reason, ushort code)
            {
                Close(reason, code);
            }

            public override void Close(string reason, ushort code = 0)
            {
                base.Close(reason, code);
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

                    while (PSendPacketPool.First != null)
                    {
                        var messageObject = PSendPacketPool.First.Value;

                        bool serializeResult;
                        try
                        {
                            DebugSendLog(messageObject);
                            serializeResult = ProcessSendMessage(messageObject);
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
                        finally
                        {
                            PSendPacketPool.RemoveFirst();
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
                    PIsConnecting = true;
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
                PIsConnecting = false;
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
                        var bodyLength = (int)(PacketReceiveHeaderHandler.PacketLength - PacketReceiveHeaderHandler.PacketHeaderLength);
                        PReceiveState.Reset(bodyLength, PacketReceiveHeaderHandler);
                        if (buffer.Length < bodyLength)
                        {
                            return;
                        }

                        var body = buffer.ReadBytes(PacketReceiveHeaderHandler.PacketHeaderLength, bodyLength);
                        if (PReceiveState.PacketHeader.ZipFlag != 0)
                        {
                            // 解压
                            GameFrameworkGuard.NotNull(MessageDecompressHandler, nameof(MessageDecompressHandler));
                            body = MessageDecompressHandler.Handler(body);
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

                        // 将收到的消息加入到链表最后
                        m_ExecutionMessageLinkedList.AddLast(messageObject);

                        PReceivedPacketCount++;
                    }
                    else
                    {
                        NetworkChannelError?.Invoke(this, NetworkErrorCode.DeserializePacketHeaderError, SocketError.Success, "Packet header is invalid.");
                    }
                }
                catch (Exception e)
                {
                    NetworkChannelError?.Invoke(this, NetworkErrorCode.DeserializePacketError, SocketError.Success, "Packet body is invalid." + e.Message + "\n" + e.StackTrace);
                }
            }
        }
    }
}
#endif