using System;
using System.Net;
using System.Net.Sockets;

namespace GameFrameX.Network.Runtime
{
    public partial class NetworkManager
    {
        private sealed class SystemNetSocket : INetworkSocket
        {
            private readonly Socket m_Socket;

            public SystemNetSocket(System.Net.Sockets.AddressFamily ipAddressAddressFamily, SocketType socketType, ProtocolType protocolType)
            {
                m_Socket = new Socket(ipAddressAddressFamily, socketType, protocolType);
            }

            public bool IsConnected
            {
                get { return m_Socket.Connected; }
            }

            public Socket Socket
            {
                get { return m_Socket; }
            }

            public EndPoint LocalEndPoint
            {
                get { return m_Socket.LocalEndPoint; }
            }

            public EndPoint RemoteEndPoint
            {
                get { return m_Socket.RemoteEndPoint; }
            }

            public int Available
            {
                get { return m_Socket.Available; }
            }

            public int ReceiveBufferSize
            {
                get { return m_Socket.ReceiveBufferSize; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("Receive buffer size is invalid.", nameof(value));
                    }

                    m_Socket.ReceiveBufferSize = value;
                }
            }

            public int SendBufferSize
            {
                get { return m_Socket.SendBufferSize; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("Send buffer size is invalid.", nameof(value));
                    }

                    m_Socket.SendBufferSize = value;
                }
            }

            public void Shutdown()
            {
                m_Socket.Shutdown(SocketShutdown.Both);
            }

            public void Close()
            {
                m_Socket.Close();
            }


            public IAsyncResult BeginSend(byte[] getBuffer, int streamPosition, int streamLength, SocketFlags none, AsyncCallback mSendCallback, INetworkSocket mSocket)
            {
                return m_Socket.BeginSend(getBuffer, streamPosition, streamLength, none, mSendCallback, mSocket);
            }

            public int EndSend(IAsyncResult asyncResult, out SocketError error)
            {
                return m_Socket.EndSend(asyncResult, out error);
            }


            public void BeginConnect(IPAddress ipAddress, int port, AsyncCallback mConnectCallback, ConnectState connectState)
            {
                m_Socket.BeginConnect(ipAddress, port, mConnectCallback, connectState);
            }

            public void EndConnect(IAsyncResult ar)
            {
                m_Socket.EndConnect(ar);
            }

            public void BeginReceive(byte[] getBuffer, int streamPosition, int streamLength, SocketFlags none, AsyncCallback mReceiveCallback, INetworkSocket mSocket)
            {
                m_Socket.BeginReceive(getBuffer, streamPosition, streamLength, none, mReceiveCallback, mSocket);
            }

            public int EndReceive(IAsyncResult asyncResult)
            {
                return m_Socket.EndReceive(asyncResult);
            }
        }
    }
}