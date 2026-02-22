package com.blockchaincommons.bcur

/**
 * A validated UR type string.
 *
 * Valid UR type characters are lowercase ASCII letters, digits, and hyphens.
 *
 * @throws URException.InvalidType if the value contains invalid characters
 */
data class URType(val value: String) {
    init {
        if (!isValidUrType(value)) {
            throw URException.InvalidType()
        }
    }

    override fun toString(): String = value

    companion object {
        private fun isValidUrType(s: String): Boolean =
            s.isNotEmpty() && s.all { c ->
                c in 'a'..'z' || c in '0'..'9' || c == '-'
            }
    }
}
