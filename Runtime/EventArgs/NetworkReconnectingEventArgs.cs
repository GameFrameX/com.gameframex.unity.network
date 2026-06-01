// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络重连中事件。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class NetworkReconnectingEventArgs : GameEventArgs
    {
        /// <summary>
        /// 网络重连中事件编号。
        /// </summary>
        public static readonly string EventId = typeof(NetworkReconnectingEventArgs).FullName;

        /// <summary>
        /// 获取网络重连中事件编号。
        /// </summary>
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 初始化网络重连中事件的新实例。
        /// </summary>
        public NetworkReconnectingEventArgs()
        {
            NetworkChannel = null;
            RetryCount = 0;
            MaxRetryCount = 0;
            DelaySeconds = 0f;
        }

        /// <summary>
        /// 获取网络频道。
        /// </summary>
        public INetworkChannel NetworkChannel { get; private set; }

        /// <summary>
        /// 获取当前重连次数。
        /// </summary>
        public int RetryCount { get; private set; }

        /// <summary>
        /// 获取最大重连次数。
        /// </summary>
        public int MaxRetryCount { get; private set; }

        /// <summary>
        /// 获取本次重连等待时间（秒）。
        /// </summary>
        public float DelaySeconds { get; private set; }

        /// <summary>
        /// 创建网络重连中事件。
        /// </summary>
        /// <param name="networkChannel">网络频道。</param>
        /// <param name="retryCount">当前重连次数。</param>
        /// <param name="maxRetryCount">最大重连次数。</param>
        /// <param name="delaySeconds">本次重连等待时间（秒）。</param>
        /// <returns>创建的网络重连中事件。</returns>
        public static NetworkReconnectingEventArgs Create(INetworkChannel networkChannel, int retryCount, int maxRetryCount, float delaySeconds)
        {
            NetworkReconnectingEventArgs networkReconnectingEventArgs = ReferencePool.Acquire<NetworkReconnectingEventArgs>();
            networkReconnectingEventArgs.NetworkChannel = networkChannel;
            networkReconnectingEventArgs.RetryCount = retryCount;
            networkReconnectingEventArgs.MaxRetryCount = maxRetryCount;
            networkReconnectingEventArgs.DelaySeconds = delaySeconds;
            return networkReconnectingEventArgs;
        }

        /// <summary>
        /// 清理网络重连中事件。
        /// </summary>
        public override void Clear()
        {
            NetworkChannel = null;
            RetryCount = 0;
            MaxRetryCount = 0;
            DelaySeconds = 0f;
        }
    }
}
