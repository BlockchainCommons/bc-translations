package com.blockchaincommons.bcur

import kotlin.test.Test
import kotlin.test.assertEquals

class Xoshiro256Test {

    @Test
    fun testRng1() {
        val rng = Xoshiro256.fromString("Wolf")
        val expected = listOf(
            42UL, 81UL, 85UL, 8UL, 82UL, 84UL, 76UL, 73UL, 70UL, 88UL, 2UL, 74UL, 40UL, 48UL,
            77UL, 54UL, 88UL, 7UL, 5UL, 88UL, 37UL, 25UL, 82UL, 13UL, 69UL, 59UL, 30UL, 39UL,
            11UL, 82UL, 19UL, 99UL, 45UL, 87UL, 30UL, 15UL, 32UL, 22UL, 89UL, 44UL, 92UL, 77UL,
            29UL, 78UL, 4UL, 92UL, 44UL, 68UL, 92UL, 69UL, 1UL, 42UL, 89UL, 50UL, 37UL, 84UL,
            63UL, 34UL, 32UL, 3UL, 17UL, 62UL, 40UL, 98UL, 82UL, 89UL, 24UL, 43UL, 85UL, 39UL,
            15UL, 3UL, 99UL, 29UL, 20UL, 42UL, 27UL, 10UL, 85UL, 66UL, 50UL, 35UL, 69UL, 70UL,
            70UL, 74UL, 30UL, 13UL, 72UL, 54UL, 11UL, 5UL, 70UL, 55UL, 91UL, 52UL, 10UL, 43UL,
            43UL, 52UL
        )
        for (e in expected) {
            assertEquals(e, rng.next() % 100u)
        }
    }

    @Test
    fun testRng2() {
        val rng = Xoshiro256.fromCrc("Wolf".toByteArray())
        val expected = listOf(
            88UL, 44UL, 94UL, 74UL, 0UL, 99UL, 7UL, 77UL, 68UL, 35UL, 47UL, 78UL, 19UL, 21UL,
            50UL, 15UL, 42UL, 36UL, 91UL, 11UL, 85UL, 39UL, 64UL, 22UL, 57UL, 11UL, 25UL, 12UL,
            1UL, 91UL, 17UL, 75UL, 29UL, 47UL, 88UL, 11UL, 68UL, 58UL, 27UL, 65UL, 21UL, 54UL,
            47UL, 54UL, 73UL, 83UL, 23UL, 58UL, 75UL, 27UL, 26UL, 15UL, 60UL, 36UL, 30UL, 21UL,
            55UL, 57UL, 77UL, 76UL, 75UL, 47UL, 53UL, 76UL, 9UL, 91UL, 14UL, 69UL, 3UL, 95UL,
            11UL, 73UL, 20UL, 99UL, 68UL, 61UL, 3UL, 98UL, 36UL, 98UL, 56UL, 65UL, 14UL, 80UL,
            74UL, 57UL, 63UL, 68UL, 51UL, 56UL, 24UL, 39UL, 53UL, 80UL, 57UL, 51UL, 81UL, 3UL,
            1UL, 30UL
        )
        for (e in expected) {
            assertEquals(e, rng.next() % 100u)
        }
    }

    @Test
    fun testRng3() {
        val rng = Xoshiro256.fromString("Wolf")
        val expected = listOf(
            6UL, 5UL, 8UL, 4UL, 10UL, 5UL, 7UL, 10UL, 4UL, 9UL, 10UL, 9UL, 7UL, 7UL, 1UL, 1UL,
            2UL, 9UL, 9UL, 2UL, 6UL, 4UL, 5UL, 7UL, 8UL, 5UL, 4UL, 2UL, 3UL, 8UL, 7UL, 4UL,
            5UL, 1UL, 10UL, 9UL, 3UL, 10UL, 2UL, 6UL, 8UL, 5UL, 7UL, 9UL, 3UL, 1UL, 5UL, 2UL,
            7UL, 1UL, 4UL, 4UL, 4UL, 4UL, 9UL, 4UL, 5UL, 5UL, 6UL, 9UL, 5UL, 1UL, 2UL, 8UL,
            3UL, 3UL, 2UL, 8UL, 4UL, 3UL, 2UL, 1UL, 10UL, 8UL, 9UL, 3UL, 10UL, 8UL, 5UL, 5UL,
            6UL, 7UL, 10UL, 5UL, 8UL, 9UL, 4UL, 6UL, 4UL, 2UL, 10UL, 2UL, 1UL, 7UL, 9UL, 6UL,
            7UL, 4UL, 2UL, 5UL
        )
        for (e in expected) {
            assertEquals(e, rng.nextInt(1u, 10u))
        }
    }

    @Test
    fun testShuffle() {
        val rng = Xoshiro256.fromString("Wolf")
        val values = listOf(1, 2, 3, 4, 5, 6, 7, 8, 9, 10)
        val expected = listOf(
            listOf(6, 4, 9, 3, 10, 5, 7, 8, 1, 2),
            listOf(10, 8, 6, 5, 1, 2, 3, 9, 7, 4),
            listOf(6, 4, 5, 8, 9, 3, 2, 1, 7, 10),
            listOf(7, 3, 5, 1, 10, 9, 4, 8, 2, 6),
            listOf(8, 5, 7, 10, 2, 1, 4, 3, 9, 6),
            listOf(4, 3, 5, 6, 10, 2, 7, 8, 9, 1),
            listOf(5, 1, 3, 9, 4, 6, 2, 10, 7, 8),
            listOf(2, 1, 10, 8, 9, 4, 7, 6, 3, 5),
            listOf(6, 7, 10, 4, 8, 9, 2, 3, 1, 5),
            listOf(10, 2, 1, 7, 9, 5, 6, 3, 4, 8)
        )
        for (e in expected) {
            assertEquals(e, rng.shuffled(values))
        }
    }
}
