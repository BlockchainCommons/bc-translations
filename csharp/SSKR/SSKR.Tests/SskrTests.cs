using System.Text;
using BlockchainCommons.BCRand;
using BlockchainCommons.SSKR;

namespace BlockchainCommons.SSKR.Tests;

public class SskrTests
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
    public void TestSplit35()
    {
        var rng = new FakeRandomNumberGenerator();
        var secret = Secret.Create(Convert.FromHexString("0ff784df000c4380a5ed683f7e6e3dcf"));
        var group = GroupSpec.Create(3, 5);
        var spec = Spec.Create(1, [group]);
        var shares = Sskr.GenerateUsing(spec, secret, rng);
        var flattenedShares = shares.SelectMany(groupShares => groupShares).ToArray();

        Assert.Equal(5, flattenedShares.Length);

        foreach (var share in flattenedShares)
            Assert.Equal(Sskr.MetadataSizeBytes + secret.Length, share.Length);

        byte[] recoveredShareIndexes = [1, 2, 4];
        var recoveredShares = recoveredShareIndexes
            .Select(index => flattenedShares[index])
            .ToArray();
        var recoveredSecret = Sskr.Combine(recoveredShares);

        Assert.Equal(secret, recoveredSecret);
    }

    [Fact]
    public void TestSplit27()
    {
        var rng = new FakeRandomNumberGenerator();
        var secret = Secret.Create(Convert.FromHexString(
            "204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a"));
        var group = GroupSpec.Create(2, 7);
        var spec = Spec.Create(1, [group]);
        var shares = Sskr.GenerateUsing(spec, secret, rng);

        Assert.Single(shares);
        Assert.Equal(7, shares[0].Length);

        var flattenedShares = shares.SelectMany(groupShares => groupShares).ToArray();
        Assert.Equal(7, flattenedShares.Length);

        foreach (var share in flattenedShares)
            Assert.Equal(Sskr.MetadataSizeBytes + secret.Length, share.Length);

        byte[] recoveredShareIndexes = [3, 4];
        var recoveredShares = recoveredShareIndexes
            .Select(index => flattenedShares[index])
            .ToArray();
        var recoveredSecret = Sskr.Combine(recoveredShares);

        Assert.Equal(secret, recoveredSecret);
    }

    [Fact]
    public void TestSplit2323()
    {
        var rng = new FakeRandomNumberGenerator();
        var secret = Secret.Create(Convert.FromHexString(
            "204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a"));
        var group1 = GroupSpec.Create(2, 3);
        var group2 = GroupSpec.Create(2, 3);
        var spec = Spec.Create(2, [group1, group2]);
        var shares = Sskr.GenerateUsing(spec, secret, rng);

        Assert.Equal(2, shares.Length);
        Assert.Equal(3, shares[0].Length);
        Assert.Equal(3, shares[1].Length);

        var flattenedShares = shares.SelectMany(groupShares => groupShares).ToArray();
        Assert.Equal(6, flattenedShares.Length);

        foreach (var share in flattenedShares)
            Assert.Equal(Sskr.MetadataSizeBytes + secret.Length, share.Length);

        byte[] recoveredShareIndexes = [0, 1, 3, 5];
        var recoveredShares = recoveredShareIndexes
            .Select(index => flattenedShares[index])
            .ToArray();
        var recoveredSecret = Sskr.Combine(recoveredShares);

        Assert.Equal(secret, recoveredSecret);
    }

    private static void FisherYatesShuffle<T>(T[] array, IRandomNumberGenerator rng)
    {
        var i = array.Length;
        while (i > 1)
        {
            i -= 1;
            var j = (int)rng.NextWithUpperBound((ulong)i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }
    }

    [Fact]
    public void TestShuffle()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        var v = Enumerable.Range(0, 100).ToArray();

        FisherYatesShuffle(v, rng);

        Assert.Equal(100, v.Length);
        Assert.Equal(
            [
                79, 70, 40, 53, 25, 30, 31, 88, 10, 1, 45, 54, 81, 58, 55, 59,
                69, 78, 65, 47, 75, 61, 0, 72, 20, 9, 80, 13, 73, 11, 60, 56,
                19, 42, 33, 12, 36, 38, 6, 35, 68, 77, 50, 18, 97, 49, 98, 85,
                89, 91, 15, 71, 99, 67, 84, 23, 64, 14, 57, 48, 62, 29, 28, 94,
                44, 8, 66, 34, 43, 21, 63, 16, 92, 95, 27, 51, 26, 86, 22, 41,
                93, 82, 7, 87, 74, 37, 46, 3, 96, 24, 90, 39, 32, 17, 76, 4,
                83, 2, 52, 5,
            ],
            v);
    }

    private sealed class RecoverSpec(
        Secret secret,
        Spec spec,
        byte[][][] shares,
        int[] recoveredGroupIndexes,
        int[][] recoveredMemberIndexes,
        byte[][] recoveredShares)
    {
        public Secret Secret { get; } = secret;
        public Spec Spec { get; } = spec;
        public byte[][][] Shares { get; } = shares;
        public int[] RecoveredGroupIndexes { get; } = recoveredGroupIndexes;
        public int[][] RecoveredMemberIndexes { get; } = recoveredMemberIndexes;
        public byte[][] RecoveredShares { get; } = recoveredShares;

        public void Recover()
        {
            var recoveredSecret = Sskr.Combine(RecoveredShares);
            Assert.Equal(Secret, recoveredSecret);
        }
    }

    private static RecoverSpec CreateRecoverSpec(
        Secret secret,
        Spec spec,
        byte[][][] shares,
        IRandomNumberGenerator rng)
    {
        var groupIndexes = Enumerable.Range(0, spec.GroupCount).ToArray();
        FisherYatesShuffle(groupIndexes, rng);
        var recoveredGroupIndexes = groupIndexes[..spec.GroupThreshold];

        var recoveredMemberIndexes = new int[recoveredGroupIndexes.Length][];
        for (var i = 0; i < recoveredGroupIndexes.Length; i++)
        {
            var groupIndex = recoveredGroupIndexes[i];
            var group = spec.Groups[groupIndex];
            var memberIndexes = Enumerable.Range(0, group.MemberCount).ToArray();
            FisherYatesShuffle(memberIndexes, rng);
            recoveredMemberIndexes[i] = memberIndexes[..group.MemberThreshold];
        }

        var recoveredShares = new List<byte[]>();
        for (var i = 0; i < recoveredGroupIndexes.Length; i++)
        {
            var groupShares = shares[recoveredGroupIndexes[i]];
            foreach (var recoveredMemberIndex in recoveredMemberIndexes[i])
                recoveredShares.Add(groupShares[recoveredMemberIndex]);
        }

        var recoveredSharesArray = recoveredShares.ToArray();
        FisherYatesShuffle(recoveredSharesArray, rng);

        return new RecoverSpec(
            secret,
            spec,
            shares,
            recoveredGroupIndexes,
            recoveredMemberIndexes,
            recoveredSharesArray);
    }

    private static void OneFuzzTest(IRandomNumberGenerator rng)
    {
        var secretLen = rng.NextInClosedRange(Sskr.MinSecretLen, Sskr.MaxSecretLen) & ~1;
        var secret = Secret.Create(rng.RandomData(secretLen));

        var groupCount = rng.NextInClosedRange(1, Sskr.MaxGroupsCount);
        var groupSpecs = new List<GroupSpec>(groupCount);

        for (var i = 0; i < groupCount; i++)
        {
            var memberCount = rng.NextInClosedRange(1, Sskr.MaxShareCount);
            var memberThreshold = rng.NextInClosedRange(1, memberCount);
            groupSpecs.Add(GroupSpec.Create(memberThreshold, memberCount));
        }

        var groupThreshold = rng.NextInClosedRange(1, groupCount);
        var spec = Spec.Create(groupThreshold, groupSpecs);
        var shares = Sskr.GenerateUsing(spec, secret, rng);

        var recoverSpec = CreateRecoverSpec(secret, spec, shares, rng);
        recoverSpec.Recover();
    }

    [Fact]
    public void FuzzTest()
    {
        var rng = SeededRandomNumberGenerator.CreateFake();
        for (var i = 0; i < 100; i++)
            OneFuzzTest(rng);
    }

    [Fact]
    public void ExampleEncode()
    {
        var secret = Secret.Create("my secret belongs to me."u8);

        var group1 = GroupSpec.Create(2, 3);
        var group2 = GroupSpec.Create(3, 5);
        var spec = Spec.Create(2, [group1, group2]);

        var shares = Sskr.Generate(spec, secret);

        Assert.Equal(2, shares.Length);
        Assert.Equal(3, shares[0].Length);
        Assert.Equal(5, shares[1].Length);

        byte[][] recoveredShares =
        [
            shares[0][0],
            shares[0][2],
            shares[1][0],
            shares[1][1],
            shares[1][4],
        ];

        var recoveredSecret = Sskr.Combine(recoveredShares);
        Assert.Equal(secret, recoveredSecret);
    }

    [Fact]
    public void ExampleEncode3()
    {
        const string Text = "my secret belongs to me.";

        static Secret Roundtrip(int m, int n)
        {
            var secret = Secret.Create(Encoding.UTF8.GetBytes(Text));
            var spec = Spec.Create(1, [GroupSpec.Create(m, n)]);
            var shares = Sskr.Generate(spec, secret);
            var flattenedShares = shares.SelectMany(group => group).ToArray();
            return Sskr.Combine(flattenedShares);
        }

        {
            var result = Roundtrip(2, 3);
            Assert.Equal(Text, Encoding.UTF8.GetString(result.Data));
        }

        {
            var result = Roundtrip(1, 1);
            Assert.Equal(Text, Encoding.UTF8.GetString(result.Data));
        }

        {
            var result = Roundtrip(1, 3);
            Assert.Equal(Text, Encoding.UTF8.GetString(result.Data));
        }
    }

    [Fact]
    public void ExampleEncode4()
    {
        const string Text = "my secret belongs to me.";
        var secret = Secret.Create(Encoding.UTF8.GetBytes(Text));
        var spec = Spec.Create(1, [GroupSpec.Create(2, 3), GroupSpec.Create(2, 3)]);
        var groupedShares = Sskr.Generate(spec, secret);
        var flattenedShares = groupedShares.SelectMany(group => group).ToArray();

        // Group threshold is 1, but one additional share is provided from the
        // second group. The correct behavior is to ignore groups that cannot be
        // decoded and continue when quorum can still be satisfied.
        byte[] recoveredShareIndexes = [0, 1, 3];
        var recoveredShares = recoveredShareIndexes
            .Select(index => flattenedShares[index])
            .ToArray();

        var recoveredSecret = Sskr.Combine(recoveredShares);
        Assert.Equal(Text, Encoding.UTF8.GetString(recoveredSecret.Data));
    }
}
