using BlockchainCommons.BCCrypto;

namespace BlockchainCommons.BCCrypto.Tests;

public class HashTests
{
    [Fact]
    public void TestCrc32()
    {
        byte[] input = "Hello, world!"u8.ToArray();
        Assert.Equal(0xebe6c6e6u, Hash.Crc32(input));
        Assert.Equal(
            Convert.FromHexString("ebe6c6e6"),
            Hash.Crc32Data(input));
        Assert.Equal(
            Convert.FromHexString("e6c6e6eb"),
            Hash.Crc32DataOpt(input, true));
    }

    [Fact]
    public void TestSha256()
    {
        byte[] input = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"u8.ToArray();
        byte[] expected = Convert.FromHexString(
            "248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1");
        Assert.Equal(expected, Hash.Sha256(input));
    }

    [Fact]
    public void TestSha512()
    {
        byte[] input = "abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq"u8.ToArray();
        byte[] expected = Convert.FromHexString(
            "204a8fc6dda82f0a0ced7beb8e08a41657c16ef468b228a8279be331a703c33596fd15c13b1b07f9aa1d3bea57789ca031ad85c7a71dd70354ec631238ca3445");
        Assert.Equal(expected, Hash.Sha512(input));
    }

    [Fact]
    public void TestHmacSha()
    {
        byte[] key = Convert.FromHexString("0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b");
        byte[] message = "Hi There"u8.ToArray();

        Assert.Equal(
            Convert.FromHexString(
                "b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7"),
            Hash.HmacSha256(key, message));

        Assert.Equal(
            Convert.FromHexString(
                "87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854"),
            Hash.HmacSha512(key, message));
    }

    [Fact]
    public void TestPbkdf2HmacSha256()
    {
        Assert.Equal(
            Convert.FromHexString(
                "120fb6cffcf8b32c43e7225256c4f837a86548c92ccc35480805987cb70be17b"),
            Hash.Pbkdf2HmacSha256(
                "password"u8, "salt"u8, 1, 32));
    }

    [Fact]
    public void TestHkdfHmacSha256()
    {
        byte[] keyMaterial = "hello"u8.ToArray();
        byte[] salt = Convert.FromHexString("8e94ef805b93e683ff18");
        Assert.Equal(
            Convert.FromHexString(
                "13485067e21af17c0900f70d885f02593c0e61e46f86450e4a0201a54c14db76"),
            Hash.HkdfHmacSha256(keyMaterial, salt, 32));
    }
}
