# AES256 加解密 .NET 類別庫 (Class Library) 需求規格書

| 項目 | 內容 |
|---|---|
| 撰寫人 | Developer |
| 建立日期 | 2026-08-22 |
| 最後更新 | 2026-08-22 |

---

## 1. 背景與目標

**要解決的問題**
各專案需要儲存或傳輸敏感的 API 金鑰字串，若以明文處理存在資安風險，若由各專案重複實作加解密容易造成邏輯不一。

**目標（Goal）**
- [ ] 開發可打包為 DLL 的 .NET 類別庫，供多專案共用 AES-256 加解密邏輯。
- [ ] 支援各專案開發前，在這個類別庫設定專屬的 Key 與 IV 並打包成 DLL 放到其他專案中做加解密使用。
- [ ] 實作防護機制：加解密失敗時**不報錯/不拋出 Exception**，僅將錯誤列印在 Console 中，並直接回傳原始輸入字串。
- [ ] 加密後的資料統一輸出為 Base64 字串格式。

**非目標（Out of Scope）**
- [ ] 不提供非對稱加密（如 RSA）或 AES 以外的加密演算法。

---

## 2. 名詞定義

| 名詞 | 定義 |
|---|---|
| AES-256 | 進階加密標準（Advanced Encryption Standard），使用 256 位元（32 位元組）金鑰的對稱式加密演算法。 |
| Key (金鑰) | 用於 AES-256 加解密的 32 位元組秘密金鑰。 |
| IV (初始向量) | 用於 AES 加密模式（如 CBC）的 16 位元組隨機向量，確保相同的明文產生不同的密文。 |
| Base64 | 將加密二進位結果轉換為可列印文字字串的編碼方式。 |

---

## 3. 使用者情境（User Story）

**As a** 後端開發人員  
**I want to** 設定專案專屬的 Key 與 IV ，打包成 DLL 檔後，供其他專案做加解密使用 
**So that** 可以安全且一致地處理敏感 API 金鑰，且當輸入非合法密文時系統能容錯降級（Fallback），避免系統崩潰  

**情境範例（Given / When / Then）**
- Given 其他專案開發人員
  When 設定專案專屬的 Key 與 IV 並打包
  Then 產出專案專屬的加解密 dll 工具
- Given 其他專案開發人員
  When 傳入明文 API 金鑰執行加密
  Then 回傳 Base64 編碼的加密字串
- Given 傳入損毀的密文或不相符的 Key/IV 執行解密
  When 解密過程發生異常或失敗
  Then 靜默攔截例外並直接回傳傳入的原始字串

---

## 4. 功能流程

**主流程（Happy Path）**
1. 其他專案開發人員於類別庫中設定專屬的 Key 與 IV。
2. 打包成 DLL 檔，放入正在開發的專案中
3. 呼叫 `Encrypt(plainText)` 傳入 API 金鑰明文。
4. 類別庫透過 AES-256 (CBC 模式 / PKCS7 填補) 進行加密，輸出 Base64 字串。
5. 呼叫 `Decrypt(cipherText)` 傳入 Base64 密文字串，類別庫解密後回傳原始 API 金鑰明文。

**例外流程 / Edge Case**

| 情境 | 處理方式 |
|---|---|
| 輸入字串為 `null` 或空字串 | 不執行加解密，直接回傳傳入的 `null` 或空字串 |
| 解密失敗（密碼錯誤、格式非合法 Base64 等） | 內部 Catch 所有人為與密碼學 Exception，不拋出錯誤，直接回傳輸入的原字串 |
| Key 或 IV 未設定或長度不符合規範 | 執行時捕捉 Exception 並記錄 internal log，直接回傳原始輸入字串 |

---

## 5. 資料規格

| 欄位名稱 | 型別 | 必填 | 預設值 | 驗證規則 | 說明 |
|---|---|---|---|---|---|
| Key | `string` / `byte[]` | 是 | 無 | 必須為 256-bit (32 bytes) | 各專案專屬加密金鑰 |
| IV | `string` / `byte[]` | 是 | 無 | 必須為 128-bit (16 bytes) | 加密初始向量 |
| inputText | `string` | 否 | null | 無 | 待加密或解密的目標字串 |

**資料來源與流向**
> 其他專案放入加解密 DLL -> 傳入目標 API 金鑰字串 -> 類別庫處理 -> 回傳加密 Base64 或解密明文（失敗時回傳原字串）。

---

## 6. 介面規格

**API / 程式介面規格**

```csharp
public interface IAesCryptoService
{
    /// <summary>
    /// 將明文字串進行 AES-256 加密
    /// </summary>
    /// <param name="plainText">待加密的 API 金鑰明文</param>
    /// <returns>加密後的 Base64 字串；若失敗則回傳原始 plainText</returns>
    string Encrypt(string plainText);

    /// <summary>
    /// 將 Base64 密文字串進行 AES-256 解密
    /// </summary>
    /// <param name="cipherText">待解密的 Base64 字串</param>
    /// <returns>解密後的明文字串；若失敗則回傳原始 cipherText</returns>
    string Decrypt(string cipherText);
}
```

---

## 7. 商業規則

| 條件 | 結果 |
|---|---|
| 加解密過程發生任何 Exception（如 `CryptographicException`, `FormatException`） | 攔截 Exception，僅將錯誤列印在 Console 中，回傳輸入的原始字串，不得讓呼叫端程式崩潰 |
| 加密輸出 | 必須轉碼為 Base64 字串，確保 API 傳輸與資料庫寫入相容性 |
| 金鑰規格 | 金鑰必須為 256 位元 (32 bytes)，向量必須為 128 位元 (16 bytes) |

---

## 8. 驗收標準（Acceptance Criteria）

- [ ] 執行 `Encrypt`，能取得正確的 Base64 加密結果。
- [ ] 傳入加密 Base64 字串執行 `Decrypt`，能正確還原原始 API 金鑰字串。
- [ ] 傳入無效字串（如未加密明文）執行 `Decrypt`，元件**不拋出 Exception** ，僅將錯誤列印在 Console 中，且回傳傳入的原始字串。
- [ ] 傳入 `null` 或 `string.Empty` 時，回傳 `null` 或 `string.Empty` 且不報錯。
- [ ] 類別庫可順利編譯為 DLL 並在其他 .NET 專案中使用。

---

## AI 交談紀錄
**以下為與AI討論區塊**
```
範例:
AI: 以下為需確認的設計選項，請在此檔案內直接回覆（輸入選項編號或文字）。回覆後我會根據選擇產出實作規劃與要修改的檔案清單。
1) Key / IV 的設定方式（擇一或多選）
   - a) 建構式 (於實例化時傳入)
   - b) 提供屬性或 SetKey/SetIV 方法 (執行時可變更)
   - c) 支援 IOptions / DI 配置
   - d) 支援靜態全域設定

- 回答:提供 private 屬性給開發人員自己做設定，直接寫死在專案中
```