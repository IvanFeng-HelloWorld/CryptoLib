---
applyTo: '**/*.cs, **/*.csproj'
description: '.NET 10 專案開發基礎指引（Base Instruction）'
---

# C# / .NET 10 開發指引（Base Instruction）

> 本文件為**所有專案共用的基礎指引**，套用於任何 `.cs` / `.csproj` 檔案。
> 特定 SDK（如 GitHub Copilot SDK）的專屬用法請參考文件末尾的附錄，或該 SDK 的獨立指引檔。

## 核心原則

- 一律使用 **.NET 10** 與 **C# 14**，除非專案 `global.json` / TFM 另有指定。
- 全面採用 async/await 模式;涉及非同步資源清理時實作 `IAsyncDisposable`。
- 每個函式撰寫清楚簡潔的註解,說明**為什麼**這樣設計,而非重述程式碼在做什麼。
- 審查程式碼變更時,只提出高信心度的建議。
- 妥善處理邊界情況,撰寫清楚的例外處理邏輯。
- 使用外部套件或函式庫時,於註解中說明其用途。
 
---

## 命名規範

- 元件名稱、方法名稱、public 成員採用 **PascalCase**。
- private 欄位與區域變數採用 **camelCase**。
- 介面名稱加上 `I` 前綴(例如 `IUserRepository`)。
- 命名風格需保持一致;一旦選定某種命名模式,全專案沿用同一種。

---

## 格式規範

- 套用 `.editorconfig` 中定義的程式碼格式化風格。
- 優先使用 file-scoped 命名空間宣告與單行 using 指示詞。
- 任何程式區塊(`if`、`for`、`while`、`foreach`、`using`、`try` 等)的左大括號前需換行。
- 方法的最後一個 `return` 陳述式需獨立成一行。
- 盡可能使用 pattern matching 與 switch expression。
- 參照成員名稱時使用 `nameof`,不要使用字串字面值。
- 所有 public API 都需要 XML doc 註解;適用時附上 `<example>` 與 `<code>` 說明。

---

## Nullable Reference Types

- 變數宣告為 non-nullable,並在方法進入點檢查 `null`。
- 使用 `is null` / `is not null`,不使用 `== null` / `!= null`。
- 信任型別系統的 null 標註;型別系統已保證非 null 時,不要多餘地重複檢查。

---

## 驗證與例外處理

- 使用 Data Annotations 或 FluentValidation 進行輸入驗證(Application 層 DTO)。
- 領域規則驗證放在 Domain 層(Entity/Aggregate 建構或行為方法內),違反規則時拋出明確的領域例外(例如 `DomainException` 的衍生類別),不要用泛用的 `Exception`。
- 使用中介軟體(middleware)實作全域例外處理,統一轉換為一致的錯誤回應格式。
- 採用 RFC 9457(Problem Details)格式回傳標準化錯誤回應。
- 不可靜默吞例外;需記錄後重新拋出,或讓例外自然往上傳遞。

---

## 驗證(Authentication)與授權(Authorization)

- 使用 JWT Bearer Token 實作身分驗證。
- 依需求說明 OAuth 2.0 / OpenID Connect 在 ASP.NET Core 中的應用。
- 實作角色型(role-based)與政策型(policy-based)授權。
- 視需要整合 Microsoft Entra ID(原 Azure AD)。
- Controller 型與 Minimal API 的安全機制需保持一致。

---

## API 版本管理與文件

- 明確規劃 API 版本策略,並套用至 Controller 型與 Minimal API。
- 使用 Swagger / OpenAPI 產生文件,清楚描述端點、參數、回應與驗證方式。
- 撰寫對 API 使用者有實質幫助的文件說明,而非僅自動產生的樣板文字。

---

## 日誌與監控

本專案統一使用 **NLog** 作為日誌套件。

### NLog 使用規範

- 安裝套件:`NLog.Web.AspNetCore`(ASP.NET Core 專案)或 `NLog.Extensions.Logging`(一般 .NET 專案),並以 NLog 取代預設的 `Microsoft.Extensions.Logging` provider。
- 於 `Program.cs` 註冊 NLog 作為 Logging Provider:
  ```csharp
  builder.Logging.ClearProviders();
  builder.Host.UseNLog();
  ```
- 全域以 `nlog.config`(或 `NLogLoggingConfiguration`)設定 log target 與 rule,不將設定寫死於程式碼中,以利不同環境(Development / Staging / Production)調整 log level 與輸出格式。
- 程式中一律透過 DI 注入 `Microsoft.Extensions.Logging.ILogger<T>`(由 NLog 提供實作),不要直接 `new` NLog 的 `Logger` 實例,以維持與 .NET 標準日誌抽象一致、方便測試時替換。
- 使用結構化日誌(structured logging)寫法,透過訊息範本帶入參數,而非用字串串接:
  ```csharp
  _logger.LogInformation("Order {OrderId} submitted by {UserId}", order.Id, userId);
  ```
- 依日誌等級(Trace/Debug/Information/Warning/Error/Critical)正確分類訊息,正式環境預設等級建議為 `Information` 以上,除錯時可於設定檔調整為 `Debug`/`Trace`。
- 為請求加上 correlation ID(可透過 NLog 的 `MappedDiagnosticsLogicalContext` / `ScopeContext`,或搭配 ASP.NET Core 的 `TraceIdentifier`),便於追蹤跨服務呼叫。

### 輸出至Console日誌(Host Log)

- **Log 輸出目標統一為Console日誌**:NLog target 設定為 **Console target**,將日誌印至 **stdout/stderr**,不要寫入容器內部檔案系統。
  ```xml
  <targets>
    <target xsi:type="Console" name="console"
            layout="${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}" />
  </targets>
  <rules>
    <logger name="*" minlevel="Info" writeTo="console" />
  </rules>
  ```
- 這樣的輸出方式與前述「容器化」章節一致:交由 **Docker logging driver** 或 **Kubernetes(kubelet + 容器執行環境)** 收集容器的 stdout/stderr,再視需要轉送至 Fluentd/Loki/ELK 等集中式日誌平台,應用程式本身不負責日誌落地與輪替(log rotation)。
- 正式環境建議搭配 **JSON layout**(NLog 的 `JsonLayout`),方便主機端日誌系統(如 Fluent Bit、Loki)解析結構化欄位;開發環境可用可讀性較高的純文字 layout。
- 若同時需要本機除錯用的檔案日誌(File target),僅限本機開發環境啟用,正式環境部署至 Docker/K8s 時一律以 Console target 為主,避免日誌寫入容器內部造成磁碟膨脹或隨 Pod 重建而遺失。
- 視需要整合 Application Insights、OpenTelemetry 或其他遙測工具(可透過 NLog 的對應 target 或搭配 `Microsoft.Extensions.Diagnostics` 匯出),作為 stdout 日誌之外的額外可觀測性管道。

---

## 測試:NUnit + Given-When-Then

本專案測試框架統一使用 **NUnit**,測試撰寫風格採用 **Given-When-Then**(行為驅動,BDD 風格),取代傳統 AAA 註解。

### 測試專案結構

- 獨立測試專案,命名為 **`{ProjectName}.Tests`**,依分層再細分(例如 `{ProjectName}.Domain.Tests`、`{ProjectName}.Application.Tests`)。
- 測試類別對應被測類別:`Order` → `OrderTests`。
- 每個測試只驗證一個行為,不使用多重斷言(如需驗證多個結果,拆成多個測試或使用 NUnit 的 `Assert.Multiple`)。

### 命名慣例(Given-When-Then)

測試方法名稱需清楚表達「情境 → 動作 → 預期結果」,建議格式:

```
Given_{前置情境}_When_{執行動作}_Then_{預期結果}
```

範例:

```csharp
[Test]
public void Given_OrderWithNoItems_When_Submitted_Then_ThrowsInvalidOperationException()
{
    // Given
    var order = OrderFactory.CreateEmpty();

    // When
    Action act = () => order.Submit();

    // Then
    Assert.Throws<InvalidOperationException>(() => act());
}
```

- 以 `// Given` / `// When` / `// Then` 取代 `// Arrange` / `// Act` / `// Assert` 註解,對應關係為 Given = Arrange、When = Act、Then = Assert。
- 若測試情境較複雜,可用巢狀 `[TestFixture]` 依「Given」情境分組,內含多個「When/Then」測試,例如:

```csharp
[TestFixture]
public class Given_ExistingOrder
{
    [TestFixture]
    public class When_ItemIsAdded : Given_ExistingOrder
    {
        [Test]
        public void Then_TotalPriceIncreases() { /* ... */ }
    }

    [TestFixture]
    public class When_OrderIsCancelled : Given_ExistingOrder
    {
        [Test]
        public void Then_StatusIsSetToCancelled() { /* ... */ }
    }
}
```

### NUnit 使用規範

- 套件:`Microsoft.NET.Test.Sdk`、`NUnit`、`NUnit3TestAdapter`。
- 測試類別加上 `[TestFixture]`,測試方法加上 `[Test]`。
- 參數化測試(多組 Given 輸入對應不同 Then 結果)使用 `[TestCase]`。
- 設定與清理邏輯使用 `[SetUp]` / `[TearDown]`(對應每個 Given 的共同前置準備)。
- 若已採用 FluentAssertions 或 AwesomeAssertions,優先使用其斷言語法以提升可讀性;否則使用 NUnit 內建 `Assert` / `Assert.That`。
- 例外驗證使用 `Assert.Throws<T>()` 或 `Assert.ThrowsAsync<T>()`。

### 一般測試原則

- 遵循專案既有命名慣例(若既有測試已用其他風格,先與團隊確認是否統一改為 Given-When-Then)。
- 避免使用 Unicode 特殊符號。
- 測試需能任意順序或平行執行,互不影響。
- 避免磁碟 I/O;若必要,路徑需隨機化、不清除產生的檔案,並記錄檔案位置。
- 透過 **public API** 測試,不因測試而放寬存取層級,避免使用 `InternalsVisibleTo`。
- 新增或變更的 **public API / Domain 行為** 須有對應測試。
- 斷言需驗證具體數值與邊界情況,不使用模糊斷言。
- **Mocking**:盡量避免使用 Mock;只可對外部相依(如 Infrastructure 層的外部服務)進行 mock,絕不 mock 屬於待測方案本身的 Domain 邏輯。Application 層測試可 mock Domain 層定義的 Repository 介面。
- 整合測試:針對 API 端點,搭配 `WebApplicationFactory` 進行整合測試,驗證跨層(Api → Application → Domain → Infrastructure)的實際行為。

### 執行測試

```bash
dotnet test
```

程式碼覆蓋率:

```bash
dotnet tool install -g dotnet-coverage
dotnet-coverage collect -f cobertura -o coverage.cobertura.xml dotnet test
```

---

## 效能優化

- 實作快取策略(In-Memory、Distributed Cache、Response Caching)。
- 全面採用非同步程式設計,避免 sync-over-async。
- 大型資料集提供分頁(pagination)、篩選(filtering)、排序(sorting)。
- 視需要實作壓縮(compression)等效能優化手段。
- 建立效能量測與 benchmark 機制,優化前先量測。

---