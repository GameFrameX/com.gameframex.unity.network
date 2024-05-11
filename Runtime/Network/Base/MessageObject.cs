#if ENABLE_GAME_FRAME_X_PROTOBUF
using ProtoBuf;
#endif

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 消息基类
    /// </summary>
#if ENABLE_GAME_FRAME_X_PROTOBUF
    [ProtoContract]
#endif
    public class MessageObject
    {
        /// <summary>
        /// 消息唯一编号
        /// </summary>
        public long UniqueId { get; private set; }

        protected MessageObject()
        {
            UpdateUniqueId();
        }

        /// <summary>
        /// 更新唯一编码
        /// </summary>
        public void UpdateUniqueId()
        {
            UniqueId = Utility.IdGenerator.GetNextUniqueId();
        }

        /// <summary>
        /// 设置唯一编码
        /// </summary>
        public void SetUpdateUniqueId(long uniqueId)
        {
            UniqueId = uniqueId;
        }
    }
}