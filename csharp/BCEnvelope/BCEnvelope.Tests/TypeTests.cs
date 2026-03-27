using BlockchainCommons.BCEnvelope;
using BlockchainCommons.BCRand;
using BlockchainCommons.KnownValues;

namespace BlockchainCommons.BCEnvelope.Tests;

public sealed class TypeTests
{
    [Fact]
    public void TestKnownValue()
    {
        var envelope = Envelope.Create(KnownValuesRegistry.Signed)
            .CheckEncoding();
        Assert.Equal("'signed'", envelope.Format());
    }

    [Fact]
    public void TestDate()
    {
        var date = DCbor.CborDate.FromYmd(2018, 1, 7);
        var envelope = Envelope.Create(date).CheckEncoding();
        Assert.Equal("2018-01-07", envelope.Format());
    }

    [Fact]
    public void TestFakeRandomData()
    {
        var data = SeededRandomNumberGenerator.FakeRandomData(100);
        var expected = Convert.FromHexString(
            "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d3545532daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a564e59b4e2");
        Assert.Equal(expected, data);
    }

    [Fact]
    public void TestFakeNumbers()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        var array = new int[100];
        for (int i = 0; i < 100; i++)
        {
            array[i] = rng.NextInClosedRange(-50, 50);
        }
        var expected = new[]
        {
            -43, -6, 43, -34, -34, 17, -9, 24, 17, -29,
            -32, -44, 12, -15, -46, 20, 50, -31, -50, 36,
            -28, -23, 6, -27, -31, -45, -27, 26, 31, -23,
            24, 19, -32, 43, -18, -17, 6, -13, -1, -27,
            4, -48, -4, -44, -6, 17, -15, 22, 15, 20,
            -25, -35, -33, -27, -17, -44, -27, 15, -14, -38,
            -29, -12, 8, 43, 49, -42, -11, -1, -42, -26,
            -25, 22, -13, 14, 42, -29, -38, 17, 2, 5,
            5, -31, 27, -3, 39, -12, 42, 46, -17, -25,
            -46, -19, 16, 2, -45, 41, 12, -22, 43, -11,
        };
        Assert.Equal(expected, array);
    }
}
