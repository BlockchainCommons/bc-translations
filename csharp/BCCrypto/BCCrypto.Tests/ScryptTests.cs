using BlockchainCommons.BCCrypto;

namespace BlockchainCommons.BCCrypto.Tests;

public class ScryptTests
{
    [Fact]
    public void TestScryptBasic()
    {
        byte[] pass = "password"u8.ToArray();
        byte[] salt = "salt"u8.ToArray();
        byte[] output = ScryptKdf.Derive(pass, salt, 32);
        Assert.Equal(32, output.Length);
        // Scrypt should be deterministic for same input
        byte[] output2 = ScryptKdf.Derive(pass, salt, 32);
        Assert.Equal(output, output2);
    }

    [Fact]
    public void TestScryptDifferentSalt()
    {
        byte[] pass = "password"u8.ToArray();
        byte[] salt1 = "salt1"u8.ToArray();
        byte[] salt2 = "salt2"u8.ToArray();
        byte[] out1 = ScryptKdf.Derive(pass, salt1, 32);
        byte[] out2 = ScryptKdf.Derive(pass, salt2, 32);
        Assert.NotEqual(out1, out2);
    }

    [Fact]
    public void TestScryptOptBasic()
    {
        byte[] pass = "password"u8.ToArray();
        byte[] salt = "salt"u8.ToArray();
        byte[] output = ScryptKdf.DeriveOpt(pass, salt, 32, 15, 8, 1);
        Assert.Equal(32, output.Length);
    }

    [Fact]
    public void TestScryptOutputLength()
    {
        byte[] pass = "password"u8.ToArray();
        byte[] salt = "salt"u8.ToArray();
        foreach (int len in new[] { 16, 24, 32, 64 })
        {
            byte[] output = ScryptKdf.Derive(pass, salt, len);
            Assert.Equal(len, output.Length);
        }
    }
}
