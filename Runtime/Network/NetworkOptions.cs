// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations and open-source licenses.
//
//  本项目采用 Apache License 2.0 许可证分发，
//  This project is licensed under the Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please see the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担任何责任。
//  shall be borne solely by the developer, the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository:  https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  CNB  仓库：https://cnb.cool/GameFrameX
//  CNB Repository:  https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
using System.Collections.Generic;
using GameFrameX.Runtime;


namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络模块启动配置（C9 配置终态）。
    /// </summary>
    /// <remarks>
    /// 默认值即开箱值（与原组件序列化字段默认值同源）；游戏在首场景加载前（BeforeSceneLoad）经 <c>GameFrameXOptions.Configure(new NetworkOptions{...})</c> 注入差异值，
    /// <see cref="NetworkComponent"/> 在 Awake 中一次性消费。
    /// </remarks>
    public sealed class NetworkOptions
    {
        /// <summary>
        /// RPC 超时时间（毫秒），默认 5000（即 5 秒）。
        /// </summary>
        /// <remarks>实时型：创建网络频道时传入，随频道创建生效。</remarks>
        public int RpcTimeout = 5000;

        /// <summary>
        /// 是否在应用程序获得焦点时发送心跳包，默认关闭。
        /// </summary>
        /// <remarks>启动期型：焦点心跳监听在组件 Awake 阶段确定，运行期修改不生效。</remarks>
        [GameFrameXStartupOnly]
        public bool FocusHeartbeat = false;

        /// <summary>
        /// 忽略发送日志打印的网络消息 ID 列表，默认为空。
        /// </summary>
        /// <remarks>实时型：创建网络频道时传入频道。</remarks>
        public List<int> IgnoredSendNetworkIds = new List<int>();

        /// <summary>
        /// 忽略接收日志打印的网络消息 ID 列表，默认为空。
        /// </summary>
        /// <remarks>实时型：创建网络频道时传入频道。</remarks>
        public List<int> IgnoredReceiveNetworkIds = new List<int>();
    }
}
