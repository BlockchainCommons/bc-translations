using BlockchainCommons.BCShamir;
using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCShamir.Tests;

public class ShamirTests
{
    private sealed class FakeRandomNumberGenerator : IRandomNumberGenerator
    {
        public uint NextUInt32() => throw new NotSupportedException();

        public ulong NextUInt64() => throw new NotSupportedException();

        public byte[] RandomData(int size)
        {
            var data = new byte[size];
            FillRandomData(data);
            return data;
        }

        public void FillRandomData(Span<byte> data)
        {
            byte b = 0;
            for (var i = 0; i < data.Length; i++)
            {
                data[i] = b;
                b = unchecked((byte)(b + 17));
            }
        }
    }

    [Fact]
    public void TestSplitSecret35()
    {
        var rng = new FakeRandomNumberGenerator();
        var secret = Convert.FromHexString("0ff784df000c4380a5ed683f7e6e3dcf");

        var shares = Shamir.SplitSecret(3, 5, secret, rng);
        Assert.Equal(5, shares.Length);

        Assert.Equal(Convert.FromHexString("00112233445566778899aabbccddeeff"), shares[0]);
        Assert.Equal(Convert.FromHexString("d43099fe444807c46921a4f33a2a798b"), shares[1]);
        Assert.Equal(Convert.FromHexString("d9ad4e3bec2e1a7485698823abf05d36"), shares[2]);
        Assert.Equal(Convert.FromHexString("0d8cf5f6ec337bc764d1866b5d07ca42"), shares[3]);
        Assert.Equal(Convert.FromHexString("1aa7fe3199bc5092ef3816b074cabdf2"), shares[4]);

        byte[] recoveredShareIndexes = [1, 2, 4];
        byte[][] recoveredShares = recoveredShareIndexes.Select(index => shares[index]).ToArray();

        var recoveredSecret = Shamir.RecoverSecret(recoveredShareIndexes, recoveredShares);
        Assert.Equal(secret, recoveredSecret);
    }

    [Fact]
    public void TestSplitSecret27()
    {
        var rng = new FakeRandomNumberGenerator();
        var secret = Convert.FromHexString(
            "204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a");

        var shares = Shamir.SplitSecret(2, 7, secret, rng);
        Assert.Equal(7, shares.Length);

        Assert.Equal(Convert.FromHexString("2dcd14c2252dc8489af3985030e74d5a48e8eff1478ab86e65b43869bf39d556"), shares[0]);
        Assert.Equal(Convert.FromHexString("a1dfdd798388aada635b9974472b4fc59a32ae520c42c9f6a0af70149b882487"), shares[1]);
        Assert.Equal(Convert.FromHexString("2ee99daf727c0c7773b89a18de64497ff7476dacd1015a45f482a893f7402cef"), shares[2]);
        Assert.Equal(Convert.FromHexString("a2fb5414d4d96ee58a109b3ca9a84be0259d2c0f9ac92bdd3199e0eed3f1dd3e"), shares[3]);
        Assert.Equal(Convert.FromHexString("2b851d188b8f5b3653659cc0f7fa45102dadf04b708767385cd803862fcb3c3f"), shares[4]);
        Assert.Equal(Convert.FromHexString("a797d4a32d2a39a4aacd9de48036478fff77b1e83b4f16a099c34bfb0b7acdee"), shares[5]);
        Assert.Equal(Convert.FromHexString("28a19475dcde9f09ba2e9e881979413592027216e60c8513cdee937c67b2c586"), shares[6]);

        byte[] recoveredShareIndexes = [3, 4];
        byte[][] recoveredShares = recoveredShareIndexes.Select(index => shares[index]).ToArray();

        var recoveredSecret = Shamir.RecoverSecret(recoveredShareIndexes, recoveredShares);
        Assert.Equal(secret, recoveredSecret);
    }

    [Fact]
    public void ExampleSplit()
    {
        var threshold = 2;
        var shareCount = 3;
        byte[] secret = "my secret belongs to me."u8.ToArray();
        var shares = Shamir.SplitSecret(
            threshold,
            shareCount,
            secret,
            SecureRandomNumberGenerator.Shared);

        Assert.Equal(shareCount, shares.Length);
    }

    [Fact]
    public void ExampleRecover()
    {
        byte[] indexes = [0, 2];
        byte[][] shares =
        [
            [
                47, 165, 102, 232, 218, 99, 6, 94, 39, 6, 253, 215, 12, 88, 64,
                32, 105, 40, 222, 146, 93, 197, 48, 129,
            ],
            [
                221, 174, 116, 201, 90, 99, 136, 33, 64, 215, 60, 84, 207, 28,
                74, 10, 111, 243, 43, 224, 48, 64, 199, 172,
            ],
        ];

        var secret = Shamir.RecoverSecret(indexes, shares);
        Assert.Equal("my secret belongs to me."u8.ToArray(), secret);
    }
}
