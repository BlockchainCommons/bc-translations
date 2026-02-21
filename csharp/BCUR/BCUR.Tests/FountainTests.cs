namespace BlockchainCommons.BCUR.Tests;

public class FountainTests
{
    [Fact]
    public void FragmentLength()
    {
        Assert.Equal(1764, FountainUtils.FragmentLength(12345, 1955));
        Assert.Equal(12345, FountainUtils.FragmentLength(12345, 30000));
        Assert.Equal(4, FountainUtils.FragmentLength(10, 4));
        Assert.Equal(5, FountainUtils.FragmentLength(10, 5));
        Assert.Equal(5, FountainUtils.FragmentLength(10, 6));
        Assert.Equal(10, FountainUtils.FragmentLength(10, 10));
    }

    [Fact]
    public void PartitionAndJoin()
    {
        var message = TestHelpers.MakeMessage("Wolf", 1024);
        var fragmentLength = FountainUtils.FragmentLength(message.Length, 100);
        var fragments = FountainUtils.Partition(message, fragmentLength);

        string[] expectedFragments =
        [
            "916ec65cf77cadf55cd7f9cda1a1030026ddd42e905b77adc36e4f2d3ccba44f7f04f2de44f42d84c374a0e149136f25b01852545961d55f7f7a8cde6d0e2ec43f3b2dcb644a2209e8c9e34af5c4747984a5e873c9cf5f965e25ee29039f",
            "df8ca74f1c769fc07eb7ebaec46e0695aea6cbd60b3ec4bbff1b9ffe8a9e7240129377b9d3711ed38d412fbb4442256f1e6f595e0fc57fed451fb0a0101fb76b1fb1e1b88cfdfdaa946294a47de8fff173f021c0e6f65b05c0a494e50791",
            "270a0050a73ae69b6725505a2ec8a5791457c9876dd34aadd192a53aa0dc66b556c0c215c7ceb8248b717c22951e65305b56a3706e3e86eb01c803bbf915d80edcd64d4d41977fa6f78dc07eecd072aae5bc8a852397e06034dba6a0b570",
            "797c3a89b16673c94838d884923b8186ee2db5c98407cab15e13678d072b43e406ad49477c2e45e85e52ca82a94f6df7bbbe7afbed3a3a830029f29090f25217e48d1f42993a640a67916aa7480177354cc7440215ae41e4d02eae9a1912",
            "33a6d4922a792c1b7244aa879fefdb4628dc8b0923568869a983b8c661ffab9b2ed2c149e38d41fba090b94155adbed32f8b18142ff0d7de4eeef2b04adf26f2456b46775c6c20b37602df7da179e2332feba8329bbb8d727a138b4ba7a5",
            "03215eda2ef1e953d89383a382c11d3f2cad37a4ee59a91236a3e56dcf89f6ac81dd4159989c317bd649d9cbc617f73fe10033bd288c60977481a09b343d3f676070e67da757b86de27bfca74392bac2996f7822a7d8f71a489ec6180390",
            "089ea80a8fcd6526413ec6c9a339115f111d78ef21d456660aa85f790910ffa2dc58d6a5b93705caef1091474938bd312427021ad1eeafbd19e0d916ddb111fabd8dcab5ad6a6ec3a9c6973809580cb2c164e26686b5b98cfb017a337968",
            "c7daaa14ae5152a067277b1b3902677d979f8e39cc2aafb3bc06fcf69160a853e6869dcc09a11b5009f91e6b89e5b927ab1527a735660faa6012b420dd926d940d742be6a64fb01cdc0cff9faa323f02ba41436871a0eab851e7f5782d10",
            "fbefde2a7e9ae9dc1e5c2c48f74f6c824ce9ef3c89f68800d44587bedc4ab417cfb3e7447d90e1e417e6e05d30e87239d3a5d1d45993d4461e60a0192831640aa32dedde185a371ded2ae15f8a93dba8809482ce49225daadfbb0fec629e",
            "23880789bdf9ed73be57fa84d555134630e8d0f7df48349f29869a477c13ccca9cd555ac42ad7f568416c3d61959d0ed568b2b81c7771e9088ad7fd55fd4386bafbf5a528c30f107139249357368ffa980de2c76ddd9ce4191376be0e6b5",
            "170010067e2e75ebe2d2904aeb1f89d5dc98cd4a6f2faaa8be6d03354c990fd895a97feb54668473e9d942bb99e196d897e8f1b01625cf48a7b78d249bb4985c065aa8cd1402ed2ba1b6f908f63dcd84b66425df00000000000000000000"
        ];

        Assert.Equal(expectedFragments.Length, fragments.Count);
        for (int i = 0; i < fragments.Count; i++)
        {
            Assert.Equal(expectedFragments[i], TestHelpers.BytesToHex(fragments[i]));
        }

        // Join
        var combined = new byte[fragments.Sum(f => f.Length)];
        int offset = 0;
        foreach (var f in fragments)
        {
            Array.Copy(f, 0, combined, offset, f.Length);
            offset += f.Length;
        }
        var rejoined = new byte[message.Length];
        Array.Copy(combined, rejoined, message.Length);
        Assert.Equal(message, rejoined);
    }

    [Fact]
    public void ChooseFragments30()
    {
        var message = TestHelpers.MakeMessage("Wolf", 1024);
        var checksum = Crc32.Checksum(message);
        var fragmentLength = FountainUtils.FragmentLength(message.Length, 100);
        var fragments = FountainUtils.Partition(message, fragmentLength);

        int[][] expected =
        [
            [0],
            [1],
            [2],
            [3],
            [4],
            [5],
            [6],
            [7],
            [8],
            [9],
            [10],
            [9],
            [2, 5, 6, 8, 9, 10],
            [8],
            [1, 5],
            [1],
            [0, 2, 4, 5, 8, 10],
            [5],
            [2],
            [2],
            [0, 1, 3, 4, 5, 7, 9, 10],
            [0, 1, 2, 3, 5, 6, 8, 9, 10],
            [0, 2, 4, 5, 7, 8, 9, 10],
            [3, 5],
            [4],
            [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10],
            [0, 1, 3, 4, 5, 6, 7, 9, 10],
            [6],
            [5, 6],
            [7]
        ];

        for (int seq = 1; seq <= 30; seq++)
        {
            var indexes = FountainUtils.ChooseFragments(seq, fragments.Count, checksum);
            indexes.Sort();
            Assert.Equal(expected[seq - 1], indexes);
        }
    }

    [Fact]
    public void XorTest()
    {
        var rng = Xoshiro256.FromString("Wolf");

        var data1 = rng.NextBytes(10);
        Assert.Equal("916ec65cf77cadf55cd7", TestHelpers.BytesToHex(data1));

        var data2 = rng.NextBytes(10);
        Assert.Equal("f9cda1a1030026ddd42e", TestHelpers.BytesToHex(data2));

        var data3 = (byte[])data1.Clone();
        FountainUtils.Xor(data3, data2);
        Assert.Equal("68a367fdf47c8b2888f9", TestHelpers.BytesToHex(data3));

        FountainUtils.Xor(data3, data1);
        Assert.Equal(TestHelpers.BytesToHex(data2), TestHelpers.BytesToHex(data3));
    }

    [Fact]
    public void FountainEncoder20Parts()
    {
        var message = TestHelpers.MakeMessage("Wolf", 256);
        var encoder = new FountainEncoder(message, 30);

        string[] expectedData =
        [
            "916ec65cf77cadf55cd7f9cda1a1030026ddd42e905b77adc36e4f2d3c",
            "cba44f7f04f2de44f42d84c374a0e149136f25b01852545961d55f7f7a",
            "8cde6d0e2ec43f3b2dcb644a2209e8c9e34af5c4747984a5e873c9cf5f",
            "965e25ee29039fdf8ca74f1c769fc07eb7ebaec46e0695aea6cbd60b3e",
            "c4bbff1b9ffe8a9e7240129377b9d3711ed38d412fbb4442256f1e6f59",
            "5e0fc57fed451fb0a0101fb76b1fb1e1b88cfdfdaa946294a47de8fff1",
            "73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5",
            "791457c9876dd34aadd192a53aa0dc66b556c0c215c7ceb8248b717c22",
            "951e65305b56a3706e3e86eb01c803bbf915d80edcd64d4d0000000000",
            "330f0f33a05eead4f331df229871bee733b50de71afd2e5a79f196de09",
            "3b205ce5e52d8c24a52cffa34c564fa1af3fdffcd349dc4258ee4ee828",
            "dd7bf725ea6c16d531b5f03254783803048ca08b87148daacd1cd7a006",
            "760be7ad1c6187902bbc04f539b9ee5eb8ea6833222edea36031306c01",
            "5bf4031217d2c3254b088fa7553778b5003632f46e21db129416f65b55",
            "73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5",
            "b8546ebfe2048541348910267331c643133f828afec9337c318f71b7df",
            "23dedeea74e3a0fb052befabefa13e2f80e4315c9dceed4c8630612e64",
            "d01a8daee769ce34b6b35d3ca0005302724abddae405bdb419c0a6b208",
            "3171c5dc365766eff25ae47c6f10e7de48cfb8474e050e5fe997a6dc24",
            "e055c2433562184fa71b4be94f262e200f01c6f74c284b0dc6fae6673f"
        ];

        Assert.Equal(9, encoder.FragmentCount);
        for (int i = 0; i < 20; i++)
        {
            Assert.Equal(i, encoder.CurrentSequence);
            var part = encoder.NextPart();
            Assert.Equal(expectedData[i], TestHelpers.BytesToHex(part.Data));
            Assert.Equal(i + 1, part.Sequence);
            Assert.Equal(9, part.SequenceCount);
            Assert.Equal(256, part.MessageLength);
        }
    }

    [Fact]
    public void FountainEncoderCbor()
    {
        var message = TestHelpers.MakeMessage("Wolf", 256);
        var encoder = new FountainEncoder(message, 30);

        string[] expected =
        [
            "8501091901001a0167aa07581d916ec65cf77cadf55cd7f9cda1a1030026ddd42e905b77adc36e4f2d3c",
            "8502091901001a0167aa07581dcba44f7f04f2de44f42d84c374a0e149136f25b01852545961d55f7f7a",
            "8503091901001a0167aa07581d8cde6d0e2ec43f3b2dcb644a2209e8c9e34af5c4747984a5e873c9cf5f",
            "8504091901001a0167aa07581d965e25ee29039fdf8ca74f1c769fc07eb7ebaec46e0695aea6cbd60b3e",
            "8505091901001a0167aa07581dc4bbff1b9ffe8a9e7240129377b9d3711ed38d412fbb4442256f1e6f59",
            "8506091901001a0167aa07581d5e0fc57fed451fb0a0101fb76b1fb1e1b88cfdfdaa946294a47de8fff1",
            "8507091901001a0167aa07581d73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5",
            "8508091901001a0167aa07581d791457c9876dd34aadd192a53aa0dc66b556c0c215c7ceb8248b717c22",
            "8509091901001a0167aa07581d951e65305b56a3706e3e86eb01c803bbf915d80edcd64d4d0000000000",
            "850a091901001a0167aa07581d330f0f33a05eead4f331df229871bee733b50de71afd2e5a79f196de09",
            "850b091901001a0167aa07581d3b205ce5e52d8c24a52cffa34c564fa1af3fdffcd349dc4258ee4ee828",
            "850c091901001a0167aa07581ddd7bf725ea6c16d531b5f03254783803048ca08b87148daacd1cd7a006",
            "850d091901001a0167aa07581d760be7ad1c6187902bbc04f539b9ee5eb8ea6833222edea36031306c01",
            "850e091901001a0167aa07581d5bf4031217d2c3254b088fa7553778b5003632f46e21db129416f65b55",
            "850f091901001a0167aa07581d73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5",
            "8510091901001a0167aa07581db8546ebfe2048541348910267331c643133f828afec9337c318f71b7df",
            "8511091901001a0167aa07581d23dedeea74e3a0fb052befabefa13e2f80e4315c9dceed4c8630612e64",
            "8512091901001a0167aa07581dd01a8daee769ce34b6b35d3ca0005302724abddae405bdb419c0a6b208",
            "8513091901001a0167aa07581d3171c5dc365766eff25ae47c6f10e7de48cfb8474e050e5fe997a6dc24",
            "8514091901001a0167aa07581de055c2433562184fa71b4be94f262e200f01c6f74c284b0dc6fae6673f"
        ];

        Assert.Equal(9, encoder.FragmentCount);
        for (int i = 0; i < 20; i++)
        {
            var part = encoder.NextPart();
            Assert.Equal(expected[i], TestHelpers.BytesToHex(part.ToCbor()));
        }
    }

    [Fact]
    public void FountainDecoder()
    {
        var message = TestHelpers.MakeMessage("Wolf", 32767);
        var encoder = new FountainEncoder(message, 1000);
        var decoder = new FountainDecoder();
        while (!decoder.IsComplete)
        {
            Assert.Null(decoder.Message());
            var part = encoder.NextPart();
            decoder.Receive(part);
        }
        Assert.Equal(message, decoder.Message());
    }

    [Fact]
    public void FountainDecoderSkip()
    {
        var message = TestHelpers.MakeMessage("Wolf", 32767);
        var encoder = new FountainEncoder(message, 1000);
        var decoder = new FountainDecoder();
        var skip = false;
        while (!decoder.IsComplete)
        {
            var part = encoder.NextPart();
            if (!skip)
            {
                decoder.Receive(part);
            }
            skip = !skip;
        }
        Assert.Equal(message, decoder.Message());
    }

    [Fact]
    public void FountainPartCbor()
    {
        var part = new FountainPart(12, 8, 100, 0x12345678, [1, 5, 3, 3, 5]);
        var cbor = part.ToCbor();
        var part2 = FountainPart.FromCbor(cbor);
        var cbor2 = part2.ToCbor();
        Assert.Equal(cbor, cbor2);
    }

    [Fact]
    public void FountainEncoderEmptyMessage()
    {
        Assert.Throws<FountainException>(() => new FountainEncoder([], 1));
    }

    [Fact]
    public void FountainEncoderZeroMaxLength()
    {
        Assert.Throws<FountainException>(() => new FountainEncoder([1, 2, 3], 0));
    }

    [Fact]
    public void FountainDecoderReceiveReturnValue()
    {
        var message = TestHelpers.MakeMessage("Wolf", 1000);
        var encoder = new FountainEncoder(message, 10);
        var decoder = new FountainDecoder();

        var part = encoder.NextPart();
        Assert.Equal(new byte[] { 0x91, 0x6e, 0xc6, 0x5c, 0xf7, 0x7c, 0xad, 0xf5, 0x5c, 0xd7 }, part.Data);

        Assert.True(decoder.Receive(part));
        // Same indexes
        Assert.False(decoder.Receive(new FountainPart(part.Sequence, part.SequenceCount, part.MessageLength, part.Checksum, (byte[])part.Data.Clone())));

        // Inconsistent part
        var badPart = encoder.NextPart();
        var badPartModified = new FountainPart(badPart.Sequence, badPart.SequenceCount, badPart.MessageLength, badPart.Checksum + 1, (byte[])badPart.Data.Clone());
        Assert.Throws<FountainException>(() => decoder.Receive(badPartModified));

        // Complete
        while (!decoder.IsComplete)
        {
            decoder.Receive(encoder.NextPart());
        }
        Assert.False(decoder.Receive(encoder.NextPart()));
    }

    [Fact]
    public void FountainDecoderPartValidation()
    {
        var encoder = new FountainEncoder([0x66, 0x6F, 0x6F], 2); // "foo"
        var decoder = new FountainDecoder();
        var part = encoder.NextPart();
        Assert.True(decoder.Receive(part.Clone()));
        Assert.True(decoder.Validate(part));

        var p1 = new FountainPart(part.Sequence, part.SequenceCount, part.MessageLength, part.Checksum + 1, (byte[])part.Data.Clone());
        Assert.False(decoder.Validate(p1));

        var p2 = new FountainPart(part.Sequence, part.SequenceCount, part.MessageLength + 1, part.Checksum, (byte[])part.Data.Clone());
        Assert.False(decoder.Validate(p2));

        var p3 = new FountainPart(part.Sequence, part.SequenceCount + 1, part.MessageLength, part.Checksum, (byte[])part.Data.Clone());
        Assert.False(decoder.Validate(p3));

        var extendedData = new byte[part.Data.Length + 1];
        Array.Copy(part.Data, extendedData, part.Data.Length);
        extendedData[^1] = 1;
        var p4 = new FountainPart(part.Sequence, part.SequenceCount, part.MessageLength, part.Checksum, extendedData);
        Assert.False(decoder.Validate(p4));
    }

    [Fact]
    public void FountainDecoderEmptyPart()
    {
        var decoder = new FountainDecoder();
        var part = new FountainPart(12, 8, 100, 0x12345678, [1, 5, 3, 3, 5]);

        // sequence_count = 0
        var p1 = new FountainPart(12, 0, 100, 0x12345678, [1, 5, 3, 3, 5]);
        Assert.Throws<FountainException>(() => decoder.Receive(p1));

        // message_length = 0
        var p2 = new FountainPart(12, 8, 0, 0x12345678, [1, 5, 3, 3, 5]);
        Assert.Throws<FountainException>(() => decoder.Receive(p2));

        // empty data
        var p3 = new FountainPart(12, 8, 100, 0x12345678, []);
        Assert.Throws<FountainException>(() => decoder.Receive(p3));

        // Fresh decoder does not validate
        Assert.False(decoder.Validate(part));
    }
}
