<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Network

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.unity.network)](https://github.com/GameFrameX/com.gameframex.unity.network/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.unity.network)](https://github.com/GameFrameX/com.gameframex.unity.network/releases)
[![Unity Version](https://img.shields.io/badge/Unity-2019.4-black?logo=unity)](https://unity.com/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援

<br />

[ドキュメント](https://gameframex.doc.alianblank.com) · [クイックスタート](#クイックスタート) · QQグループ: 467608841 / 233840761

<br />

[English](README.md) | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | **日本語** | [한국어](README.ko.md)

</div>
## プロジェクト概要

**Network 長接続ネットワークコンポーネント** - Unity 向けの長接続ネットワークコンポーネント関連インターフェースを提供します。TCP、WebSocket、カスタムプロトコルサポートを含み、ネットワーク機能の使用をよりシンプルかつ効率的にします。

### 機能

- 長接続ネットワークサポート（TCP / WebSocket）
- RPC 呼び出しメカニズムとタイムアウト処理
- ハートビートパケットメカニズム（フォーカス取得/喪失時の送信設定対応）
- プラグイン可能なメッセージシリアライズ（`IMessageSerializer` インターフェース）、2 レベル登録（グローバルデフォルト + チャネルごとの上書き）対応
- ネットワークメッセージのシリアライズ/デシリアライズ
- ネットワークチャネル管理
- ネットワークイベントシステム

## クイックスタート

### インストール

以下のいずれかの方法を選択してください：

1. プロジェクトの `manifest.json` の `dependencies` セクションに以下を追加：
   ```json
   {"com.gameframex.unity.network": "https://github.com/AlianBlank/com.gameframex.unity.network.git"}
   ```
2. Unity の `Package Manager` で `Git URL` を使用して追加：https://github.com/AlianBlank/com.gameframex.unity.network.git
3. リポジトリをダウンロードして Unity プロジェクトの `Packages` ディレクトリに配置（自動的に読み込まれます）。

### 使用例

```csharp
// 標準: GameEntry 経由（com.gameframex.unity.entry 非依存）
var networkComponent = GameEntry.GetComponent<NetworkComponent>();
networkComponent.Connect("127.0.0.1", 8080);
```

#### プラグイン可能なシリアライズ

ネットワークパッケージは `IMessageSerializer` インターフェースを定義し、メッセージのシリアライズ/デシリアライズを扱います。カスタムシリアライザを 2 つのレベルで登録できます：

**グローバル登録** — すべてのチャネルのデフォルトシリアライザを設定します：

```csharp
// グローバルシリアライザを登録（例：アプリ起動時）
MessageSerializerRegistry.RegisterGlobal(new MyCustomSerializer());
```

**チャネルごとの上書き** — 特定のチャネルにシリアライザを指定します（`Initialize` の前に呼び出す必要があります）：

```csharp
var helper = new DefaultNetworkChannelHelper();
helper.SetChannelSerializer(new MyCustomSerializer()); // Initialize() の前に呼び出す必要があります
```

シリアライザが登録されていない場合、`DefaultMessageSerializer` が使用されます。このデフォルト実装は登録を促すために `InvalidOperationException` をスローします。`com.gameframex.unity.google.protobuf` パッケージは読み込み時に `ProtobufMessageSerializer` をグローバルデフォルトとして自動登録し、設定不要の後方互換性を提供します。

## プラットフォーム対応

| プラットフォーム | 対応 |
|-----------------|------|
| Windows | はい |
| macOS | はい |
| Linux | はい |
| Android | はい |
| iOS | はい |
| WebGL | はい |

## ドキュメントとリソース

- [ドキュメント](https://gameframex.doc.alianblank.com)
- [GitHub リポジトリ](https://github.com/gameframex/com.gameframex.unity.network)

## コミュニティとサポート

- QQグループ：[QRコード](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6)から参加

## 変更履歴

詳細は [CHANGELOG.md](CHANGELOG.md) をご覧ください。

## ライセンス

このプロジェクトは [LICENSE](LICENSE) ファイルに定義された条件に基づいてライセンスされています。
