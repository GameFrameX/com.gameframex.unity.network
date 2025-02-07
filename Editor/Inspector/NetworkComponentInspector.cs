//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Editor;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using UnityEditor;
using UnityEngine;

namespace GameFrameX.Network.Editor
{
    [CustomEditor(typeof(NetworkComponent))]
    internal sealed class NetworkComponentInspector : ComponentTypeComponentInspector
    {
        private SerializedProperty m_IgnoredSendNetworkIds;
        private SerializedProperty m_IgnoredReceiveNetworkIds;
        private SerializedProperty m_rpcTimeout;

        private readonly GUIContent m_IgnoredSendNetworkIdsGUIContent = new GUIContent("忽略发送消息ID的日志打印");
        private readonly GUIContent m_IgnoredReceiveNetworkIdsGUIContent = new GUIContent("忽略接收消息ID的日志打印");
        private readonly GUIContent m_rpcTimeoutGUIContent = new GUIContent("RPC超时时间,单位:毫秒");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                GUI.enabled = !EditorApplication.isPlaying;
                EditorGUILayout.IntSlider(m_rpcTimeout, 3000, 50000, m_rpcTimeoutGUIContent);
                EditorGUILayout.PropertyField(m_IgnoredSendNetworkIds, m_IgnoredSendNetworkIdsGUIContent);
                EditorGUILayout.PropertyField(m_IgnoredReceiveNetworkIds, m_IgnoredReceiveNetworkIdsGUIContent);
                GUI.enabled = false;
            }
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();
            if (!EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox("Available during runtime only.", MessageType.Info);
                return;
            }

            NetworkComponent t = (NetworkComponent)target;

            if (IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Network Channel Count", t.NetworkChannelCount.ToString());

                INetworkChannel[] networkChannels = t.GetAllNetworkChannels();
                foreach (INetworkChannel networkChannel in networkChannels)
                {
                    DrawNetworkChannel(networkChannel);
                }
            }

            Repaint();
        }

        protected override void RefreshTypeNames()
        {
            RefreshComponentTypeNames(typeof(INetworkManager));
        }

        private void DrawNetworkChannel(INetworkChannel networkChannel)
        {
            EditorGUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField(networkChannel.Name, networkChannel.Connected ? "Connected" : "Disconnected");
                // EditorGUILayout.LabelField("Service Type", networkChannel.ServiceType.ToString());
                EditorGUILayout.LabelField("Address Family", networkChannel.AddressFamily.ToString());
                EditorGUILayout.LabelField("Local Address", networkChannel.Connected ? networkChannel.Socket.LocalEndPoint.ToString() : "Unavailable");
                EditorGUILayout.LabelField("Remote Address", networkChannel.Connected ? networkChannel.Socket.RemoteEndPoint.ToString() : "Unavailable");
                EditorGUILayout.LabelField("Send Packet", Utility.Text.Format("{0} / {1}", networkChannel.SendPacketCount, networkChannel.SentPacketCount));
                EditorGUILayout.LabelField("Miss Heart Beat Count", networkChannel.MissHeartBeatCount.ToString());
                EditorGUILayout.LabelField("Heart Beat", Utility.Text.Format("{0:F2} / {1:F2}", networkChannel.HeartBeatElapseSeconds, networkChannel.HeartBeatInterval));
                EditorGUI.BeginDisabledGroup(!networkChannel.Connected);
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        networkChannel.Close();
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Separator();
        }

        protected override void Enable()
        {
            base.Enable();
            m_IgnoredSendNetworkIds = serializedObject.FindProperty("m_IgnoredSendNetworkIds");
            m_IgnoredReceiveNetworkIds = serializedObject.FindProperty("m_IgnoredReceiveNetworkIds");
            m_rpcTimeout = serializedObject.FindProperty("m_rpcTimeout");
        }
    }
}