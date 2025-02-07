//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace GameFrameX.Network.Runtime
{
    public sealed partial class NetworkManager
    {
        public sealed class ConnectState
        {
            private readonly INetworkSocket m_Socket;

            [UnityEngine.Scripting.Preserve]
            public ConnectState(INetworkSocket socket, object userData)
            {
                m_Socket = socket;
                UserData = userData;
            }

            /// <summary>
            /// Socket
            /// </summary>
            public INetworkSocket Socket
            {
                get { return m_Socket; }
            }

            /// <summary>
            /// 用户自定义数据
            /// </summary>
            public object UserData { get; }
        }
    }
}