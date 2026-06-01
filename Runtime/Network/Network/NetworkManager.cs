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
using System.Collections.Generic;
using System.Net.Sockets;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络管理器。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed partial class NetworkManager : GameFrameworkModule, INetworkManager
    {
        private readonly Dictionary<string, NetworkChannelBase> m_NetworkChannels;
        private readonly List<NetworkChannelBase> m_NetworkChannelSnapshot = new List<NetworkChannelBase>();

        private EventHandler<NetworkConnectedEventArgs> m_NetworkConnectedEventHandler;
        private EventHandler<NetworkClosedEventArgs> m_NetworkClosedEventHandler;
        private EventHandler<NetworkMissHeartBeatEventArgs> m_NetworkMissHeartBeatEventHandler;
        private EventHandler<NetworkErrorEventArgs> m_NetworkErrorEventHandler;
        private EventHandler<NetworkReconnectingEventArgs> m_NetworkReconnectingEventHandler;
        private EventHandler<NetworkReconnectedEventArgs> m_NetworkReconnectedEventHandler;
        private EventHandler<NetworkReconnectFailedEventArgs> m_NetworkReconnectFailedEventHandler;

        private readonly object m_NetworkConnectedLock = new object();
        private readonly object m_NetworkClosedLock = new object();
        private readonly object m_NetworkMissHeartBeatLock = new object();
        private readonly object m_NetworkErrorLock = new object();
        private readonly object m_NetworkReconnectingLock = new object();
        private readonly object m_NetworkReconnectedLock = new object();
        private readonly object m_NetworkReconnectFailedLock = new object();

        private readonly Dictionary<string, ReconnectState> m_ReconnectStates = new Dictionary<string, ReconnectState>(StringComparer.Ordinal);
        private readonly Dictionary<string, ConnectInfo> m_ChannelConnectInfos = new Dictionary<string, ConnectInfo>(StringComparer.Ordinal);
        private bool m_AutoReconnectEnabled;
        private int m_AutoReconnectMaxRetryCount = 5;

        /// <summary>
        /// 初始化网络管理器的新实例。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public NetworkManager()
        {
            m_NetworkChannels = new Dictionary<string, NetworkChannelBase>(StringComparer.Ordinal);
            m_NetworkConnectedEventHandler = null;
            m_NetworkClosedEventHandler = null;
            m_NetworkMissHeartBeatEventHandler = null;
            m_NetworkErrorEventHandler = null;
            m_NetworkReconnectingEventHandler = null;
            m_NetworkReconnectedEventHandler = null;
            m_NetworkReconnectFailedEventHandler = null;
            m_AutoReconnectEnabled = false;
            m_AutoReconnectMaxRetryCount = 5;
        }

        /// <summary>
        /// 获取网络频道数量。
        /// </summary>
        public int NetworkChannelCount
        {
            get { return m_NetworkChannels.Count; }
        }

        /// <summary>
        /// 网络连接成功事件。
        /// </summary>
        public event EventHandler<NetworkConnectedEventArgs> NetworkConnected
        {
            add { m_NetworkConnectedEventHandler += value; }
            remove { m_NetworkConnectedEventHandler -= value; }
        }

        /// <summary>
        /// 网络连接关闭事件。
        /// </summary>
        public event EventHandler<NetworkClosedEventArgs> NetworkClosed
        {
            add { m_NetworkClosedEventHandler += value; }
            remove { m_NetworkClosedEventHandler -= value; }
        }

        /// <summary>
        /// 网络心跳包丢失事件。
        /// </summary>
        public event EventHandler<NetworkMissHeartBeatEventArgs> NetworkMissHeartBeat
        {
            add { m_NetworkMissHeartBeatEventHandler += value; }
            remove { m_NetworkMissHeartBeatEventHandler -= value; }
        }

        /// <summary>
        /// 网络错误事件。
        /// </summary>
        public event EventHandler<NetworkErrorEventArgs> NetworkError
        {
            add { m_NetworkErrorEventHandler += value; }
            remove { m_NetworkErrorEventHandler -= value; }
        }

        /// <summary>
        /// 网络重连中事件。
        /// </summary>
        public event EventHandler<NetworkReconnectingEventArgs> NetworkReconnecting
        {
            add { m_NetworkReconnectingEventHandler += value; }
            remove { m_NetworkReconnectingEventHandler -= value; }
        }

        /// <summary>
        /// 网络重连成功事件。
        /// </summary>
        public event EventHandler<NetworkReconnectedEventArgs> NetworkReconnected
        {
            add { m_NetworkReconnectedEventHandler += value; }
            remove { m_NetworkReconnectedEventHandler -= value; }
        }

        /// <summary>
        /// 网络重连失败事件。
        /// </summary>
        public event EventHandler<NetworkReconnectFailedEventArgs> NetworkReconnectFailed
        {
            add { m_NetworkReconnectFailedEventHandler += value; }
            remove { m_NetworkReconnectFailedEventHandler -= value; }
        }

        /// <summary>
        /// 网络管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected override void Update(float elapseSeconds, float realElapseSeconds)
        {
            m_NetworkChannelSnapshot.Clear();
            m_NetworkChannelSnapshot.AddRange(m_NetworkChannels.Values);
            for (int i = 0; i < m_NetworkChannelSnapshot.Count; i++)
            {
                m_NetworkChannelSnapshot[i].Update(elapseSeconds, realElapseSeconds);
            }

            ProcessReconnectStates(realElapseSeconds);
        }

        /// <summary>
        /// 关闭并清理网络管理器。
        /// </summary>
        protected override void Shutdown()
        {
            foreach (var networkChannel in m_NetworkChannels)
            {
                var networkChannelBase = networkChannel.Value;
                networkChannelBase.NetworkChannelConnected -= OnNetworkChannelConnected;
                networkChannelBase.NetworkChannelClosed -= OnNetworkChannelClosed;
                networkChannelBase.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                networkChannelBase.NetworkChannelError -= OnNetworkChannelError;
                networkChannelBase.Shutdown();
            }

            m_NetworkChannels.Clear();
            m_ReconnectStates.Clear();
            m_ChannelConnectInfos.Clear();
        }

        /// <summary>
        /// 检查是否存在网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>是否存在网络频道。</returns>
        public bool HasNetworkChannel(string channelName)
        {
            return m_NetworkChannels.ContainsKey(channelName ?? string.Empty);
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>要获取的网络频道。</returns>
        public INetworkChannel GetNetworkChannel(string channelName)
        {
            if (m_NetworkChannels.TryGetValue(channelName ?? string.Empty, out var networkChannel))
            {
                return networkChannel;
            }

            return null;
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <returns>所有网络频道。</returns>
        public INetworkChannel[] GetAllNetworkChannels()
        {
            var index = 0;
            var results = new INetworkChannel[m_NetworkChannels.Count];
            foreach (var networkChannel in m_NetworkChannels)
            {
                results[index++] = networkChannel.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有网络频道。
        /// </summary>
        /// <param name="results">所有网络频道。</param>
        public void GetAllNetworkChannels(List<INetworkChannel> results)
        {
            GameFrameworkGuard.NotNull(results, nameof(results));

            results.Clear();
            foreach (var networkChannel in m_NetworkChannels)
            {
                results.Add(networkChannel.Value);
            }
        }

        /// <summary>
        /// 创建网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <param name="networkChannelHelper">网络频道辅助器。</param>
        /// <param name="rpcTimeout">RPC超时时间</param>
        /// <returns>要创建的网络频道。</returns>
        public INetworkChannel CreateNetworkChannel(string channelName, INetworkChannelHelper networkChannelHelper, int rpcTimeout)
        {
            GameFrameworkGuard.NotNullOrEmpty(channelName, nameof(channelName));
            GameFrameworkGuard.NotNull(networkChannelHelper, nameof(networkChannelHelper));

            if (HasNetworkChannel(channelName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Already exist network channel '{0}'.", channelName ?? string.Empty));
            }
#if (ENABLE_GAME_FRAME_X_WEB_SOCKET && UNITY_WEBGL) || FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET
            NetworkChannelBase networkChannel = new WebSocketNetworkChannel(channelName, networkChannelHelper, rpcTimeout);
#else
            NetworkChannelBase networkChannel = new SystemTcpNetworkChannel(channelName, networkChannelHelper, rpcTimeout);
#endif
            networkChannel.NetworkChannelConnected += OnNetworkChannelConnected;
            networkChannel.NetworkChannelClosed += OnNetworkChannelClosed;
            networkChannel.NetworkChannelMissHeartBeat += OnNetworkChannelMissHeartBeat;
            networkChannel.NetworkChannelError += OnNetworkChannelError;
            m_NetworkChannels.Add(channelName, networkChannel);
            return networkChannel;
        }

        /// <summary>
        /// 销毁网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        /// <returns>是否销毁网络频道成功。</returns>
        public bool DestroyNetworkChannel(string channelName)
        {
            GameFrameworkGuard.NotNullOrEmpty(channelName, nameof(channelName));
            if (m_NetworkChannels.TryGetValue(channelName, out var networkChannel))
            {
                networkChannel.NetworkChannelConnected -= OnNetworkChannelConnected;
                networkChannel.NetworkChannelClosed -= OnNetworkChannelClosed;
                networkChannel.NetworkChannelMissHeartBeat -= OnNetworkChannelMissHeartBeat;
                networkChannel.NetworkChannelError -= OnNetworkChannelError;
                networkChannel.Shutdown();
                return m_NetworkChannels.Remove(channelName);
            }

            return false;
        }

        /// <summary>
        /// 设置是否开启心跳包失去焦点时也发送心跳包
        /// </summary>
        /// <param name="hasFocus">是否开启心跳包失去焦点时也发送心跳包</param>
        public void SetFocusHeartbeat(bool hasFocus)
        {
            foreach (var networkChannel in m_NetworkChannels.Values)
            {
                networkChannel.SetFocusHeartbeat(hasFocus);
            }
        }

        private void OnNetworkChannelConnected(NetworkChannelBase networkChannel, object userData)
        {
            // 存储连接信息，用于后续可能的重连
            m_ChannelConnectInfos[networkChannel.Name] = new ConnectInfo
            {
                Address = networkChannel.PLastConnectAddress,
                UserData = networkChannel.PLastConnectUserData
            };

            // 检测重连成功
            if (m_ReconnectStates.TryGetValue(networkChannel.Name, out var reconnectState))
            {
                int retryCount = reconnectState.RetryCount;
                reconnectState.Reset();
                m_ReconnectStates.Remove(networkChannel.Name);
                OnNetworkReconnected(networkChannel, retryCount);
            }

            if (m_NetworkConnectedEventHandler != null)
            {
                lock (m_NetworkConnectedLock)
                {
                    NetworkConnectedEventArgs networkConnectedEventArgs = NetworkConnectedEventArgs.Create(networkChannel, userData);
                    m_NetworkConnectedEventHandler(this, networkConnectedEventArgs);
                    // ReferencePool.Release(networkConnectedEventArgs);
                }
            }
        }

        private void OnNetworkChannelClosed(NetworkChannelBase networkChannel, string reason, ushort errorCode)
        {
            if (m_NetworkClosedEventHandler != null)
            {
                lock (m_NetworkClosedLock)
                {
                    NetworkClosedEventArgs networkClosedEventArgs = NetworkClosedEventArgs.Create(networkChannel, reason, errorCode);
                    m_NetworkClosedEventHandler(this, networkClosedEventArgs);
                    // ReferencePool.Release(networkClosedEventArgs);
                }
            }

            // 判断是否需要自动重连
            if (!m_AutoReconnectEnabled)
            {
                return;
            }

            if (reason == NetworkCloseReason.ServerKick ||
                reason == NetworkCloseReason.Dispose ||
                reason == NetworkCloseReason.Normal)
            {
                return;
            }

            // 通道已销毁则不重连
            if (!m_NetworkChannels.ContainsKey(networkChannel.Name))
            {
                return;
            }

            // 已在重连中，说明本次关闭是重连失败
            if (m_ReconnectStates.TryGetValue(networkChannel.Name, out var existingState))
            {
                bool canRetry = existingState.PrepareNextRetry();
                if (!canRetry)
                {
                    OnNetworkReconnectFailed(networkChannel, existingState.RetryCount, "Max retry count reached.");
                    existingState.Reset();
                    m_ReconnectStates.Remove(networkChannel.Name);
                }

                return;
            }

            // 首次触发重连
            if (!m_ChannelConnectInfos.TryGetValue(networkChannel.Name, out var connectInfo))
            {
                return;
            }

            var newReconnectState = new ReconnectState();
            newReconnectState.Start(m_AutoReconnectMaxRetryCount);
            m_ReconnectStates[networkChannel.Name] = newReconnectState;
        }

        private void OnNetworkChannelMissHeartBeat(NetworkChannelBase networkChannel, int missHeartBeatCount)
        {
            if (m_NetworkMissHeartBeatEventHandler != null)
            {
                lock (m_NetworkMissHeartBeatLock)
                {
                    NetworkMissHeartBeatEventArgs networkMissHeartBeatEventArgs = NetworkMissHeartBeatEventArgs.Create(networkChannel, missHeartBeatCount);
                    m_NetworkMissHeartBeatEventHandler(this, networkMissHeartBeatEventArgs);
                    // ReferencePool.Release(networkMissHeartBeatEventArgs);
                }
            }
        }

        private void OnNetworkChannelError(NetworkChannelBase networkChannel, NetworkErrorCode errorCode, SocketError socketErrorCode, string errorMessage)
        {
            if (m_NetworkErrorEventHandler != null)
            {
                lock (m_NetworkErrorLock)
                {
                    NetworkErrorEventArgs networkErrorEventArgs = NetworkErrorEventArgs.Create(networkChannel, errorCode, socketErrorCode, errorMessage);
                    m_NetworkErrorEventHandler(this, networkErrorEventArgs);
                    // ReferencePool.Release(networkErrorEventArgs);
                }
            }
        }

        /// <summary>
        /// 设置是否启用自动重连。
        /// </summary>
        /// <param name="enabled">是否启用自动重连。</param>
        public void SetAutoReconnect(bool enabled)
        {
            m_AutoReconnectEnabled = enabled;
        }

        /// <summary>
        /// 设置自动重连最大重试次数。
        /// </summary>
        /// <param name="maxRetryCount">最大重试次数。</param>
        public void SetAutoReconnectMaxRetryCount(int maxRetryCount)
        {
            m_AutoReconnectMaxRetryCount = maxRetryCount;
        }

        /// <summary>
        /// 手动触发重连指定的网络频道。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        public void ManualReconnect(string channelName)
        {
            if (!m_NetworkChannels.TryGetValue(channelName, out var channel))
            {
                return;
            }

            if (!m_ChannelConnectInfos.TryGetValue(channelName, out var connectInfo))
            {
                return;
            }

            // 取消已有的重连状态
            if (m_ReconnectStates.TryGetValue(channelName, out var existingState))
            {
                existingState.Reset();
                m_ReconnectStates.Remove(channelName);
            }

            var newReconnectState = new ReconnectState();
            newReconnectState.Start(m_AutoReconnectMaxRetryCount);
            m_ReconnectStates[channelName] = newReconnectState;
        }

        /// <summary>
        /// 取消指定网络频道的重连。
        /// </summary>
        /// <param name="channelName">网络频道名称。</param>
        public void CancelReconnect(string channelName)
        {
            if (m_ReconnectStates.TryGetValue(channelName, out var state))
            {
                state.Cancel();
                state.Reset();
                m_ReconnectStates.Remove(channelName);
            }
        }

        private void ProcessReconnectStates(float realElapseSeconds)
        {
            if (m_ReconnectStates.Count <= 0)
            {
                return;
            }

            var snapshot = new List<string>(m_ReconnectStates.Keys);
            for (int i = 0; i < snapshot.Count; i++)
            {
                string channelName = snapshot[i];
                if (!m_ReconnectStates.TryGetValue(channelName, out var reconnectState))
                {
                    continue;
                }

                if (!reconnectState.IsWaiting)
                {
                    continue;
                }

                bool delayElapsed = reconnectState.Update(realElapseSeconds);
                if (!delayElapsed)
                {
                    continue;
                }

                if (!m_ChannelConnectInfos.TryGetValue(channelName, out var connectInfo))
                {
                    OnNetworkReconnectFailed(channelName, reconnectState.RetryCount, "No connect address stored.");
                    reconnectState.Reset();
                    m_ReconnectStates.Remove(channelName);
                    continue;
                }

                if (!m_NetworkChannels.TryGetValue(channelName, out var channel))
                {
                    m_ReconnectStates.Remove(channelName);
                    continue;
                }

                // 触发重连中事件
                OnNetworkReconnecting(channel, reconnectState);

                // 尝试连接
                channel.Connect(connectInfo.Address, connectInfo.UserData);
            }
        }

        private void OnNetworkReconnecting(NetworkChannelBase networkChannel, ReconnectState reconnectState)
        {
            if (m_NetworkReconnectingEventHandler != null)
            {
                lock (m_NetworkReconnectingLock)
                {
                    NetworkReconnectingEventArgs args = NetworkReconnectingEventArgs.Create(
                        networkChannel, reconnectState.RetryCount, reconnectState.MaxRetryCount, reconnectState.TargetDelaySeconds);
                    m_NetworkReconnectingEventHandler(this, args);
                }
            }
        }

        private void OnNetworkReconnected(NetworkChannelBase networkChannel, int retryCount)
        {
            if (m_NetworkReconnectedEventHandler != null)
            {
                lock (m_NetworkReconnectedLock)
                {
                    NetworkReconnectedEventArgs args = NetworkReconnectedEventArgs.Create(networkChannel, retryCount);
                    m_NetworkReconnectedEventHandler(this, args);
                }
            }
        }

        private void OnNetworkReconnectFailed(string channelName, int retryCount, string reason)
        {
            if (m_NetworkReconnectFailedEventHandler == null)
            {
                return;
            }

            if (!m_NetworkChannels.TryGetValue(channelName, out var channel))
            {
                return;
            }

            lock (m_NetworkReconnectFailedLock)
            {
                NetworkReconnectFailedEventArgs args = NetworkReconnectFailedEventArgs.Create(channel, retryCount, reason);
                m_NetworkReconnectFailedEventHandler(this, args);
            }
        }

        private void OnNetworkReconnectFailed(NetworkChannelBase networkChannel, int retryCount, string reason)
        {
            if (m_NetworkReconnectFailedEventHandler != null)
            {
                lock (m_NetworkReconnectFailedLock)
                {
                    NetworkReconnectFailedEventArgs args = NetworkReconnectFailedEventArgs.Create(networkChannel, retryCount, reason);
                    m_NetworkReconnectFailedEventHandler(this, args);
                }
            }
        }

        /// <summary>
        /// 连接信息。
        /// </summary>
        private sealed class ConnectInfo
        {
            public Uri Address { get; set; }
            public object UserData { get; set; }
        }
    }
}