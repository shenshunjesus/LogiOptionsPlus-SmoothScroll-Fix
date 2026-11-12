<div align="center">

# LogiOptionsPlus-SmoothScroll-Fix

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

</div>

An in-memory runtime patch for **Logitech Options+ (Windows)** that fixes two long-standing scroll engine issues on MX Master series mice:

1. **Global Smooth Scrolling**: Bypasses the hardcoded 4-browser whitelist (`msedge.exe`, `chrome.exe`, `iexplore.exe`, `firefox.exe`) in `logioptionsplus_agent.exe`, enabling high-precision smooth scrolling globally across all desktop apps (VS Code, ChatGPT Desktop, Slack, Notepad++, etc.).
2. **SmartShift Ratchet & Free-spin Desync Fix**: Automatically routes discrete notched movements directly to the native rigid scroll path during tactile ratchet mode, while engaging high-resolution smooth scrolling only in free-spin mode.

---

## Quick Start (Recommended)

1. Download the pre-built **[`LogiOptionsPlus-SmoothScroll-Fix-v1.0.0.zip`](https://github.com/shenshunjesus/LogiOptionsPlus-SmoothScroll-Fix/releases/tag/v1.0.0)** from Releases and extract it.
2. **To Enable**: Double-click **`Install-Fix.bat`** (Done! Global smooth scrolling and user autostart are now active).
3. **To Uninstall**: Double-click **`Uninstall-Fix.bat`** (Cleanly restores factory default behavior).
4. **To Check Status**: Double-click **`Check-Status.bat`**.

> [!IMPORTANT]
> **No Administrator Privileges Required!**  
> `logioptionsplus_agent.exe` runs under normal user privileges. This fix operates completely in user space and **does NOT need UAC elevation**. Simply double-click without "Run as Administrator".

---

## Security & Transparency (Why Is It Safe?)

Running unknown `.exe` files on Windows requires caution. This project is built on strict security transparency principles:

1. **Non-Elevated Execution**: Never requests Administrator rights. It cannot modify system folders, install virtual drivers, or hook global OS APIs.
2. **100% Offline (Zero Network Connections)**: Contains no HTTP/Socket code, no telemetry, no analytics, and no background communication.
3. **Fully Open Source (<200 lines)**: The core implementation in [`src/LogiSmoothScroll.cs`](src/LogiSmoothScroll.cs) is under 200 lines of standard C# that anyone can audit in minutes.
4. **Build It Yourself Directly from Source**: If you prefer not to run pre-compiled binaries, clone the repo and execute:
   ```powershell
   powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action global
   ```
   The script invokes Microsoft's built-in .NET compiler (`csc.exe`) to compile the executable directly on your machine.
5. **Binary Checksum Verification (SHA-256)**:
   - `LogiSmoothScroll.exe` (v1.0.0):
     `735250A7ED081BC8525C9B4B00DBFA4EA4C621FAF097F5F616BC4CBB7BF9F167`

---

## Technical Engineering Report

For complete reverse engineering analysis and C++ diff proposals for the Logitech engineering team, see:
- [`docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md`](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md) ([繁體中文](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md) · [简体中文](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-CN.md) · [日本語](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.ja.md))

---

## License
MIT License
