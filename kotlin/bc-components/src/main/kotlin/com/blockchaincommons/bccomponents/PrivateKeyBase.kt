package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.deriveSigningPrivateKey
import com.blockchaincommons.bccrypto.deriveAgreementPrivateKey
import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.bctags.TAG_PRIVATE_KEY_BASE
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A secure key derivation container.
 *
 * [PrivateKeyBase] derives multiple cryptographic keys from a single seed
 * using HKDF key derivation. It provides access to signing keys (EC/Schnorr,
 * Ed25519), agreement keys (X25519), and implements the [Signer], [Verifier],
 * and [Decrypter] interfaces.
 *
 * The minimum seed length is 16 bytes to ensure sufficient entropy.
 */
class PrivateKeyBase private constructor(
    private val data: ByteArray,
) : PrivateKeyDataProvider, PrivateKeysProvider, PublicKeysProvider, Signer, Verifier, Decrypter, ReferenceProvider, CborTaggedCodable, URCodable {

    /** Returns a copy of the underlying seed data. */
    fun data(): ByteArray = data.copyOf()

    // -- Key derivation --

    /** Derives the secp256k1 signing private key. */
    fun signingPrivateKey(): ECPrivateKey =
        ECPrivateKey.deriveFromKeyMaterial(data)

    /** Derives the ECDSA signing private key (alias for [signingPrivateKey]). */
    fun ecdsaSigningPrivateKey(): ECPrivateKey = signingPrivateKey()

    /** Derives the Schnorr signing private key (alias for [signingPrivateKey]). */
    fun schnorrSigningPrivateKey(): ECPrivateKey = signingPrivateKey()

    /** Derives the Ed25519 signing private key. */
    fun ed25519SigningPrivateKey(): Ed25519PrivateKey =
        Ed25519PrivateKey.deriveFromKeyMaterial(data)

    /** Derives the X25519 agreement private key. */
    fun x25519PrivateKey(): X25519PrivateKey =
        X25519PrivateKey.deriveFromKeyMaterial(data)

    /** Derives the X25519 agreement public key. */
    fun x25519PublicKey(): X25519PublicKey =
        x25519PrivateKey().publicKey()

    /** Returns the default signing public key (Schnorr). */
    fun signingPublicKey(): SigningPublicKey =
        SigningPublicKey.SchnorrKey(signingPrivateKey().schnorrPublicKey())

    /** Returns the default encapsulation public key (X25519). */
    fun encapsulationPublicKey(): EncapsulationPublicKey =
        EncapsulationPublicKey.X25519(x25519PublicKey())

    // -- PrivateKeysProvider --

    /**
     * Returns the complete set of private keys (signing + encapsulation).
     */
    override fun privateKeys(): PrivateKeys =
        PrivateKeys(
            SigningPrivateKey.SchnorrKey(signingPrivateKey()),
            EncapsulationPrivateKey.X25519(x25519PrivateKey()),
        )

    // -- PublicKeysProvider --

    /**
     * Returns the complete set of public keys (signing + encapsulation).
     */
    override fun publicKeys(): PublicKeys =
        PublicKeys(
            signingPublicKey(),
            encapsulationPublicKey(),
        )

    // -- PrivateKeyDataProvider --

    override fun privateKeyData(): ByteArray = data.copyOf()

    // -- Signer --

    override fun signWithOptions(message: ByteArray, options: SigningOptions?): Signature {
        val signingKey = SigningPrivateKey.SchnorrKey(signingPrivateKey())
        return signingKey.signWithOptions(message, options)
    }

    // -- Verifier --

    override fun verify(signature: Signature, message: ByteArray): Boolean {
        return signingPublicKey().verify(signature, message)
    }

    // -- Decrypter --

    override fun encapsulationPrivateKey(): EncapsulationPrivateKey =
        EncapsulationPrivateKey.X25519(x25519PrivateKey())

    override fun decapsulateSharedSecret(ciphertext: EncapsulationCiphertext): SymmetricKey =
        encapsulationPrivateKey().decapsulateSharedSecret(ciphertext)

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is PrivateKeyBase) return false
        return data.contentEquals(other.data)
    }

    override fun hashCode(): Int = data.contentHashCode()

    override fun toString(): String = "PrivateKeyBase(${refHexShort()})"

    // -- CBOR --

    override fun cborTags(): List<Tag> = tagsForValues(listOf(TAG_PRIVATE_KEY_BASE))

    override fun untaggedCbor(): Cbor = Cbor.fromByteString(data)

    companion object {
        /**
         * Creates a new random private key base with 32 bytes of entropy.
         */
        fun create(): PrivateKeyBase {
            val rng = SecureRandomNumberGenerator()
            return createUsing(rng)
        }

        /**
         * Creates a new random private key base using the given [rng].
         */
        fun createUsing(rng: RandomNumberGenerator): PrivateKeyBase {
            val data = rng.randomData(32)
            return PrivateKeyBase(data)
        }

        /**
         * Restores a private key base from the given data.
         *
         * @throws BcComponentsException.DataTooShort if [data] length < 16
         */
        fun fromData(data: ByteArray): PrivateKeyBase {
            if (data.size < 16) {
                throw BcComponentsException.dataTooShort("private key base", 16, data.size)
            }
            return PrivateKeyBase(data.copyOf())
        }

        /**
         * Creates a private key base from a hexadecimal string.
         */
        fun fromHex(hex: String): PrivateKeyBase = fromData(hex.hexToByteArray())

        /** Decodes a [PrivateKeyBase] from untagged CBOR (a byte string). */
        fun fromUntaggedCbor(cbor: Cbor): PrivateKeyBase {
            val bytes = cbor.tryByteStringData()
            return fromData(bytes)
        }

        /** Decodes a [PrivateKeyBase] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): PrivateKeyBase =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_PRIVATE_KEY_BASE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [PrivateKeyBase] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): PrivateKeyBase =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_PRIVATE_KEY_BASE)),
            ) { fromUntaggedCbor(it) }

        /** Decodes a [PrivateKeyBase] from a UR. */
        fun fromUr(ur: UR): PrivateKeyBase {
            ur.checkType("crypto-prvkey-base")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes a [PrivateKeyBase] from a UR string. */
        fun fromUrString(urString: String): PrivateKeyBase =
            fromUr(UR.fromUrString(urString))
    }
}
