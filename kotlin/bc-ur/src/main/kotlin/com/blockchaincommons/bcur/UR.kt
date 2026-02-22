package com.blockchaincommons.bcur

import com.blockchaincommons.dcbor.Cbor

/** A Uniform Resource (UR) is a URI-encoded CBOR object. */
data class UR(val urType: URType, val cbor: Cbor) {
    /** The UR string representation (e.g. "ur:test/lsadaoaxjygonesw"). */
    val string: String
        get() {
            val data = cbor.toCborData()
            return UREncoding.encode(data, urType.value)
        }

    /** The UR string in uppercase, most efficient for QR codes. */
    val qrString: String get() = string.uppercase()

    /** The QR string as a byte array. */
    val qrData: ByteArray get() = qrString.toByteArray(Charsets.UTF_8)

    /**
     * Checks that this UR's type matches the expected type.
     *
     * @throws URException.UnexpectedType if the types don't match
     */
    fun checkType(expected: String) {
        val expectedType = URType(expected)
        if (urType != expectedType) {
            throw URException.UnexpectedType(expectedType.value, urType.value)
        }
    }

    /** The UR type as a string. */
    val urTypeStr: String get() = urType.value

    override fun toString(): String = string

    companion object {
        /**
         * Creates a new UR from a type string and CBOR value.
         *
         * @throws URException.InvalidType if the type string is not valid
         */
        fun create(urType: String, cbor: Cbor): UR =
            UR(URType(urType), cbor)

        /**
         * Parses a UR from its string representation.
         *
         * Accepts both lowercase and uppercase input (e.g. "UR:TEST/..." is valid).
         *
         * @throws URException if the string is not a valid single-part UR
         */
        fun fromUrString(urString: String): UR {
            val lower = urString.lowercase()
            if (!lower.startsWith("ur:")) {
                throw URException.InvalidScheme()
            }
            val withoutScheme = lower.removePrefix("ur:")
            val slashIdx = withoutScheme.indexOf('/')
            if (slashIdx < 0) {
                throw URException.TypeUnspecified()
            }
            val typeStr = withoutScheme.substring(0, slashIdx)
            val urType = URType(typeStr)
            val (kind, data) = UREncoding.decode(lower)
            if (kind != UREncoding.Kind.SinglePart) {
                throw URException.NotSinglePart()
            }
            val cbor = Cbor.tryFromData(data)
            return UR(urType, cbor)
        }
    }
}
