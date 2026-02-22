package com.blockchaincommons.bccomponents

import com.blockchaincommons.dcbor.Cbor

/**
 * The authentication tag produced by the encryption process to verify message
 * integrity.
 *
 * An [AuthenticationTag] is a 16-byte value generated during ChaCha20-Poly1305
 * authenticated encryption. It serves as a message authentication code (MAC)
 * that verifies both the authenticity and integrity of the encrypted message.
 *
 * During decryption, the tag is verified to ensure:
 * - The message has not been tampered with (integrity)
 * - The message was encrypted by someone who possesses the encryption key
 *   (authenticity)
 *
 * This implementation follows the Poly1305 MAC algorithm as specified in
 * [RFC-8439](https://datatracker.ietf.org/doc/html/rfc8439).
 */
class AuthenticationTag private constructor(private val data: ByteArray) {

    init {
        require(data.size == AUTHENTICATION_TAG_SIZE) {
            "AuthenticationTag data must be exactly $AUTHENTICATION_TAG_SIZE bytes, got ${data.size}"
        }
    }

    /** Returns a copy of the underlying 16-byte tag data. */
    fun data(): ByteArray = data.copyOf()

    /** Returns the tag bytes as a copy. */
    fun asBytes(): ByteArray = data.copyOf()

    /** The tag as a lowercase hexadecimal string. */
    val hex: String get() = data.toHexString()

    /** Returns the tag bytes as a new [ByteArray]. */
    fun toByteArray(): ByteArray = data.copyOf()

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is AuthenticationTag) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    // -- toString --

    override fun toString(): String = "AuthenticationTag($hex)"

    // -- CBOR (untagged, used as part of EncryptedMessage) --

    /** Encodes this authentication tag as CBOR (a byte string, not tagged). */
    fun toCbor(): Cbor = Cbor.fromByteString(data)

    companion object {
        const val AUTHENTICATION_TAG_SIZE: Int = 16

        /** Restores an [AuthenticationTag] from exactly [AUTHENTICATION_TAG_SIZE] bytes. */
        fun fromData(data: ByteArray): AuthenticationTag {
            require(data.size == AUTHENTICATION_TAG_SIZE) {
                "AuthenticationTag data must be exactly $AUTHENTICATION_TAG_SIZE bytes, got ${data.size}"
            }
            return AuthenticationTag(data.copyOf())
        }

        /**
         * Restores an [AuthenticationTag] from a byte array, throwing a
         * [BcComponentsException] if the length is wrong.
         */
        fun fromDataChecked(data: ByteArray): AuthenticationTag {
            if (data.size != AUTHENTICATION_TAG_SIZE) {
                throw BcComponentsException.invalidSize(
                    "authentication tag",
                    AUTHENTICATION_TAG_SIZE,
                    data.size,
                )
            }
            return AuthenticationTag(data.copyOf())
        }

        /** Decodes an [AuthenticationTag] from a CBOR byte string. */
        fun fromCbor(cbor: Cbor): AuthenticationTag {
            val bytes = cbor.tryByteStringData()
            return fromDataChecked(bytes)
        }
    }
}
