namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 服务器返回给客户端的消息基类接口
    /// </summary>
    public interface IResponseMessage
    {
        /// <summary>
        /// 错误码，非 0 表示错误
        /// </summary>
        int ErrorCode { get; }
    }
}