using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class SymmetricKeyTests
{
    // RFC-8439 Section 2.8.2 test vectors
    private readonly byte[] _plaintext =
        Encoding.UTF8.GetBytes(
            "Ladies and Gentlemen of the class of '99: " +
            "If I could offer you only one tip for the future, sunscreen would be it.");
    private readonly byte[] _aad =
        Convert.FromHexString("50515253c0c1c2c3c4c5c6c7");
    private readonly SymmetricKey _key =
        SymmetricKey.FromHex("808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f");
    private readonly Nonce _nonce =
        Nonce.FromHex("070000004041424344454647");
    private readonly byte[] _expectedCiphertext =
        Convert.FromHexString(
            "d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116");
    private readonly AuthenticationTag _expectedAuth =
        AuthenticationTag.FromData(Convert.FromHexString("1ae10b594f09e26a7e902ecbd0600691"));

    private EncryptedMessage MakeEncryptedMessage() =>
        _key.Encrypt(_plaintext, _aad, _nonce);

    [Fact]
    public void TestRfcTestVector()
    {
        var encrypted = MakeEncryptedMessage();
        Assert.Equal(_expectedCiphertext, encrypted.Ciphertext);
        Assert.Equal(_aad, encrypted.Aad);
        Assert.Equal(_nonce, encrypted.Nonce);
        Assert.Equal(_expectedAuth, encrypted.AuthenticationTag);

        var decrypted = _key.Decrypt(encrypted);
        Assert.Equal(_plaintext, decrypted);
    }

    [Fact]
    public void TestRandomKeyAndNonce()
    {
        var randomKey = SymmetricKey.New();
        var randomNonce = Nonce.New();
        var encrypted = randomKey.Encrypt(_plaintext, _aad, randomNonce);
        var decrypted = randomKey.Decrypt(encrypted);
        Assert.Equal(_plaintext, decrypted);
    }

    [Fact]
    public void TestEmptyData()
    {
        var randomKey = SymmetricKey.New();
        var encrypted = randomKey.Encrypt(Array.Empty<byte>());
        var decrypted = randomKey.Decrypt(encrypted);
        Assert.Empty(decrypted);
    }

    [Fact]
    public void TestCborData()
    {
        TagsRegistry.RegisterTags();
        var encrypted = MakeEncryptedMessage();
        var cbor = encrypted.TaggedCbor();
        var data = cbor.ToCborData();

        var expectedHex =
            "d99c42845872d31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b61164c070000004041424344454647501ae10b594f09e26a7e902ecbd06006914c50515253c0c1c2c3c4c5c6c7";
        Assert.Equal(expectedHex, Convert.ToHexString(data).ToLowerInvariant());
    }

    [Fact]
    public void TestCborRoundtrip()
    {
        var encrypted = MakeEncryptedMessage();
        var cbor = encrypted.TaggedCbor();
        var decoded = EncryptedMessage.FromTaggedCbor(cbor);
        Assert.Equal(encrypted, decoded);
    }
}
