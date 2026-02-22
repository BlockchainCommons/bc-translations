package com.blockchaincommons.bcur

import kotlin.test.Test
import kotlin.test.assertEquals

class FountainTest {

    @Test
    fun testFragmentLength() {
        assertEquals(1764, FountainUtils.fragmentLength(12345, 1955))
        assertEquals(12345, FountainUtils.fragmentLength(12345, 30000))

        assertEquals(4, FountainUtils.fragmentLength(10, 4))
        assertEquals(5, FountainUtils.fragmentLength(10, 5))
        assertEquals(5, FountainUtils.fragmentLength(10, 6))
        assertEquals(10, FountainUtils.fragmentLength(10, 10))
    }

    @Test
    fun testPartitionAndJoin() {
        val message = Xoshiro256.makeMessage("Wolf", 1024)
        val fragmentLen = FountainUtils.fragmentLength(message.size, 100)
        val fragments = FountainUtils.partition(message, fragmentLen)

        val expectedFragments = listOf(
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
        )

        assertEquals(expectedFragments.size, fragments.size)
        for ((fragment, expectedHex) in fragments.zip(expectedFragments)) {
            assertEquals(expectedHex, fragment.toHexString())
        }

        // Verify round-trip join
        val joined = fragments.reduce { acc, bytes -> acc + bytes }
            .copyOfRange(0, message.size)
        assertEquals(message.toList(), joined.toList())
    }

    @Test
    fun testChooseFragments() {
        val message = Xoshiro256.makeMessage("Wolf", 1024)
        val checksum = Crc32.checksum(message)
        val fragmentLen = FountainUtils.fragmentLength(message.size, 100)
        val fragments = FountainUtils.partition(message, fragmentLen)

        val expectedFragmentIndexes = listOf(
            listOf(0),
            listOf(1),
            listOf(2),
            listOf(3),
            listOf(4),
            listOf(5),
            listOf(6),
            listOf(7),
            listOf(8),
            listOf(9),
            listOf(10),
            listOf(9),
            listOf(2, 5, 6, 8, 9, 10),
            listOf(8),
            listOf(1, 5),
            listOf(1),
            listOf(0, 2, 4, 5, 8, 10),
            listOf(5),
            listOf(2),
            listOf(2),
            listOf(0, 1, 3, 4, 5, 7, 9, 10),
            listOf(0, 1, 2, 3, 5, 6, 8, 9, 10),
            listOf(0, 2, 4, 5, 7, 8, 9, 10),
            listOf(3, 5),
            listOf(4),
            listOf(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10),
            listOf(0, 1, 3, 4, 5, 6, 7, 9, 10),
            listOf(6),
            listOf(5, 6),
            listOf(7)
        )

        for (seqNum in 1..30) {
            val indexes = FountainUtils.chooseFragments(seqNum, fragments.size, checksum).sorted()
            assertEquals(expectedFragmentIndexes[seqNum - 1], indexes)
        }
    }

    @Test
    fun testXor() {
        val rng = Xoshiro256.fromString("Wolf")

        val data1 = rng.nextBytes(10)
        assertEquals("916ec65cf77cadf55cd7", data1.toHexString())

        val data2 = rng.nextBytes(10)
        assertEquals("f9cda1a1030026ddd42e", data2.toHexString())

        val data3 = data1.copyOf()
        FountainUtils.xorInPlace(data3, data2)
        assertEquals("68a367fdf47c8b2888f9", data3.toHexString())

        FountainUtils.xorInPlace(data3, data1)
        assertEquals(data2.toHexString(), data3.toHexString())
    }

    @Test
    fun testFountainEncoder() {
        val message = Xoshiro256.makeMessage("Wolf", 256)
        val encoder = FountainEncoder(message, 30)

        val expectedParts = listOf(
            FountainPart(
                sequence = 1, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "916ec65cf77cadf55cd7f9cda1a1030026ddd42e905b77adc36e4f2d3c".hexToByteArray()
            ),
            FountainPart(
                sequence = 2, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "cba44f7f04f2de44f42d84c374a0e149136f25b01852545961d55f7f7a".hexToByteArray()
            ),
            FountainPart(
                sequence = 3, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "8cde6d0e2ec43f3b2dcb644a2209e8c9e34af5c4747984a5e873c9cf5f".hexToByteArray()
            ),
            FountainPart(
                sequence = 4, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "965e25ee29039fdf8ca74f1c769fc07eb7ebaec46e0695aea6cbd60b3e".hexToByteArray()
            ),
            FountainPart(
                sequence = 5, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "c4bbff1b9ffe8a9e7240129377b9d3711ed38d412fbb4442256f1e6f59".hexToByteArray()
            ),
            FountainPart(
                sequence = 6, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "5e0fc57fed451fb0a0101fb76b1fb1e1b88cfdfdaa946294a47de8fff1".hexToByteArray()
            ),
            FountainPart(
                sequence = 7, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5".hexToByteArray()
            ),
            FountainPart(
                sequence = 8, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "791457c9876dd34aadd192a53aa0dc66b556c0c215c7ceb8248b717c22".hexToByteArray()
            ),
            FountainPart(
                sequence = 9, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "951e65305b56a3706e3e86eb01c803bbf915d80edcd64d4d0000000000".hexToByteArray()
            ),
            FountainPart(
                sequence = 10, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "330f0f33a05eead4f331df229871bee733b50de71afd2e5a79f196de09".hexToByteArray()
            ),
            FountainPart(
                sequence = 11, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "3b205ce5e52d8c24a52cffa34c564fa1af3fdffcd349dc4258ee4ee828".hexToByteArray()
            ),
            FountainPart(
                sequence = 12, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "dd7bf725ea6c16d531b5f03254783803048ca08b87148daacd1cd7a006".hexToByteArray()
            ),
            FountainPart(
                sequence = 13, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "760be7ad1c6187902bbc04f539b9ee5eb8ea6833222edea36031306c01".hexToByteArray()
            ),
            FountainPart(
                sequence = 14, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "5bf4031217d2c3254b088fa7553778b5003632f46e21db129416f65b55".hexToByteArray()
            ),
            FountainPart(
                sequence = 15, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "73f021c0e6f65b05c0a494e50791270a0050a73ae69b6725505a2ec8a5".hexToByteArray()
            ),
            FountainPart(
                sequence = 16, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "b8546ebfe2048541348910267331c643133f828afec9337c318f71b7df".hexToByteArray()
            ),
            FountainPart(
                sequence = 17, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "23dedeea74e3a0fb052befabefa13e2f80e4315c9dceed4c8630612e64".hexToByteArray()
            ),
            FountainPart(
                sequence = 18, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "d01a8daee769ce34b6b35d3ca0005302724abddae405bdb419c0a6b208".hexToByteArray()
            ),
            FountainPart(
                sequence = 19, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "3171c5dc365766eff25ae47c6f10e7de48cfb8474e050e5fe997a6dc24".hexToByteArray()
            ),
            FountainPart(
                sequence = 20, sequenceCount = 9, messageLength = 256,
                checksum = 23_570_951u,
                data = "e055c2433562184fa71b4be94f262e200f01c6f74c284b0dc6fae6673f".hexToByteArray()
            )
        )

        for ((i, expected) in expectedParts.withIndex()) {
            assertEquals(i, encoder.currentSequence)
            assertEquals(expected, encoder.nextPart())
        }
    }

    @Test
    fun testFountainEncoderCbor() {
        val maxFragmentLength = 30
        val size = 256
        val message = Xoshiro256.makeMessage("Wolf", size)
        val encoder = FountainEncoder(message, maxFragmentLength)

        val expectedParts = listOf(
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
        )

        assertEquals(size / maxFragmentLength + 1, encoder.fragmentCount)
        for (e in expectedParts) {
            assertEquals(e, encoder.nextPart().toCbor().toHexString())
        }
    }

    @Test
    fun testFountainDecoder() {
        val seed = "Wolf"
        val messageSize = 32767
        val maxFragmentLength = 1000

        val message = Xoshiro256.makeMessage(seed, messageSize)
        val encoder = FountainEncoder(message, maxFragmentLength)
        val decoder = FountainDecoder()

        while (!decoder.isComplete) {
            assertEquals(null, decoder.message())
            val part = encoder.nextPart()
            decoder.receive(part)
        }
        assertEquals(message.toList(), decoder.message()!!.toList())
    }

    @Test
    fun testFountainDecoderSkip() {
        val seed = "Wolf"
        val messageSize = 32767
        val maxFragmentLength = 1000

        val message = Xoshiro256.makeMessage(seed, messageSize)
        val encoder = FountainEncoder(message, maxFragmentLength)
        val decoder = FountainDecoder()
        var skip = false

        while (!decoder.isComplete) {
            val part = encoder.nextPart()
            if (!skip) {
                decoder.receive(part)
            }
            skip = !skip
        }
        assertEquals(message.toList(), decoder.message()!!.toList())
    }

    @Test
    fun testFountainCbor() {
        val part = FountainPart(
            sequence = 12,
            sequenceCount = 8,
            messageLength = 100,
            checksum = 0x1234_5678u,
            data = byteArrayOf(1, 5, 3, 3, 5)
        )
        val cbor = part.toCbor()
        val part2 = FountainPart.fromCbor(cbor)
        val cbor2 = part2.toCbor()
        assertEquals(cbor.toList(), cbor2.toList())
    }
}
