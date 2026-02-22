package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.ECDSA_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.ED25519_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.SCHNORR_SIGNATURE_SIZE
import com.blockchaincommons.bctags.TAG_MLDSA_SIGNATURE
import com.blockchaincommons.bctags.TAG_SIGNATURE
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborCase
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A digital signature created with various signature algorithms.
 *
 * [Signature] is a sealed class representing different types of digital
 * signatures: Schnorr (BIP-340, 64 bytes), ECDSA (secp256k1, 64 bytes),
 * Ed25519 (64 bytes), and ML-DSA (post-quantum).
 *
 * Signatures can be serialized to and from CBOR with the tag 40020.
 */
sealed class Signature : CborTaggedCodable, URCodable {

    /** A BIP-340 Schnorr signature (64 bytes). */
    class Schnorr(val data: ByteArray) : Signature() {
        init {
            require(data.size == SCHNORR_SIGNATURE_SIZE) {
                "Schnorr signature must be exactly $SCHNORR_SIGNATURE_SIZE bytes, got ${data.size}"
            }
        }

        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is Schnorr) return false
            return data.contentEquals(other.data)
        }

        override fun hashCode(): Int = data.contentHashCode()

        override fun toString(): String = "Schnorr(${data.toHexString()})"
    }

    /** An ECDSA signature using the secp256k1 curve (64 bytes). */
    class ECDSA(val data: ByteArray) : Signature() {
        init {
            require(data.size == ECDSA_SIGNATURE_SIZE) {
                "ECDSA signature must be exactly $ECDSA_SIGNATURE_SIZE bytes, got ${data.size}"
            }
        }

        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is ECDSA) return false
            return data.contentEquals(other.data)
        }

        override fun hashCode(): Int = data.contentHashCode()

        override fun toString(): String = "ECDSA(${data.toHexString()})"
    }

    /** An Ed25519 signature (64 bytes). */
    class Ed25519(val data: ByteArray) : Signature() {
        init {
            require(data.size == ED25519_SIGNATURE_SIZE) {
                "Ed25519 signature must be exactly $ED25519_SIGNATURE_SIZE bytes, got ${data.size}"
            }
        }

        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is Ed25519) return false
            return data.contentEquals(other.data)
        }

        override fun hashCode(): Int = data.contentHashCode()

        override fun toString(): String = "Ed25519(${data.toHexString()})"
    }

    /** A post-quantum ML-DSA signature. */
    class MLDSASig(val signature: MLDSASignature) : Signature() {
        override fun equals(other: Any?): Boolean {
            if (this === other) return true
            if (other !is MLDSASig) return false
            return signature == other.signature
        }

        override fun hashCode(): Int = signature.hashCode()

        override fun toString(): String = "MLDSA($signature)"
    }

    /**
     * The signature scheme that produced this signature.
     */
    val scheme: SignatureScheme get() = when (this) {
        is Schnorr -> SignatureScheme.Schnorr
        is ECDSA -> SignatureScheme.ECDSA
        is Ed25519 -> SignatureScheme.Ed25519
        is MLDSASig -> when (signature.level) {
            MLDSA.MLDSA44 -> SignatureScheme.MLDSA44
            MLDSA.MLDSA65 -> SignatureScheme.MLDSA65
            MLDSA.MLDSA87 -> SignatureScheme.MLDSA87
        }
    }

    /**
     * Returns the Schnorr signature data, or `null` if this is not a
     * Schnorr signature.
     */
    fun toSchnorr(): ByteArray? = (this as? Schnorr)?.data

    /**
     * Returns the ECDSA signature data, or `null` if this is not an
     * ECDSA signature.
     */
    fun toEcdsa(): ByteArray? = (this as? ECDSA)?.data

    /**
     * Returns the Ed25519 signature data, or `null` if this is not an
     * Ed25519 signature.
     */
    fun toEd25519(): ByteArray? = (this as? Ed25519)?.data

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_SIGNATURE))

    /**
     * Converts this signature to an untagged CBOR value.
     *
     * - Schnorr: a byte string containing the 64-byte signature
     * - ECDSA: an array [1, byte_string]
     * - Ed25519: an array [2, byte_string]
     */
    override fun untaggedCbor(): Cbor = when (this) {
        is Schnorr -> Cbor.fromByteString(data)
        is ECDSA -> Cbor.fromArray(
            listOf(Cbor.fromInt(1), Cbor.fromByteString(data)),
        )
        is Ed25519 -> Cbor.fromArray(
            listOf(Cbor.fromInt(2), Cbor.fromByteString(data)),
        )
        is MLDSASig -> signature.taggedCbor()
    }

    companion object {
        /** Creates a Schnorr signature from a byte array. */
        fun fromSchnorr(data: ByteArray): Signature {
            if (data.size != SCHNORR_SIGNATURE_SIZE) {
                throw BcComponentsException.invalidSize(
                    "Schnorr signature", SCHNORR_SIGNATURE_SIZE, data.size,
                )
            }
            return Schnorr(data.copyOf())
        }

        /** Creates an ECDSA signature from a byte array. */
        fun fromEcdsa(data: ByteArray): Signature {
            if (data.size != ECDSA_SIGNATURE_SIZE) {
                throw BcComponentsException.invalidSize(
                    "ECDSA signature", ECDSA_SIGNATURE_SIZE, data.size,
                )
            }
            return ECDSA(data.copyOf())
        }

        /** Creates an Ed25519 signature from a byte array. */
        fun fromEd25519(data: ByteArray): Signature {
            if (data.size != ED25519_SIGNATURE_SIZE) {
                throw BcComponentsException.invalidSize(
                    "Ed25519 signature", ED25519_SIGNATURE_SIZE, data.size,
                )
            }
            return Ed25519(data.copyOf())
        }

        /** Decodes a [Signature] from untagged CBOR. */
        fun fromUntaggedCbor(cbor: Cbor): Signature {
            return when (val case = cbor.cborCase) {
                is CborCase.CborByteString -> {
                    val bytes = case.value.toByteArray()
                    fromSchnorr(bytes)
                }
                is CborCase.Array -> {
                    val elements = case.value
                    if (elements.size == 2) {
                        val first = elements[0].cborCase
                        val second = elements[1]
                        when (first) {
                            is CborCase.Unsigned -> {
                                val discriminator = first.value.toInt()
                                val data = second.tryByteStringData()
                                when (discriminator) {
                                    1 -> fromEcdsa(data)
                                    2 -> fromEd25519(data)
                                    else -> throw BcComponentsException.invalidData(
                                        "Signature",
                                        "unknown discriminator: $discriminator",
                                    )
                                }
                            }
                            is CborCase.CborByteString -> {
                                // Legacy format: [byte_string, ...] interpreted as Schnorr
                                fromSchnorr(first.value.toByteArray())
                            }
                            else -> throw BcComponentsException.invalidData(
                                "Signature", "invalid array format",
                            )
                        }
                    } else {
                        throw BcComponentsException.invalidData(
                            "Signature", "invalid array length: ${elements.size}",
                        )
                    }
                }
                is CborCase.Tagged -> when (case.tag.value) {
                    TAG_MLDSA_SIGNATURE -> {
                        val sig = MLDSASignature.fromTaggedCbor(cbor)
                        MLDSASig(sig)
                    }
                    else -> throw BcComponentsException.invalidData(
                        "Signature",
                        "unsupported tagged signature type: ${case.tag.value}",
                    )
                }
                else -> throw BcComponentsException.invalidData(
                    "Signature", "invalid CBOR format",
                )
            }
        }

        /** Decodes a [Signature] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): Signature =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_SIGNATURE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Signature] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): Signature =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_SIGNATURE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [Signature] from a UR. */
        fun fromUr(ur: UR): Signature {
            ur.checkType("signature")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [Signature] from a UR string. */
        fun fromUrString(urString: String): Signature =
            fromUr(UR.fromUrString(urString))
    }
}
