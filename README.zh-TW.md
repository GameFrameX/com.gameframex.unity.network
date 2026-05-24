<p align="center">
  <img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="GameFrameX Logo" width="160" />
</p>

<h1 align="center">Game Frame X Network</h1>

<p align="center">
  <a href="https://github.com/gameframex/com.gameframex.unity.network/releases">
    <img src="https://img.shields.io/github/v/release/gameframex/com.gameframex.unity.network" alt="Version" />
  </a>
  <a href="https://github.com/gameframex/com.gameframex.unity.network/blob/main/LICENSE">
    <img src="https://img.shields.io/github/license/gameframex/com.gameframex.unity.network" alt="License" />
  </a>
  <a href="https://gameframex.doc.alianblank.com">
    <img src="https://img.shields.io/badge/Documentation-online-blue" alt="Documentation" />
  </a>
</p>

<p align="center">
  獨立遊戲前後端一體化解決方案 · 獨立遊戲開發者的圓夢大使
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">文檔</a> ·
  <a href="#快速開始">快速開始</a> ·
  <a href="https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6">QQ群</a> ·
  語言: <a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <strong>繁體中文</strong> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a>
</p>

---

## 項目簡介

**Network 長連接網絡組件** - 提供 Unity 長連接網絡組件相關的介面，包括 TCP、WebSocket 和自訂協議支援，使網絡功能的使用更加簡單高效。

### 功能特性

- 長連接網絡支援（TCP / WebSocket）
- RPC 呼叫機制及超時處理
- 心跳包機制（支援應用獲得/失去焦點時傳送設定）
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
