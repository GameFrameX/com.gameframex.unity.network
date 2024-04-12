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
    }
}