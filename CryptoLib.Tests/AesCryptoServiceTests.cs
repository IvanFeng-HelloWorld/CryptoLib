using NUnit.Framework;
using System;

namespace CryptoLib.Tests;

[TestFixture]
public class Given_AesCryptoService
{
    private const string TestKey = "01234567890123456789012345678901"; // 32 chars -> 32 bytes in ASCII/UTF8
    private const string TestIv = "0123456789012345"; // 16 chars

    [Test]
    public void Given_ValidPlainText_When_EncryptedAndDecrypted_Then_ReturnsOriginal()
    {
        // Given
        var service = new AesCryptoService(TestKey, TestIv);
        var plain = "secret-api-key-123";

        // When
        var cipher = service.Encrypt(plain);
        var decrypted = service.Decrypt(cipher);

        // Then
        Assert.IsNotNull(cipher);
        Assert.AreNotEqual(plain, cipher);
        Assert.AreEqual(plain, decrypted);
    }

    [Test]
    public void Given_InvalidBase64_When_Decrypt_Then_ReturnsOriginalAndNotThrow()
    {
        // Given
        var service = new AesCryptoService(TestKey, TestIv);
        var input = "not-a-base64@@@";

        // When
        Action act = () => service.Decrypt(input);
        var result = service.Decrypt(input);

        // Then
        Assert.DoesNotThrow(act);
        Assert.AreEqual(input, result);
    }

    [Test]
    public void Given_NullOrEmpty_When_EncryptOrDecrypt_Then_ReturnsSame()
    {
        var service = new AesCryptoService(TestKey, TestIv);

        Assert.AreEqual(null, service.Encrypt(null!));
        Assert.AreEqual(string.Empty, service.Encrypt(string.Empty));

        Assert.AreEqual(null, service.Decrypt(null!));
        Assert.AreEqual(string.Empty, service.Decrypt(string.Empty));
    }
}