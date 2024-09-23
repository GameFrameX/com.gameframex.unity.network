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
            private bool m_Disposed = false;

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
                if (message.GetType().IsImplWithInterface(typeof(IResponseMessage)))
                {
                    if (m_HandlingObjects.TryRemove(message.UniqueId, out var messageActorObject))
                    {
                        messageActorObject.Reply(message as IResponseMessage);
                        try
                        {
                            m_RpcEndHandler?.Invoke(this, message);
                            return true;
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e);
                        }
                    }
                }

                return false;
            }

            /// <summary>
            /// 调用并等待返回结果。可能引发超时异常
            /// </summary>
            /// <param name="messageObject">消息对象</param>
            /// <returns></returns>
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
            public void SetRPCErrorHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_RpcErrorHandler = handler;
            }

            /// <summary>
            /// 设置RPC开始的处理函数
            /// </summary>
            /// <param name="handler">处理函数</param>
            public void SetRPCStartHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_RpcStartHandler = handler;
            }

            /// <summary>
            /// 设置RPC结束的处理函数
            /// </summary>
            /// <param name="handler">处理函数</param>
            public void SetRPCEndHandler(EventHandler<MessageObject> handler)
            {
                GameFrameworkGuard.NotNull(handler, nameof(handler));
                m_RpcEndHandler = handler;
            }
        }
    }
}