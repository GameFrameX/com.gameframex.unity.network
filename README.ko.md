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
  인디 게임 개발자를 위한 올인원 솔루션 · 인디 개발자의 꿈을 실현
</p>

<p align="center">
  <a href="https://gameframex.doc.alianblank.com">문서</a> ·
  <a href="#빠른-시작">빠른 시작</a> ·
  <a href="https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6">QQ 그룹</a> ·
  언어: <a href="README.md">English</a> | <a href="README.zh-CN.md">简体中文</a> | <a href="README.zh-TW.md">繁體中文</a> | <a href="README.ja.md">日本語</a> | <strong>한국어</strong>
</p>

---

## 프로젝트 개요

**Network 장기 연결 네트워크 컴포넌트** - Unity용 장기 연결 네트워크 컴포넌트 관련 인터페이스를 제공합니다. TCP, WebSocket 및 사용자 정의 프로토콜 지원을 포함하여 네트워크 기능을 더 간단하고 효율적으로 만듭니다.

### 기능

- 장기 연결 네트워크 지원 (TCP / WebSocket)
- RPC 호출 메커니즘 및 타임아웃 처리
- 하트비트 패킷 메커니즘 (포커스 획득/손실 시 전송 설정 지원)
- 네트워크 메시지 직렬화 및 역직렬화
- 네트워크 채널 관리
- 네트워크 이벤트 시스템

## 빠른 시작

### 설치

다음 방법 중 하나를 선택하세요:

1. 프로젝트의 `manifest.json` 파일의 `dependencies` 섹션에 다음 내용을 추가:
   ```json
   {"com.gameframex.unity.network": "https://github.com/AlianBlank/com.gameframex.unity.network.git"}
   ```
2. Unity의 `Package Manager`에서 `Git URL`을 사용하여 추가: https://github.com/AlianBlank/com.gameframex.unity.network.git
3. 리포지토리를 다운로드하여 Unity 프로젝트의 `Packages` 디렉토리에 배치 (자동으로 로드됩니다).

### 사용 예시

```csharp
// 표준: GameEntry를 통해 (com.gameframex.unity.entry 비의존)
var networkComponent = GameEntry.GetComponent<NetworkComponent>();
networkComponent.Connect("127.0.0.1", 8080);
```

## 플랫폼 지원

| 플랫폼 | 지원 |
|--------|------|
| Windows | 예 |
| macOS | 예 |
| Linux | 예 |
| Android | 예 |
| iOS | 예 |
| WebGL | 예 |

## 문서 및 자료

- [문서](https://gameframex.doc.alianblank.com)
- [GitHub 리포지토리](https://github.com/gameframex/com.gameframex.unity.network)

## 커뮤니티 및 지원

- QQ 그룹: [QR 코드](https://qm.qq.com/cgi-bin/qm/qr?k=ikT9gA5m2sKwOyNOfYmQvSAPK_c3GmD6)에서 가입

## 변경 로그

자세한 내용은 [CHANGELOG.md](CHANGELOG.md)를 참조하세요.

## 라이선스

이 프로젝트는 [LICENSE](LICENSE) 파일에 정의된 조건에 따라 라이선스가 부여됩니다.
