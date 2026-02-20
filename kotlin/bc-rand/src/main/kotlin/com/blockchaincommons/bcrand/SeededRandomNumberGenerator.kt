package com.blockchaincommons.bcrand

/**
 * A deterministic PRNG for testing.
 *
 * Seeded with four [ULong] values. Uses Xoshiro256** internally.
 * NOT cryptographically secure.
 *
 * The [randomData] and [fillRandomData] methods generate each byte
 * individually from `nextU64() and 0xFF` to match the Swift implementation
 * and ensure cross-platform test-vector compatibility.
 */
class SeededRandomNumberGenerator(seed: ULongArray) : RandomNumberGenerator() {

    init {
        require(seed.size == 4) { "Seed must have exactly 4 ULong values" }
    }

    private val rng = Xoshiro256StarStar(seed[0], seed[1], seed[2], seed[3])

    override fun nextU32(): UInt = nextU64().toUInt()

    override fun nextU64(): ULong = rng.nextU64()

    override fun randomData(size: Int): ByteArray =
        ByteArray(size) { (nextU64() and 0xFFuL).toByte() }

    override fun fillRandomData(data: ByteArray) {
        for (i in data.indices) {
            data[i] = (nextU64() and 0xFFuL).toByte()
        }
    }
}

private val FAKE_SEED = ulongArrayOf(
    17295166580085024720uL,
    422929670265678780uL,
    5577237070365765850uL,
    7953171132032326923uL,
)

/** Return a [SeededRandomNumberGenerator] with the standard test seed. */
fun makeFakeRandomNumberGenerator(): SeededRandomNumberGenerator =
    SeededRandomNumberGenerator(FAKE_SEED)

/** Return [size] bytes of deterministic random data from the standard test seed. */
fun fakeRandomData(size: Int): ByteArray =
    makeFakeRandomNumberGenerator().randomData(size)
