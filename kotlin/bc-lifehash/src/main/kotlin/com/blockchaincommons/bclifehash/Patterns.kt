package com.blockchaincommons.bclifehash

/** The symmetry pattern used when rendering a LifeHash image. */
internal enum class Pattern {
    Snowflake,
    Pinwheel,
    Fiducial,
}

/** Selects a [Pattern] based on the rendering [version] and available [entropy]. */
internal fun selectPattern(entropy: BitEnumerator, version: Version): Pattern =
    when (version) {
        Version.Fiducial,
        Version.GrayscaleFiducial,
        -> Pattern.Fiducial

        else -> if (entropy.next()) Pattern.Snowflake else Pattern.Pinwheel
    }
