# WatchdogApp - 进程监控看门狗

## 项目简介

WatchdogApp 是一个进程监控和自动重启工具，用于保障关键后台进程持续运行。当被监控的进程意外退出时，看门狗会自动将其重新启动。

## 使用方法

### 启动看门狗

```bash
WatchdogApp.exe <进程名> <进程路径> [检查间隔(毫秒)]
```

**参数说明**：

| 参数 | 必填 | 默认值 | 说明 |
|------|------|--------|------|
| 进程名 | 是 | — | 要监控的进程名称（不含 .exe 后缀） |
| 进程路径 | 是 | — | 进程的完整可执行文件路径 |
| 检查间隔 | 否 | 5000 | 检查进程状态的间隔时间（毫秒） |

**示例**：

```bash
# 监控 SmartHotel 服务，每 3 秒检查一次
WatchdogApp.exe SmartHotel "C:\app\SmartHotel.exe" 3000

# 使用默认 5 秒间隔
WatchdogApp.exe HotelHelper "C:\app\HotelHelper.exe"
```

### 停止看门狗

```bash
WatchdogApp.exe STOP
```

发送 `STOP` 命令将终止当前运行的看门狗实例。

## 应用场景

- 监控 SmartHotelService（身份证读卡服务），确保 HTTP API 持续可用
- 监控 HotelHelper（自助终端），防止界面程序崩溃后无人值守

## 功能特性

- **托盘图标**：以系统托盘方式运行，显示当前监控状态
- **自动重启**：检测到进程退出后立即重新启动
- **日志记录**：运行日志保存在 `logs/` 目录下
- **后台运行**：以 Windows 窗体应用方式隐藏运行

## 技术信息

- **框架**：.NET 8.0 + Windows Forms
- **平台**：x86
- **作者**：郭强（Kevin）
- **公司**：新途径数科

## 编译要求

```powershell
dotnet build -c Release -p:Platform=x86 -p:EnableWindowsTargeting=true
```

## 许可证

版权所有 © 新途径数科
