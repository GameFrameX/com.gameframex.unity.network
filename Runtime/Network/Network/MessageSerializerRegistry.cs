using System;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 消息序列化器全局注册表，用于管理全局默认的消息序列化器实例。
    /// </summary>
    /// <remarks>
    /// Global registry for managing the default message serializer instance.
    /// </remarks>
    public static class MessageSerializerRegistry
    {
        private static IMessageSerializer _global;

        /// <summary>
        /// 获取全局消息序列化器，若未注册则返回默认兜底实现。
        /// </summary>
        /// <remarks>
        /// Gets the global message serializer; returns the default fallback if not registered.
        /// </remarks>
        /// <value>全局消息序列化器实例 / The global message serializer instance</value>
        public static IMessageSerializer Global
        {
            get { return _global ?? DefaultMessageSerializer.Instance; }
        }

        /// <summary>
        /// 注册全局消息序列化器。
        /// </summary>
        /// <remarks>
        /// Registers the global message serializer.
        /// </remarks>
        /// <param name="serializer">要注册的消息序列化器实例 / The message serializer instance to register</param>
        /// <exception cref="ArgumentNullException">当 <paramref name="serializer"/> 为 null 时抛出 / Thrown when <paramref name="serializer"/> is null</exception>
        public static void RegisterGlobal(IMessageSerializer serializer)
        {
            if (serializer == null)
            {
                throw new ArgumentNullException(nameof(serializer));
            }
            _global = serializer;
        }

        /// <summary>
        /// 重置全局消息序列化器为默认兜底实现。
        /// </summary>
        /// <remarks>
        /// Resets the global message serializer to the default fallback.
        /// </remarks>
        public static void Reset()
        {
            _global = null;
        }
    }
}
