using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCRand.Tests;

public class SecureRandomNumberGeneratorTests
{
    [Fact]
    public void TestRandomData()
    {
        byte[] data1 = SecureRandomNumberGenerator.SecureRandomData(32);
        byte[] data2 = SecureRandomNumberGenerator.SecureRandomData(32);
        byte[] data3 = SecureRandomNumberGenerator.SecureRandomData(32);

        Assert.Equal(32, data1.Length);
        Assert.NotEqual(data1, data2);
        Assert.NotEqual(data1, data3);
    }
}
