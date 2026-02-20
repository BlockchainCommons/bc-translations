package com.blockchaincommons.bcrand

/**
 * A random number generator that can be used as a source of deterministic
 * pseudo-randomness for testing purposes.
 *
 * Uses Xoshiro256** internally. The [randomData] and [fillRandomData] methods
 * generate each byte individually from `nextU64() and 0xFF` to ensure
 * cross-platform test-vector compatibility.
 *
 * This is not cryptographically secure, and should only be used for testing purposes.
 */
class SeededRandomNumberGenerator
/**
 * Creates a new seeded random number generator.
 *
 * The seed should be a 256-bit value, represented as an array of 4 [ULong]
 * values. For the output distribution to look random, the seed should not
 * have any obvious patterns, like all zeroes or all ones.
 *
 * This is not cryptographically secure, and should only be used for
 * testing purposes.
 *
 * @param seed An array of exactly 4 [ULong] values used to seed the generator.
 */
(seed: ULongArray) : RandomNumberGenerator() {

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

/**
 * Creates a seeded random number generator with a fixed seed.
 *
 * @return A [SeededRandomNumberGenerator] initialized with the standard test seed.
 */
fun makeFakeRandomNumberGenerator(): SeededRandomNumberGenerator =
    SeededRandomNumberGenerator(FAKE_SEED)

/**
 * Creates a byte array of random data with a fixed seed.
 *
 * @param size The number of random bytes to generate.
 * @return A new [ByteArray] containing [size] bytes of deterministic random data.
 */
fun fakeRandomData(size: Int): ByteArray =
    makeFakeRandomNumberGenerator().randomData(size)
