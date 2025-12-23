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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// 网络频道基类。
        /// </summary>
        public abstract class NetworkChannelBase : INetworkChannel, IDisposable
        {
            /// <summary>
            /// 默认心跳间隔
            /// </summary>
            private const float DefaultHeartBeatInterval = 30f;

            /// <summary>
            /// 默认心跳丢失断开次数
            /// </summary>
            private const int DefaultMissHeartBeatCountByClose = 10;

            protected readonly GameFrameworkLinkedList<MessageObject> PSendPacketPool;
            protected readonly INetworkChannelHelper PNetworkChannelHelper;
            protected AddressFamily PAddressFamily;

            /// <summary>
            /// 当收到数据包时是否重置心跳流逝时长
            /// </summary>
            protected bool PResetHeartBeatElapseSecondsWhenReceivePacket;

            /// <summary>
            /// 心跳间隔
            /// </summary>
            protected float PHeartBeatInterval;

            /// <summary>
            /// 心跳丢失次数
            /// </summary>
            protected int MissHeartBeatCountByClose;

            /// <summary>
            /// 发送消息ID忽略列表
            /// </summary>
            protected List<int> IgnoreSendIds = new List<int>();

            /// <summary>
            /// 接收消息ID忽略列表
            /// </summary>
            protected List<int> IgnoreReceiveIds = new List<int>();

            /// <summary>
            /// 网络Socket 对象
            /// </summary>
            protected INetworkSocket PSocket;

            protected readonly SendState PSendState;
            protected readonly ReceiveState PReceiveState;
            protected readonly HeartBeatState PHeartBeatState;
            protected readonly RpcState PRpcState;

            /// <summary>
            /// 是否验证地址
            /// </summary>
            protected bool IsVerifyAddress = true;

            /// <summary>
            /// 链接目标地址
            /// </summary>
            protected IPEndPoint ConnectEndPoint;

            /// <summary>
            /// 发送数据包的数量
            /// </summary>
            protected int PSentPacketCount;

            /// <summary>
            /// 接收数据包数量
            /// </summary>
            protected int PReceivedPacketCount;

            /// <summary>
            /// 是否在应用程序获得焦点时发送心跳包
            /// </summary>
            protected bool PFocusHeartbeat;

            /// <summary>
            /// 是否正在连接中
            /// </summary>
            protected bool PIsConnecting = false;

            private bool m_Disposed;
            private bool m_PActive;

            /// <summary>
            /// 网络是否激活
            /// </summary>
            protected bool PActive
            {
                get { return m_PActive; }
                set
                {
                    if (m_PActive == value)
                    {
                        return;
                    }

                    m_PActive = value;
                    NetworkChannelActiveChanged?.Invoke(this, m_PActive);
                }
            }

            private IPacketSendHeaderHandler m_PacketSendHeaderHandler;
            private IPacketSendBodyHandler m_PacketSendBodyHandler;
            private IPacketReceiveHeaderHandler m_PacketReceiveHeaderHandler;
            private IPacketReceiveBodyHandler m_PacketReceiveBodyHandler;
            private IPacketHeartBeatHandler m_PacketHeartBeatHandler;

            protected readonly GameFrameworkLinkedList<MessageObject> m_ExecutionMessageLinkedList = new GameFrameworkLinkedList<MessageObject>();

            public Action<NetworkChannelBase, object> NetworkChannelConnected;
            public Action<NetworkChannelBase, string, ushort> NetworkChannelClosed;
            public Action<NetworkChannelBase, bool> NetworkChannelActiveChanged;
            public Action<NetworkChannelBase, int> NetworkChannelMissHeartBeat;
            public Action<NetworkChannelBase, NetworkErrorCode, SocketError, string> NetworkChannelError;

            /// <summary>
            /// 初始化网络频道基类的新实例。
            /// </summary>
            /// <param name="name">网络频道名称。</param>
            /// <param name="networkChannelHelper">网络频道辅助器。</param>
            /// <param name="rpcTimeout">RPC超时时间</param>
            [UnityEngine.Scripting.Preserve]
            public NetworkChannelBase(string name, INetworkChannelHelper networkChannelHelper, int rpcTimeout)
            {
                Name = name ?? string.Empty;
                PSendPacketPool = new GameFrameworkLinkedList<MessageObject>();
                PNetworkChannelHelper = networkChannelHelper;
                PAddressFamily = AddressFamily.Unknown;
                PResetHeartBeatElapseSecondsWhenReceivePacket = false;
                PHeartBeatInterval = DefaultHeartBeatInterval;
                MissHeartBeatCountByClose = DefaultMissHeartBeatCountByClose;
                PSocket = null;
                PSendState = new SendState();
                PReceiveState = new ReceiveState();
                PHeartBeatState = new HeartBeatState();
                PRpcState = new RpcState(rpcTimeout);
                PSentPacketCount = 0;
                PReceivedPacketCount = 0;
                PActive = false;
                PFocusHeartbeat = true;
                PIsConnecting = false;
                m_Disposed = false;
                NetworkChannelConnected = null;
                NetworkChannelClosed = null;
                NetworkChannelMissHeartBeat = null;
                NetworkChannelError = null;

                networkChannelHelper.Initialize(this);
            }

            #region 属性

            /// <summary>
            /// 获取网络频道名称。
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// 获取网络频道所使用的 Socket。
            /// </summary>
            public INetworkSocket Socket
            {
                get { return PSocket; }
            }

            /// <summary>
            /// 获取是否已连接。
            /// </summary>
            public bool Connected
            {
                get
                {
                    if (PSocket != null)
                    {
                        return PSocket.IsConnected;
                    }

                    return false;
                }
            }

            /// <summary>
            /// 获取网络地址类型。
            /// </summary>
            public AddressFamily AddressFamily
            {
                get { return PAddressFamily; }
            }

            /// <summary>
            /// 获取要发送的消息包数量。
            /// </summary>
            public int SendPacketCount
            {
                get
                {
                    lock (PSendPacketPool)
                    {
                        return PSendPacketPool.Count;
                    }
                }
            }

            /// <summary>
            /// 获取累计发送的消息包数量。
            /// </summary>
            public int SentPacketCount
            {
                get { return PSentPacketCount; }
            }

            /// <summary>
            /// 获取累计已接收的消息包数量。
            /// </summary>
            public int ReceivedPacketCount
            {
                get { return PReceivedPacketCount; }
            }

            /// <summary>
            /// 获取或设置当收到消息包时是否重置心跳流逝时间。
            /// </summary>
            public bool ResetHeartBeatElapseSecondsWhenReceivePacket
            {
                get { return PResetHeartBeatElapseSecondsWhenReceivePacket; }
                set { PResetHeartBeatElapseSecondsWhenReceivePacket = value; }
            }

            /// <summary>
            /// 获取丢失心跳的次数。
            /// </summary>
            public int MissHeartBeatCount
            {
                get
                {
                    lock (PHeartBeatState)
                    {
                        return PHeartBeatState.MissHeartBeatCount;
                    }
                }
            }

            /// <summary>
            /// 获取或设置心跳间隔时长，以秒为单位。
            /// </summary>
            public float HeartBeatInterval
            {
                get { return PHeartBeatInterval; }
                set { PHeartBeatInterval = value; }
            }

            /// <summary>
            /// 获取心跳等待时长，以秒为单位。
            /// </summary>
            public float HeartBeatElapseSeconds
            {
                get
                {
                    lock (PHeartBeatState)
                    {
                        return PHeartBeatState.HeartBeatElapseSeconds;
                    }
                }
            }

            /// <summary>
            /// 消息发送包头处理器
            /// </summary>
            public IPacketSendHeaderHandler PacketSendHeaderHandler
            {
                get { return m_PacketSendHeaderHandler; }
            }

            /// <summary>
            /// 消息发送内容处理器
            /// </summary>
            public IPacketSendBodyHandler PacketSendBodyHandler
            {
                get { return m_PacketSendBodyHandler; }
            }

            /// <summary>
            /// 消息接收包头处理器
            /// </summary>
            public IPacketReceiveHeaderHandler PacketReceiveHeaderHandler
            {
                get { return m_PacketReceiveHeaderHandler; }
            }

            /// <summary>
            /// 心跳消息处理器
            /// </summary>
            public IPacketHeartBeatHandler PacketHeartBeatHandler
            {
                get { return m_PacketHeartBeatHandler; }
            }

            /// <summary>
            /// 消息接收内容处理器
            /// </summary>
            public IPacketReceiveBodyHandler PacketReceiveBodyHandler
            {
                get { return m_PacketReceiveBodyHandler; }
            }

            /// <summary>
            /// 消息压缩处理器
            /// </summary>
            public IMessageCompressHandler MessageCompressHandler { get; private set; }

            /// <summary>
            /// 消息解压处理器
            /// </summary>
            public IMessageDecompressHandler MessageDecompressHandler { get; private set; }

            #endregion

            /// <summary>
            /// 网络频道轮询。
            /// </summary>
            /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
            /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
            public virtual void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (PSocket == null || !PActive)
                {
                    return;
                }

                ProcessSend();
                ProcessReceive();
                if (PSocket == null || !PActive)
                {
                    return;
                }

                ProcessHeartBeat(realElapseSeconds);
                ProcessReceivedMessage();
                PRpcState.Update(elapseSeconds, realElapseSeconds);
            }

            /// <summary>
            /// 处理接收到的消息
            /// </summary>
            private void ProcessReceivedMessage()
            {
                while (m_ExecutionMessageLinkedList.First != null)
                {
                    var messageObject = m_ExecutionMessageLinkedList.First.Value;
                    try
                    {
                        // 执行RPC匹配
                        var replySuccess = PRpcState.TryReply(messageObject);
                        if (replySuccess)
                        {
                            continue;
                        }

                        // 执行通知消息
                        var handlers = ProtoMessageHandler.GetHandlers(messageObject.GetType());
                        foreach (var handler in handlers)
                        {
                            handler.SetMessageObject(messageObject);
                            try
                            {
                                handler.Invoke();
                            }
                            catch (Exception e)
                            {
                                Log.Fatal(e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Fatal(e);
                    }
                    finally
                    {
                        m_ExecutionMessageLinkedList.RemoveFirst();
                    }
                }
            }

            /// <summary>
            /// 处理心跳
            /// </summary>
            /// <param name="realElapseSeconds"></param>
            private void ProcessHeartBeat(float realElapseSeconds)
            {
                if (PHeartBeatInterval > 0f)
                {
                    bool sendHeartBeat = false;
                    int missHeartBeatCount = 0;
                    lock (PHeartBeatState)
                    {
                        if (PSocket == null || !PActive)
                        {
                            return;
                        }

                        PHeartBeatState.HeartBeatElapseSeconds += realElapseSeconds;
                        if (PHeartBeatState.HeartBeatElapseSeconds >= PHeartBeatInterval)
                        {
                            sendHeartBeat = true;
                            missHeartBeatCount = PHeartBeatState.MissHeartBeatCount;
                            PHeartBeatState.HeartBeatElapseSeconds = 0f;
                            PHeartBeatState.MissHeartBeatCount++;
                        }

                        if (sendHeartBeat && PFocusHeartbeat)
                        {
                            if (PNetworkChannelHelper.SendHeartBeat())
                            {
                                if (missHeartBeatCount > 0 && NetworkChannelMissHeartBeat != null)
                                {
                                    NetworkChannelMissHeartBeat(this, missHeartBeatCount);
                                }

                                // PHeartBeatState.Reset(this.ResetHeartBeatElapseSecondsWhenReceivePacket);
                                return;
                            }
                        }

                        if (PHeartBeatState.MissHeartBeatCount > MissHeartBeatCountByClose)
                        {
                            // 心跳丢失达到上线。触发断开
                            Close(NetworkCloseReason.MissHeartBeat, (ushort)NetworkErrorCode.MissHeartBeatError);
                        }
                    }
                }
            }

            /// <summary>
            /// 关闭网络频道。
            /// </summary>
            public virtual void Shutdown()
            {
                Close(NetworkCloseReason.Normal);
                PSendState.Reset();
                PNetworkChannelHelper.Shutdown();
            }

            /// <summary>
            /// 注册消息压缩处理器
            /// </summary>
            /// <param name="handler">处理器对象,当设置为空的时候，不启用消息压缩</param>
            [UnityEngine.Scripting.Preserve]
            public void RegisterMessageCompressHandler(IMessageCompressHandler handler)
            {
                MessageCompressHandler = handler;
            }

            /// <summary>
            /// 注册消息解压处理器
            /// </summary>
            /// <param name="handler">处理器对象,当设置为空的时候，不启用消息解压</param>
            [UnityEngine.Scripting.Preserve]
            public void RegisterMessageDecompressHandler(IMessageDecompressHandler handler)
            {
                MessageDecompressHandler = handler;
            }

            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数。</param>
            [UnityEngine.Scripting.Preserve]
            public void RegisterHandler(IPacketSendHeaderHandler handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_PacketSendHeaderHandler = handler;
            }


            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数。</param>
            [UnityEngine.Scripting.Preserve]
            public void RegisterHandler(IPacketSendBodyHandler handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_PacketSendBodyHandler = handler;
            }

            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数。</param>
            [UnityEngine.Scripting.Preserve]
            public void RegisterHandler(IPacketReceiveHeaderHandler handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_PacketReceiveHeaderHandler = handler;
            }

            /// <summary>
            /// 注册网络消息包处理函数。
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数。</param>
            [UnityEngine.Scripting.Preserve]
            public void RegisterHandler(IPacketReceiveBodyHandler handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_PacketReceiveBodyHandler = handler;
            }

            /// <summary>
            /// 注册网络消息心跳处理函数，用于处理心跳消息
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数</param>
            [Obsolete("Use RegisterHeartBeatHandler instead")]
            [UnityEngine.Scripting.Preserve]
            public void RegisterHandler(IPacketHeartBeatHandler handler)
            {
                RegisterHeartBeatHandler(handler);
            }

            /// <summary>
            /// 注册网络消息心跳处理函数，用于处理心跳消息
            /// </summary>
            /// <param name="handler">要注册的网络消息包处理函数</param>
            [UnityEngine.Scripting.Preserve]
            public void RegisterHeartBeatHandler(IPacketHeartBeatHandler handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_PacketHeartBeatHandler = handler;
                if (handler.HeartBeatInterval > 0)
                {
                    PHeartBeatInterval = handler.HeartBeatInterval;
                }

                if (handler.MissHeartBeatCountByClose > 0)
                {
                    MissHeartBeatCountByClose = handler.MissHeartBeatCountByClose;
                }
            }

            /// <summary>
            /// 设置RPC 的 ErrorCode 不为 0 的时候的处理函数
            /// </summary>
            /// <param name="handler"></param>
            [UnityEngine.Scripting.Preserve]
            public void SetRPCErrorCodeHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                PRpcState.SetRPCErrorCodeHandler(handler);
            }

            /// <summary>
            /// 设置RPC错误的处理函数
            /// </summary>
            /// <param name="handler"></param>
            [UnityEngine.Scripting.Preserve]
            public void SetRPCErrorHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                PRpcState.SetRPCErrorHandler(handler);
            }

            /// <summary>
            /// 设置RPC开始的处理函数
            /// </summary>
            /// <param name="handler"></param>
            [UnityEngine.Scripting.Preserve]
            public void SetRPCStartHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                PRpcState.SetRPCStartHandler(handler);
            }

            /// <summary>
            /// 设置RPC结束的处理函数
            /// </summary>
            /// <param name="handler"></param>
            [UnityEngine.Scripting.Preserve]
            public void SetRPCEndHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                PRpcState.SetRPCEndHandler(handler);
            }


            /// <summary>
            /// 连接到远程主机。
            /// </summary>
            /// <param name="address">远程主机的地址。</param>
            /// <param name="userData">用户自定义数据。</param>
            [UnityEngine.Scripting.Preserve]
            public virtual void Connect(Uri address, object userData = null)
            {
                if (PSocket != null)
                {
                    Close(NetworkCloseReason.ConnectClose, (ushort)NetworkErrorCode.DisposeError);
                    PSocket = null;
                }

                IsVerifyAddress = true;
                ConnectEndPoint = null;
                if (IPAddress.TryParse(address.Host, out var ipAddress))
                {
                    ConnectEndPoint = new IPEndPoint(ipAddress, address.Port);
                }
                else
                {
                    try
                    {
                        var ipHost = Utility.Net.GetHostIPv4(address.Host);
                        if (IPAddress.TryParse(ipHost, out ipAddress))
                        {
                            ConnectEndPoint = new IPEndPoint(ipAddress, address.Port);
                        }
                        else
                        {
                            // 获取IP失败
                            Log.Error($"IP address is invalid.{address.Host}");
                            IsVerifyAddress = false;
                            Close(NetworkCloseReason.ConnectAddressError, (ushort)NetworkErrorCode.ConnectError);
                            PSocket = null;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Error($"IP address is invalid.{address.Host} {e.Message}");
                        IsVerifyAddress = false;
                        Close(NetworkCloseReason.ConnectAddressExceptionError, (ushort)NetworkErrorCode.ConnectError);
                        PSocket = null;
                    }
                }

                if (IsVerifyAddress && ConnectEndPoint != null)
                {
                    switch (ConnectEndPoint.AddressFamily)
                    {
                        case System.Net.Sockets.AddressFamily.InterNetwork:
                            PAddressFamily = AddressFamily.IPv4;
                            break;

                        case System.Net.Sockets.AddressFamily.InterNetworkV6:
                            PAddressFamily = AddressFamily.IPv6;
                            break;

                        default:
                            string errorMessage = Utility.Text.Format("Not supported address family '{0}'.", ConnectEndPoint.AddressFamily);
                            if (NetworkChannelError != null)
                            {
                                NetworkChannelError(this, NetworkErrorCode.AddressFamilyError, SocketError.Success, errorMessage);
                                return;
                            }

                            throw new GameFrameworkException(errorMessage);
                    }
                }


                PSendState.Reset();
                PReceiveState.PrepareForPacketHeader();
            }

            /// <summary>
            /// 关闭连接并释放所有相关资源。
            /// </summary>
            /// <param name="reason">关闭原因。</param>
            /// <param name="code">关闭错误码。</param>
            [UnityEngine.Scripting.Preserve]
            public virtual void Close(string reason, ushort code = 0)
            {
                lock (this)
                {
                    if (PSocket == null)
                    {
                        return;
                    }

                    PActive = false;

                    try
                    {
                        PSocket.Shutdown();
                    }
                    catch
                    {
                        // ignored
                    }
                    finally
                    {
                        PSocket.Close();
                        PSocket = null;
                        NetworkChannelClosed?.Invoke(this, reason, code);
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

                    PRpcState.Dispose();
                    m_ExecutionMessageLinkedList.Clear();
                }
            }

            /// <summary>
            /// 向远程主机发送消息包
            /// </summary>
            /// <param name="messageObject">要发送的消息包。</param>
            /// <param name="isIgnoreErrorCode">是否忽略错误码，默认值为 false。如果为 true，则在调用时不会抛出异常。和RPC的错误码回调也会忽略错误码。</param>
            /// <typeparam name="TResult"></typeparam>
            [UnityEngine.Scripting.Preserve]
            public async Task<TResult> Call<TResult>(MessageObject messageObject, bool isIgnoreErrorCode = false) where TResult : MessageObject, IResponseMessage
            {
                GameFrameworkGuard.NotNull(messageObject, nameof(messageObject));
                Send(messageObject);
                var result = await PRpcState.Call(messageObject, isIgnoreErrorCode);
                return result as TResult;
            }

            /// <summary>
            /// 向远程主机发送消息包。
            /// </summary>
            /// <typeparam name="T">消息包类型。</typeparam>
            /// <param name="messageObject">要发送的消息包。</param>
            [UnityEngine.Scripting.Preserve]
            public void Send<T>(T messageObject) where T : MessageObject
            {
                GameFrameworkGuard.NotNull(messageObject, nameof(messageObject));
                if (PSocket == null)
                {
                    const string errorMessage = "You must connect first.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (!PActive)
                {
                    const string errorMessage = "Socket is not active.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                if (messageObject == null)
                {
                    const string errorMessage = "Packet is invalid.";
                    if (NetworkChannelError != null)
                    {
                        NetworkChannelError(this, NetworkErrorCode.SendError, SocketError.Success, errorMessage);
                        return;
                    }

                    throw new GameFrameworkException(errorMessage);
                }

                lock (PSendPacketPool)
                {
                    PSendPacketPool.AddLast(messageObject);
                }
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// 释放资源。
            /// </summary>
            /// <param name="disposing">释放资源标记。</param>
            private void Dispose(bool disposing)
            {
                if (m_Disposed)
                {
                    return;
                }

                m_Disposed = true;
                if (disposing)
                {
                    Close(NetworkCloseReason.Dispose, (ushort)NetworkErrorCode.DisposeError);
                    PSendState.Dispose();
                    PReceiveState.Dispose();
                }
            }

            /// <summary>
            /// 处理发送消息对象
            /// </summary>
            /// <param name="messageObject">消息对象</param>
            /// <returns></returns>
            /// <exception cref="InvalidOperationException"></exception>
            protected virtual bool ProcessSendMessage(MessageObject messageObject)
            {
                bool serializeResult = PNetworkChannelHelper.SerializePacketHeader(messageObject, PSendState.Stream, out var messageBodyBuffer);
                if (serializeResult)
                {
                    serializeResult = PNetworkChannelHelper.SerializePacketBody(messageBodyBuffer, PSendState.Stream);
                }
                else
                {
                    const string errorMessage = "Serialized packet failure.";
                    throw new InvalidOperationException(errorMessage);
                }

                ReferencePool.Release(messageObject);
                return serializeResult;
            }

            /// <summary>
            /// 处理消息发送
            /// </summary>
            /// <returns></returns>
            /// <exception cref="GameFrameworkException"></exception>
            protected virtual bool ProcessSend()
            {
                lock (PSendPacketPool)
                {
                    if (PSendState.Stream.Length > 0 || PSendPacketPool.Count <= 0)
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

                        // PSendState.Reset();
                    }

                    PSendState.Stream.Position = 0L;
                    return true;
                }
            }

            protected void ProcessReceive()
            {
            }

            protected void DebugSendLog(MessageObject messageObject)
            {
#if ENABLE_GAMEFRAMEX_NETWORK_SEND_LOG
                var messageId = ProtoMessageIdHandler.GetReqMessageIdByType(messageObject.GetType());
                if (!IgnoreSendIds.Contains(messageId))
                {
                    Log.Debug($"发送消息 ID:[{messageId},{messageObject.UniqueId},{messageObject.GetType().Name}] 消息内容:{Utility.Json.ToJson(messageObject)}");
                }
#endif
            }

            protected void DebugReceiveLog(MessageObject messageObject)
            {
#if ENABLE_GAMEFRAMEX_NETWORK_RECEIVE_LOG
                var messageId = ProtoMessageIdHandler.GetRespMessageIdByType(messageObject.GetType());
                if (!IgnoreReceiveIds.Contains(messageId))
                {
                    Log.Debug($"收到消息 ID:[{messageId},{messageObject.UniqueId},{messageObject.GetType().Name}] 消息内容:{Utility.Json.ToJson(messageObject)}");
                }
#endif
            }


            /// <summary>
            /// 设置忽略的消息打印列表
            /// </summary>
            /// <param name="sendIds">发送列表</param>
            /// <param name="receiveIds">接收列表</param>
            [UnityEngine.Scripting.Preserve]
            public void SetIgnoreLogNetworkIds(List<int> sendIds, List<int> receiveIds)
            {
                IgnoreSendIds = sendIds;
                IgnoreReceiveIds = receiveIds;
            }

            /// <summary>
            /// 设置是否开启心跳包失去焦点时也发送心跳包
            /// </summary>
            /// <param name="hasFocus">是否开启心跳包失去焦点时也发送心跳包</param>
            public void SetFocusHeartbeat(bool hasFocus)
            {
                PFocusHeartbeat = hasFocus;
            }
        }
    }
}