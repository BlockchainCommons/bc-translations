package com.blockchaincommons.dcbor

import java.text.Normalizer

internal object StringUtil {
    fun flanked(s: String, left: String, right: String): String = "$left$s$right"

    fun isNfc(s: String): Boolean = Normalizer.isNormalized(s, Normalizer.Form.NFC)

    fun toNfc(s: String): String = Normalizer.normalize(s, Normalizer.Form.NFC)

    fun isPrintable(c: Char): Boolean = c.code in 32..126 || c.code > 127

    fun sanitized(s: String): String? {
        val hasPrintable = s.any { isPrintable(it) }
        return if (hasPrintable) {
            s.map { if (isPrintable(it)) it else '.' }.joinToString("")
        } else {
            null
        }
    }
}
