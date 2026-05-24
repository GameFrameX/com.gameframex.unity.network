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
  インディゲーム開発者向けオールインワンソリューション · インディ開発者の夢を支援
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">ドキュメント</a> ·
  <a href="#クイックスタート">クイックスタート</a> ·
  <a href="https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6">QQグループ</a> ·
  言語: <a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <strong>日本語</strong> | <a href="README.ko.md">한국어</a>
</p>

---

## プロジェクト概要

**Network 長接続ネットワークコンポーネント** - Unity 向けの長接続ネットワークコンポーネント関連インターフェースを提供します。TCP、WebSocket、カスタムプロトコルサポートを含み、ネットワーク機能の使用をよりシンプルかつ効率的にします。

### 機能

- 長接続ネットワークサポート（TCP / WebSocket）
- RPC 呼び出しメカニズムとタイムアウト処理
- ハートビートパケットメカニズム（フォーカス取得/喪失時の送信設定対応）
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
