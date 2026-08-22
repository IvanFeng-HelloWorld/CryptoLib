using System;
using System.Security.Cryptography;
using System.Text;

namespace CryptoLib;

/// <summary>
/// AES-256/CBC/PKCS7 加解密實作。
/// 注意：預設使用 private static 欄位作為 key/iv 的 placeholder。請在編譯前以安全方式替換為實際值。
/// </summary>
public class AesCryptoService : IAesCryptoService
{
    // TODO: 請在打包為 DLL 前，將下列預設值替換為專案專屬的 Key 與 IV。為安全起見，不應將實際金鑰上傳到原始碼倉庫。
    private static readonly string DefaultKeyString = "REPLACE_WITH_32_BYTE_KEY________________"; // 32 chars placeholder

    private static readonly string DefaultIVString = "REPLACE_WITH_16B_IV__"; // 16 chars placeholder

    private readonly byte[] _keyBytes;
    private readonly byte[] _ivBytes;

    /// <summary>
    /// 預設建構式：使用類別內靜態預設 Key/IV（請在專案中替換）
    /// </summary>
    public AesCryptoService()
    {
        _keyBytes = GetBytesOrNull(DefaultKeyString);
        _ivBytes = GetBytesOrNull(DefaultIVString);
    }

    /// <summary>
    /// 可選的建構式：允許注入 key/iv 字串以利測試或特殊使用情境。
    /// 接受"單純的字串"（使用 UTF8 bytes），呼叫端需確保長度符合規範。
    /// </summary>
    public AesCryptoService(string keyString, string ivString)
    {
        _keyBytes = GetBytesOrNull(keyString);
        _ivBytes = GetBytesOrNull(ivString);
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        try
        {
            ValidateKeyIv(_keyBytes, _ivBytes);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _keyBytes!;
            aes.IV = _ivBytes!;

            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            return Convert.ToBase64String(cipherBytes);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AesCryptoService.Encrypt error: {ex.Message}");
            return plainText;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        try
        {
            ValidateKeyIv(_keyBytes, _ivBytes);

            var cipherBytes = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.Key = _keyBytes!;
            aes.IV = _ivBytes!;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (FormatException fex)
        {
            // 非 Base64 格式
            Console.WriteLine($"AesCryptoService.Decrypt format error: {fex.Message}");
            return cipherText;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AesCryptoService.Decrypt error: {ex.Message}");
            return cipherText;
        }
    }

    private static byte[]? GetBytesOrNull(string? s)
    {
        if (string.IsNullOrEmpty(s))
            return null;
        return Encoding.UTF8.GetBytes(s);
    }

    private static void ValidateKeyIv(byte[]? key, byte[]? iv)
    {
        if (key is null || iv is null)
            throw new InvalidOperationException("Key or IV is not set.");
        if (key.Length != 32)
            throw new InvalidOperationException($"Key length invalid. Expected 32 bytes, actual: {key.Length}.");
        if (iv.Length != 16)
            throw new InvalidOperationException($"IV length invalid. Expected 16 bytes, actual: {iv.Length}.");
    }
}