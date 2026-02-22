import Testing
@testable import BCUR

struct FountainTests {
    @Test func testFragmentLength() {
        #expect(FountainUtils.fragmentLength(dataLength: 12_345, maxFragmentLength: 1_955) == 1_764)
        #expect(FountainUtils.fragmentLength(dataLength: 12_345, maxFragmentLength: 30_000) == 12_345)

        #expect(FountainUtils.fragmentLength(dataLength: 10, maxFragmentLength: 4) == 4)
        #expect(FountainUtils.fragmentLength(dataLength: 10, maxFragmentLength: 5) == 5)
        #expect(FountainUtils.fragmentLength(dataLength: 10, maxFragmentLength: 6) == 5)
        #expect(FountainUtils.fragmentLength(dataLength: 10, maxFragmentLength: 10) == 10)
    }

    @Test func testPartitionAndJoin() {
        let message = Xoshiro256.makeMessage(seed: "Wolf", size: 1024)
        let fragmentLength = FountainUtils.fragmentLength(dataLength: message.count, maxFragmentLength: 100)
        let fragments = FountainUtils.partition(message, fragmentLength: fragmentLength)

        let expectedFragments = [
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
            "170010067e2e75ebe2d2904aeb1f89d5dc98cd4a6f2faaa8be6d03354c990fd895a97feb54668473e9d942bb99e196d897e8f1b01625cf48a7b78d249bb4985c065aa8cd1402ed2ba1b6f908f63dcd84b66425df00000000000000000000",
        ]

        #expect(fragments.count == expectedFragments.count)
        for (fragment, expected) in zip(fragments, expectedFragments) {
            #expect(bytesToHex(fragment) == expected)
        }

        let joined = fragments.flatMap { $0 }.prefix(message.count)
        #expect(Array(joined) == message)
    }

    @Test func testChooseFragments() {
        let message = Xoshiro256.makeMessage(seed: "Wolf", size: 1024)
        let checksum = Crc32.checksum(message)
        let fragmentLength = FountainUtils.fragmentLength(dataLength: message.count, maxFragmentLength: 100)
        let fragments = FountainUtils.partition(message, fragmentLength: fragmentLength)

        let expected = [
            [0], [1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [9],
            [2, 5, 6, 8, 9, 10], [8], [1, 5], [1], [0, 2, 4, 5, 8, 10], [5], [2], [2],
            [0, 1, 3, 4, 5, 7, 9, 10], [0, 1, 2, 3, 5, 6, 8, 9, 10],
            [0, 2, 4, 5, 7, 8, 9, 10], [3, 5], [4],
            [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10], [0, 1, 3, 4, 5, 6, 7, 9, 10],
            [6], [5, 6], [7],
        ]

        for sequence in 1...30 {
            let indexes = FountainUtils.chooseFragments(
                sequence: sequence,
                fragmentCount: fragments.count,
                checksum: checksum
            ).sorted()
            #expect(indexes == expected[sequence - 1])
        }
    }

    @Test func testXor() {
        var rng = Xoshiro256.fromString("Wolf")

        let data1 = rng.nextBytes(10)
        #expect(bytesToHex(data1) == "916ec65cf77cadf55cd7")

        let data2 = rng.nextBytes(10)
        #expect(bytesToHex(data2) == "f9cda1a1030026ddd42e")

        var data3 = data1
        FountainUtils.xorInPlace(&data3, with: data2)
        #expect(bytesToHex(data3) == "68a367fdf47c8b2888f9")

        FountainUtils.xorInPlace(&data3, with: data1)
        #expect(data3 == data2)
    }

    @Test func testFountainEncoder() throws {
        let message = Xoshiro256.makeMessage(seed: "Wolf", size: 256)
        var encoder = try FountainEncoder(message: message, maxFragmentLength: 30)

        let expectedParts: [FountainPart] = [
            FountainPart(sequence: 1, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("916ec65cf77cadf55cd7f9cda1a1030026ddd42e905b77adc36e4f2d3c")),
            FountainPart(sequence: 2, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("cba44f7f04f2de44f42d84c374a0e149136f25b01852545961d55f7f7a")),
            FountainPart(sequence: 3, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("8cde6d0e2ec43f3b2dcb644a2209e8c9e34af5c4747984a5e873c9cf5f")),
            FountainPart(sequence: 4, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("965e25ee29039fdf8ca74f1c769fc07eb7ebaec46e0695aea6cbd60b3e")),
            FountainPart(sequence: 5, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("c4bbff1b9ffe8a9e7240129377b9d3711ed38d412fbb4442256f1e6f59")),
            FountainPart(sequence: 6, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("5e0fc57fed451fb0a0101fb76b1fb1e1b88cfdfdaa946294a47de8fff1")),
            FountainPart(sequence: 7, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5")),
            FountainPart(sequence: 8, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("791457c9876dd34aadd192a53aa0dc66b556c0c215c7ceb8248b717c22")),
            FountainPart(sequence: 9, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("951e65305b56a3706e3e86eb01c803bbf915d80edcd64d4d0000000000")),
            FountainPart(sequence: 10, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("330f0f33a05eead4f331df229871bee733b50de71afd2e5a79f196de09")),
            FountainPart(sequence: 11, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("3b205ce5e52d8c24a52cffa34c564fa1af3fdffcd349dc4258ee4ee828")),
            FountainPart(sequence: 12, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("dd7bf725ea6c16d531b5f03254783803048ca08b87148daacd1cd7a006")),
            FountainPart(sequence: 13, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("760be7ad1c6187902bbc04f539b9ee5eb8ea6833222edea36031306c01")),
            FountainPart(sequence: 14, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("5bf4031217d2c3254b088fa7553778b5003632f46e21db129416f65b55")),
            FountainPart(sequence: 15, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5")),
            FountainPart(sequence: 16, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("b8546ebfe2048541348910267331c643133f828afec9337c318f71b7df")),
            FountainPart(sequence: 17, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("23dedeea74e3a0fb052befabefa13e2f80e4315c9dceed4c8630612e64")),
            FountainPart(sequence: 18, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("d01a8daee769ce34b6b35d3ca0005302724abddae405bdb419c0a6b208")),
            FountainPart(sequence: 19, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("3171c5dc365766eff25ae47c6f10e7de48cfb8474e050e5fe997a6dc24")),
            FountainPart(sequence: 20, sequenceCount: 9, messageLength: 256, checksum: 23_570_951, data: hexToBytes("e055c2433562184fa71b4be94f262e200f01c6f74c284b0dc6fae6673f")),
        ]

        for (index, expected) in expectedParts.enumerated() {
            #expect(encoder.currentSequence == index)
            #expect(encoder.nextPart() == expected)
        }
    }

    @Test func testFountainEncoderCbor() throws {
        let maxFragmentLength = 30
        let size = 256
        let message = Xoshiro256.makeMessage(seed: "Wolf", size: size)
        var encoder = try FountainEncoder(message: message, maxFragmentLength: maxFragmentLength)

        let expectedParts = [
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
            "8514091901001a0167aa07581de055c2433562184fa71b4be94f262e200f01c6f74c284b0dc6fae6673f",
        ]

        #expect(encoder.fragmentCount == (size / maxFragmentLength + 1))
        for expected in expectedParts {
            #expect(bytesToHex(try encoder.nextPart().cborEncoded()) == expected)
        }
    }

    @Test func testFountainEncoderZeroMaxLength() {
        #expect(throws: FountainError.self) {
            _ = try FountainEncoder(message: Array("foo".utf8), maxFragmentLength: 0)
        }
    }

    @Test func testFountainEncoderIsComplete() throws {
        let message = Xoshiro256.makeMessage(seed: "Wolf", size: 256)
        var encoder = try FountainEncoder(message: message, maxFragmentLength: 30)

        for _ in 0..<encoder.fragmentCount {
            _ = encoder.nextPart()
        }

        #expect(encoder.complete)
    }

    @Test func testDecoder() throws {
        let message = Xoshiro256.makeMessage(seed: "Wolf", size: 32_767)
        var encoder = try FountainEncoder(message: message, maxFragmentLength: 1_000)
        var decoder = FountainDecoder()

        while !decoder.complete {
            #expect(try decoder.message() == nil)
            let part = encoder.nextPart()
            _ = try decoder.receive(part)
        }

        #expect(try decoder.message() == message)
    }

    @Test func testEmptyEncoder() {
        #expect(throws: FountainError.self) {
            _ = try FountainEncoder(message: [], maxFragmentLength: 1)
        }
    }

    @Test func testDecoderSkipSomeSimpleFragments() throws {
        let message = Xoshiro256.makeMessage(seed: "Wolf", size: 32_767)
        var encoder = try FountainEncoder(message: message, maxFragmentLength: 1_000)
        var decoder = FountainDecoder()
        var skip = false

        while !decoder.complete {
            let part = encoder.nextPart()
            if !skip {
                _ = try decoder.receive(part)
            }
            skip.toggle()
        }

        #expect(try decoder.message() == message)
    }

    @Test func testDecoderReceiveReturnValue() throws {
        let message = Xoshiro256.makeMessage(seed: "Wolf", size: 1_000)
        var encoder = try FountainEncoder(message: message, maxFragmentLength: 10)
        var decoder = FountainDecoder()

        let firstPart = encoder.nextPart()
        #expect(firstPart.data == [0x91, 0x6e, 0xc6, 0x5c, 0xf7, 0x7c, 0xad, 0xf5, 0x5c, 0xd7])
        #expect(try decoder.receive(firstPart))
        #expect(!(try decoder.receive(firstPart)))

        var inconsistent = encoder.nextPart()
        inconsistent.checksum &+= 1
        #expect(throws: FountainError.self) {
            _ = try decoder.receive(inconsistent)
        }

        while !decoder.complete {
            _ = try decoder.receive(encoder.nextPart())
        }

        #expect(!(try decoder.receive(encoder.nextPart())))
    }

    @Test func testDecoderPartValidation() throws {
        var encoder = try FountainEncoder(message: Array("foo".utf8), maxFragmentLength: 2)
        var decoder = FountainDecoder()

        var part = encoder.nextPart()
        #expect(try decoder.receive(part))
        #expect(decoder.validate(part))

        part.checksum &+= 1
        #expect(!decoder.validate(part))
        part.checksum &-= 1

        part.messageLength += 1
        #expect(!decoder.validate(part))
        part.messageLength -= 1

        part.sequenceCount += 1
        #expect(!decoder.validate(part))
        part.sequenceCount -= 1

        part.data.append(1)
        #expect(!decoder.validate(part))
    }

    @Test func testEmptyDecoderEmptyPart() {
        var decoder = FountainDecoder()
        var part = FountainPart(
            sequence: 12,
            sequenceCount: 8,
            messageLength: 100,
            checksum: 0x1234_5678,
            data: [1, 5, 3, 3, 5]
        )

        part.sequenceCount = 0
        #expect(throws: FountainError.self) {
            _ = try decoder.receive(part)
        }
        part.sequenceCount = 8

        part.messageLength = 0
        #expect(throws: FountainError.self) {
            _ = try decoder.receive(part)
        }
        part.messageLength = 100

        part.data = []
        #expect(throws: FountainError.self) {
            _ = try decoder.receive(part)
        }

        part.data = [1, 5, 3, 3, 5]
        #expect(!decoder.validate(part))
    }

    @Test func testFountainCbor() throws {
        let part = FountainPart(
            sequence: 12,
            sequenceCount: 8,
            messageLength: 100,
            checksum: 0x1234_5678,
            data: [1, 5, 3, 3, 5]
        )

        let cbor = try part.cborEncoded()
        let part2 = try FountainPart(cborBytes:cbor)
        let cbor2 = try part2.cborEncoded()
        #expect(cbor == cbor2)
    }

    @Test func testPartFromCborErrors() {
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[0x18])
        }
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[0x01])
        }
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[0x84, 0x01, 0x02, 0x03, 0x04])
        }
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[0x86, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06])
        }

        // First four entries must be unsigned integers.
        var cbor: [UInt8] = [0x85, 0x01, 0x02, 0x03, 0x04, 0x41, 0x05]
        for index in 1...4 {
            #expect((try? FountainPart(cborBytes:cbor)) != nil)
            cbor[index] = 0x41
            #expect(throws: FountainError.self) {
                _ = try FountainPart(cborBytes:cbor)
            }
            cbor[index] = UInt8(index)
        }

        // Fifth entry must be bytes.
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[0x85, 0x01, 0x02, 0x03, 0x04, 0x05])
        }
    }

    @Test func testPartFromCborUnsignedTypes() {
        // u8
        #expect((try? FountainPart(cborBytes:[0x85, 0x01, 0x02, 0x03, 0x04, 0x41, 0x05])) != nil)

        // u16
        #expect((try? FountainPart(cborBytes:[
            0x85, 0x19, 0x01, 0x02, 0x19, 0x03, 0x04, 0x19, 0x05, 0x06, 0x19, 0x07, 0x08, 0x41, 0x05,
        ])) != nil)

        // u32
        #expect((try? FountainPart(cborBytes:[
            0x85, 0x1a, 0x01, 0x02, 0x03, 0x04, 0x1a, 0x05, 0x06, 0x07, 0x08,
            0x1a, 0x09, 0x10, 0x11, 0x12, 0x1a, 0x13, 0x14, 0x15, 0x16, 0x41, 0x05,
        ])) != nil)

        // u64 out-of-range for u32 fields.
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[
                0x85, 0x1b, 0x01, 0x02, 0x03, 0x04, 0x0a, 0x0b, 0x0c, 0x0d,
                0x1a, 0x05, 0x06, 0x07, 0x08, 0x1a, 0x09, 0x10, 0x11, 0x12,
                0x1a, 0x13, 0x14, 0x15, 0x16, 0x41, 0x05,
            ])
        }
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[
                0x85, 0x1a, 0x01, 0x02, 0x03, 0x04,
                0x1b, 0x05, 0x06, 0x07, 0x08, 0x0a, 0x0b, 0x0c, 0x0d,
                0x1a, 0x09, 0x10, 0x11, 0x12, 0x1a, 0x13, 0x14, 0x15, 0x16,
                0x41, 0x05,
            ])
        }
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[
                0x85, 0x1a, 0x01, 0x02, 0x03, 0x04, 0x1a, 0x05, 0x06, 0x07, 0x08,
                0x1b, 0x09, 0x10, 0x11, 0x12, 0x0a, 0x0b, 0x0c, 0x0d,
                0x1a, 0x13, 0x14, 0x15, 0x16, 0x41, 0x05,
            ])
        }
        #expect(throws: FountainError.self) {
            _ = try FountainPart(cborBytes:[
                0x85, 0x1a, 0x01, 0x02, 0x03, 0x04, 0x1a, 0x05, 0x06, 0x07, 0x08,
                0x1a, 0x09, 0x10, 0x11, 0x12,
                0x1b, 0x13, 0x14, 0x15, 0x16, 0x0a, 0x0b, 0x0c, 0x0d,
                0x41, 0x05,
            ])
        }
    }
}
