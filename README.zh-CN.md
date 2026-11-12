<div align="center">

# LogiOptionsPlus-SmoothScroll-Fix

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

</div>

针对 **Logitech Options+ (Windows)** 的纯内存动态修补程序（In-Memory Patch），彻底修复 MX Master 系列鼠标滚轮的两大底层缺陷：

1. **破除写死白名单，全局启用平滑滚动**：绕过 `logioptionsplus_agent.exe` 内部写死的 4 款浏览器限制（`msedge.exe`、`chrome.exe`、`iexplore.exe`、`firefox.exe`），让所有现代桌面应用程序（VS Code、ChatGPT 桌面版、Slack、Notepad++ 等）均能原生享受高分辨率平滑滚动。
2. **修复 SmartShift 物理齿轮与画面平滑的视觉触觉冲突**：当滚轮处于物理刻度（Ratchet 棘轮）模式时，慢速滚动自动分流至原生刚性（Rigid）滚动路径；仅在快速拨动触发 SmartShift 或手动切换为无阻尼（Free-spin）模式时，才启动平滑滚动动画。

---

## 快速开始（最简单推荐）

1. 前往 Releases 下载免编译打包文件 **[`LogiOptionsPlus-SmoothScroll-Fix-v1.0.0.zip`](https://github.com/shenshunjesus/LogiOptionsPlus-SmoothScroll-Fix/releases/tag/v1.0.0)** 并解压缩。
2. **一键启用**：双击运行 **`Install-Fix.bat`**（完成！全局平滑滚动与开机自启动立即生效）。
3. **一键解除**：如需恢复罗技官方默认状态，双击运行 **`Uninstall-Fix.bat`** 即可干净卸载。
4. **检查状态**：双击运行 **`Check-Status.bat`** 可随时查看当前驻留状态。

> [!IMPORTANT]
> **完全不需要“以管理员身份运行”！**  
> `logioptionsplus_agent.exe` 本身是以普通用户身份运行，本工具**仅需普通用户权限**即可正常运行。请直接双击运行，切勿使用管理员权限。

---

## 安全性与透明度（为什么您可以放心运行？）

在 Windows 上运行第三方的 `.exe` 程序确实需要谨慎。本项目采取最严格的透明度设计：

1. **无需管理员权限（Non-Elevated）**：不申请 UAC 提权，不修改系统目录，不安装虚拟驱动，不做全局系统 Hook。
2. **零网络连接（100% Offline）**：源代码中绝无任何 HTTP/Socket 网络连接，无遥测、无数据上报。
3. **开源可审查（不足 200 行代码）**：核心逻辑位于 [`src/LogiSmoothScroll.cs`](src/LogiSmoothScroll.cs)，任何人均可在 3 分钟内通读审计。
4. **支持完全自源码现场编译**：若对下载的 `.exe` 有所顾虑，克隆本项目并在 PowerShell 中运行：
   ```powershell
   powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action global
   ```
   脚本将调用微软系统内置的 C# 编译器（`csc.exe`）在您本地现场编译。
5. **官方二进制校验码（SHA-256）**：
   - `LogiSmoothScroll.exe` (v1.0.0):
     `735250A7ED081BC8525C9B4B00DBFA4EA4C621FAF097F5F616BC4CBB7BF9F167`

---

## 深度技术报告

欲了解完整的二进制逆向工程分析以及提供给罗技官方工程团队的 C++ 修复建议，请参阅：
- [`docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-CN.md`](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-CN.md) ([English](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md) · [繁體中文](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md) · [日本語](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.ja.md))

---

## 开源许可
本项目基于 [MIT License](LICENSE) 开源。
