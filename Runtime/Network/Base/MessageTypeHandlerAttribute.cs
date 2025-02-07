using System;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络消息处理器
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageTypeHandlerAttribute : Attribute
    {
        /// <summary>
        /// 消息ID,不能重复
        /// </summary>
        public int MessageId { get; }

        /// <summary>
        /// 网络消息处理器
        /// </summary>
        /// <param name="messageId">消息ID,不能重复</param>
        [UnityEngine.Scripting.Preserve]
        public MessageTypeHandlerAttribute(int messageId)
        {
            MessageId = messageId;
        }
    }
}