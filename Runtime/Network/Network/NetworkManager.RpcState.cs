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
        public partial class RpcState : IDisposable
        {
            private readonly ConcurrentDictionary<long, RpcMessageData> m_HandlingObjects = new ConcurrentDictionary<long, RpcMessageData>();
            private readonly HashSet<long> m_HandlingObjectIds = new HashSet<long>();
            private EventHandler<MessageObject> m_RpcStartHandler;
            private EventHandler<MessageObject> m_RpcEndHandler;
            private EventHandler<MessageObject> m_RpcErrorHandler;
            private int m_rpcTimeout = 5000;
            private bool m_Disposed = false;

            public RpcState(int timeout)
            {
                m_rpcTimeout = timeout;
                if (m_rpcTimeout < 3000)
                {
                    throw new ArgumentOutOfRangeException(nameof(timeout), "RPC超时时间不能小于3000毫秒");
                }
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
            /// 处理RPC回复消息。
            /// 此方法用于处理接收到的RPC回复消息，并触发相应的结束处理程序。
            /// </summary>
            /// <param name="message">要处理的消息对象，必须实现IResponseMessage接口。</param>
            /// <returns>如果成功处理回复消息，则返回true；否则返回false。</returns>
            public bool Reply(MessageObject message)
            {
                if (message.GetType().IsImplWithInterface(typeof(IResponseMessage)))
                {
                    if (m_HandlingObjects.TryRemove(message.UniqueId, out var messageActorObject))
                    {
                        try
                        {
                            messageActorObject.Reply(message as IResponseMessage);
                            m_RpcEndHandler?.Invoke(this, message);
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e);
                        }

                        return true;
                    }
                }

                return false;
            }

            /// <summary>
            /// 调用RPC并等待返回结果。
            /// 此方法会发送一个请求消息并返回一个任务，任务将在收到响应时完成。
            /// 可能会引发超时异常。
            /// </summary>
            /// <param name="messageObject">要发送的消息对象，必须实现IRequestMessage接口。</param>
            /// <returns>返回一个任务，该任务在收到响应时完成，并返回IResponseMessage。</returns>
            public Task<IResponseMessage> Call(MessageObject messageObject)
            {
                if (m_HandlingObjects.TryGetValue(messageObject.UniqueId, out var messageActorObject))
                {
                    return messageActorObject.Task;
                }

                var defaultMessageActorObject = RpcMessageData.Create(messageObject as IRequestMessage);
                m_HandlingObjects.TryAdd(messageObject.UniqueId, defaultMessageActorObject);
                try
                {
                    m_RpcStartHandler?.Invoke(this, messageObject);
                }
                catch (Exception e)
                {
                    Log.Fatal(e);
                }

                return defaultMessageActorObject.Task;
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
                            try
                            {
                                m_RpcErrorHandler?.Invoke(this, handlingObject.Value.RequestMessage as MessageObject);
                            }
                            catch (Exception e)
                            {
                                Log.Fatal(e);
                            }
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

            /// <summary>
            /// 设置RPC错误的处理函数
            /// </summary>
            /// <param name="handler">处理函数</param>
            [UnityEngine.Scripting.Preserve]
            public void SetRPCErrorHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_RpcErrorHandler = handler;
            }

            /// <summary>
            /// 设置RPC开始的处理函数
            /// </summary>
            /// <param name="handler">处理函数</param>
            [UnityEngine.Scripting.Preserve]
            public void SetRPCStartHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_RpcStartHandler = handler;
            }

            /// <summary>
            /// 设置RPC结束的处理函数
            /// </summary>
            /// <param name="handler">处理函数</param>
            [UnityEngine.Scripting.Preserve]
            public void SetRPCEndHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_RpcEndHandler = handler;
            }
        }
    }
}