using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class EncryptedKeyTests
{
    private readonly byte[] _testSecret = Encoding.UTF8.GetBytes("correct horse battery staple");

    private SymmetricKey MakeContentKey() => SymmetricKey.New();

    [Fact]
    public void TestEncryptedKeyHkdfRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var contentKey = MakeContentKey();

        var encrypted = EncryptedKey.Lock(
            KeyDerivationMethod.HKDF, _testSecret, contentKey);
        Assert.Contains("HKDF", encrypted.ToString());
        Assert.Contains("SHA256", encrypted.ToString());

        var cbor = encrypted.TaggedCbor();
        var encrypted2 = EncryptedKey.FromTaggedCbor(cbor);
        var decrypted = encrypted2.Unlock(_testSecret);

        Assert.Equal(contentKey, decrypted);
    }

    [Fact]
    public void TestEncryptedKeyPbkdf2Roundtrip()
    {
        TagsRegistry.RegisterTags();
        var contentKey = MakeContentKey();

        var encrypted = EncryptedKey.Lock(
            KeyDerivationMethod.PBKDF2, _testSecret, contentKey);
        Assert.Contains("PBKDF2", encrypted.ToString());
        Assert.Contains("SHA256", encrypted.ToString());

        var cbor = encrypted.TaggedCbor();
        var encrypted2 = EncryptedKey.FromTaggedCbor(cbor);
        var decrypted = encrypted2.Unlock(_testSecret);

        Assert.Equal(contentKey, decrypted);
    }

    [Fact]
    public void TestEncryptedKeyScryptRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var contentKey = MakeContentKey();

        var encrypted = EncryptedKey.Lock(
            KeyDerivationMethod.Scrypt, _testSecret, contentKey);
        Assert.Contains("Scrypt", encrypted.ToString());

        var cbor = encrypted.TaggedCbor();
        var encrypted2 = EncryptedKey.FromTaggedCbor(cbor);
        var decrypted = encrypted2.Unlock(_testSecret);

        Assert.Equal(contentKey, decrypted);
    }

    [Fact]
    public void TestEncryptedKeyArgon2idRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var contentKey = MakeContentKey();

        var encrypted = EncryptedKey.Lock(
            KeyDerivationMethod.Argon2id, _testSecret, contentKey);
        Assert.Contains("Argon2id", encrypted.ToString());

        var cbor = encrypted.TaggedCbor();
        var encrypted2 = EncryptedKey.FromTaggedCbor(cbor);
        var decrypted = encrypted2.Unlock(_testSecret);

        Assert.Equal(contentKey, decrypted);
    }

    [Fact]
    public void TestEncryptedKeyWrongSecretFailsHkdf()
    {
        var contentKey = MakeContentKey();
        var wrongSecret = Encoding.UTF8.GetBytes("wrong secret");

        var encrypted = EncryptedKey.Lock(
            KeyDerivationMethod.HKDF, _testSecret, contentKey);
        Assert.Throws<BCComponentsException>(() => encrypted.Unlock(wrongSecret));
    }

    [Fact]
    public void TestEncryptedKeyWrongSecretFailsPbkdf2()
    {
        var contentKey = MakeContentKey();
        var wrongSecret = Encoding.UTF8.GetBytes("wrong secret");

        var encrypted = EncryptedKey.Lock(
            KeyDerivationMethod.PBKDF2, _testSecret, contentKey);
        Assert.Throws<BCComponentsException>(() => encrypted.Unlock(wrongSecret));
    }

    [Fact]
    public void TestEncryptedKeyWrongSecretFailsScrypt()
    {
        var contentKey = MakeContentKey();
        var wrongSecret = Encoding.UTF8.GetBytes("wrong secret");

        var encrypted = EncryptedKey.Lock(
            KeyDerivationMethod.Scrypt, _testSecret, contentKey);
        Assert.Throws<BCComponentsException>(() => encrypted.Unlock(wrongSecret));
    }

    [Fact]
    public void TestEncryptedKeyWrongSecretFailsArgon2id()
    {
        var contentKey = MakeContentKey();
        var wrongSecret = Encoding.UTF8.GetBytes("wrong secret");

        var encrypted = EncryptedKey.Lock(
            KeyDerivationMethod.Argon2id, _testSecret, contentKey);
        Assert.Throws<BCComponentsException>(() => encrypted.Unlock(wrongSecret));
    }
}
