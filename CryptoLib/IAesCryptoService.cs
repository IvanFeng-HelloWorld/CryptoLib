namespace CryptoLib;

/// <summary>
/// AES-256 加解密服務介面
/// </summary>
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
