using System.Text;
using BlockchainCommons.BCComponents;
using BlockchainCommons.DCbor;
using BlockchainCommons.SSKR;

namespace BlockchainCommons.BCComponents.Tests;

public sealed class SSKRShareTests
{
    [Fact]
    public void TestShareMetadata()
    {
        // Construct a share with known metadata bytes
        var data = new byte[]
        {
            0x12, 0x34,        // identifier: 0x1234
            0x21,              // group_threshold-1=2 (high nibble), group_count-1=1 (low nibble)
            0x31,              // group_index=3 (high nibble), member_threshold-1=1 (low nibble)
            0x01,              // member_index=1 (low nibble)
            0xAA, 0xBB, 0xCC, // share value
        };
        var share = SSKRShare.FromData(data);

        Assert.Equal(0x1234, share.Identifier());
        Assert.Equal("1234", share.IdentifierHex());
        Assert.Equal(3, share.GroupThreshold());
        Assert.Equal(2, share.GroupCount());
        Assert.Equal(3, share.GroupIndex());
        Assert.Equal(2, share.MemberThreshold());
        Assert.Equal(1, share.MemberIndex());
    }

    [Fact]
    public void TestShareHexRoundtrip()
    {
        var hex = "1234213101aabbcc";
        var share = SSKRShare.FromHex(hex);
        Assert.Equal(hex, share.Hex);
    }

    [Fact]
    public void TestGenerateAndCombineSimple()
    {
        // Single group, 1 of 1
        var secretData = Encoding.UTF8.GetBytes("0123456789abcdef"); // 16 bytes
        var masterSecret = Secret.Create(secretData);
        var group = GroupSpec.Create(1, 1);
        var spec = Spec.Create(1, new[] { group });

        var shares = SSKRShare.SskrGenerate(spec, masterSecret);
        Assert.Single(shares);
        Assert.Single(shares[0]);

        var recovered = SSKRShare.SskrCombine(shares[0]);
        Assert.Equal(secretData, recovered.Data);
    }

    [Fact]
    public void TestGenerateAndCombine2of3()
    {
        var secretData = Encoding.UTF8.GetBytes("0123456789abcdef"); // 16 bytes
        var masterSecret = Secret.Create(secretData);
        var group = GroupSpec.Create(2, 3);
        var spec = Spec.Create(1, new[] { group });

        var shares = SSKRShare.SskrGenerate(spec, masterSecret);
        Assert.Single(shares);
        Assert.Equal(3, shares[0].Count);

        // Use first 2 shares to recover
        var recoveryShares = new List<SSKRShare> { shares[0][0], shares[0][1] };
        var recovered = SSKRShare.SskrCombine(recoveryShares);
        Assert.Equal(secretData, recovered.Data);
    }

    [Fact]
    public void TestGenerateAndCombineMultiGroup()
    {
        var secretData = Encoding.UTF8.GetBytes("0123456789abcdef"); // 16 bytes
        var masterSecret = Secret.Create(secretData);
        var group1 = GroupSpec.Create(2, 3);
        var group2 = GroupSpec.Create(3, 5);
        var spec = Spec.Create(2, new[] { group1, group2 });

        var shares = SSKRShare.SskrGenerate(spec, masterSecret);
        Assert.Equal(2, shares.Count);
        Assert.Equal(3, shares[0].Count);
        Assert.Equal(5, shares[1].Count);

        // Collect shares that meet threshold requirements:
        // 2 from group 1 + 3 from group 2
        var recoveryShares = new List<SSKRShare>
        {
            shares[0][0], shares[0][1],
            shares[1][0], shares[1][1], shares[1][2],
        };
        var recovered = SSKRShare.SskrCombine(recoveryShares);
        Assert.Equal(secretData, recovered.Data);
    }

    [Fact]
    public void TestShareCborRoundtrip()
    {
        TagsRegistry.RegisterTags();
        var secretData = Encoding.UTF8.GetBytes("0123456789abcdef");
        var masterSecret = Secret.Create(secretData);
        var group = GroupSpec.Create(1, 1);
        var spec = Spec.Create(1, new[] { group });

        var shares = SSKRShare.SskrGenerate(spec, masterSecret);
        var share = shares[0][0];

        var cbor = share.TaggedCbor();
        var decoded = SSKRShare.FromTaggedCbor(cbor);
        Assert.Equal(share, decoded);
    }
}
