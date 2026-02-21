using BlockchainCommons.BCRand;

namespace BlockchainCommons.BCRand.Tests;

public class SeededRandomNumberGeneratorTests
{
    private static readonly ulong[] TestSeed =
    [
        17295166580085024720,
        422929670265678780,
        5577237070365765850,
        7953171132032326923
    ];

    [Fact]
    public void TestNextU64()
    {
        var rng = new SeededRandomNumberGenerator(TestSeed);
        Assert.Equal(1104683000648959614UL, rng.NextUInt64());
    }

    [Fact]
    public void TestNext50()
    {
        var rng = new SeededRandomNumberGenerator(TestSeed);
        ulong[] expectedValues =
        [
            1104683000648959614,
            9817345228149227957,
            546276821344993881,
            15870950426333349563,
            830653509032165567,
            14772257893953840492,
            3512633850838187726,
            6358411077290857510,
            7897285047238174514,
            18314839336815726031,
            4978716052961022367,
            17373022694051233817,
            663115362299242570,
            9811238046242345451,
            8113787839071393872,
            16155047452816275860,
            673245095821315645,
            1610087492396736743,
            1749670338128618977,
            3927771759340679115,
            9610589375631783853,
            5311608497352460372,
            11014490817524419548,
            6320099928172676090,
            12513554919020212402,
            6823504187935853178,
            1215405011954300226,
            8109228150255944821,
            4122548551796094879,
            16544885818373129566,
            5597102191057004591,
            11690994260783567085,
            9374498734039011409,
            18246806104446739078,
            2337407889179712900,
            12608919248151905477,
            7641631838640172886,
            8421574250687361351,
            8697189342072434208,
            8766286633078002696,
            14800090277885439654,
            17865860059234099833,
            4673315107448681522,
            14288183874156623863,
            7587575203648284614,
            9109213819045273474,
            11817665411945280786,
            1745089530919138651,
            5730370365819793488,
            5496865518262805451
        ];

        foreach (ulong expected in expectedValues)
        {
            Assert.Equal(expected, rng.NextUInt64());
        }
    }

    [Fact]
    public void TestFakeRandomData()
    {
        byte[] expected = Convert.FromHexString(
            "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed" +
            "518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d354553" +
            "2daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a56" +
            "4e59b4e2");
        Assert.Equal(expected, SeededRandomNumberGenerator.FakeRandomData(100));
    }

    [Fact]
    public void TestNextWithUpperBound()
    {
        var rng = new SeededRandomNumberGenerator(TestSeed);
        Assert.Equal(745u, rng.NextWithUpperBound(10000u));
    }

    [Fact]
    public void TestInRange()
    {
        var rng = new SeededRandomNumberGenerator(TestSeed);
        int[] v = Enumerable.Range(0, 100)
            .Select(_ => rng.NextInRange(0, 100))
            .ToArray();

        int[] expectedValues =
        [
            7, 44, 92, 16, 16, 67, 41, 74, 66, 20, 18, 6, 62, 34, 4, 69, 99,
            19, 0, 85, 22, 27, 56, 23, 19, 5, 23, 76, 80, 27, 74, 69, 17, 92,
            31, 32, 55, 36, 49, 23, 53, 2, 46, 6, 43, 66, 34, 71, 64, 69, 25,
            14, 17, 23, 32, 6, 23, 65, 35, 11, 21, 37, 58, 92, 98, 8, 38, 49,
            7, 24, 24, 71, 37, 63, 91, 21, 11, 66, 52, 54, 55, 19, 76, 46, 89,
            38, 91, 95, 33, 25, 4, 30, 66, 51, 5, 91, 62, 27, 92, 39
        ];

        Assert.Equal(expectedValues, v);
    }

    [Fact]
    public void TestFillRandomData()
    {
        var rng1 = new SeededRandomNumberGenerator(TestSeed);
        byte[] v1 = rng1.RandomData(100);

        var rng2 = new SeededRandomNumberGenerator(TestSeed);
        byte[] v2 = new byte[100];
        rng2.FillRandomData(v2);

        Assert.Equal(v1, v2);
    }

    [Fact]
    public void TestNullSeed()
    {
        Assert.Throws<ArgumentNullException>(() => new SeededRandomNumberGenerator(null!));
    }

    [Fact]
    public void TestSpanSeedConstructor()
    {
        var rng = new SeededRandomNumberGenerator(TestSeed.AsSpan());
        Assert.Equal(1104683000648959614UL, rng.NextUInt64());
    }

    [Fact]
    public void TestRandomDataNegativeSize()
    {
        var rng = new SeededRandomNumberGenerator(TestSeed);
        Assert.Throws<ArgumentOutOfRangeException>(() => rng.RandomData(-1));
    }
}
