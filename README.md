# USB Secure Vault

一个基于白名单授权的 U 盘加密防拷贝系统，专为带有 CD-ROM 模拟固件的加密 U 盘设计。

## 功能特性

### 核心安全

| 功能 | 说明 |
|------|------|
| **白名单授权** | 最多授权 5 台电脑，超出后新机器需管理员移除旧授权 |
| **机器指纹** | WMI 采集 CPU/主板/BIOS 序列号，SHA-256 哈希，精准绑定设备 |
| **AES-256 加密** | 白名单数据加密存储，主密钥存于 U 盘固件安全区 |
| **反调试** | IsDebuggerPresent + NtQueryInformationProcess 双检测，发现即退出 |
| **程序自校验** | 运行时比对 EXE 哈希，被篡改立即终止 |
| **防剪贴板** | WM_CLIPBOARDUPDATE 拦截，禁止 Ctrl+C/Ctrl+V 复制 |
| **防截图** | SetWindowDisplayAffinity 阻止窗口出现在截图/录屏中 |
| **防打印** | PrintScreen 键拦截并清空剪贴板 |

### 授权机制

```
未授权机器  →  显示"未授权 0/5"，禁止访问数据
已授权机器  →  显示"已授权 X/5"，正常访问
管理员      →  密码登录后可管理授权列表
```

## 系统架构

```
┌─────────────────────────────────────────────┐
│  U 盘                                       │
│  ┌──────────────────┐  ┌──────────────────┐ │
│  │  CD-ROM 分区      │  │  数据分区         │ │
│  │  (固件只读)       │  │  (加密存储)       │ │
│  │                  │  │                  │ │
│  │  USBVault.exe   │  │  vault/          │ │
│  │  .firmware/     │  │    whitelist.enc │ │
│  │    key.bin      │  │  (加密白名单)     │ │
│  └──────────────────┘  └──────────────────┘ │
└─────────────────────────────────────────────┘
```

- **CD-ROM 分区**：固件控制只读，程序无法被复制
- **数据分区**：存放加密文件和授权白名单

## 使用方式

### 首次使用（管理员初始化）

1. 将 U 盘插入电脑
2. 程序从 CD-ROM 分区自动运行（首次无白名单，自动进入管理员模式）
3. 输入预设管理员密码登录
4. 点击「添加当前电脑」将本机加入白名单
5. 重复以上步骤，在其他需要授权的电脑上逐一添加

### 普通用户使用

1. 插入 U 盘，程序启动
2. 界面显示授权状态（如 `已授权 3/5 台`）
3. 点击「打开文件」选择并使用加密文件
4. 授权机器显示绿色勾号，未授权机器显示红色叉号

### 管理员管理授权

1. 点击「管理员登录」，输入密码
2. 查看已授权电脑列表（显示编号前8位 + 注册时间）
3. 「添加当前电脑」：将新电脑指纹加入白名单
4. 「移除选中」：从白名单中删除指定机器
5. 「修改密码」：更改管理员密码

> 授权上限 5 台；满额后需先移除旧机器才能添加新机器。

## 构建

### 环境要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)（Windows Desktop 运行时）
- Windows 10/11（用于运行 WPF 客户端）

### 构建步骤

```bash
# 克隆仓库
git clone https://github.com/zhangleifire-coder/USBVault.git
cd USBVault

# 还原依赖
dotnet restore

# Debug 构建
dotnet build

# Release 构建
dotnet build -c Release

# 运行
dotnet run --project src/USBVault.Client

# 运行测试
dotnet test
```

### 发布独立 EXE

```bash
dotnet publish src/USBVault.Client -c Release -r win-x64 --self-contained -o ./publish
```

生成 `publish/USBVault.Client.exe`，可在没有 .NET 运行时的 Windows 电脑上直接运行。

## 项目结构

```
USBVault/
├── src/
│   ├── USBVault.Client/           # WPF 主客户端
│   │   ├── Models/                # 数据模型
│   │   ├── Services/              # 核心服务
│   │   ├── Security/              # 安全组件
│   │   └── ViewModels/            # MVVM 视图模型
│   └── USBVault.Common/           # 共享库
└── tests/
    └── USBVault.Tests/             # 单元测试
```

## 安全设计要点

- 白名单存储为 AES-256-CBC 加密的 JSON，需固件密钥解密
- 机器指纹基于硬件序列号，不依赖网络
- 所有内存解密数据通过 `VirtualLock` 锁定，防止换页到磁盘
- 管理员密码使用 PBKDF2-SHA256（100,000 次迭代）存储

## 部署到 U 盘

1. 将 `publish/` 目录下的 `USBVault.Client.exe` 及相关文件复制到 U 盘 CD-ROM 分区
2. 在数据分区创建 `vault/` 目录
3. 首次运行时程序自动在 `vault/` 下生成加密白名单文件
4. 固件密钥文件 `.firmware/key.bin` 由 U 盘厂商写入 CD-ROM 分区

## 注意事项

- 本程序依赖 WMI 读取硬件序列号，部分虚拟机环境可能无法获取准确的指纹
- 防拷贝机制为应用层防护，无法对抗硬件级物理复制
- 管理员密码请勿设置为简单密码，建议 12 位以上