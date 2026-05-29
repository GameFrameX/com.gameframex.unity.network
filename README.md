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
  All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">Documentation</a> ·
  <a href="#quick-start">Quick Start</a> ·
  <a href="https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6">QQ Group</a> ·
  Language: <strong>English</strong> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <a href="README.ko.md">한국어</a>
</p>

---

## Project Overview

**Network Component** - A long-connection network component for Unity that provides network interfaces including TCP, WebSocket, and custom protocol support, making network functionality simpler and more efficient.

### Features

- Long-connection network support (TCP / WebSocket)
- RPC call mechanism with timeout handling
- Heartbeat packet mechanism (configurable on focus lost/gained)
- Pluggable message serialization (`IMessageSerializer` interface) with two-level registration (global default + per-channel override)
- Network message serialization and deserialization
- Network channel management
- Network event system

## Quick Start

### Installation

Choose one of the following methods:

1. Add the following to the `dependencies` section of your project's `manifest.json`:
   ```json
   {"com.gameframex.unity.network": "https://github.com/AlianBlank/com.gameframex.unity.network.git"}
   ```
2. Use `Git URL` in Unity's Package Manager: https://github.com/AlianBlank/com.gameframex.unity.network.git
3. Download the repository and place it in your Unity project's `Packages` directory.

### Usage Examples

```csharp
// Standard: via GameEntry (no dependency on com.gameframex.unity.entry)
var networkComponent = GameEntry.GetComponent<NetworkComponent>();
networkComponent.Connect("127.0.0.1", 8080);
```

#### Pluggable Serialization

The network package defines the `IMessageSerializer` interface for message serialization/deserialization. You can register a custom serializer at two levels:

**Global registration** — sets the default serializer for all channels:

```csharp
// Register a global serializer (e.g. at app startup)
MessageSerializerRegistry.RegisterGlobal(new MyCustomSerializer());
```

**Per-channel override** — overrides the serializer for a specific channel (must be called before `Initialize`):

```csharp
var helper = new DefaultNetworkChannelHelper();
helper.SetChannelSerializer(new MyCustomSerializer()); // Must be called before Initialize()
```

If no serializer is registered, a `DefaultMessageSerializer` is used that throws an `InvalidOperationException` to remind you to register one. The `com.gameframex.unity.google.protobuf` package auto-registers `ProtobufMessageSerializer` as the global default on load, providing zero-config backward compatibility.

## Platform Support

| Platform | Supported |
|----------|-----------|
| Windows | Yes |
| macOS | Yes |
| Linux | Yes |
| Android | Yes |
| iOS | Yes |
| WebGL | Yes |

## Documentation & Resources

- [Documentation](https://gameframex.doc.alianblank.com)
- [GitHub Repository](https://github.com/gameframex/com.gameframex.unity.network)

## Community & Support

- QQ Group: Join via [QR Code](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6)

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for details.

## License

This project is licensed under the terms of the [LICENSE](LICENSE) file.
