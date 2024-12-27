using GameFrameX.Runtime;
using Newtonsoft.Json;
#if ENABLE_GAME_FRAME_X_PROTOBUF
using ProtoBuf;
#endif

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// HTTP消息包装基类
    /// </summary>
#if ENABLE_GAME_FRAME_X_PROTOBUF
    [ProtoContract]
#endif
    public class MessageHttpObject
    {
        /// <summary>
        /// 消息ID
        /// </summary>
#if ENABLE_GAME_FRAME_X_PROTOBUF
        [ProtoMember(1)]
#endif
        public int Id { get; set; }

        /// <summary>
        /// 消息序列号
        /// </summary>
#if ENABLE_GAME_FRAME_X_PROTOBUF
        [ProtoMember(2)]
#endif
        public int UniqueId { get; set; }

        [JsonIgnore]
#if ENABLE_GAME_FRAME_X_PROTOBUF
        [ProtoMember(3)]
#endif
        public byte[] Body { get; set; }

        public override string ToString()
        {
            return Utility.Json.ToJson(this);
        }
    }
}