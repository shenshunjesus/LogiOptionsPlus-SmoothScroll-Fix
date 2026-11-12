<div align="center">

# LogiOptionsPlus-SmoothScroll-Fix

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

</div>

針對 **Logitech Options+ (Windows)** 的純記憶體動態修補程式（In-Memory Patch），徹底修復 MX Master 系列滑鼠滾輪的兩大底層瑕疵：

1. **破除寫死白名單，全域啟用平滑捲動**：繞過 `logioptionsplus_agent.exe` 內部寫死的 4 款瀏覽器限制（`msedge.exe`、`chrome.exe`、`iexplore.exe`、`firefox.exe`），讓所有現代桌面應用程式（VS Code、ChatGPT 桌面版、Slack、Notepad++ 等）皆能原生享受高解析度平滑捲動。
2. **修復 SmartShift 物理鋸齒與畫面平滑的視覺觸覺衝突**：當滾輪處於物理刻度（Ratchet 棘輪）模式時，慢速滾動自動分流至原生剛性（Rigid）捲動路徑；只有在快速撥動觸發 SmartShift 或手動切換為無段（Free-spin）模式時，才啟動平滑捲動動畫。

---

## 核心特色

- **零磁碟修改（不破壞數位簽章）**：完全不修改硬碟上的 `.exe` 二進位檔案，100% 保留 Logitech Authenticode 官方簽章，絕不會觸發 IPC 簽章驗證失敗（避免出現 `Getting resources error` 或滾輪失效）。
- **無需系統管理員權限**：`logioptionsplus_agent.exe` 本身以一般登入使用者權限執行，修補程式以相同使用者層級呼叫 Win32 記憶體 API，無需 UAC 提權。
- **開機自動常駐守護**：內建極輕量守護程序，開機自動常駐背景，即使 Logi Options+ 重新啟動或自動更新，也能在 3 秒內自動完成 RAM 記憶體修補。

---

## 快速開始（最簡單推薦）

1. 前往 Releases 下載免編譯打包檔 **[`LogiOptionsPlus-SmoothScroll-Fix-v1.0.0.zip`](https://github.com/shenshunjesus/LogiOptionsPlus-SmoothScroll-Fix/releases/tag/v1.0.0)** 並解壓縮。
2. **一鍵啟用**：對著 `Install-Fix.bat` 點擊兩下（完成！全域平滑捲動與開機自啟動立即生效）。
3. **一鍵解除**：如果日後想還原原廠設定，對著 `Uninstall-Fix.bat` 點擊兩下即可乾淨移除。
4. **檢查狀態**：對著 `Check-Status.bat` 點擊兩下可隨時檢視目前常駐狀況。

---

## 進階：透過 PowerShell 原始碼執行

### 1. 啟用修補程式（全域平滑捲動 + SmartShift 棘輪修復）
開啟 PowerShell 並執行：
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action global
```

### 2. 檢查常駐狀態
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action status
```

### 3. 還原原廠預設狀態
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action restore
```

---

## 深度技術報告

欲了解完整的二進位逆向工程過程、底層架構分析，以及提供給羅技官方工程團隊的 C++ 修復建議，請參閱：
- [`docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md`](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md)（[English Report](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md)）

---

## 授權條款
本專案採用 [MIT License](LICENSE) 授權。
