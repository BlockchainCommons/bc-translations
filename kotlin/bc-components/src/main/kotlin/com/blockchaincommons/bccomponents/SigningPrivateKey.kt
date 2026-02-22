package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.SCHNORR_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.schnorrSign
import com.blockchaincommons.bccrypto.schnorrSignWithAuxRand
import com.blockchaincommons.bctags.TAG_MLDSA_PRIVATE_KEY
import com.blockchaincommons.bctags.TAG_SIGNING_PRIVATE_KEY
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborCase
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A private key used for creating digital signatures.
 *
 * [SigningPrivateKey] is a sealed class representing different types of
 * signing private keys, including Schnorr, ECDSA, Ed25519, and ML-DSA
 * (post-quantum). This type implements the [Signer] interface, allowing it
 * to create signatures of the appropriate type.
 */
sealed class SigningPrivateKey :
    Signer,
    Verifier,
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    /** A Schnorr private key based on the secp256k1 curve. */
    data class SchnorrKey(val key: ECPrivateKey) : SigningPrivateKey()

    /** An ECDSA private key based on the secp256k1 curve. */
    data class ECDSAKey(val key: ECPrivateKey) : SigningPrivateKey()

    /** An Ed25519 private key. */
    data class Ed25519Key(val key: Ed25519PrivateKey) : SigningPrivateKey()

    /** A post-quantum ML-DSA private key. */
    data class MLDSAKey(val key: MLDSAPrivateKey) : SigningPrivateKey()

    /**
     * The signature scheme of this private key.
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
     * Derives the corresponding signing public key.
     *
     * @throws BcComponentsException for ML-DSA keys, which derive the public
     *   key via the underlying [MLDSAPrivateKey.publicKey] method and wrap it
     *   in [SigningPublicKey.MLDSAKey].
     */
    fun publicKey(): SigningPublicKey = when (this) {
        is SchnorrKey -> SigningPublicKey.SchnorrKey(key.schnorrPublicKey())
        is ECDSAKey -> SigningPublicKey.ECDSAKey(key.publicKey())
        is Ed25519Key -> SigningPublicKey.Ed25519Key(key.publicKey())
        is MLDSAKey -> SigningPublicKey.MLDSAKey(key.publicKey())
    }

    // -- Signer --

    override fun signWithOptions(message: ByteArray, options: SigningOptions?): Signature {
        return when (this) {
            is SchnorrKey -> {
                val sigBytes = if (options is SigningOptions.SchnorrAuxRand) {
                    schnorrSignWithAuxRand(key.data(), message, options.auxRand)
                } else {
                    schnorrSign(key.data(), message)
                }
                require(sigBytes.size == SCHNORR_SIGNATURE_SIZE)
                Signature.Schnorr(sigBytes)
            }
            is ECDSAKey -> {
                val sigBytes = key.ecdsaSign(message)
                Signature.ECDSA(sigBytes)
            }
            is Ed25519Key -> {
                val sigBytes = key.sign(message)
                Signature.Ed25519(sigBytes)
            }
            is MLDSAKey -> {
                val mldsaSig = key.sign(message)
                Signature.MLDSASig(mldsaSig)
            }
        }
    }

    // -- Verifier --

    override fun verify(signature: Signature, message: ByteArray): Boolean {
        return publicKey().verify(signature, message)
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_SIGNING_PRIVATE_KEY))

    /**
     * Converts this signing private key to an untagged CBOR value.
     *
     * - Schnorr: a byte string containing the 32-byte private key
     * - ECDSA: an array [1, byte_string]
     * - Ed25519: an array [2, byte_string]
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

    override fun toString(): String = when (this) {
        is SchnorrKey -> "SigningPrivateKey(${refHexShort()}, SchnorrPrivateKey(${key.refHexShort()}))"
        is ECDSAKey -> "SigningPrivateKey(${refHexShort()}, ECDSAPrivateKey(${key.refHexShort()}))"
        is Ed25519Key -> "SigningPrivateKey(${refHexShort()}, ${key})"
        is MLDSAKey -> "SigningPrivateKey(${refHexShort()}, ${key})"
    }

    companion object {
        /** Decodes a [SigningPrivateKey] from untagged CBOR. */
        fun fromUntaggedCbor(cbor: Cbor): SigningPrivateKey {
            return when (val case = cbor.cborCase) {
                is CborCase.CborByteString -> {
                    // Byte string -> Schnorr private key
                    val data = case.value.toByteArray()
                    SchnorrKey(ECPrivateKey.fromData(data))
                }
                is CborCase.Array -> {
                    val elements = case.value
                    if (elements.size != 2) {
                        throw BcComponentsException.invalidData(
                            "SigningPrivateKey",
                            "expected array of length 2, got ${elements.size}",
                        )
                    }
                    val discriminator = elements[0].tryInt()
                    val data = elements[1].tryByteStringData()
                    when (discriminator) {
                        1 -> ECDSAKey(ECPrivateKey.fromData(data))
                        2 -> Ed25519Key(Ed25519PrivateKey.fromData(data))
                        else -> throw BcComponentsException.invalidData(
                            "SigningPrivateKey",
                            "unknown discriminator: $discriminator",
                        )
                    }
                }
                is CborCase.Tagged -> when (case.tag.value) {
                    TAG_MLDSA_PRIVATE_KEY -> {
                        val key = MLDSAPrivateKey.fromUntaggedCbor(case.item)
                        MLDSAKey(key)
                    }
                    else -> throw BcComponentsException.invalidData(
                        "SigningPrivateKey",
                        "unsupported tagged private key type: ${case.tag.value}",
                    )
                }
                else -> throw BcComponentsException.invalidData(
                    "SigningPrivateKey", "invalid CBOR format",
                )
            }
        }

        /** Decodes a [SigningPrivateKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): SigningPrivateKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_SIGNING_PRIVATE_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [SigningPrivateKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): SigningPrivateKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_SIGNING_PRIVATE_KEY)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [SigningPrivateKey] from a UR. */
        fun fromUr(ur: UR): SigningPrivateKey {
            ur.checkType("signing-private-key")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [SigningPrivateKey] from a UR string. */
        fun fromUrString(urString: String): SigningPrivateKey =
            fromUr(UR.fromUrString(urString))
    }
}
