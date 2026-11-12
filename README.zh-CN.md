<div align="center">

# LogiOptionsPlus-SmoothScroll-Fix

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

</div>

针对 **Logitech Options+ (Windows)** 的纯内存动态修补程序（In-Memory Patch），彻底修复 MX Master 系列鼠标滚轮的两大底层缺陷：

1. **破除写死白名单，全局启用平滑滚动**：绕过 `logioptionsplus_agent.exe` 内部写死的 4 款浏览器限制（`msedge.exe`、`chrome.exe`、`iexplore.exe`、`firefox.exe`），让所有现代桌面应用程序（VS Code、ChatGPT 桌面版、Slack、Notepad++ 等）均能原生享受高分辨率平滑滚动。
2. **修复 SmartShift 物理齿轮与画面平滑的视觉触觉冲突**：当滚轮处于物理刻度（Ratchet 棘轮）模式时，慢速滚动自动分流至原生刚性（Rigid）滚动路径；仅在快速拨动触发 SmartShift 或手动切换为无阻尼（Free-spin）模式时，才启动平滑滚动动画。

---

## 核心特色

- **零磁盘修改（不破坏数字签名）**：完全不修改硬盘上的 `.exe` 二进制文件，100% 保留 Logitech Authenticode 官方签名，绝不会触发 IPC 签名校验失败（避免出现 `Getting resources error` 或滚轮失灵）。
- **无需管理员权限**：`logioptionsplus_agent.exe` 本身以当前登录用户权限运行，修补程序以相同用户层级调用 Win32 内存 API，无需 UAC 提权。
- **开机自动常驻守护**：内置极轻量守护进程，开机静默常驻后台，即使 Logi Options+ 重启或自动更新，也能在 3 秒内自动完成 RAM 内存修补。

---

## 快速开始

### 1. 启用修补程序（全局平滑滚动 + SmartShift 棘轮修复）
打开 PowerShell 并运行：
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action global
```

### 2. 检查运行状态
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action status
```

### 3. 还原默认状态
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action restore
```

---

## 深度技术报告

欲了解完整的二进制逆向工程分析以及提供给罗技官方工程团队的 C++ 修复建议，请参阅：
- [`docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md`](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md)（[English Report](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md)）

---

## 开源许可
本项目基于 [MIT License](LICENSE) 开源。
