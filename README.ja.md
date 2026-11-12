<div align="center">

# LogiOptionsPlus-SmoothScroll-Fix

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

</div>

**Logitech Options+ (Windows)** のスクロールエンジンに存在する 2 つの深刻な不具合を解消する、インメモリ（RAM）動的パッチです：

1. **スムーズスクロールの全アプリ解放**：`logioptionsplus_agent.exe` 内部でハードコードされている 4 つのブラウザ（`msedge.exe`、`chrome.exe`、`iexplore.exe`、`firefox.exe`）の制限を回避し、VS Code、ChatGPT デスクトップアプリ、Slack、Notepad++ など、すべてのモダンデスクトップアプリでスムーズスクロールを有効化します。
2. **SmartShift ラチェット（クリック感）モードの操作感の不一致を修正**：ホイールが物理的なラチェット（ノッチ）状態のときは、画面の不要な慣性アニメーションをバイパスして即座にカチカチと動く剛性スクロール（Rigid Scroll）に切り替えます。SmartShift で勢いよく回した際やフリースピンモード時のみ、高解像度のスムーズスクロールが発動します。

---

## 主な特徴

- **ディスクファイル改変ゼロ（デジタル署名を完全維持）**：SSD/HDD 上の `.exe` バイナリを一切書き換えないため、Logitech の Authenticode 署名が壊れず、IPC 認証エラー（`Getting resources error` やホイール無反応）が発生しません。
- **管理者権限（UAC）不要**：`logioptionsplus_agent.exe` は一般ユーザー権限で動作しているため、本パッチも管理者権限なしで適用可能です。
- **スタートアップ自動常駐対応**：極めて軽量な常駐プロセス（メモリ使用量約 5MB、CPU 0%）が付属しており、Windows 起動時や Logi Options+ の自動更新時にも 3 秒以内に自動でパッチを再適用します。

---

## クイックスタート（おすすめ・最も簡単）

1. Releases からビルド済みパッケージ **[`LogiOptionsPlus-SmoothScroll-Fix-v1.0.0.zip`](https://github.com/shenshunjesus/LogiOptionsPlus-SmoothScroll-Fix/releases/tag/v1.0.0)** をダウンロードして展開（解凍）します。
2. **有効化**：`Install-Fix.bat` をダブルクリックします（完了！スムーズスクロールとスタートアップ自動常駐が即座に有効化されます）。
3. **アンインストール（解除）**：公式デフォルトの動作に戻したい場合は、`Uninstall-Fix.bat` をダブルクリックするだけで完全に復元されます。
4. **ステータス確認**：`Check-Status.bat` をダブルクリックすると、現在のパッチ適用状態を確認できます。

---

## 開発者向け：PowerShell から直接実行する場合

### 1. パッチの適用（全アプリ有効化 ＋ SmartShift 修正）
PowerShell を開き、次のコマンドを実行します：
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action global
```

### 2. 動作ステータスの確認
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action status
```

### 3. 公式デフォルト状態への復元
```powershell
powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action restore
```

---

## 技術レポート（エンジニアリング詳細）

バイナリ逆アセンブル解析、内部データフロー、および Logitech 開発チーム向けの C++ 修正提案の詳細は、以下をご参照ください：
- [`docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md`](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md) ([繁體中文版レポート](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md))

---

## ライセンス
本プロジェクトは [MIT License](LICENSE) のもとで公開されています。
