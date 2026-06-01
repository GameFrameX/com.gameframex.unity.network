# 断线重连 + 服务器主动踢人

## Context

当前网络模块只有心跳超时检测和 Socket 异常检测，缺少：
1. 服务器主动通知客户端断开的协议支持
2. 自动重连机制（指数退避）
3. 重连生命周期事件（Reconnecting / Reconnected / ReconnectFailed）

上层业务无法区分"心跳断开可重连"和"服务器踢人不可重连"。

## 实现方案

### Phase 1：基础类型（无依赖，可并行）

**1.1 修改 `Runtime/Network/Base/NetworkErrorCode.cs`**
- `NetworkCloseReason` 加 `ServerKick = "ServerKick"`
- `NetworkErrorCode` 枚举末尾加 `ServerKickError`

**1.2 新增 `Runtime/EventArgs/NetworkReconnectingEventArgs.cs`**
- 字段：`NetworkChannel`, `RetryCount` (int), `MaxRetryCount` (int), `DelaySeconds` (float)
- 遵循现有 EventArgs 模式（sealed、GameEventArgs、ReferencePool、工厂方法、Clear）

**1.3 新增 `Runtime/EventArgs/NetworkReconnectedEventArgs.cs`**
- 字段：`NetworkChannel`, `RetryCount` (int)

**1.4 新增 `Runtime/EventArgs/NetworkReconnectFailedEventArgs.cs`**
- 字段：`NetworkChannel`, `RetryCount` (int), `Reason` (string)

### Phase 2：接口 + 状态类（依赖 Phase 1）

**2.1 修改 `Runtime/Network/Interface/INetworkManager.cs`**
- 加 3 个事件：`NetworkReconnecting`、`NetworkReconnected`、`NetworkReconnectFailed`
- 加 4 个方法：`SetAutoReconnect(bool)`、`SetAutoReconnectMaxRetryCount(int)`、`ManualReconnect(string)`、`CancelReconnect(string)`

**2.2 新增 `Runtime/Network/Network/NetworkManager.ReconnectState.cs`**
- `NetworkManager` 的嵌套 sealed partial 类，独立文件
- 状态机：Idle → Waiting →（延迟到期）→ 触发 Connect → 成功 Reset / 失败 PrepareNextRetry
- 指数退避：1s → 2s → 4s → 8s → 16s → 30s（上限）
- 核心方法：`Start()`、`Update(float)`、`PrepareNextRetry()`、`Cancel()`、`Reset()`

### Phase 3：核心逻辑（依赖 Phase 2）

**3.1 修改 `Runtime/Network/Network/NetworkManager.NetworkChannelBase.cs`**
- 加 `protected Uri PLastConnectAddress` 和 `protected object PLastConnectUserData`
- 在 `Connect()` 方法顶部保存这两个值（SystemTcpNetworkChannel 和 WebSocketNetworkChannel 都调用了 `base.Connect()`，所以自动生效）

**3.2 修改 `Runtime/Network/Network/NetworkManager.cs`**
- 新增字段：3 个 EventHandler + 3 个 lock 对象 + `m_ReconnectStates` 字典 + `m_ChannelConnectInfos` 字典 + 配置字段
- 新增私有嵌套类 `ConnectInfo`（Address + UserData）
- 新增 3 个事件 add/remove 访问器
- 修改 `Update()`：遍历通道后调用 `ProcessReconnectStates(realElapseSeconds)`
- 新增 `ProcessReconnectStates()`：遍历所有 ReconnectState，延迟到期时触发 `NetworkReconnecting` 事件并调用 `channel.Connect()`
- 修改 `OnNetworkChannelConnected()`：检测到 ReconnectState 时触发 `NetworkReconnected`，清除状态
- 修改 `OnNetworkChannelClosed()`：现有事件触发后，判断是否需要启动重连（排除 ServerKick / Dispose / Normal），首次断开创建 ReconnectState，重连中失败调用 PrepareNextRetry，耗尽触发 `NetworkReconnectFailed`
- 新增 `SetAutoReconnect()`、`SetAutoReconnectMaxRetryCount()`、`ManualReconnect()`、`CancelReconnect()`
- 修改 `Shutdown()`：清理 `m_ReconnectStates` 和 `m_ChannelConnectInfos`

### Phase 4：上层接入（依赖 Phase 3）

**4.1 修改 `Runtime/Network/NetworkComponent.cs`**
- Awake() 订阅 3 个新事件
- 新增 3 个事件转发方法（Fire 到 EventComponent）
- 新增 4 个公共 API 传递方法

**4.2 修改 `Runtime/Network/Helper/DefaultNetworkChannelHelper.cs`**
- Initialize() 中 `Event.CheckSubscribe()` 3 个新事件
- Shutdown() 中 `Event.Unsubscribe()` 3 个新事件
- 新增 3 个事件处理方法（日志输出）

### Phase 5：防裁剪（依赖 Phase 4）

**5.1 修改 `Runtime/GameFrameXNetworkCroppingHelper.cs`**
- Start() 中加 3 个新 EventArgs 的 typeof 引用 + ReconnectState

## 关键设计决策

1. **重连逻辑在 NetworkManager 层**，不在 Channel 层 — Channel 只管 Socket I/O，不关心重连策略
2. **ServerKick 不触发重连** — `OnNetworkChannelClosed` 中排除 `ServerKick` / `Dispose` / `Normal` 三种 Reason
3. **时间戳驱动，无 async/await** — 全部在主线程 Update 循环中计时，对 Unity 兼容
4. **RpcState.Reset() 已就绪** — 现有 Close() 已经调用 `PRpcState.Reset()`，重连后 RPC 可用
5. **NotifyServerDisconnect 协议由业务层定义** — 网络包只提供 `ServerKick` 关闭原因，不内置协议消息类

## 事件流

```
心跳超时断开：
  NetworkMissHeartBeat → ... → NetworkClosed(MissHeartBeat) →
  NetworkReconnecting(1/5, 1s) → NetworkReconnecting(2/5, 2s) → ... →
  NetworkReconnected(2)  [成功时]
  NetworkReconnectFailed(5, "Max retry count reached.")  [失败时]

服务器踢人：
  收到 NotifyServerDisconnect 协议 → 业务调 Close(ServerKick) →
  NetworkClosed(ServerKick, reasonCode)  [不触发重连]
```

## 新增文件清单

| 文件 | 类型 |
|------|------|
| `Runtime/EventArgs/NetworkReconnectingEventArgs.cs` | 新增 |
| `Runtime/EventArgs/NetworkReconnectedEventArgs.cs` | 新增 |
| `Runtime/EventArgs/NetworkReconnectFailedEventArgs.cs` | 新增 |
| `Runtime/Network/Network/NetworkManager.ReconnectState.cs` | 新增 |

## 修改文件清单

| 文件 | 改动点 |
|------|--------|
| `Runtime/Network/Base/NetworkErrorCode.cs` | +1 常量, +1 枚举值 |
| `Runtime/Network/Interface/INetworkManager.cs` | +3 事件, +4 方法 |
| `Runtime/Network/Network/NetworkManager.NetworkChannelBase.cs` | +2 protected 字段 |
| `Runtime/Network/Network/NetworkManager.cs` | 重连状态机核心 |
| `Runtime/Network/NetworkComponent.cs` | +3 事件订阅, +4 API |
| `Runtime/Network/Helper/DefaultNetworkChannelHelper.cs` | +3 事件订阅/处理 |
| `Runtime/GameFrameXNetworkCroppingHelper.cs` | +4 typeof 引用 |

## 验证方式

1. 编译通过（无语法错误）
2. 检查所有新增 .cs 文件都有对应 .meta 文件
3. 代码审查：确认 C# 8.0 语法约束、大括号规范、命名空间块级写法
