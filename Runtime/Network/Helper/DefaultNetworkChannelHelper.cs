using System;
using System.IO;
using System.Reflection;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;


namespace GameFrameX.Network.Runtime
{
    public class DefaultNetworkChannelHelper : INetworkChannelHelper, IReference
    {
        private MemoryStream _cachedStream;
        private INetworkChannel m_NetworkChannel;

        public DefaultNetworkChannelHelper()
        {
            _cachedStream = new MemoryStream(1024);
            m_NetworkChannel = null;
        }

        /// <summary>
        /// 获取事件组件。
        /// </summary>
        public EventComponent Event
        {
            get
            {
                if (_event == null)
                {
                    _event = GameEntry.GetComponent<EventComponent>();
                }

                return _event;
            }
        }

        private static EventComponent _event;

        public void Initialize(INetworkChannel netChannel)
        {
            m_NetworkChannel = netChannel;
            // 反射注册包和包处理函数。
            var packetReceiveHeaderHandlerBaseType = typeof(IPacketReceiveHeaderHandler);
            var packetReceiveBodyHandlerBaseType = typeof(IPacketReceiveBodyHandler);
            var packetSendHeaderHandlerBaseType = typeof(IPacketSendHeaderHandler);
            var packetSendBodyHandlerBaseType = typeof(IPacketSendBodyHandler);
            var packetHeartBeatHandlerBaseType = typeof(IPacketHeartBeatHandler);
            var packetHandlerBaseType = typeof(IPacketHandler);
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (!type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                if (!type.IsImplWithInterface(packetHandlerBaseType))
                {
                    continue;
                }

                if (type.IsImplWithInterface(packetReceiveHeaderHandlerBaseType))
                {
                    var packetHandler = (IPacketReceiveHeaderHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(packetHandler);
                }
                else if (type.IsImplWithInterface(packetReceiveBodyHandlerBaseType))
                {
                    var packetHandler = (IPacketReceiveBodyHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(packetHandler);
                }
                else if (type.IsImplWithInterface(packetSendHeaderHandlerBaseType))
                {
                    var packetHandler = (IPacketSendHeaderHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(packetHandler);
                }
                else if (type.IsImplWithInterface(packetSendBodyHandlerBaseType))
                {
                    var packetHandler = (IPacketSendBodyHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(packetHandler);
                }
                else if (type.IsImplWithInterface(packetHeartBeatHandlerBaseType))
                {
                    var packetHandler = (IPacketHeartBeatHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(packetHandler);
                }
            }

            Event.Subscribe(NetworkConnectedEventArgs.EventId, OnNetConnected);
            Event.Subscribe(NetworkClosedEventArgs.EventId, OnNetClosed);
            Event.Subscribe(NetworkMissHeartBeatEventArgs.EventId, OnNetMissHeartBeat);
            Event.Subscribe(NetworkErrorEventArgs.EventId, OnNetError);
            Event.Subscribe(NetworkConnectedEventArgs.EventId, OnNetCustomError);
        }

        public void Shutdown()
        {
            Event.Unsubscribe(NetworkConnectedEventArgs.EventId, OnNetConnected);
            Event.Unsubscribe(NetworkClosedEventArgs.EventId, OnNetClosed);
            Event.Unsubscribe(NetworkMissHeartBeatEventArgs.EventId, OnNetMissHeartBeat);
            Event.Unsubscribe(NetworkErrorEventArgs.EventId, OnNetError);
            Event.Unsubscribe(NetworkConnectedEventArgs.EventId, OnNetCustomError);
            m_NetworkChannel = null;
        }

        public void PrepareForConnecting()
        {
            m_NetworkChannel.Socket.ReceiveBufferSize = 1024 * 1024 * 8;
            m_NetworkChannel.Socket.SendBufferSize = 1024 * 1024 * 8;
        }

        public bool SendHeartBeat()
        {
            var message = m_NetworkChannel.PacketHeartBeatHandler.Handler();
            m_NetworkChannel.Send(message);
            return true;
        }

        public bool SerializePacketHeader<T>(T messageObject, Stream destination, out byte[] messageBodyBuffer) where T : MessageObject
        {
            GameFrameworkGuard.NotNull(m_NetworkChannel, nameof(m_NetworkChannel));
            GameFrameworkGuard.NotNull(m_NetworkChannel.PacketSendHeaderHandler, nameof(m_NetworkChannel.PacketSendHeaderHandler));
            GameFrameworkGuard.NotNull(messageObject, nameof(messageObject));
            GameFrameworkGuard.NotNull(destination, nameof(destination));

            return m_NetworkChannel.PacketSendHeaderHandler.Handler(messageObject, _cachedStream, out messageBodyBuffer);
        }

        public bool SerializePacketBody(byte[] messageBodyBuffer, Stream destination)
        {
            GameFrameworkGuard.NotNull(m_NetworkChannel, nameof(m_NetworkChannel));
            GameFrameworkGuard.NotNull(m_NetworkChannel.PacketSendHeaderHandler, nameof(m_NetworkChannel.PacketSendHeaderHandler));
            GameFrameworkGuard.NotNull(m_NetworkChannel.PacketSendBodyHandler, nameof(m_NetworkChannel.PacketSendBodyHandler));
            GameFrameworkGuard.NotNull(messageBodyBuffer, nameof(messageBodyBuffer));
            GameFrameworkGuard.NotNull(destination, nameof(destination));

            return m_NetworkChannel.PacketSendBodyHandler.Handler(messageBodyBuffer, _cachedStream, destination);
        }

        public bool DeserializePacketHeader(byte[] source)
        {
            GameFrameworkGuard.NotNull(source, nameof(source));

            return m_NetworkChannel.PacketReceiveHeaderHandler.Handler(source);
        }

        public bool DeserializePacketBody(byte[] source, int messageId, out MessageObject messageObject)
        {
            GameFrameworkGuard.NotNull(source, nameof(source));

            return m_NetworkChannel.PacketReceiveBodyHandler.Handler(source, messageId, out messageObject);
        }

        public void Clear()
        {
            _cachedStream?.Dispose();
            _cachedStream = null;
            m_NetworkChannel?.Close();
            m_NetworkChannel = null;
        }

        private void OnNetConnected(object sender, GameEventArgs e)
        {
            if (!(e is NetworkConnectedEventArgs ne) || ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("网络连接成功......");
        }

        private void OnNetClosed(object sender, GameEventArgs e)
        {
            if (!(e is NetworkClosedEventArgs ne) || ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Info("网络连接关闭......");
        }

        private void OnNetMissHeartBeat(object sender, GameEventArgs e)
        {
            if (!(e is NetworkMissHeartBeatEventArgs ne) || ne.NetworkChannel != m_NetworkChannel) return;
            Log.Warning(Utility.Text.Format("Network channel '{0}' miss heart beat '{1}' times.", ne.NetworkChannel.Name, ne.MissCount));
            // if (ne.MissCount < 2) return;
            // ne.NetChannel.Close();
        }

        private void OnNetError(object sender, GameEventArgs e)
        {
            if (!(e is NetworkErrorEventArgs ne) || ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Error(Utility.Text.Format("Network channel '{0}' error, error code is '{1}', error message is '{2}'.", ne.NetworkChannel.Name, ne.ErrorCode, ne.ErrorMessage));
            //ne.NetworkChannel.Close();
        }

        private void OnNetCustomError(object sender, GameEventArgs e)
        {
            if (!(e is NetworkCustomErrorEventArgs ne) || ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }
        }
    }
}