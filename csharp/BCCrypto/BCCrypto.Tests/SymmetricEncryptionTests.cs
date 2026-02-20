using BlockchainCommons.BCCrypto;
using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCCrypto.Tests;

public class SymmetricEncryptionTests
{
    private static readonly byte[] Plaintext = "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it."u8.ToArray();
    private static readonly byte[] Aad = Convert.FromHexString("50515253c0c1c2c3c4c5c6c7");
    private static readonly byte[] Key = Convert.FromHexString(
        "808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f");
    private static readonly byte[] Nonce = Convert.FromHexString("070000004041424344454647");
    private static readonly byte[] ExpectedCiphertext = Convert.FromHexString(
        "d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116");
    private static readonly byte[] ExpectedAuth = Convert.FromHexString("1ae10b594f09e26a7e902ecbd0600691");

    [Fact]
    public void TestRfcTestVector()
    {
        var (ciphertext, auth) = SymmetricEncryption.AeadChaCha20Poly1305EncryptWithAad(
            Plaintext, Key, Nonce, Aad);
        Assert.Equal(ExpectedCiphertext, ciphertext);
        Assert.Equal(ExpectedAuth, auth);

        byte[] decrypted = SymmetricEncryption.AeadChaCha20Poly1305DecryptWithAad(
            ciphertext, Key, Nonce, Aad, auth);
        Assert.Equal(Plaintext, decrypted);
    }

    [Fact]
    public void TestRandomKeyAndNonce()
    {
        var rng = new SecureRandomNumberGenerator();
        byte[] key = rng.RandomData(32);
        byte[] nonce = rng.RandomData(12);
        var (ciphertext, auth) = SymmetricEncryption.AeadChaCha20Poly1305EncryptWithAad(
            Plaintext, key, nonce, Aad);
        byte[] decrypted = SymmetricEncryption.AeadChaCha20Poly1305DecryptWithAad(
            ciphertext, key, nonce, Aad, auth);
        Assert.Equal(Plaintext, decrypted);
    }

    [Fact]
    public void TestEmptyData()
    {
        var rng = new SecureRandomNumberGenerator();
        byte[] key = rng.RandomData(32);
        byte[] nonce = rng.RandomData(12);
        var (ciphertext, auth) = SymmetricEncryption.AeadChaCha20Poly1305EncryptWithAad(
            Array.Empty<byte>(), key, nonce, Array.Empty<byte>());
        byte[] decrypted = SymmetricEncryption.AeadChaCha20Poly1305DecryptWithAad(
            ciphertext, key, nonce, Array.Empty<byte>(), auth);
        Assert.Empty(decrypted);
    }
}
