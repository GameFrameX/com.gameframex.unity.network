using GameFrameX.Runtime;
using Newtonsoft.Json;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// HTTP消息包装基类
    /// </summary>
    public class MessageHttpObject
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 消息序列号
        /// </summary>
        public int UniqueId { get; set; }

        [JsonIgnore]
        public byte[] Body { get; set; }

        public override string ToString()
        {
            return Utility.Json.ToJson(this);
        }
    }
}
