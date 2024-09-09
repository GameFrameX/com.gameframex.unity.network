using System;
using System.Collections.Generic;
using System.Reflection;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络非RPC返回消息处理器
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    [UnityEngine.Scripting.Preserve]
    public class MessageHandlerAttribute : Attribute
    {
        public const BindingFlags Flags = BindingFlags.Default | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

        /// <summary>
        /// 消息对象
        /// </summary>
        public Type MessageType { get; }

        /// <summary>
        /// 执行的方法名称
        /// </summary>
        private readonly string m_InvokeMethodName;

        /// <summary>
        /// 执行的方法
        /// </summary>
        private MethodInfo m_InvokeMethod;

        /// <summary>
        /// 消息处理器
        /// </summary>
        private IMessageHandler m_MessageHandler;

        /// <summary>
        /// 消息处理对象队列
        /// </summary>
        private readonly Queue<MessageObject> m_MessageObjects = new Queue<MessageObject>();

        /// <summary>
        /// 网络消息处理器
        /// </summary>
        /// <param name="message">注册的消息对象。需要继承MessageObject和实现IResponseMessage</param>
        /// <param name="invokeMethodName">执行的方法名称。建议使用nameof标记当前的函数</param>
        public MessageHandlerAttribute(Type message, string invokeMethodName)
        {
            GameFrameworkGuard.NotNull(message, nameof(message));
            GameFrameworkGuard.NotNullOrEmpty(invokeMethodName, nameof(invokeMethodName));
            m_InvokeMethodName = invokeMethodName;
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

        /// <summary>
        /// 设置消息对象
        /// </summary>
        /// <param name="messageObject">消息对象</param>
        public void SetMessageObject(MessageObject messageObject)
        {
            GameFrameworkGuard.NotNull(messageObject, nameof(messageObject));
            m_MessageObjects.Enqueue(messageObject);
        }

        internal void Invoke()
        {
            if (m_InvokeMethod == null)
            {
                throw new ArgumentNullException(nameof(m_InvokeMethod), $"未找到方法：{m_InvokeMethodName}.请确认是否注册成功");
            }

            if (m_MessageObjects.Count <= 0)
            {
                Log.Warning($"没有消息对象转发到方法：{m_InvokeMethodName}");
                return;
            }

            var messageObject = m_MessageObjects.Dequeue();

            if (m_InvokeMethod.IsStatic)
            {
                m_InvokeMethod?.Invoke(null, new object[] { messageObject });
            }
            else
            {
                m_InvokeMethod?.Invoke(m_MessageHandler, new object[] { messageObject });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageHandler"></param>
        /// <exception cref="TargetParameterCountException"></exception>
        /// <exception cref="ArgumentException"></exception>
        [Obsolete("已过时, 使用Add 函数")]
        internal bool Init(IMessageHandler messageHandler)
        {
            return Add(messageHandler);
        }

        /// <summary>
        /// 增加消息处理器
        /// </summary>
        /// <param name="messageHandler">消息处理器对象</param>
        /// <exception cref="TargetParameterCountException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal bool Add(IMessageHandler messageHandler)
        {
            GameFrameworkGuard.NotNull(MessageType, nameof(MessageType));
            GameFrameworkGuard.NotNull(messageHandler, nameof(messageHandler));
            m_MessageHandler = messageHandler;
            var target = messageHandler.GetType();

            var methodInfos = target.GetMethods(Flags);

            foreach (var method in methodInfos)
            {
                if (!method.IsDefined(typeof(MessageHandlerAttribute), true))
                {
                    continue;
                }

                if (method.Name == m_InvokeMethodName)
                {
                    if (method.GetParameters().Length != 1)
                    {
                        throw new TargetParameterCountException("参数个数必须为1");
                    }

                    if (method.GetParameters()[0].ParameterType.FullName != MessageType.FullName)
                    {
                        throw new ArgumentException("参数类型数必须为:" + MessageType.FullName);
                    }

                    m_InvokeMethod = method;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 删除消息处理器
        /// </summary>
        /// <param name="messageHandler">消息处理器对象</param>
        /// <exception cref="TargetParameterCountException"></exception>
        /// <exception cref="ArgumentException"></exception>
        internal bool Remove(IMessageHandler messageHandler)
        {
            GameFrameworkGuard.NotNull(MessageType, nameof(MessageType));
            GameFrameworkGuard.NotNull(messageHandler, nameof(messageHandler));
            m_MessageHandler = null;
            var target = messageHandler.GetType();

            var methodInfos = target.GetMethods(Flags);

            foreach (var method in methodInfos)
            {
                if (!method.IsDefined(typeof(MessageHandlerAttribute), true))
                {
                    continue;
                }

                if (method.Name == m_InvokeMethodName)
                {
                    if (method.GetParameters().Length != 1)
                    {
                        throw new TargetParameterCountException("参数个数必须为1");
                    }

                    if (method.GetParameters()[0].ParameterType.FullName != MessageType.FullName)
                    {
                        throw new ArgumentException("参数类型数必须为:" + MessageType.FullName);
                    }

                    m_InvokeMethod = null;
                    return true;
                }
            }

            return false;
        }
    }
}