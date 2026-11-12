<div align="center">

# LogiOptionsPlus-SmoothScroll-Fix

[English](README.md) · [繁體中文](README.zh-TW.md) · [简体中文](README.zh-CN.md) · [日本語](README.ja.md)

</div>

**Logitech Options+ (Windows)** のスクロールエンジンに存在する 2 つの深刻な不具合を解消する、インメモリ（RAM）動的パッチです：

1. **スムーズスクロールの全アプリ解放**：`logioptionsplus_agent.exe` 内部でハードコードされている 4 つのブラウザ（`msedge.exe`、`chrome.exe`、`iexplore.exe`、`firefox.exe`）の制限を回避し、VS Code、ChatGPT デスクトップアプリ、Slack、Notepad++ など、すべてのモダンデスクトップアプリでスムーズスクロールを有効化します。
2. **SmartShift ラチェット（クリック感）モードの操作感の不一致を修正**：ホイールが物理的なラチェット（ノッチ）状態のときは、画面の不要な慣性アニメーションをバイパスして即座にカチカチと動く剛性スクロール（Rigid Scroll）に切り替えます。SmartShift で勢いよく回した際やフリースピンモード時のみ、高解像度のスムーズスクロールが発動します。

---

## クイックスタート（おすすめ・最も簡単）

1. Releases からビルド済みパッケージ **[`LogiOptionsPlus-SmoothScroll-Fix-v1.0.0.zip`](https://github.com/shenshunjesus/LogiOptionsPlus-SmoothScroll-Fix/releases/tag/v1.0.0)** をダウンロードして展開（解凍）します。
2. **有効化**：**`Install-Fix.bat`** をダブルクリックします。
3. **アンインストール**：**`Uninstall-Fix.bat`** をダブルクリックします。
4. **ステータス確認**：**`Check-Status.bat`** をダブルクリックします。

> [!IMPORTANT]
> **管理者権限（管理者として実行）は不要です！**  
> `logioptionsplus_agent.exe` は一般ユーザー権限で動作しているため、本ツールも**一般ユーザー権限のみ**で動作します。右クリックして「管理者として実行」を行わず、そのままダブルクリックしてください。

---

## セキュリティと透明性（なぜ安全なのか？）

Windows でサードパーティの `.exe` を実行する際は注意が必要です。本プロジェクトは完全な透明性に基づいて設計されています：

1. **管理者権限を要求しない**：UAC 昇格は不要であり、システムディレクトリの変更や仮想ドライバのインストールは行いません。
2. **完全オフライン（通信ゼロ）**：ソースコードにネットワーク通信コード（HTTP/Socket 等）は一切含まれておらず、外部へのデータ送信はありません。
3. **完全オープンソース（200行未満）**：中核ロジックは [`src/LogiSmoothScroll.cs`](src/LogiSmoothScroll.cs) にあり、数分でコード全体を監査可能です。
4. **ローカル環境でソースから直接ビルド可能**：配布バイナリの実行に不安がある場合は、リポジトリをクローンして PowerShell で以下を実行してください：
   ```powershell
   powershell -ExecutionPolicy Bypass -File ".\scripts\apply_patch.ps1" -Action global
   ```
   Windows 標準搭載の C# コンパイラ（`csc.exe`）によって、お使いの PC 上で直接ビルドされます。
5. **バイナリ検証ハッシュ（SHA-256）**：
   - `LogiSmoothScroll.exe` (v1.0.0):
     `735250A7ED081BC8525C9B4B00DBFA4EA4C621FAF097F5F616BC4CBB7BF9F167`

---

## 技術レポート（エンジニアリング詳細）

バイナリ逆アセンブル解析、内部データフロー、および Logitech 開発チーム向けの C++ 修正提案の詳細は、以下をご参照ください：
- [技術レポート（日本語）](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.ja.md)（[English](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.md) · [繁體中文](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-TW.md) · [简体中文](docs/Logi_Options_Plus_Smooth_Scrolling_Engineering_Report.zh-CN.md)）

---

## ライセンス
本プロジェクトは [MIT License](LICENSE) のもとで公開されています。
