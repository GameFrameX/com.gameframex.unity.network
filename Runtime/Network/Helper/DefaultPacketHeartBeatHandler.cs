namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 默认心跳包处理
    /// </summary>
    public class BasePacketHeartBeatHandler : IPacketHeartBeatHandler, IPacketHandler
    {
        /// <summary>
        /// 每次心跳的间隔
        /// </summary>
        public virtual float HeartBeatInterval
        {
            get { return 5; }
        }

        /// <summary>
        /// 几次心跳丢失。触发断开网络
        /// </summary>
        public virtual int MissHeartBeatCountByClose { get; } = 5;

        public virtual MessageObject Handler()
        {
            throw new System.NotImplementedException();
        }
    }
}