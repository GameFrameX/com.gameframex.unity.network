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
        private const string EnableNetworkReceiveLogScriptingDefineSymbol = "ENABLE_GAMEFRAMEX_NETWORK_RECEIVE_LOG";
        private const string EnableNetworkSendLogScriptingDefineSymbol = "ENABLE_GAMEFRAMEX_NETWORK_SEND_LOG";
        private const string ForceEnableNetworkSendLogScriptingDefineSymbol = "FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET";

        /// <summary>
        /// 禁用网络强制为WebSocket脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Log Scripting Define Symbols/Disable Force WebSocket", false, 105)]
        public static void DisableForceWebSocketNetwork()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(ForceEnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 开启网络强制为WebSocket脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Log Scripting Define Symbols/Enable Force WebSocket", false, 106)]
        public static void EnableForceWebSocketNetwork()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(ForceEnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 禁用网络接收日志脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Log Scripting Define Symbols/Disable Network Receive Logs", false, 100)]
        public static void DisableNetworkReceiveLogs()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableNetworkReceiveLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 开启网络接收日志脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Log Scripting Define Symbols/Enable Network Receive Logs", false, 101)]
        public static void EnableNetworkReceiveLogs()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableNetworkReceiveLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 禁用网络发送日志脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Log Scripting Define Symbols/Disable Network Send Logs", false, 103)]
        public static void DisableNetworkSendLogs()
        {
            ScriptingDefineSymbols.RemoveScriptingDefineSymbol(EnableNetworkSendLogScriptingDefineSymbol);
        }

        /// <summary>
        /// 开启网络发送日志脚本宏定义。
        /// </summary>
        [MenuItem("GameFrameX/Log Scripting Define Symbols/Enable Network Send Logs", false, 104)]
        public static void EnableNetworkSendLogs()
        {
            ScriptingDefineSymbols.AddScriptingDefineSymbol(EnableNetworkSendLogScriptingDefineSymbol);
        }
    }
}