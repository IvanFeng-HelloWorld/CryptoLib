# CryptoLib

簡介
--
CryptoLib 是一個針對 .NET 10 的輕量級類別庫，提供 AES-256 (CBC / PKCS7) 的加解密功能，方便多個專案共用敏感字串的加密邏輯。

快速開始
--
安裝／編譯：

dotnet build

執行測試：

dotnet test

使用範例
--
```csharp
using CryptoLib;

// 使用專案內預設的 Key/IV（請在打包前替換為專案專屬值）
var svc = new AesCryptoService();

// 或：以字串建構式注入（建議用於測試或從安全來源載入）
var svc2 = new AesCryptoService("01234567890123456789012345678901", "0123456789012345");

var cipher = svc2.Encrypt("my-secret-api-key");
var plain = svc2.Decrypt(cipher);
```

重要說明
--
- AesCryptoService 預設在程式碼內提供 placeholder 的 Key/IV（位於 CryptoLib/AesCryptoService.cs），請在打包為 DLL 前以安全方式替換為專案專屬的金鑰或改用建構式注入 / 環境變數 / DI (IOptions)。
- 根據 specs，當加解密發生任何錯誤時，函式會捕捉例外、以 Console.WriteLine 記錄錯誤，並回傳輸入的原始字串；不會拋出例外以避免呼叫端崩潰。

測試
--
測試專案位於 CryptoLib.Tests，包含基本的 Encrypt/Decrypt 與錯誤情境測試。

目標環境
--
- .NET 10
