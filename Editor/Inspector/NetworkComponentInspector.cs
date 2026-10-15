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

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
            if (!EditorApplication.isPlaying)
            {
                // C9 活视图：编辑态提示 NetworkOptions 入口（启动参数不再序列化编辑）。
                GameFrameXOptionsLiveViewUtility.DrawLiveViewHeader(typeof(NetworkOptions).Name);
                return;
            }

            NetworkComponent t = (NetworkComponent)target;

            // C9 活视图：Play 态 RpcTimeout 可实时修改（写回私有字段）；FocusHeartbeat 为启动期型只读。
            GameFrameXOptionsLiveViewUtility.DrawLiveView(t, typeof(NetworkOptions));
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
                EditorGUILayout.LabelField("Local Address", networkChannel.Connected && networkChannel.Socket != null
                    ? (networkChannel.Socket.LocalEndPoint?.ToString() ?? "Unavailable")
                    : "Unavailable");
                EditorGUILayout.LabelField("Remote Address", networkChannel.Connected && networkChannel.Socket != null
                    ? (networkChannel.Socket.RemoteEndPoint?.ToString() ?? "Unavailable")
                    : "Unavailable");
                EditorGUILayout.LabelField("Send Packet", Utility.Text.Format("{0} / {1}", networkChannel.SendPacketCount, networkChannel.SentPacketCount));
                EditorGUILayout.LabelField("Miss Heart Beat Count", networkChannel.MissHeartBeatCount.ToString());
                EditorGUILayout.LabelField("Heart Beat", Utility.Text.Format("{0:F2} / {1:F2}", networkChannel.HeartBeatElapseSeconds, networkChannel.HeartBeatInterval));
                EditorGUI.BeginDisabledGroup(!networkChannel.Connected);
                {
                    if (GUILayout.Button("Disconnect"))
                    {
                        networkChannel.Close(NetworkCloseReason.ConnectClose, (ushort)NetworkErrorCode.DisposeError);
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
        }
    }
}