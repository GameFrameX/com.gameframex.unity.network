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
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// 网络频道接口。
    /// </summary>
    public interface INetworkChannel
    {
        /// <summary>
        /// 获取网络频道名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取网络频道所使用的 Socket。
        /// </summary>
        INetworkSocket Socket { get; }

        /// <summary>
        /// 获取是否已连接。
        /// </summary>
        bool Connected { get; }

        /// <summary>
        /// 获取网络地址类型。
        /// </summary>
        AddressFamily AddressFamily { get; }

        /// <summary>
        /// 获取要发送的消息包数量。
        /// </summary>
        int SendPacketCount { get; }

        /// <summary>
        /// 获取累计发送的消息包数量。
        /// </summary>
        int SentPacketCount { get; }

        /// <summary>
        /// 获取累计已接收的消息包数量。
        /// </summary>
        int ReceivedPacketCount { get; }

        /// <summary>
        /// 获取或设置当收到消息包时是否重置心跳流逝时间。
        /// </summary>
        bool ResetHeartBeatElapseSecondsWhenReceivePacket { get; set; }

        /// <summary>
        /// 获取丢失心跳的次数。
        /// </summary>
        int MissHeartBeatCount { get; }

        /// <summary>
        /// 获取或设置心跳间隔时长，以秒为单位。
        /// </summary>
        float HeartBeatInterval { get; set; }

        /// <summary>
        /// 获取心跳等待时长，以秒为单位。
        /// </summary>
        float HeartBeatElapseSeconds { get; }

        /// <summary>
        /// 消息发送包头处理器
        /// </summary>
        IPacketSendHeaderHandler PacketSendHeaderHandler { get; }

        /// <summary>
        /// 消息发送内容处理器
        /// </summary>
        IPacketSendBodyHandler PacketSendBodyHandler { get; }

        /// <summary>
        /// 心跳消息处理器
        /// </summary>
        IPacketHeartBeatHandler PacketHeartBeatHandler { get; }

        /// <summary>
        /// 消息接收包头处理器
        /// </summary>
        IPacketReceiveHeaderHandler PacketReceiveHeaderHandler { get; }

        /// <summary>
        /// 消息接收内容处理器
        /// </summary>
        IPacketReceiveBodyHandler PacketReceiveBodyHandler { get; }

        /// <summary>
        /// 消息压缩处理器
        /// </summary>
        IMessageCompressHandler MessageCompressHandler { get; }

        /// <summary>
        /// 消息解压处理器
        /// </summary>
        IMessageDecompressHandler MessageDecompressHandler { get; }

        /// <summary>
        /// 注册消息压缩处理器
        /// </summary>
        /// <param name="handler">处理器对象</param>
        void RegisterMessageCompressHandler(IMessageCompressHandler handler);

        /// <summary>
        /// 注册消息解压处理器
        /// </summary>
        /// <param name="handler">处理器对象</param>
        void RegisterMessageDecompressHandler(IMessageDecompressHandler handler);

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="handler">处理器对象</param>
        void RegisterHandler(IPacketSendHeaderHandler handler);

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="handler">处理器对象。</param>
        void RegisterHandler(IPacketSendBodyHandler handler);

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="handler">处理器对象。</param>
        void RegisterHandler(IPacketReceiveHeaderHandler handler);

        /// <summary>
        /// 注册网络消息包处理函数。
        /// </summary>
        /// <param name="handler">处理器对象。</param>
        void RegisterHandler(IPacketReceiveBodyHandler handler);

        /// <summary>
        /// 注册网络消息心跳处理函数，用于处理心跳消息
        /// </summary>
        /// <param name="handler">处理器对象</param>
        void RegisterHeartBeatHandler(IPacketHeartBeatHandler handler);

        /// <summary>
        /// 注册网络消息心跳处理函数，用于处理心跳消息
        /// </summary>
        /// <param name="handler">要注册的网络消息包处理函数</param>
        [Obsolete("Use RegisterHeartBeatHandler instead")]
        void RegisterHandler(IPacketHeartBeatHandler handler);

        /// <summary>
        /// 设置RPC 的 ErrorCode 不为 0 的时候的处理函数
        /// </summary>
        /// <param name="handler"></param>
        void SetRPCErrorCodeHandler(EventHandler<MessageObject> handler);

        /// <summary>
        /// 设置RPC错误的处理函数
        /// </summary>
        /// <param name="handler"></param>
        void SetRPCErrorHandler(EventHandler<MessageObject> handler);

        /// <summary>
        /// 设置RPC开始的处理函数
        /// </summary>
        /// <param name="handler"></param>
        void SetRPCStartHandler(EventHandler<MessageObject> handler);

        /// <summary>
        /// 设置RPC结束的处理函数
        /// </summary>
        /// <param name="handler"></param>
        void SetRPCEndHandler(EventHandler<MessageObject> handler);

        /// <summary>
        /// 连接到远程主机。
        /// </summary>
        /// <param name="address">远程主机的地址。</param>
        /// <param name="userData">用户自定义数据。</param>
        void Connect(Uri address, object userData = null);

        /// <summary>
        /// 设置忽略指定的消息包的发送和接收ID列表
        /// </summary>
        /// <param name="sendIds">发送消息ID列表</param>
        /// <param name="receiveIds">接收消息ID列表</param>
        void SetIgnoreLogNetworkIds(List<int> sendIds, List<int> receiveIds);

        /// <summary>
        /// 关闭网络频道。
        /// </summary>
        /// <param name="reason">关闭原因</param>
        /// <param name="code">关闭错误码</param>
        void Close(string reason, ushort code = 0);

        /// <summary>
        /// 向远程主机发送消息包。
        /// </summary>
        /// <typeparam name="T">消息包类型。</typeparam>
        /// <param name="messageObject">要发送的消息包。</param>
        void Send<T>(T messageObject) where T : MessageObject;

        /// <summary>
        /// 向远程主机发送消息包
        /// </summary>
        /// <param name="messageObject">要发送的消息包。</param>
        /// <param name="isIgnoreErrorCode">是否忽略错误码，默认值为 false。如果为 true，则在调用时不会抛出异常。和RPC的错误码回调也会忽略错误码。</param>
        /// <typeparam name="TResult"></typeparam>
        Task<TResult> Call<TResult>(MessageObject messageObject, bool isIgnoreErrorCode = false) where TResult : MessageObject, IResponseMessage;
    }
}