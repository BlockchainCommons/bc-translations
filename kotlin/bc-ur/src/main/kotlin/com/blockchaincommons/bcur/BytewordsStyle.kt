package com.blockchaincommons.bcur

/** The three bytewords encoding styles. */
enum class BytewordsStyle {
    /** Four-letter words, separated by spaces. */
    Standard,
    /** Four-letter words, separated by dashes. */
    Uri,
    /** Two-letter words, concatenated without separators. */
    Minimal
}
