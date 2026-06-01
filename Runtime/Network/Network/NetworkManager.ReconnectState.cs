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

using System;

namespace GameFrameX.Network.Runtime
{
    public sealed partial class NetworkManager
    {
        /// <summary>
        /// 重连状态。
        /// </summary>
        public sealed class ReconnectState
        {
            private const float InitialDelaySeconds = 1f;
            private const float MaxDelaySeconds = 30f;
            private const float BackoffMultiplier = 2f;

            private int _retryCount;
            private int _maxRetryCount;
            private float _targetDelaySeconds;
            private float _elapsedSeconds;
            private bool _isWaiting;
            private bool _disposed;

            [UnityEngine.Scripting.Preserve]
            public ReconnectState()
            {
                _retryCount = 0;
                _maxRetryCount = 0;
                _targetDelaySeconds = 0f;
                _elapsedSeconds = 0f;
                _isWaiting = false;
                _disposed = false;
            }

            /// <summary>
            /// 获取当前重连次数。
            /// </summary>
            public int RetryCount
            {
                get { return _retryCount; }
            }

            /// <summary>
            /// 获取最大重连次数。
            /// </summary>
            public int MaxRetryCount
            {
                get { return _maxRetryCount; }
            }

            /// <summary>
            /// 获取本次重连目标等待时间（秒）。
            /// </summary>
            public float TargetDelaySeconds
            {
                get { return _targetDelaySeconds; }
            }

            /// <summary>
            /// 获取是否正在等待重连。
            /// </summary>
            public bool IsWaiting
            {
                get { return _isWaiting; }
            }

            /// <summary>
            /// 启动重连状态。
            /// </summary>
            /// <param name="maxRetryCount">最大重试次数。</param>
            public void Start(int maxRetryCount)
            {
                _retryCount = 0;
                _maxRetryCount = maxRetryCount;
                _targetDelaySeconds = CalculateDelay();
                _elapsedSeconds = 0f;
                _isWaiting = true;
                _disposed = false;
            }

            /// <summary>
            /// 更新重连计时器。
            /// </summary>
            /// <param name="realElapseSeconds">实际流逝时间（秒）。</param>
            /// <returns>延迟是否已到期。</returns>
            public bool Update(float realElapseSeconds)
            {
                if (!_isWaiting)
                {
                    return false;
                }

                _elapsedSeconds += realElapseSeconds;
                if (_elapsedSeconds >= _targetDelaySeconds)
                {
                    _isWaiting = false;
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 准备下一次重试。递增重试次数并计算下一次延迟。
            /// </summary>
            /// <returns>是否还可以继续重试。</returns>
            public bool PrepareNextRetry()
            {
                _retryCount++;
                if (_retryCount > _maxRetryCount)
                {
                    return false;
                }

                _targetDelaySeconds = CalculateDelay();
                _elapsedSeconds = 0f;
                _isWaiting = true;
                return true;
            }

            /// <summary>
            /// 取消重连。
            /// </summary>
            public void Cancel()
            {
                _isWaiting = false;
            }

            /// <summary>
            /// 重置重连状态。
            /// </summary>
            public void Reset()
            {
                _retryCount = 0;
                _maxRetryCount = 0;
                _targetDelaySeconds = 0f;
                _elapsedSeconds = 0f;
                _isWaiting = false;
                _disposed = false;
            }

            private float CalculateDelay()
            {
                float delay = InitialDelaySeconds;
                for (int i = 0; i < _retryCount; i++)
                {
                    delay *= BackoffMultiplier;
                    if (delay >= MaxDelaySeconds)
                    {
                        delay = MaxDelaySeconds;
                        break;
                    }
                }

                return delay;
            }
        }
    }
}
