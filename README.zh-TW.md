<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Network

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.network)](https://github.com/GameFrameX/com.gameframex.unity.network/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.network)](https://github.com/GameFrameX/com.gameframex.unity.network/releases)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使

<br />

[文檔](https://gameframex.doc.alianblank.com) · [快速開始](#快速開始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | **繁體中文** | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>
## 項目簡介

**Network 長連接網絡組件** - 提供 Unity 長連接網絡組件相關的介面，包括 TCP、WebSocket 和自訂協議支援，使網絡功能的使用更加簡單高效。

### 功能特性

- 長連接網絡支援（TCP / WebSocket）
- RPC 呼叫機制及超時處理
- 心跳包機制（支援應用獲得/失去焦點時傳送設定）
- 可插拔的訊息序列化（`IMessageSerializer` 介面），支援兩級註冊（全域預設 + 按頻道覆蓋）
- 網絡訊息序列化與反序列化
- 網絡頻道管理
- 網絡事件系統

## 快速開始

### 安裝方式

任選其一：

1. 直接在 `manifest.json` 的文件中的 `dependencies` 節點下添加以下內容
   ```json
   {"com.gameframex.unity.network": "https://github.com/AlianBlank/com.gameframex.unity.network.git"}
   ```
2. 在 Unity 的 `Packages Manager` 中使用 `Git URL` 的方式添加庫，地址為：https://github.com/AlianBlank/com.gameframex.unity.network.git
3. 直接下載倉庫放置到 Unity 專案的 `Packages` 目錄下，會自動載入識別。

### 使用範例

```csharp
// 標準方式：透過 GameEntry（不依賴 com.gameframex.unity.entry）
var networkComponent = GameEntry.GetComponent<NetworkComponent>();
networkComponent.Connect("127.0.0.1", 8080);
```

#### 可插拔序列化

網絡包透過 `IMessageSerializer` 介面定義訊息的序列化與反序列化行為。支援在兩個層級註冊自訂序列化器：

**全域註冊** — 設定所有頻道的預設序列化器：

```csharp
// 註冊全域序列化器（例如在應用啟動時）
MessageSerializerRegistry.RegisterGlobal(new MyCustomSerializer());
```

**按頻道覆蓋** — 為特定頻道指定序列化器（必須在 `Initialize` 之前呼叫）：

```csharp
var helper = new DefaultNetworkChannelHelper();
helper.SetChannelSerializer(new MyCustomSerializer()); // 必須在 Initialize() 之前呼叫
```

如果未註冊任何序列化器，將使用 `DefaultMessageSerializer`，該預設實作會拋出 `InvalidOperationException` 以提醒開發者進行註冊。`com.gameframex.unity.google.protobuf` 包在載入時會自動將 `ProtobufMessageSerializer` 註冊為全域預設序列化器，提供零配置的向後相容性。

## 平台支援

| 平台 | 支援 |
|------|------|
| Windows | 是 |
| macOS | 是 |
| Linux | 是 |
| Android | 是 |
| iOS | 是 |
| WebGL | 是 |

## 文檔與資源

- [文檔](https://gameframex.doc.alianblank.com)
- [GitHub 倉庫](https://github.com/gameframex/com.gameframex.unity.network)

## 社區與支援

- QQ群：透過 [二維碼](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6) 加入

## 更新日誌

詳見 [CHANGELOG.md](CHANGELOG.md)。

## 開源協議

本專案基於 [LICENSE](LICENSE) 文件中定義的條款授權。
