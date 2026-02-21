package com.blockchaincommons.bcshamir

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals

private class FakeRandomNumberGenerator : RandomNumberGenerator() {
    override fun nextU32(): UInt = throw NotImplementedError("nextU32 is not used in these tests")

    override fun nextU64(): ULong = throw NotImplementedError("nextU64 is not used in these tests")

    override fun fillRandomData(data: ByteArray) {
        var b = 0
        for (i in data.indices) {
            data[i] = b.toByte()
            b = (b + 17) and 0xFF
        }
    }
}

private fun hex(hex: String): ByteArray {
    require((hex.length and 1) == 0)
    val bytes = ByteArray(hex.length / 2)
    for (i in bytes.indices) {
        val n = hex.substring(i * 2, i * 2 + 2).toInt(16)
        bytes[i] = n.toByte()
    }
    return bytes
}

class ShamirTest {
    @Test
    fun testSplitSecret35() {
        val rng = FakeRandomNumberGenerator()
        val secret = hex("0ff784df000c4380a5ed683f7e6e3dcf")
        val shares = splitSecret(3, 5, secret, rng)
        assertEquals(5, shares.size)

        assertContentEquals(hex("00112233445566778899aabbccddeeff"), shares[0])
        assertContentEquals(hex("d43099fe444807c46921a4f33a2a798b"), shares[1])
        assertContentEquals(hex("d9ad4e3bec2e1a7485698823abf05d36"), shares[2])
        assertContentEquals(hex("0d8cf5f6ec337bc764d1866b5d07ca42"), shares[3])
        assertContentEquals(hex("1aa7fe3199bc5092ef3816b074cabdf2"), shares[4])

        val recoveredShareIndexes = listOf(1, 2, 4)
        val recoveredShares = recoveredShareIndexes.map { shares[it].copyOf() }
        val recoveredSecret = recoverSecret(recoveredShareIndexes, recoveredShares)
        assertContentEquals(secret, recoveredSecret)
    }

    @Test
    fun testSplitSecret27() {
        val rng = FakeRandomNumberGenerator()
        val secret = hex("204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a")
        val shares = splitSecret(2, 7, secret, rng)
        assertEquals(7, shares.size)

        assertContentEquals(hex("2dcd14c2252dc8489af3985030e74d5a48e8eff1478ab86e65b43869bf39d556"), shares[0])
        assertContentEquals(hex("a1dfdd798388aada635b9974472b4fc59a32ae520c42c9f6a0af70149b882487"), shares[1])
        assertContentEquals(hex("2ee99daf727c0c7773b89a18de64497ff7476dacd1015a45f482a893f7402cef"), shares[2])
        assertContentEquals(hex("a2fb5414d4d96ee58a109b3ca9a84be0259d2c0f9ac92bdd3199e0eed3f1dd3e"), shares[3])
        assertContentEquals(hex("2b851d188b8f5b3653659cc0f7fa45102dadf04b708767385cd803862fcb3c3f"), shares[4])
        assertContentEquals(hex("a797d4a32d2a39a4aacd9de48036478fff77b1e83b4f16a099c34bfb0b7acdee"), shares[5])
        assertContentEquals(hex("28a19475dcde9f09ba2e9e881979413592027216e60c8513cdee937c67b2c586"), shares[6])

        val recoveredShareIndexes = listOf(3, 4)
        val recoveredShares = recoveredShareIndexes.map { shares[it].copyOf() }
        val recoveredSecret = recoverSecret(recoveredShareIndexes, recoveredShares)
        assertContentEquals(secret, recoveredSecret)
    }

    @Test
    fun exampleSplit() {
        val threshold = 2
        val shareCount = 3
        val secret = "my secret belongs to me.".encodeToByteArray()
        val randomGenerator = SecureRandomNumberGenerator()

        val shares = splitSecret(threshold, shareCount, secret, randomGenerator)
        assertEquals(shareCount, shares.size)
    }

    @Test
    fun exampleRecover() {
        val indexes = listOf(0, 2)
        val shares = listOf(
            byteArrayOf(
                47,
                -91,
                102,
                -24,
                -38,
                99,
                6,
                94,
                39,
                6,
                -3,
                -41,
                12,
                88,
                64,
                32,
                105,
                40,
                -34,
                -110,
                93,
                -59,
                48,
                -127,
            ),
            byteArrayOf(
                -35,
                -82,
                116,
                -55,
                90,
                99,
                -120,
                33,
                64,
                -41,
                60,
                84,
                -49,
                28,
                74,
                10,
                111,
                -13,
                43,
                -32,
                48,
                64,
                -57,
                -84,
            ),
        )

        val secret = recoverSecret(indexes, shares)
        assertContentEquals("my secret belongs to me.".encodeToByteArray(), secret)
    }
}
