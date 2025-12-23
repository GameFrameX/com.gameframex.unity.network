# [2.5.0](https://github.com/gameframex/com.gameframex.unity.network/compare/2.4.0...2.5.0) (2025-12-23)


### Bug Fixes

* **Network:** 优化RPC超时错误信息，包含消息类型全名 ([ce4fb0d](https://github.com/gameframex/com.gameframex.unity.network/commit/ce4fb0d704f04b9536ca0ba776a1cc4addae2322))
* **Network:** 修复RPC超时处理逻辑错误并优化性能 ([213b99d](https://github.com/gameframex/com.gameframex.unity.network/commit/213b99dac04baba62fd5225abfc86d33a9e54e31))
* **Network:** 修复RPC错误码处理条件判断错误 ([37c2cb1](https://github.com/gameframex/com.gameframex.unity.network/commit/37c2cb10484381a57de27c79a33d8c34f6fa503a))
* **Network:** 修正应用获得焦点时心跳包的默认行为 ([b5840f6](https://github.com/gameframex/com.gameframex.unity.network/commit/b5840f62295458cf37a87db9dafbe6b4dfafdf20))
* **Network:** 修正接收消息日志中使用错误的MessageId获取方法 ([09c5853](https://github.com/gameframex/com.gameframex.unity.network/commit/09c58538831507e8d1e4f69cb552c7605cf3bc75))
* **Network:** 修正网络日志中消息ID的获取方式 ([d60741a](https://github.com/gameframex/com.gameframex.unity.network/commit/d60741af11e2d9594b9d3f1ecea703ff5f18a2b7))
* **Network:** 移除未实现的Close方法并修正SystemTcpNetworkChannel中的Close调用 ([c07dee1](https://github.com/gameframex/com.gameframex.unity.network/commit/c07dee1191f8f5f42982de7c94b2e0dbd77384c9))
* **Network:** 调整消息发送和日志记录顺序以避免潜在问题 ([38a77c7](https://github.com/gameframex/com.gameframex.unity.network/commit/38a77c7598ee0f503c0ff1cf0b986aef8d478d63))
* 在NetworkChannelBase中释放messageObject引用 ([d8634ec](https://github.com/gameframex/com.gameframex.unity.network/commit/d8634ecd9cf8d32851d83f30d53e650b9a00bdcb))


### Features

* **network:** 为网络关闭添加原因和错误码参数 ([688de90](https://github.com/gameframex/com.gameframex.unity.network/commit/688de90357b6b053ae4d382d147f97397319bbb7))
* **网络:** 为RPC调用添加忽略错误码选项 ([f50f845](https://github.com/gameframex/com.gameframex.unity.network/commit/f50f84505f8cf20d78cdac9e31e61344795b7a8b))
* **网络:** 添加失去焦点时发送心跳包的功能 ([1927396](https://github.com/gameframex/com.gameframex.unity.network/commit/1927396e6eadd9180636f252ba2ab867c01bea25))
* **网络:** 添加应用获得焦点时发送心跳包的功能 ([ccc8504](https://github.com/gameframex/com.gameframex.unity.network/commit/ccc8504dffd6ac81023244c1d5d217650891afc9))

# Changelog

## [2.4.0](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.4.0) (2025-10-15)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.3.4...2.4.0)

## [2.3.4](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.3.4) (2025-05-31)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.3.3...2.3.4)

## [2.3.3](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.3.3) (2025-05-19)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.3.2...2.3.3)

## [2.3.2](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.3.2) (2025-05-13)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.3.1...2.3.2)

## [2.3.1](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.3.1) (2025-05-10)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.3.0...2.3.1)

## [2.3.0](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.3.0) (2025-02-20)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.2.2...2.3.0)

## [2.2.2](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.2.2) (2025-02-18)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.2.1...2.2.2)

## [2.2.1](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.2.1) (2025-02-08)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.2.0...2.2.1)

## [2.2.0](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.2.0) (2025-02-07)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.1.0...2.2.0)

## [2.1.0](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.1.0) (2025-02-05)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.0.1...2.1.0)

## [2.0.1](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.0.1) (2025-01-02)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2.0.0...2.0.1)

## [2.0.0](https://github.com/GameFrameX/com.gameframex.unity.network/tree/2.0.0) (2024-11-29)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.1.0...2.0.0)

## [1.1.0](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.1.0) (2024-10-11)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.19...1.1.0)

## [1.0.19](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.19) (2024-09-23)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.18...1.0.19)

## [1.0.18](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.18) (2024-09-09)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.17...1.0.18)

## [1.0.17](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.17) (2024-09-09)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.16...1.0.17)

## [1.0.16](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.16) (2024-09-09)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.15...1.0.16)

## [1.0.15](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.15) (2024-08-15)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.14...1.0.15)

## [1.0.14](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.14) (2024-08-13)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.13...1.0.14)

## [1.0.13](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.13) (2024-08-13)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.12...1.0.13)

**Merged pull requests:**

- \[修改\]1. 修改消息线程问题 [\#1](https://github.com/GameFrameX/com.gameframex.unity.network/pull/1) ([StarryGaming](https://github.com/StarryGaming))

## [1.0.12](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.12) (2024-08-05)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.11...1.0.12)

## [1.0.11](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.11) (2024-08-05)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.10...1.0.11)

## [1.0.10](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.10) (2024-08-05)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.9...1.0.10)

## [1.0.9](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.9) (2024-07-06)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.8...1.0.9)

## [1.0.8](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.8) (2024-07-02)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.7...1.0.8)

## [1.0.7](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.7) (2024-06-28)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.6...1.0.7)

## [1.0.6](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.6) (2024-06-28)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.5...1.0.6)

## [1.0.5](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.5) (2024-06-21)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.4...1.0.5)

## [1.0.4](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.4) (2024-06-19)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.3...1.0.4)

## [1.0.3](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.3) (2024-05-26)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.2...1.0.3)

## [1.0.2](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.2) (2024-05-25)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.1...1.0.2)

## [1.0.1](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.1) (2024-05-20)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/1.0.0...1.0.1)

## [1.0.0](https://github.com/GameFrameX/com.gameframex.unity.network/tree/1.0.0) (2024-04-10)

[Full Changelog](https://github.com/GameFrameX/com.gameframex.unity.network/compare/2c9e2d4d42112ca6848ad5cd9dfa5093d1cd93e0...1.0.0)



\* *This Changelog was automatically generated by [github_changelog_generator](https://github.com/github-changelog-generator/github-changelog-generator)*
