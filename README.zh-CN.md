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
  独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">文档</a> ·
  <a href="#快速开始">快速开始</a> ·
  <a href="https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6">QQ群</a> ·
  语言: <a href="README.md">English</a> | <strong>简体中文</strong> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a>
</p>

---

## 项目简介

**Network 长连接网络组件** - 提供 Unity 长连接网络组件相关的接口，包括 TCP、WebSocket 和自定义协议支持，使网络功能的使用更加简单高效。

### 功能特性

- 长连接网络支持（TCP / WebSocket）
- RPC 调用机制及超时处理
- 心跳包机制（支持应用获得/失去焦点时发送配置）
- 可插拔的消息序列化（`IMessageSerializer` 接口），支持两级注册（全局默认 + 按频道覆盖）
- 网络消息序列化与反序列化
- 网络频道管理
- 网络事件系统

## 快速开始

### 安装方式

任选其一：

1. 直接在 `manifest.json` 的文件中的 `dependencies` 节点下添加以下内容
   ```json
   {"com.gameframex.unity.network": "https://github.com/AlianBlank/com.gameframex.unity.network.git"}
   ```
2. 在 Unity 的 `Packages Manager` 中使用 `Git URL` 的方式添加库，地址为：https://github.com/AlianBlank/com.gameframex.unity.network.git
3. 直接下载仓库放置到 Unity 项目的 `Packages` 目录下，会自动加载识别。

### 使用示例

```csharp
// 标准方式：通过 GameEntry（不依赖 com.gameframex.unity.entry）
var networkComponent = GameEntry.GetComponent<NetworkComponent>();
networkComponent.Connect("127.0.0.1", 8080);
```

#### 可插拔序列化

网络包通过 `IMessageSerializer` 接口定义消息的序列化与反序列化行为。支持在两个层级注册自定义序列化器：

**全局注册** — 设置所有频道的默认序列化器：

```csharp
// 注册全局序列化器（例如在应用启动时）
MessageSerializerRegistry.RegisterGlobal(new MyCustomSerializer());
```

**按频道覆盖** — 为特定频道指定序列化器（必须在 `Initialize` 之前调用）：

```csharp
var helper = new DefaultNetworkChannelHelper();
helper.SetChannelSerializer(new MyCustomSerializer()); // 必须在 Initialize() 之前调用
```

如果未注册任何序列化器，将使用 `DefaultMessageSerializer`，该默认实现会抛出 `InvalidOperationException` 以提醒开发者进行注册。`com.gameframex.unity.google.protobuf` 包在加载时会自动将 `ProtobufMessageSerializer` 注册为全局默认序列化器，提供零配置的向后兼容性。

## 平台支持

| 平台 | 支持 |
|------|------|
| Windows | 是 |
| macOS | 是 |
| Linux | 是 |
| Android | 是 |
| iOS | 是 |
| WebGL | 是 |

## 文档与资源

- [文档](https://gameframex.doc.alianblank.com)
- [GitHub 仓库](https://github.com/gameframex/com.gameframex.unity.network)

## 社区与支持

- QQ群：通过 [二维码](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6) 加入

## 更新日志

详见 [CHANGELOG.md](CHANGELOG.md)。

## 开源协议

本项目基于 [LICENSE](LICENSE) 文件中定义的条款授权。
