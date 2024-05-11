//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    public sealed partial class NetworkManager
    {
        public sealed class RpcState : IDisposable
        {
            private readonly ConcurrentDictionary<long, RpcMessageData> m_HandlingObjects = new ConcurrentDictionary<long, RpcMessageData>();
            private readonly HashSet<long> m_HandlingObjectIds = new HashSet<long>();
            private bool m_Disposed;

            public RpcState()
            {
                m_Disposed = false;
            }

            public void Dispose()
            {
                if (m_Disposed)
                {
                    return;
                }

                m_HandlingObjects.Clear();
                m_HandlingObjectIds.Clear();
                m_Disposed = true;
            }

            /// <summary>
            /// RPC回复
            /// </summary>
            /// <param name="message">消息对象</param>
            /// <returns></returns>
            public bool Reply(MessageObject message)
            {
                if (m_HandlingObjects.TryRemove(message.UniqueId, out var messageActorObject))
                {
                    messageActorObject.Reply(message as IResponseMessage);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 调用并等待返回结果。可能引发超时异常
            /// </summary>
            /// <param name="message"></param>
            /// <returns></returns>
            public Task<IResponseMessage> Call(MessageObject message)
            {
                if (m_HandlingObjects.TryGetValue(message.UniqueId, out var messageActorObject))
                {
                    return messageActorObject.Task;
                }

                var defaultMessageActorObject = RpcMessageData.Create(message as IRequestMessage);
                m_HandlingObjects.TryAdd(message.UniqueId, defaultMessageActorObject);
                return defaultMessageActorObject.Task;
            }

            class RpcMessageData
            {
                /// <summary>
                /// 消息的唯一ID
                /// </summary>
                public long UniqueId { get; }

                /// <summary>
                /// 创建时间
                /// </summary>
                public long CreatedTime { get; }

                /// <summary>
                /// 消耗的时间
                /// </summary>
                public long ElapseTime { get; private set; }

                /// <summary>
                /// 请求消息
                /// </summary>
                public IRequestMessage RequestMessage { get; protected set; }

                /// <summary>
                /// 超时时间。单位毫秒
                /// </summary>
                public int Timeout { get; }

                /// <summary>
                /// 响应消息
                /// </summary>
                public IResponseMessage ResponseMessage { get; protected set; }

                /// <summary>
                /// 设置等待的返回结果
                /// </summary>
                /// <param name="responseMessage"></param>
                public void Reply(IResponseMessage responseMessage)
                {
                    ResponseMessage = responseMessage;
                    m_Tcs.SetResult(responseMessage);
                }

                /// <summary>
                /// 增加时间。如果超时返回true
                /// </summary>
                /// <param name="time"></param>
                /// <returns></returns>
                internal bool IncrementalElapseTime(long time)
                {
                    ElapseTime += time;
                    if (ElapseTime >= Timeout)
                    {
                        m_Tcs.TrySetException(new TimeoutException("Rpc call timeout! Message is :" + RequestMessage));
                        return true;
                    }

                    return false;
                }

                internal static RpcMessageData Create(IRequestMessage actorRequestMessage, int timeout = 5000)
                {
                    var defaultMessageActorObject = new RpcMessageData(actorRequestMessage, timeout);
                    return defaultMessageActorObject;
                }

                private RpcMessageData(IRequestMessage requestMessage, int timeout)
                {
                    CreatedTime = GameTimeHelper.UnixTimeMilliseconds();
                    RequestMessage = requestMessage;
                    Timeout = timeout;
                    UniqueId = ((MessageObject)requestMessage).UniqueId;
                    m_Tcs = new TaskCompletionSource<IResponseMessage>();
                }

                private readonly TaskCompletionSource<IResponseMessage> m_Tcs;
                public Task<IResponseMessage> Task => m_Tcs.Task;
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (m_HandlingObjects.Count > 0)
                {
                    var elapseSecondsTime = (long)(elapseSeconds * 1000);
                    m_HandlingObjectIds.Clear();
                    foreach (var handlingObject in m_HandlingObjects)
                    {
                        bool isTimeout = handlingObject.Value.IncrementalElapseTime(elapseSecondsTime);
                        if (isTimeout)
                        {
                            m_HandlingObjectIds.Add(handlingObject.Key);
                        }
                    }
                }

                if (m_HandlingObjectIds.Count > 0)
                {
                    foreach (var objectId in m_HandlingObjectIds)
                    {
                        m_HandlingObjects.TryRemove(objectId, out _);
                    }

                    m_HandlingObjectIds.Clear();
                }
            }
        }
    }
}