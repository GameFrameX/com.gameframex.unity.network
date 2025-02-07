//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using GameFrameX.Editor;
using UnityEditor;

namespace GameFrameX.Network.Editor
{
    /// <summary>
    /// 网络日志脚本宏定义。
    /// </summary>
    public static class NetworkLogScriptingDefineSymbols
    {
        public const string EnableNetworkReceiveLogScriptingDefineSymbol = "ENABLE_GAMEFRAMEX_NETWORK_RECEIVE_LOG";
        public const string EnableNetworkSendLogScriptingDefineSymbol = "ENABLE_GAMEFRAMEX_NETWORK_SEND_LOG";
        public const string ForceEnableNetworkSendLogScriptingDefineSymbol = "FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET";

        /// <summary>
        /// 禁用网络强制为WebSocket脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Scripting Define Symbols/Disable Force WebSocket(关闭强制使用WebSocket网络)", false, 400)]
        public static void DisableForceWebSocketNetwork()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(ForceEnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 开启网络强制为WebSocket脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Scripting Define Symbols/Enable Force WebSocket(开启强制使用WebSocket网络)", false, 401)]
        public static void EnableForceWebSocketNetwork()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(ForceEnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 禁用网络接收日志脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Scripting Define Symbols/Disable Network Receive Logs(关闭网络接收日志打印)", false, 300)]
        public static void DisableNetworkReceiveLogs()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableNetworkReceiveLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 开启网络接收日志脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Scripting Define Symbols/Enable Network Receive Logs(开启网络接收日志打印)", false, 301)]
        public static void EnableNetworkReceiveLogs()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableNetworkReceiveLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 禁用网络发送日志脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Scripting Define Symbols/Disable Network Send Logs(关闭网络发送日志打印)", false, 200)]
        public static void DisableNetworkSendLogs()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 开启网络发送日志脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Scripting Define Symbols/Enable Network Send Logs(开启网络发送日志打印)", false, 201)]
        public static void EnableNetworkSendLogs()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableNetworkSendLogScriptingDefineSymbol);
        }
    }
}