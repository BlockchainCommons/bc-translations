package com.blockchaincommons.bcrand

import kotlin.test.Test
import kotlin.test.assertContentEquals
import kotlin.test.assertEquals

@OptIn(ExperimentalStdlibApi::class)

class SeededRandomTest {

    private val testSeed = ulongArrayOf(
        17295166580085024720uL,
        422929670265678780uL,
        5577237070365765850uL,
        7953171132032326923uL,
    )

    @Test
    fun testNextU64() {
        val rng = SeededRandomNumberGenerator(testSeed)
        assertEquals(1104683000648959614uL, rng.nextU64())
    }

    @Test
    fun testNext50() {
        val rng = SeededRandomNumberGenerator(testSeed)
        val expected = listOf(
            1104683000648959614uL,
            9817345228149227957uL,
            546276821344993881uL,
            15870950426333349563uL,
            830653509032165567uL,
            14772257893953840492uL,
            3512633850838187726uL,
            6358411077290857510uL,
            7897285047238174514uL,
            18314839336815726031uL,
            4978716052961022367uL,
            17373022694051233817uL,
            663115362299242570uL,
            9811238046242345451uL,
            8113787839071393872uL,
            16155047452816275860uL,
            673245095821315645uL,
            1610087492396736743uL,
            1749670338128618977uL,
            3927771759340679115uL,
            9610589375631783853uL,
            5311608497352460372uL,
            11014490817524419548uL,
            6320099928172676090uL,
            12513554919020212402uL,
            6823504187935853178uL,
            1215405011954300226uL,
            8109228150255944821uL,
            4122548551796094879uL,
            16544885818373129566uL,
            5597102191057004591uL,
            11690994260783567085uL,
            9374498734039011409uL,
            18246806104446739078uL,
            2337407889179712900uL,
            12608919248151905477uL,
            7641631838640172886uL,
            8421574250687361351uL,
            8697189342072434208uL,
            8766286633078002696uL,
            14800090277885439654uL,
            17865860059234099833uL,
            4673315107448681522uL,
            14288183874156623863uL,
            7587575203648284614uL,
            9109213819045273474uL,
            11817665411945280786uL,
            1745089530919138651uL,
            5730370365819793488uL,
            5496865518262805451uL,
        )
        val actual = List(expected.size) { rng.nextU64() }
        assertEquals(expected, actual)
    }

    @Test
    fun testFakeRandomData() {
        val expected = (
            "7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed" +
            "518684c556472008a67932f7c682125b50cb72e8216f6906358fdaf28d354553" +
            "2daee0c5bb5023f50cd8e71ec14901ac746c576c481b893be6656b80622b3a56" +
            "4e59b4e2"
        ).hexToByteArray()
        assertContentEquals(expected, fakeRandomData(100))
    }

    @Test
    fun testNextWithUpperBound() {
        val rng = SeededRandomNumberGenerator(testSeed)
        assertEquals(745uL, rngNextWithUpperBound(rng, 10000uL, bits = 32))
    }

    @Test
    fun testInRange() {
        val rng = SeededRandomNumberGenerator(testSeed)
        val v = List(100) { rngNextInRange(rng, 0, 100, bits = 32) }
        val expected = listOf<Long>(
            7, 44, 92, 16, 16, 67, 41, 74, 66, 20, 18, 6, 62, 34, 4, 69, 99,
            19, 0, 85, 22, 27, 56, 23, 19, 5, 23, 76, 80, 27, 74, 69, 17, 92,
            31, 32, 55, 36, 49, 23, 53, 2, 46, 6, 43, 66, 34, 71, 64, 69, 25,
            14, 17, 23, 32, 6, 23, 65, 35, 11, 21, 37, 58, 92, 98, 8, 38, 49,
            7, 24, 24, 71, 37, 63, 91, 21, 11, 66, 52, 54, 55, 19, 76, 46, 89,
            38, 91, 95, 33, 25, 4, 30, 66, 51, 5, 91, 62, 27, 92, 39,
        )
        assertEquals(expected, v)
    }

    @Test
    fun testFillRandomData() {
        val rng1 = SeededRandomNumberGenerator(testSeed)
        val v1 = rng1.randomData(100)
        val rng2 = SeededRandomNumberGenerator(testSeed)
        val v2 = ByteArray(100)
        rng2.fillRandomData(v2)
        assertContentEquals(v1, v2)
    }
}
