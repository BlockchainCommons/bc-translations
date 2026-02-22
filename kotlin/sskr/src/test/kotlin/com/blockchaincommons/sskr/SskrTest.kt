package com.blockchaincommons.sskr

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.fakeRandomNumberGenerator
import com.blockchaincommons.bcrand.nextInClosedRange
import kotlin.test.Test
import kotlin.test.assertEquals

/**
 * Deterministic fake RNG for SSKR test vectors.
 *
 * Fills bytes by incrementing a counter by 17 (mod 256) per byte, matching
 * the Rust test suite's `FakeRandomNumberGenerator`.
 */
private class FakeRandomNumberGenerator : RandomNumberGenerator() {
    override fun nextU32(): UInt = throw NotImplementedError("nextU32 is not used in these tests")

    override fun nextU64(): ULong = throw NotImplementedError("nextU64 is not used in these tests")

    override fun randomData(size: Int): ByteArray {
        val data = ByteArray(size)
        fillRandomData(data)
        return data
    }

    override fun fillRandomData(data: ByteArray) {
        var b = 0
        for (i in data.indices) {
            data[i] = b.toByte()
            b = (b + 17) and 0xFF
        }
    }
}

@OptIn(ExperimentalStdlibApi::class)
private fun hex(s: String): ByteArray = s.hexToByteArray()

private fun <T> fisherYatesShuffle(list: MutableList<T>, rng: RandomNumberGenerator) {
    var i = list.size
    while (i > 1) {
        i -= 1
        val j = rng.nextInClosedRange(0, i.toLong()).toInt()
        val temp = list[i]
        list[i] = list[j]
        list[j] = temp
    }
}

private class RecoverSpec(
    val secret: Secret,
    val spec: Spec,
    val shares: List<List<ByteArray>>,
    rng: RandomNumberGenerator,
) {
    val recoveredGroupIndexes: List<Int>
    val recoveredMemberIndexes: List<List<Int>>
    val recoveredShares: List<ByteArray>

    init {
        val groupIndexes = (0 until spec.groupCount).toMutableList()
        fisherYatesShuffle(groupIndexes, rng)
        recoveredGroupIndexes = groupIndexes.take(spec.groupThreshold)

        val recoveredMemberIndexesMutable = ArrayList<List<Int>>(spec.groupThreshold)
        for (groupIndex in recoveredGroupIndexes) {
            val group = spec.groups[groupIndex]
            val memberIndexes = (0 until group.memberCount).toMutableList()
            fisherYatesShuffle(memberIndexes, rng)
            recoveredMemberIndexesMutable.add(memberIndexes.take(group.memberThreshold))
        }
        recoveredMemberIndexes = recoveredMemberIndexesMutable

        val recoveredSharesMutable = ArrayList<ByteArray>()
        for ((i, recoveredGroupIndex) in recoveredGroupIndexes.withIndex()) {
            val groupShares = shares[recoveredGroupIndex]
            for (recoveredMemberIndex in recoveredMemberIndexes[i]) {
                recoveredSharesMutable.add(groupShares[recoveredMemberIndex].copyOf())
            }
        }
        fisherYatesShuffle(recoveredSharesMutable, rng)
        recoveredShares = recoveredSharesMutable
    }

    fun printState() {
        println("---")
        println("secret: ${secret.toByteArray().joinToString(separator = "") { "%02x".format(it) }}")
        println("spec: $spec")
        println("shares: $shares")
        println("recoveredGroupIndexes: $recoveredGroupIndexes")
        println("recoveredMemberIndexes: $recoveredMemberIndexes")
        println("recoveredShares: $recoveredShares")
    }

    fun recover() {
        val success = try {
            sskrCombine(recoveredShares) == secret
        } catch (error: Exception) {
            println("error: $error")
            false
        }

        if (!success) {
            printState()
            throw AssertionError("Recovery failed")
        }
    }
}

private fun oneFuzzTest(rng: RandomNumberGenerator) {
    val secretLength = rng.nextInClosedRange(MIN_SECRET_LEN.toLong(), MAX_SECRET_LEN.toLong()).toInt() and 0xFFFE
    val secret = Secret(rng.randomData(secretLength))

    val groupCount = rng.nextInClosedRange(1, MAX_GROUPS_COUNT.toLong()).toInt()
    val groupSpecs = (0 until groupCount).map {
        val memberCount = rng.nextInClosedRange(1, MAX_SHARE_COUNT.toLong()).toInt()
        val memberThreshold = rng.nextInClosedRange(1, memberCount.toLong()).toInt()
        GroupSpec(memberThreshold, memberCount)
    }
    val groupThreshold = rng.nextInClosedRange(1, groupCount.toLong()).toInt()
    val spec = Spec(groupThreshold, groupSpecs)

    val shares = sskrGenerateUsing(spec, secret, rng)

    val recoverSpec = RecoverSpec(secret, spec, shares, rng)
    recoverSpec.recover()
}

class SskrTest {
    // Rust metadata sync tests (`test_readme_deps`, `test_html_root_url`) are
    // intentionally omitted because they are Rust-tooling checks.

    @Test
    fun testSplit3of5() {
        val rng = FakeRandomNumberGenerator()
        val secret = Secret(hex("0ff784df000c4380a5ed683f7e6e3dcf"))
        val group = GroupSpec(3, 5)
        val spec = Spec(1, listOf(group))
        val shares = sskrGenerateUsing(spec, secret, rng)
        val flattenedShares = shares.flatten()

        assertEquals(5, flattenedShares.size)
        for (share in flattenedShares) {
            assertEquals(METADATA_SIZE_BYTES + secret.length, share.size)
        }

        val recoveredShareIndexes = listOf(1, 2, 4)
        val recoveredShares = recoveredShareIndexes.map { index ->
            flattenedShares[index].copyOf()
        }
        val recoveredSecret = sskrCombine(recoveredShares)
        assertEquals(secret, recoveredSecret)
    }

    @Test
    fun testSplit2of7() {
        val rng = FakeRandomNumberGenerator()
        val secret = Secret(
            hex("204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a"),
        )
        val group = GroupSpec(2, 7)
        val spec = Spec(1, listOf(group))
        val shares = sskrGenerateUsing(spec, secret, rng)

        assertEquals(1, shares.size)
        assertEquals(7, shares[0].size)

        val flattenedShares = shares.flatten()
        assertEquals(7, flattenedShares.size)

        for (share in flattenedShares) {
            assertEquals(METADATA_SIZE_BYTES + secret.length, share.size)
        }

        val recoveredShareIndexes = listOf(3, 4)
        val recoveredShares = recoveredShareIndexes.map { index ->
            flattenedShares[index].copyOf()
        }
        val recoveredSecret = sskrCombine(recoveredShares)
        assertEquals(secret, recoveredSecret)
    }

    @Test
    fun testSplit2of3and2of3() {
        val rng = FakeRandomNumberGenerator()
        val secret = Secret(
            hex("204188bfa6b440a1bdfd6753ff55a8241e07af5c5be943db917e3efabc184b1a"),
        )
        val group1 = GroupSpec(2, 3)
        val group2 = GroupSpec(2, 3)
        val spec = Spec(2, listOf(group1, group2))
        val shares = sskrGenerateUsing(spec, secret, rng)

        assertEquals(2, shares.size)
        assertEquals(3, shares[0].size)
        assertEquals(3, shares[1].size)

        val flattenedShares = shares.flatten()
        assertEquals(6, flattenedShares.size)

        for (share in flattenedShares) {
            assertEquals(METADATA_SIZE_BYTES + secret.length, share.size)
        }

        val recoveredShareIndexes = listOf(0, 1, 3, 5)
        val recoveredShares = recoveredShareIndexes.map { index ->
            flattenedShares[index].copyOf()
        }
        val recoveredSecret = sskrCombine(recoveredShares)
        assertEquals(secret, recoveredSecret)
    }

    @Test
    fun testShuffle() {
        val rng = fakeRandomNumberGenerator()
        val values = (0 until 100).toMutableList()
        fisherYatesShuffle(values, rng)

        assertEquals(100, values.size)
        assertEquals(
            listOf(
                79, 70, 40, 53, 25, 30, 31, 88, 10, 1, 45, 54, 81, 58, 55, 59,
                69, 78, 65, 47, 75, 61, 0, 72, 20, 9, 80, 13, 73, 11, 60, 56,
                19, 42, 33, 12, 36, 38, 6, 35, 68, 77, 50, 18, 97, 49, 98, 85,
                89, 91, 15, 71, 99, 67, 84, 23, 64, 14, 57, 48, 62, 29, 28, 94,
                44, 8, 66, 34, 43, 21, 63, 16, 92, 95, 27, 51, 26, 86, 22, 41,
                93, 82, 7, 87, 74, 37, 46, 3, 96, 24, 90, 39, 32, 17, 76, 4,
                83, 2, 52, 5,
            ),
            values,
        )
    }

    @Test
    fun testFuzz() {
        val rng = fakeRandomNumberGenerator()
        repeat(100) {
            oneFuzzTest(rng)
        }
    }

    @Test
    fun testExampleEncode() {
        val secretString = "my secret belongs to me.".encodeToByteArray()
        val secret = Secret(secretString)

        val group1 = GroupSpec(2, 3)
        val group2 = GroupSpec(3, 5)
        val spec = Spec(2, listOf(group1, group2))

        val shares = sskrGenerate(spec, secret)

        assertEquals(2, shares.size)
        assertEquals(3, shares[0].size)
        assertEquals(5, shares[1].size)

        val recoveredShares = listOf(
            shares[0][0].copyOf(),
            shares[0][2].copyOf(),
            shares[1][0].copyOf(),
            shares[1][1].copyOf(),
            shares[1][4].copyOf(),
        )

        val recoveredSecret = sskrCombine(recoveredShares)
        assertEquals(secret, recoveredSecret)
    }

    @Test
    fun testExampleRoundtrip() {
        val text = "my secret belongs to me."

        fun roundtrip(m: Int, n: Int): Secret {
            val secret = Secret(text.encodeToByteArray())
            val spec = Spec(1, listOf(GroupSpec(m, n)))
            val shares = sskrGenerate(spec, secret)
            return sskrCombine(shares.flatten())
        }

        assertEquals(text, roundtrip(2, 3).toByteArray().decodeToString())
        assertEquals(text, roundtrip(1, 1).toByteArray().decodeToString())
        assertEquals(text, roundtrip(1, 3).toByteArray().decodeToString())
    }

    @Test
    fun testExampleIgnoreExtraGroup() {
        val text = "my secret belongs to me."
        val secret = Secret(text.encodeToByteArray())
        val spec = Spec(
            1,
            listOf(
                GroupSpec(2, 3),
                GroupSpec(2, 3),
            ),
        )
        val groupedShares = sskrGenerate(spec, secret)
        val flattenedShares = groupedShares.flatten()

        val recoveredShareIndexes = listOf(0, 1, 3)
        val recoveredShares = recoveredShareIndexes.map { index ->
            flattenedShares[index].copyOf()
        }

        // The group threshold is 1, but this includes an additional share from
        // the second group. Recovery must ignore any group's shares that cannot
        // be decoded and still return the correct master secret.
        val recoveredSecret = sskrCombine(recoveredShares)
        assertEquals(text, recoveredSecret.toByteArray().decodeToString())
    }
}
