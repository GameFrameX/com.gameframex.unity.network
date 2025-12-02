using System;
using System.IO;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;


namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认网络通道帮助
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class DefaultNetworkChannelHelper : INetworkChannelHelper, IReference
    {
        private INetworkChannel m_NetworkChannel;

        public DefaultNetworkChannelHelper()
        {
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
            var messageCompressHandlerBaseType = typeof(IMessageCompressHandler);
            var messageDecompressHandlerBaseType = typeof(IMessageDecompressHandler);
            var packetHandlerBaseType = typeof(IPacketHandler);

            var types = Utility.Assembly.GetTypes();
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
                    var handler = (IPacketReceiveHeaderHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(handler);
                }
                else if (type.IsImplWithInterface(packetReceiveBodyHandlerBaseType))
                {
                    var handler = (IPacketReceiveBodyHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(handler);
                }
                else if (type.IsImplWithInterface(packetSendHeaderHandlerBaseType))
                {
                    var handler = (IPacketSendHeaderHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(handler);
                }
                else if (type.IsImplWithInterface(packetSendBodyHandlerBaseType))
                {
                    var handler = (IPacketSendBodyHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHandler(handler);
                }
                else if (type.IsImplWithInterface(packetHeartBeatHandlerBaseType))
                {
                    var handler = (IPacketHeartBeatHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterHeartBeatHandler(handler);
                }
                else if (type.IsImplWithInterface(messageCompressHandlerBaseType))
                {
                    var handler = (IMessageCompressHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterMessageCompressHandler(handler);
                }
                else if (type.IsImplWithInterface(messageDecompressHandlerBaseType))
                {
                    var handler = (IMessageDecompressHandler)Activator.CreateInstance(type);
                    m_NetworkChannel.RegisterMessageDecompressHandler(handler);
                }
            }

            Event.CheckSubscribe(NetworkConnectedEventArgs.EventId, OnNetworkConnectedEventArgs);
            Event.CheckSubscribe(NetworkClosedEventArgs.EventId, OnNetworkClosedEventArgs);
            Event.CheckSubscribe(NetworkMissHeartBeatEventArgs.EventId, OnNetworkMissHeartBeatEventArgs);
            Event.CheckSubscribe(NetworkErrorEventArgs.EventId, OnNetworkErrorEventArgs);
        }

        public void Shutdown()
        {
            Event.Unsubscribe(NetworkConnectedEventArgs.EventId, OnNetworkConnectedEventArgs);
            Event.Unsubscribe(NetworkClosedEventArgs.EventId, OnNetworkClosedEventArgs);
            Event.Unsubscribe(NetworkMissHeartBeatEventArgs.EventId, OnNetworkMissHeartBeatEventArgs);
            Event.Unsubscribe(NetworkErrorEventArgs.EventId, OnNetworkErrorEventArgs);
            m_NetworkChannel = null;
        }

        public void PrepareForConnecting()
        {
            m_NetworkChannel.Socket.ReceiveBufferSize = 1024 * 64 - 1;
            m_NetworkChannel.Socket.SendBufferSize = 1024 * 64 - 1;
        }

        public bool SendHeartBeat()
        {
            var message = m_NetworkChannel.PacketHeartBeatHandler.Handler();
            m_NetworkChannel.Send(message);
            return true;
        }

        public bool SerializePacketHeader<T>(T messageObject, MemoryStream destination, out byte[] messageBodyBuffer) where T : MessageObject
        {
            GameFrameworkGuard.NotNull(m_NetworkChannel, nameof(m_NetworkChannel));
            GameFrameworkGuard.NotNull(m_NetworkChannel.PacketSendHeaderHandler, nameof(m_NetworkChannel.PacketSendHeaderHandler));
            GameFrameworkGuard.NotNull(messageObject, nameof(messageObject));
            GameFrameworkGuard.NotNull(destination, nameof(destination));

            return m_NetworkChannel.PacketSendHeaderHandler.Handler(messageObject, m_NetworkChannel.MessageCompressHandler, destination, out messageBodyBuffer);
        }

        public bool SerializePacketBody(byte[] messageBodyBuffer, MemoryStream destination)
        {
            GameFrameworkGuard.NotNull(m_NetworkChannel, nameof(m_NetworkChannel));
            GameFrameworkGuard.NotNull(m_NetworkChannel.PacketSendHeaderHandler, nameof(m_NetworkChannel.PacketSendHeaderHandler));
            GameFrameworkGuard.NotNull(m_NetworkChannel.PacketSendBodyHandler, nameof(m_NetworkChannel.PacketSendBodyHandler));
            GameFrameworkGuard.NotNull(messageBodyBuffer, nameof(messageBodyBuffer));
            GameFrameworkGuard.NotNull(destination, nameof(destination));

            return m_NetworkChannel.PacketSendBodyHandler.Handler(messageBodyBuffer, destination);
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
            m_NetworkChannel?.Close(NetworkCloseReason.Dispose, (ushort)NetworkErrorCode.DisposeError);
            m_NetworkChannel = null;
        }

        private void OnNetworkConnectedEventArgs(object sender, GameEventArgs e)
        {
            if (!(e is NetworkConnectedEventArgs ne) || ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Debug($"网络连接成功......{ne.NetworkChannel.Name}");
        }

        private void OnNetworkClosedEventArgs(object sender, GameEventArgs e)
        {
            if (!(e is NetworkClosedEventArgs ne) || ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Debug($"网络连接关闭......{ne.NetworkChannel.Name}, 关闭原因: {ne.Reason}, 错误码: {ne.ErrorCode}");
        }

        private void OnNetworkMissHeartBeatEventArgs(object sender, GameEventArgs e)
        {
            if (!(e is NetworkMissHeartBeatEventArgs ne) || ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Warning(Utility.Text.Format("Network channel '{0}' miss heart beat '{1}' times.", ne.NetworkChannel.Name, ne.MissCount));
        }

        private void OnNetworkErrorEventArgs(object sender, GameEventArgs e)
        {
            if (!(e is NetworkErrorEventArgs ne) || ne.NetworkChannel != m_NetworkChannel)
            {
                return;
            }

            Log.Error(Utility.Text.Format("Network channel '{0}' error, error code is '{1}', error message is '{2}'.", ne.NetworkChannel.Name, ne.ErrorCode, ne.ErrorMessage));
            ne.NetworkChannel.Close(ne.ErrorMessage, (ushort)ne.ErrorCode);
        }
    }
}