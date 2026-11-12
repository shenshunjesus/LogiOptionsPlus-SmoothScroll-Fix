<div align="center">

# LogiOptionsPlus-SmoothScroll-Fix

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

</div>

針對 **Logitech Options+ (Windows)** 的純記憶體動態修補程式（In-Memory Patch），徹底修復 MX Master 系列滑鼠滾輪的兩大底層瑕疵：

1. **破除寫死白名單，全域啟用平滑捲動**：繞過 `logioptionsplus_agent.exe` 內部寫死的 4 款瀏覽器限制（`msedge.exe`、`chrome.exe`、`iexplore.exe`、`firefox.exe`），讓所有現代桌面應用程式（VS Code、ChatGPT 桌面版、Slack、Notepad++ 等）皆能原生享受高解析度平滑捲動。
2. **修復 SmartShift 物理鋸齒與畫面平滑的視覺觸覺衝突**：當滾輪處於物理刻度（Ratchet 棘輪）模式時，慢速滾動自動分流至原生剛性（Rigid）捲動路徑；只有在快速撥動觸發 SmartShift 或手動切換為無段（Free-spin）模式時，才啟動平滑捲動動畫。

---

## 快速開始（最簡單推薦）

1. 前往 Releases 下載免編譯打包檔 **[`LogiOptionsPlus-SmoothScroll-Fix-v1.0.0.zip`](https://github.com/shenshunjesus/LogiOptionsPlus-SmoothScroll-Fix/releases/tag/v1.0.0)** 並解壓縮。
2. **一鍵啟用**：對著 **`Install-Fix.bat`** 點擊兩下。
3. **一鍵解除**：對著 **`Uninstall-Fix.bat`** 點擊兩下。
4. **檢查狀態**：對著 **`Check-Status.bat`** 點擊兩下。

> [!IMPORTANT]
> **完全不需要「以系統管理員身分執行」！**
> `logioptionsplus_agent.exe` 本身是以一般使用者身分執行，本工具**只需一般使用者權限**即可正常運作。請直接點擊兩下執行，切勿使用管理員權限。

---

## 安全性與透明度（為什麼您可以放心執行？）

在 Windows 上執行第三方的 `.exe` 程式確實需要謹慎。本專案採取最嚴格的透明度設計，以確保使用者安全：

1. **絕對不需要系統管理員權限（Non-Elevated）**：
   - 本程式不需要 UAC 提權，不會修改系統目錄、不會安裝任何虛擬驅動程式、亦不會做任何系統層級的 Hook。
2. **零網路連線（100% Offline）**：
   - 原始碼中完全不包含任何 HTTP / Socket 連網代碼，沒有任何遙測（Telemetry）、資料回傳或背景通訊。
3. **開源可審查（不到 200 行代碼）**：
   - 核心邏輯僅在 [`src/LogiSmoothScroll.cs`](src/LogiSmoothScroll.cs) 之中，任何人都能在 3 分鐘內逐行審核完畢。
4. **若您對下載的 `.exe` 有疑慮，可直接從本機原始碼現場編譯**：
   - 您完全不需要使用 zip 內附的 `.exe`！只要 clone 本專案並在 PowerShell 執行：
     ```powershell
     powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action global
     ```
   - 腳本會直接呼叫微軟官方內建的 C# 編譯器（`csc.exe`）在您電腦中現場編譯，確保執行檔 100% 來自您審查過的源碼。
5. **官方二進位檔案雜湊比對（SHA-256）**：
   - `LogiSmoothScroll.exe` (v1.0.0):
     `735250A7ED081BC8525C9B4B00DBFA4EA4C621FAF097F5F616BC4CBB7BF9F167`

---

## 深度技術報告

欲了解完整的二進位逆向工程過程、底層架構分析，以及提供給羅技官方工程團隊的 C++ 修復建議，請參閱：
- [深度技術報告（正體中文）](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md)（[English](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md) · [简体中文](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-CN.md) · [日本語](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.ja.md)）

---

## 授權條款
本專案採用 [MIT License](LICENSE) 授權。
