package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_ENCRYPTED
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A secure encrypted message using IETF ChaCha20-Poly1305 authenticated
 * encryption.
 *
 * [EncryptedMessage] represents data that has been encrypted using a symmetric
 * key with the ChaCha20-Poly1305 AEAD (Authenticated Encryption with
 * Associated Data) construction as specified in
 * [RFC-8439](https://datatracker.ietf.org/doc/html/rfc8439).
 *
 * An [EncryptedMessage] contains:
 * - `ciphertext`: The encrypted data (same length as the original plaintext)
 * - `aad`: Additional Authenticated Data that is not encrypted but is
 *   authenticated (optional)
 * - `nonce`: A 12-byte number used once for this specific encryption operation
 * - `auth`: A 16-byte authentication tag that verifies the integrity of the
 *   message
 *
 * The `aad` field is often used to include the [Digest] of the plaintext,
 * which allows verification of the plaintext after decryption and preserves
 * the unique identity of the data when used with structures like Gordian
 * Envelope.
 *
 * CDDL:
 * ```
 * EncryptedMessage =
 *     #6.40002([ ciphertext: bstr, nonce: bstr, auth: bstr, ? aad: bstr ])
 * ```
 */
class EncryptedMessage(
    private val ciphertext: ByteArray,
    private val aad: ByteArray,
    private val nonce: Nonce,
    private val auth: AuthenticationTag,
) : DigestProvider,
    CborTaggedCodable,
    URCodable {

    /** Returns a copy of the ciphertext data. */
    fun ciphertext(): ByteArray = ciphertext.copyOf()

    /** Returns a copy of the additional authenticated data (AAD). */
    fun aad(): ByteArray = aad.copyOf()

    /** Returns the nonce used for encryption. */
    fun nonce(): Nonce = nonce

    /** Returns the authentication tag used for encryption. */
    fun authenticationTag(): AuthenticationTag = auth

    /**
     * Returns a CBOR representation parsed from the AAD field, if it exists
     * and is valid CBOR.
     */
    fun aadCbor(): Cbor? {
        if (aad.isEmpty()) return null
        return try {
            Cbor.tryFromData(aad)
        } catch (_: Exception) {
            null
        }
    }

    /**
     * Returns a [Digest] if the AAD data can be parsed as tagged CBOR
     * containing a digest.
     */
    fun aadDigest(): Digest? {
        val cbor = aadCbor() ?: return null
        return try {
            Digest.fromTaggedCbor(cbor)
        } catch (_: Exception) {
            null
        }
    }

    /** Returns `true` if the AAD data contains a valid [Digest]. */
    fun hasDigest(): Boolean = aadDigest() != null

    // -- DigestProvider --

    override fun digest(): Digest = aadDigest()
        ?: throw BcComponentsException.invalidData("EncryptedMessage", "no digest in AAD")

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is EncryptedMessage) return false
        return ciphertext.contentEquals(other.ciphertext) &&
            aad.contentEquals(other.aad) &&
            nonce == other.nonce &&
            auth == other.auth
    }

    override fun hashCode(): Int {
        var result = ciphertext.contentHashCode()
        result = 31 * result + aad.contentHashCode()
        result = 31 * result + nonce.hashCode()
        result = 31 * result + auth.hashCode()
        return result
    }

    // -- toString --

    override fun toString(): String = buildString {
        append("EncryptedMessage(")
        append("ciphertext=")
        append(ciphertext.toHexString())
        append(", aad=")
        append(aad.toHexString())
        append(", nonce=")
        append(nonce)
        append(", auth=")
        append(auth)
        append(")")
    }

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_ENCRYPTED))

    override fun untaggedCbor(): Cbor {
        val elements = mutableListOf(
            Cbor.fromByteString(ciphertext),
            Cbor.fromByteString(nonce.data()),
            Cbor.fromByteString(auth.data()),
        )
        if (aad.isNotEmpty()) {
            elements.add(Cbor.fromByteString(aad))
        }
        return Cbor.fromArray(elements)
    }

    // -- Companion --

    companion object {
        /** Decodes an [EncryptedMessage] from untagged CBOR (an array). */
        fun fromUntaggedCbor(cbor: Cbor): EncryptedMessage {
            val elements = cbor.tryArray()
            if (elements.size < 3) {
                throw BcComponentsException.invalidData(
                    "EncryptedMessage",
                    "must have at least 3 elements, got ${elements.size}",
                )
            }
            val ciphertext = elements[0].tryByteStringData()
            val nonceData = elements[1].tryByteStringData()
            val nonce = Nonce.fromDataChecked(nonceData)
            val authData = elements[2].tryByteStringData()
            val auth = AuthenticationTag.fromDataChecked(authData)
            val aad = if (elements.size > 3) {
                elements[3].tryByteStringData()
            } else {
                ByteArray(0)
            }
            return EncryptedMessage(ciphertext, aad, nonce, auth)
        }

        /** Decodes an [EncryptedMessage] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): EncryptedMessage =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_ENCRYPTED)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [EncryptedMessage] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): EncryptedMessage =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_ENCRYPTED)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [EncryptedMessage] from a UR. */
        fun fromUr(ur: UR): EncryptedMessage {
            ur.checkType("encrypted")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [EncryptedMessage] from a UR string. */
        fun fromUrString(urString: String): EncryptedMessage =
            fromUr(UR.fromUrString(urString))
    }
}
