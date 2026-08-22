# .NET DLL 混淆需求規格書

| 項目   | 內容         |
| ---- | ---------- |
| 撰寫人  | Developer  |
| 建立日期 | 2026-08-22 |
| 最後更新 | 2026-08-22 |

---

## 1. 目的

本文件定義 .NET 類別庫使用 **Obfuscar** 進行 DLL 程式碼混淆的規範。

主要目的：

* 降低 DLL 被反組譯後的可讀性。
* 增加第三方分析程式碼的難度。
* 混淆類別、方法、欄位等程式碼名稱。
* 避免直接透過反組譯工具即可輕易理解 DLL 的內部實作。
* 將 Obfuscar 納入正式 Release 建置流程。

> Obfuscar 的目的為「提高反組譯與程式碼分析的難度」，並非保證 DLL 無法被反組譯。

---

## 2. 安裝方式

Obfuscar 必須使用 **.NET Local Tool** 安裝。

### 2.1 建立 Tool Manifest

如果專案尚未建立 Local Tool Manifest：

```bash
dotnet new tool-manifest
```

執行後會產生：

```text
.config/
└── dotnet-tools.json
```

### 2.2 安裝 Obfuscar

執行：

```bash
dotnet tool install Obfuscar.GlobalTool
```

### 2.3 禁止使用 Global Tool

不得使用：

```bash
dotnet tool install --global Obfuscar.GlobalTool
```

Obfuscar 必須由 Repository 的 `.config/dotnet-tools.json` 管理。

---

## 3. Tool 版本管理

`.config/dotnet-tools.json` 必須納入 Git Repository。

目的：

* 確保所有開發人員使用相同版本。
* 確保 CI/CD 使用相同版本。
* 避免因不同 Obfuscar 版本造成混淆結果不同。

在新的開發環境或 CI/CD 環境中，可以使用：

```bash
dotnet tool restore
```

還原 Repository 所指定的 Local Tool。

---

## 4. 執行方式

Obfuscar 必須透過 Local Tool 執行。

例如：

```bash
dotnet tool run obfuscar.console Obfuscar.xml
```

實際命令名稱應以目前安裝的 `Obfuscar.GlobalTool` 所提供的 Tool Command 為準。

不得要求開發人員在系統環境中另外安裝 Global Tool。

---

## 5. 混淆設定檔

專案必須提供：

```text
Obfuscar.xml
```

作為 Obfuscar 的設定檔。

基本範例：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Obfuscator>

  <Var name="InPath" value="bin\\Release\\net10.0" />

  <Var name="OutPath" value="bin\\Release\\obfuscated" />

  <Module file="$(InPath)\\AesCryptoLib.dll">
  </Module>

</Obfuscator>
```

實際 `Target Framework`、DLL 名稱及輸出目錄應依專案實際設定調整。

---

## 6. 驗收標準

### Obfuscar 安裝

* [x] 執行 `dotnet new tool-manifest` 可以建立 Tool Manifest。
* [x] 執行 `dotnet tool install Obfuscar.GlobalTool` 可以完成安裝。
* [ ] Obfuscar 不得安裝為 Global Tool。
* [x] `.config/dotnet-tools.json` 已納入 Git Repository。
* [x] 新環境可以透過 `dotnet tool restore` 還原 Obfuscar。
* [x] 可以在 Aes256CryptoLib 類別庫目錄下使用 `dotnet tool run obfuscar.console .\\Obfuscar.xml` 進行 DLL 混淆。

---

## 7. 相關檔案

```text
.config/
└── dotnet-tools.json

Obfuscar.xml
```

其中：

| 檔案                          | 用途                         |
| --------------------------- | -------------------------- |
| `.config/dotnet-tools.json` | 管理 Obfuscar Local Tool 及版本 |
| `Obfuscar.xml`              | 定義 DLL 混淆規則                |
| `Obfuscar.md`               | Obfuscar 安裝、設定、建置及安全規範     |


---

## AI 交談紀錄
**以下為與AI討論區塊**

2026-08-22: 已讀取 agent.md、.github/instruction.md、specs/Obfuscar.md 三份文件，並完成重點摘要。

2026-08-22: 等候使用者指示。請在下列選項中選擇一項以繼續：

- A: 在專案加入 .config/dotnet-tools.json 與 Obfuscar.xml 範例設定（由 AI 建立檔案並提交至工作區）。
- B: 撰寫 CI/Release 流程中納入 Obfuscar 的實作說明（提供步驟與範例 YAML/PowerShell 指令）。

備註：使用者已要求所有互動內容記錄於此區塊；後續每次互動將持續補記於此處。

2026-08-22: 使用者選擇 A。已執行下列動作：

- 已建立檔案： `.config/dotnet-tools.json`，內容包含 Obfuscar.GlobalTool 的版本與命令註冊。
- 已確認專案目錄 `Aes256CryptoLib/Obfuscar.xml` 已存在；其內容使用 Debug 路徑（`.\bin\Debug\net10.0\`）作為預設 InPath，並指定輸出到 `.
bin\Debug\net10.0\obfuscated`。

驗證指令（於專案根目錄或 CI 中執行）：

1. 還原 local tool：

```powershell
dotnet tool restore
```

2. 切換至 Aes256CryptoLib 專案資料夾，執行混淆（Release build 範例）：

```powershell
cd Aes256CryptoLib
dotnet build -c Release
dotnet tool run obfuscar.console .\Obfuscar.xml
```

說明：若需在 Release 輸出中執行混淆，請更新 `Aes256CryptoLib\Obfuscar.xml` 中的 `InPath` 與 `OutPath` 為 `bin\\Release\\net10.0` 與適當輸出資料夾。

請確認是否要我：

- 1) 把 `.config/dotnet-tools.json` 新增到 Git (commit & push)，或
- 2) 同步更新 `Aes256CryptoLib\Obfuscar.xml` 使其預設使用 Release 輸出路徑，或
- 3) 產生 CI 範例腳本（GitHub Actions / Azure Pipelines）。

使用者回答: 同步更新 `Aes256CryptoLib\Obfuscar.xml` 使其預設使用 Release 輸出路徑

2026-08-22: 已執行並完成：已更新 `Aes256CryptoLib/Obfuscar.xml`，將 InPath 改為 `.\\bin\\Release\\net10.0\\`，OutPath 改為 `.\\bin\\Release\\net10.0\\obfuscated`。

驗證建議（於專案根目錄執行）：

```powershell
dotnet tool restore
cd Aes256CryptoLib
dotnet build -c Release
dotnet tool run obfuscar.console .\\Obfuscar.xml

