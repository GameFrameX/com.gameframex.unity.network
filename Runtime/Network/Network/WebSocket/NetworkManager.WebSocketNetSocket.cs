#if ENABLE_GAME_FRAME_X_WEB_SOCKET
using System;
using System.Net;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using UnityWebSocket;

namespace GameFrameX.Network.Runtime
{
    public partial class NetworkManager
    {
        private sealed class WebSocketNetSocket : INetworkSocket
        {
            private readonly IWebSocket _client;

            /// <summary>
            /// 是否是加密协议
            /// </summary>
            private readonly bool _isSSL = false;

            /// <summary>
            /// 是否正在连接
            /// </summary>
            private bool _isConnecting = false;

            TaskCompletionSource<bool> _connectTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private Action<byte[]> _action;
            private Action<string> _onCloseAction;

            public WebSocketNetSocket(IPAddress ipAddress, int port, bool isSSL, Action<byte[]> action, Action<string> onCloseAction)
            {
                _isSSL = isSSL;
                _client = new UnityWebSocket.WebSocket("ws://" + ipAddress + ":" + port + "/" + (_isSSL ? "wss" : "ws"));
                _action = action;
                _onCloseAction = onCloseAction;
                _client.OnOpen += OnOpen;
                _client.OnError += OnError;
                _client.OnClose += OnClose;
                _client.OnMessage += OnMessage;
            }

            private void OnMessage(object sender, MessageEventArgs e)
            {
                if (e.IsBinary)
                {
                    _action.Invoke(e.RawData);
                }
            }

            private void OnClose(object sender, CloseEventArgs e)
            {
                _onCloseAction?.Invoke(e.Reason + " " + e.Code);
                Log.Error(e.Code + " " + e.Reason);
            }

            private void OnError(object sender, ErrorEventArgs e)
            {
                if (_isConnecting)
                {
                    // 连接错误
                }
                else
                {
                    // 非连接错误
                }

                Log.Error(e.Message);
                _connectTask.TrySetResult(false);
            }

            private void OnOpen(object sender, OpenEventArgs e)
            {
                _isConnecting = false;
                _connectTask.TrySetResult(true);
            }


            public async Task ConnectAsync()
            {
                _isConnecting = true;
                _connectTask = new TaskCompletionSource<bool>();
                _client.ConnectAsync();
                await _connectTask.Task;
            }

            public IWebSocket Client
            {
                get { return _client; }
            }

            public bool IsConnected
            {
                get { return _client.IsConnected; }
            }

            public EndPoint LocalEndPoint
            {
                get { return null; }
            }

            public EndPoint RemoteEndPoint
            {
                get { return null; }
            }

            public int ReceiveBufferSize { get; set; }
            public int SendBufferSize { get; set; }

            public void Shutdown()
            {
                _client.CloseAsync();
            }

            public void Close()
            {
                _client.CloseAsync();
            }
        }
    }
}

#endif