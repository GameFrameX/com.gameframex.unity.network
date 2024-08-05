using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 协议消息处理帮助类
    /// </summary>
    public static class ProtoMessageHandler
    {
        private static readonly ConcurrentDictionary<Type, List<MessageHandlerAttribute>> MessageHandlerDictionary = new ConcurrentDictionary<Type, List<MessageHandlerAttribute>>();
        private static readonly List<MessageHandlerAttribute> EmptyList = new List<MessageHandlerAttribute>();

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="messageHandler">消息接收对象</param>
        public static void Init(IMessageHandler messageHandler)
        {
            GameFrameworkGuard.NotNull(messageHandler, nameof(messageHandler));
            var type = messageHandler.GetType();
            /*var isImplWithIMessageHandler = type.IsImplWithInterface(typeof(IMessageHandler));
            if (isImplWithIMessageHandler == false)
            {
                throw new ArrayTypeMismatchException("类型必须继承" + nameof(IMessageHandler));
            }*/
            var methodInfos = type.GetMethods(MessageHandlerAttribute.Flags);

            foreach (var methodInfo in methodInfos)
            {
                var messageHandlerAttribute = methodInfo.GetCustomAttribute<MessageHandlerAttribute>();
                if (messageHandlerAttribute == null)
                {
                    continue;
                }

                bool isInitSuccess = messageHandlerAttribute.Init(messageHandler);
                if (isInitSuccess)
                {
                    MessageHandlerDictionary.TryGetValue(messageHandlerAttribute.MessageType, out var list);
                    if (list == null)
                    {
                        list = new List<MessageHandlerAttribute>(32);
                        MessageHandlerDictionary.TryAdd(messageHandlerAttribute.MessageType, list);
                    }

                    if (!list.Contains(messageHandlerAttribute))
                    {
                        list.Add(messageHandlerAttribute);
                    }
                    else
                    {
                        Log.Error("重复注册消息处理器：" + type.FullName + "->" + methodInfo.Name);
                    }
                }
                else
                {
                    Log.Error("初始化消息处理器：" + type.FullName + "->" + methodInfo.Name + " 失败");
                }
            }
        }

        /// <summary>
        /// 获取消息处理器
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <returns>消息处理器</returns>
        internal static List<MessageHandlerAttribute> GetHandlers(Type messageType)
        {
            if (MessageHandlerDictionary.TryGetValue(messageType, out var list))
            {
                return list?.ToList();
            }

            Log.Warning("没有找到消息处理器消息类型：" + messageType.Name);
            return EmptyList;
        }
    }
}