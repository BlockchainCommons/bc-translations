using BlockchainCommons.BCCrypto;

namespace BlockchainCommons.BCCrypto.Tests;

public class ArgonTests
{
    [Fact]
    public void TestArgon2IdBasic()
    {
        byte[] pass = "password"u8.ToArray();
        byte[] salt = "example salt"u8.ToArray();
        byte[] output = ArgonKdf.Argon2Id(pass, salt, 32);
        Assert.Equal(32, output.Length);
        // Argon2 should be deterministic for same input
        byte[] output2 = ArgonKdf.Argon2Id(pass, salt, 32);
        Assert.Equal(output, output2);
    }

    [Fact]
    public void TestArgon2IdDifferentSalt()
    {
        byte[] pass = "password"u8.ToArray();
        byte[] salt1 = "example salt"u8.ToArray();
        byte[] salt2 = "example salt2"u8.ToArray();
        byte[] out1 = ArgonKdf.Argon2Id(pass, salt1, 32);
        byte[] out2 = ArgonKdf.Argon2Id(pass, salt2, 32);
        Assert.NotEqual(out1, out2);
    }
}
