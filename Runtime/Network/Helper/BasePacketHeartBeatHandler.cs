namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认心跳包处理
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class BasePacketHeartBeatHandler : IPacketHeartBeatHandler, IPacketHandler
    {
        /// <summary>
        /// 每次心跳的间隔
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public virtual float HeartBeatInterval
        {
            get { return 5; }
        }

        /// <summary>
        /// 几次心跳丢失。触发断开网络
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public virtual int MissHeartBeatCountByClose { get; } = 5;

        [UnityEngine.Scripting.Preserve]
        public virtual MessageObject Handler()
        {
            throw new System.NotImplementedException(
                "BasePacketHeartBeatHandler.Handler() must be overridden in a subclass. " +
                "Please provide a concrete heart beat message implementation.");
        }
    }
}