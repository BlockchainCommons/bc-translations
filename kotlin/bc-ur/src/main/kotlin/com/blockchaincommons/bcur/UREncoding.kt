package com.blockchaincommons.bcur

/**
 * Static encode/decode for UR strings.
 *
 * Handles the `ur:type/payload` format for single-part URs and
 * `ur:type/seq-total/payload` for multi-part URs.
 */
internal object UREncoding {
    enum class Kind {
        SinglePart,
        MultiPart
    }

    /** Encodes binary data as a single-part UR string. */
    fun encode(data: ByteArray, urType: String): String {
        val body = Bytewords.encode(data, BytewordsStyle.Minimal)
        return "ur:$urType/$body"
    }

    /**
     * Decodes a UR string into its kind and binary payload.
     *
     * Handles both single-part (`ur:type/payload`) and
     * multi-part (`ur:type/seq-total/payload`) formats.
     *
     * @throws URException if the string is not a valid UR
     */
    fun decode(value: String): Pair<Kind, ByteArray> {
        if (!value.startsWith("ur:")) {
            throw URException.InvalidScheme()
        }
        val stripScheme = value.removePrefix("ur:")

        val firstSlash = stripScheme.indexOf('/')
        if (firstSlash < 0) {
            throw URException.TypeUnspecified()
        }

        val type = stripScheme.substring(0, firstSlash)
        if (!type.all { c -> c in 'a'..'z' || c in '0'..'9' || c == '-' }) {
            throw URException.DecoderError("Type contains invalid characters")
        }

        val rest = stripScheme.substring(firstSlash + 1)
        val lastSlash = rest.lastIndexOf('/')

        return if (lastSlash < 0) {
            // Single-part: ur:type/payload
            val payload = Bytewords.decode(rest, BytewordsStyle.Minimal)
            Kind.SinglePart to payload
        } else {
            // Multi-part: ur:type/idx-total/payload
            val indices = rest.substring(0, lastSlash)
            val payload = rest.substring(lastSlash + 1)

            val parts = indices.split('-')
            if (parts.size != 2) {
                throw URException.DecoderError("Invalid indices")
            }
            val idx = parts[0].toIntOrNull()
            val total = parts[1].toIntOrNull()
            if (idx == null || total == null) {
                throw URException.DecoderError("Invalid indices")
            }

            val decoded = Bytewords.decode(payload, BytewordsStyle.Minimal)
            Kind.MultiPart to decoded
        }
    }
}
