using System;
using System.Reflection;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络非RPC返回消息处理器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class MessageHandlerAttribute : Attribute
    {
        public const BindingFlags Flags = BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// 消息对象
        /// </summary>
        public Type MessageType { get; }

        private readonly string _invokeMethodName;
        private MethodInfo _invokeMethod;
        private IMessageHandler _messageHandler;

        /// <summary>
        /// 网络消息处理器
        /// </summary>
        /// <param name="message">注册的消息对象。需要继承MessageObject和实现IResponseMessage</param>
        /// <param name="invokeMethodName">执行的方法名称。建议使用nameof标记当前的函数</param>
        public MessageHandlerAttribute(Type message, string invokeMethodName)
        {
            GameFrameworkGuard.NotNull(message, nameof(message));
            GameFrameworkGuard.NotNullOrEmpty(invokeMethodName, nameof(invokeMethodName));
            _invokeMethodName = invokeMethodName;
            if (message.BaseType != typeof(MessageObject))
            {
                throw new ArgumentException("message必须继承:" + nameof(MessageObject));
            }

            if (!message.IsImplWithInterface(typeof(INotifyMessage)))
            {
                throw new ArgumentException($"message:{message.FullName}必须实现:" + nameof(INotifyMessage));
            }

            MessageType = message;
        }

        internal void Invoke(MessageObject message)
        {
            if (_invokeMethod == null)
            {
                throw new ArgumentNullException(nameof(_invokeMethod), $"未找到方法：{_invokeMethodName}.请确认是否注册成功");
            }

            if (_invokeMethod.IsStatic)
            {
                _invokeMethod?.Invoke(null, new object[] { message });
            }
            else
            {
                _invokeMethod?.Invoke(_messageHandler, new object[] { message });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageHandler"></param>
        /// <exception cref="TargetParameterCountException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal bool Init(IMessageHandler messageHandler)
        {
            GameFrameworkGuard.NotNull(MessageType, nameof(MessageType));
            GameFrameworkGuard.NotNull(messageHandler, nameof(messageHandler));
            _messageHandler = messageHandler;
            var target = messageHandler.GetType();

            var methodInfos = target.GetMethods(Flags);

            foreach (var method in methodInfos)
            {
                if (!method.IsDefined(typeof(MessageHandlerAttribute), true))
                {
                    continue;
                }

                if (method.Name == _invokeMethodName)
                {
                    if (method.GetParameters().Length != 1)
                    {
                        throw new TargetParameterCountException("参数个数必须为1");
                    }

                    if (method.GetParameters()[0].ParameterType.FullName != MessageType.FullName)
                    {
                        throw new ArgumentException("参数类型数必须为:" + MessageType.FullName);
                    }

                    _invokeMethod = method;
                    return true;
                }
            }

            return false;
        }
    }
}