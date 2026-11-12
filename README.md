<div align="center">

# LogiOptionsPlus-SmoothScroll-Fix

[English](README.md) · [繁體中文](README.zh-TW.md)

</div>

An in-memory runtime patch for **Logitech Options+ (Windows)** that fixes two long-standing scroll engine issues on MX Master series mice:

1. **Global Smooth Scrolling**: Bypasses the hardcoded 4-browser whitelist (`msedge.exe`, `chrome.exe`, `iexplore.exe`, `firefox.exe`) in `logioptionsplus_agent.exe`, enabling high-precision smooth scrolling globally across all desktop apps (VS Code, ChatGPT Desktop, Slack, Notepad++, etc.).
2. **SmartShift Ratchet & Free-spin Desync Fix**: Automatically routes discrete notched movements directly to the native rigid scroll path during tactile ratchet mode, while engaging high-resolution smooth scrolling only in free-spin mode.

---

## Key Features

- **Zero Disk Modification**: Does not patch or alter binaries on disk. Logitech Authenticode digital signatures remain 100% intact, preventing IPC authentication errors (`Getting resources error`).
- **No Administrator Privileges Required**: Patches user-space agent pages directly via standard Win32 memory APIs.
- **Auto-Persistent**: Includes a lightweight background daemon that automatically applies the memory patch when Logi Options+ starts or updates.

---

## Quick Start

### 1. Enable Global Fix
Open PowerShell and run:
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action global
```

### 2. Check Daemon Status
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action status
```

### 3. Restore Default Behavior
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action restore
```

---

## Technical Details

For complete reverse engineering analysis and C++ diff proposals for the Logitech engineering team, see:
- [`docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md`](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md)

---

## License
[MIT License](LICENSE)
