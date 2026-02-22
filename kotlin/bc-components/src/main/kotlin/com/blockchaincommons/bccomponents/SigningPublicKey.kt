package com.blockchaincommons.bccomponents

import com.blockchaincommons.bctags.TAG_MLDSA_PUBLIC_KEY
import com.blockchaincommons.bctags.TAG_SIGNING_PUBLIC_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborCase
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A public key used for verifying digital signatures.
 *
 * [SigningPublicKey] is a sealed class representing different types of
 * signing public keys, including Schnorr, ECDSA, Ed25519, and ML-DSA
 * (post-quantum). This type implements the [Verifier] interface, allowing
 * it to verify signatures of the appropriate type.
 */
sealed class SigningPublicKey :
    Verifier,
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    /** A Schnorr public key (BIP-340, x-only, 32 bytes). */
    data class SchnorrKey(val key: SchnorrPublicKey) : SigningPublicKey()

    /** An ECDSA public key (compressed, 33 bytes). */
    data class ECDSAKey(val key: ECPublicKey) : SigningPublicKey()

    /** An Ed25519 public key (32 bytes). */
    data class Ed25519Key(val key: Ed25519PublicKey) : SigningPublicKey()

    /** A post-quantum ML-DSA public key. */
    data class MLDSAKey(val key: MLDSAPublicKey) : SigningPublicKey()

    /**
     * The signature scheme of this public key.
     */
    val scheme: SignatureScheme get() = when (this) {
        is SchnorrKey -> SignatureScheme.Schnorr
        is ECDSAKey -> SignatureScheme.ECDSA
        is Ed25519Key -> SignatureScheme.Ed25519
        is MLDSAKey -> when (key.level) {
            MLDSA.MLDSA44 -> SignatureScheme.MLDSA44
            MLDSA.MLDSA65 -> SignatureScheme.MLDSA65
            MLDSA.MLDSA87 -> SignatureScheme.MLDSA87
        }
    }

    /**
     * Returns the underlying Schnorr public key, or `null` if this is
     * not a Schnorr key.
     */
    fun toSchnorr(): SchnorrPublicKey? = (this as? SchnorrKey)?.key

    /**
     * Returns the underlying ECDSA public key, or `null` if this is
     * not an ECDSA key.
     */
    fun toEcdsa(): ECPublicKey? = (this as? ECDSAKey)?.key

    /**
     * Returns the underlying Ed25519 public key, or `null` if this is
     * not an Ed25519 key.
     */
    fun toEd25519(): Ed25519PublicKey? = (this as? Ed25519Key)?.key

    // -- Verifier --

    override fun verify(signature: Signature, message: ByteArray): Boolean {
        return when (this) {
            is SchnorrKey -> {
                val sig = signature as? Signature.Schnorr ?: return false
                key.schnorrVerify(sig.data, message)
            }
            is ECDSAKey -> {
                val sig = signature as? Signature.ECDSA ?: return false
                key.verify(sig.data, message)
            }
            is Ed25519Key -> {
                val sig = signature as? Signature.Ed25519 ?: return false
                key.verify(sig.data, message)
            }
            is MLDSAKey -> {
                val sig = signature as? Signature.MLDSASig ?: return false
                try {
                    key.verify(sig.signature, message)
                } catch (_: Exception) {
                    false
                }
            }
        }
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_SIGNING_PUBLIC_KEY))

    /**
     * Converts this signing public key to an untagged CBOR value.
     *
     * - Schnorr: a byte string containing the 32-byte x-only public key
     * - ECDSA: an array [1, byte_string] with the 33-byte compressed key
     * - Ed25519: an array [2, byte_string] with the 32-byte public key
     */
    override fun untaggedCbor(): Cbor = when (this) {
        is SchnorrKey -> Cbor.fromByteString(key.data())
        is ECDSAKey -> Cbor.fromArray(
            listOf(Cbor.fromInt(1), Cbor.fromByteString(key.data())),
        )
        is Ed25519Key -> Cbor.fromArray(
            listOf(Cbor.fromInt(2), Cbor.fromByteString(key.data())),
        )
        is MLDSAKey -> key.taggedCbor()
    }

    // -- toString --

    override fun toString(): String {
        val innerKey = when (this) {
            is SchnorrKey -> key
            is ECDSAKey -> key
            is Ed25519Key -> key
            is MLDSAKey -> key
        }
        return "SigningPublicKey(${refHexShort()}, $innerKey)"
    }

    companion object {
        /** Creates a signing public key from a Schnorr public key. */
        fun fromSchnorr(key: SchnorrPublicKey): SigningPublicKey = SchnorrKey(key)

        /** Creates a signing public key from an ECDSA public key. */
        fun fromEcdsa(key: ECPublicKey): SigningPublicKey = ECDSAKey(key)

        /** Creates a signing public key from an Ed25519 public key. */
        fun fromEd25519(key: Ed25519PublicKey): SigningPublicKey = Ed25519Key(key)

        /** Creates a signing public key from an ML-DSA public key. */
        fun fromMldsa(key: MLDSAPublicKey): SigningPublicKey = MLDSAKey(key)

        /** Decodes a [SigningPublicKey] from untagged CBOR. */
        fun fromUntaggedCbor(cbor: Cbor): SigningPublicKey {
            return when (val case = cbor.cborCase) {
                is CborCase.CborByteString -> {
                    // Byte string -> Schnorr public key
                    val data = case.value.toByteArray()
                    SchnorrKey(SchnorrPublicKey.fromData(data))
                }
                is CborCase.Array -> {
                    val elements = case.value
                    if (elements.size != 2) {
                        throw BcComponentsException.invalidData(
                            "SigningPublicKey",
                            "expected array of length 2, got ${elements.size}",
                        )
                    }
                    val discriminator = elements[0].tryInt()
                    val data = elements[1].tryByteStringData()
                    when (discriminator) {
                        1 -> ECDSAKey(ECPublicKey.fromData(data))
                        2 -> Ed25519Key(Ed25519PublicKey.fromData(data))
                        else -> throw BcComponentsException.invalidData(
                            "SigningPublicKey",
                            "unknown discriminator: $discriminator",
                        )
                    }
                }
                is CborCase.Tagged -> when (case.tag.value) {
                    TAG_MLDSA_PUBLIC_KEY -> {
                        val key = MLDSAPublicKey.fromTaggedCbor(cbor)
                        MLDSAKey(key)
                    }
                    else -> throw BcComponentsException.invalidData(
                        "SigningPublicKey",
                        "unsupported tagged public key type: ${case.tag.value}",
                    )
                }
                else -> throw BcComponentsException.invalidData(
                    "SigningPublicKey", "invalid CBOR format",
                )
            }
        }

        /** Decodes a [SigningPublicKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): SigningPublicKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_SIGNING_PUBLIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [SigningPublicKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): SigningPublicKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_SIGNING_PUBLIC_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [SigningPublicKey] from a UR. */
        fun fromUr(ur: UR): SigningPublicKey {
            ur.checkType("signing-public-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [SigningPublicKey] from a UR string. */
        fun fromUrString(urString: String): SigningPublicKey =
            fromUr(UR.fromUrString(urString))
    }
}
