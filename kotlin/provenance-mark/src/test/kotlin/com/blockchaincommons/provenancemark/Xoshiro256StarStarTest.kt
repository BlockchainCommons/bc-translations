package com.blockchaincommons.provenancemark

import kotlin.test.Test
import kotlin.test.assertContentEquals

class Xoshiro256StarStarTest {
    @Test
    fun testRng() {
        val data = "Hello World".encodeToByteArray()
        val digest = CryptoUtils.sha256(data)
        val rng = Xoshiro256StarStar.fromData(digest)
        val key = rng.nextBytes(32)

        assertContentEquals(
            hex("b18b446df414ec00714f19cb0f03e45cd3c3d5d071d2e7483ba8627c65b9926a"),
            key,
        )
    }

    @Test
    fun testSaveRngState() {
        val state = ulongArrayOf(
            17295166580085024720uL,
            422929670265678780uL,
            5577237070365765850uL,
            7953171132032326923uL,
        )

        val data = Xoshiro256StarStar.fromState(state).toData()
        assertContentEquals(
            hex("d0e72cf15ec604f0bcab28594b8cde05dab04ae79053664d0b9dadc201575f6e"),
            data,
        )

        val state2 = Xoshiro256StarStar.fromData(data).toState()
        val data2 = Xoshiro256StarStar.fromState(state2).toData()
        assertContentEquals(data, data2)
    }
}
