using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCRand.Tests;

public class RandomNumberGeneratorTests
{
    [Fact]
    public void TestFakeNumbers()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        int[] array = Enumerable.Range(0, 100)
            .Select(_ => rng.NextInClosedRange(-50, 50))
            .ToArray();

        int[] expected =
        [
            -43, -6, 43, -34, -34, 17, -9, 24, 17, -29, -32, -44, 12, -15,
            -46, 20, 50, -31, -50, 36, -28, -23, 6, -27, -31, -45, -27, 26,
            31, -23, 24, 19, -32, 43, -18, -17, 6, -13, -1, -27, 4, -48, -4,
            -44, -6, 17, -15, 22, 15, 20, -25, -35, -33, -27, -17, -44, -27,
            15, -14, -38, -29, -12, 8, 43, 49, -42, -11, -1, -42, -26, -25,
            22, -13, 14, 42, -29, -38, 17, 2, 5, 5, -31, 27, -3, 39, -12, 42,
            46, -17, -25, -46, -19, 16, 2, -45, 41, 12, -22, 43, -11
        ];

        Assert.Equal(expected, array);
    }
}
