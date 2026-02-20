package com.blockchaincommons.bclifehash

enum class Pattern {
    Snowflake,
    Pinwheel,
    Fiducial,
}

fun selectPattern(entropy: BitEnumerator, version: Version): Pattern =
    when (version) {
        Version.Fiducial,
        Version.GrayscaleFiducial,
        -> Pattern.Fiducial

        else -> if (entropy.next()) Pattern.Snowflake else Pattern.Pinwheel
    }
