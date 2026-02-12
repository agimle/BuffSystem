# BuffSystem 网络同步模块

本模块提供BuffSystem的多人网络同步支持，兼容主流Unity网络框架。

## 支持的框架

- **Mirror** - 使用条件编译符号 `MIRROR`
- **Netcode for GameObjects** - 使用条件编译符号 `UNITY_NETCODE_GAMEOBJECTS`

## 使用方式

### 1. Mirror 框架

#### 启用步骤
1. 在Unity中安装Mirror框架
2. 在 `Edit > Project Settings > Player > Scripting Define Symbols` 中添加 `MIRROR`
3. 在需要同步的BuffOwner对象上添加 `BuffMirrorSync` 组件

#### 代码示例

```csharp
using BuffSystem.Networking.Mirror;

public class MyPlayer : NetworkBehaviour
{
    private BuffMirrorSync buffSync;
    private BuffOwner buffOwner;
    
    void Awake()
    {
        buffOwner = GetComponent<BuffOwner>();
        buffSync = GetComponent<BuffMirrorSync>();
    }
    
    // 服务器添加Buff（自动同步到所有客户端）
    [Server]
    public void ServerAddBuff(int buffId)
    {
        var data = BuffDatabase.Instance.GetBuffData(buffId);
        buffOwner.BuffContainer.AddBuff(data, this);
    }
    
    // 客户端请求添加Buff（需要服务器权限）
    [Client]
    public void ClientRequestAddBuff(int buffId)
    {
        // 方式1：使用Command
        CmdAddBuff(buffId);
        
        // 方式2：使用预测
        buffSync.PredictAddBuff(buffId);
    }
    
    [Command]
    void CmdAddBuff(int buffId)
    {
        ServerAddBuff(buffId);
    }
}
```

### 2. Netcode for GameObjects 框架

#### 启用步骤
1. 在Unity中安装Netcode for GameObjects包
2. 在 `Edit > Project Settings > Player > Scripting Define Symbols` 中添加 `UNITY_NETCODE_GAMEOBJECTS`
3. 在需要同步的BuffOwner对象上添加 `BuffNetcodeSync` 组件

#### 代码示例

```csharp
using BuffSystem.Networking.Netcode;

public class MyPlayer : NetworkBehaviour
{
    private BuffNetcodeSync buffSync;
    private BuffOwner buffOwner;
    
    void Awake()
    {
        buffOwner = GetComponent<BuffOwner>();
        buffSync = GetComponent<BuffNetcodeSync>();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            // 服务器直接添加Buff
            var data = BuffDatabase.Instance.GetBuffData(1001);
            buffOwner.BuffContainer.AddBuff(data, this);
        }
    }
    
    // 客户端请求添加Buff
    public void RequestAddBuff(int buffId)
    {
        if (buffSync != null)
        {
            buffSync.RequestAddBuffServerRpc(buffId);
        }
    }
}
```

## 同步机制

### 服务器权威模式
- 所有Buff变更由服务器控制
- 客户端只接收同步状态
- 防止作弊，保证一致性

### 同步数据优化
每个Buff同步数据仅9字节：
- InstanceId: 4 bytes (int)
- BuffId: 2 bytes (ushort)
- Stack: 1 byte (byte)
- Duration: 2 bytes (ushort, 压缩)

### 事件驱动同步
- Buff添加/移除/层数变化时自动触发同步
- 无需手动调用同步方法
- 支持增量更新

## 注意事项

1. **BuffDatabase同步** - 确保所有客户端和服务器的BuffDatabase配置一致
2. **Source同步** - Source对象（如施法者）不会自动同步，需要单独处理
3. **自定义数据** - IBuffSerializable的自定义数据不会自动网络同步
4. **性能优化** - 大量Buff同时变更时考虑批量处理

## 调试

启用 `BuffSystemConfig.EnableDebugLog` 可查看网络同步日志：

```
[BuffMirrorSync] Server: 添加Buff SpeedUp (ID: 12345)
[BuffMirrorSync] Client: 同步添加Buff SpeedUp (层数: 2)
```
