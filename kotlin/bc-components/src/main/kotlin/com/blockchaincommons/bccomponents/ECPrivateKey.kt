package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.ECDSA_PRIVATE_KEY_SIZE
import com.blockchaincommons.bccrypto.ECDSA_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.SCHNORR_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.deriveSigningPrivateKey
import com.blockchaincommons.bccrypto.ecdsaPublicKeyFromPrivateKey
import com.blockchaincommons.bccrypto.ecdsaSign
import com.blockchaincommons.bccrypto.schnorrPublicKeyFromPrivateKey
import com.blockchaincommons.bccrypto.schnorrSign
import com.blockchaincommons.bccrypto.schnorrSignUsing
import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.bctags.TAG_EC_KEY
import com.blockchaincommons.bctags.TAG_EC_KEY_V1
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.URCodable
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborMap
import com.blockchaincommons.dcbor.CborTaggedCodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues

/**
 * A private key for elliptic curve digital signature algorithms on secp256k1.
 *
 * An [ECPrivateKey] is a 32-byte secret value that can be used to generate its
 * corresponding public key, sign messages using ECDSA, and sign messages using
 * the Schnorr signature scheme (BIP-340).
 *
 * These keys use the secp256k1 curve, the same curve used in Bitcoin and
 * other cryptocurrencies.
 */
class ECPrivateKey private constructor(private val keyData: ByteArray) :
    ECKey,
    ReferenceProvider,
    CborTaggedCodable,
    URCodable {

    init {
        require(keyData.size == KEY_SIZE) {
            "EC private key must be exactly $KEY_SIZE bytes, got ${keyData.size}"
        }
    }

    // -- ECKeyBase --

    override fun data(): ByteArray = keyData.copyOf()

    // -- ECKey --

    /** Derives the corresponding ECDSA compressed public key. */
    override fun publicKey(): ECPublicKey =
        ECPublicKey.fromData(ecdsaPublicKeyFromPrivateKey(keyData))

    /** Derives the Schnorr (x-only) public key from this private key. */
    fun schnorrPublicKey(): SchnorrPublicKey =
        SchnorrPublicKey.fromData(schnorrPublicKeyFromPrivateKey(keyData))

    /**
     * Signs a message using ECDSA.
     *
     * @param message the data to sign
     * @return a 64-byte compact ECDSA signature
     */
    fun ecdsaSign(message: ByteArray): ByteArray {
        val sig = ecdsaSign(keyData, message)
        require(sig.size == ECDSA_SIGNATURE_SIZE)
        return sig
    }

    /**
     * Signs a message using the Schnorr signature scheme (BIP-340).
     *
     * Uses the secure random number generator for nonce generation.
     *
     * @param message the data to sign
     * @return a 64-byte Schnorr signature
     */
    fun schnorrSign(message: ByteArray): ByteArray {
        val sig = schnorrSign(keyData, message)
        require(sig.size == SCHNORR_SIGNATURE_SIZE)
        return sig
    }

    /**
     * Signs a message using the Schnorr signature scheme with a custom RNG.
     *
     * @param message the data to sign
     * @param rng the random number generator to use for nonce generation
     * @return a 64-byte Schnorr signature
     */
    fun schnorrSignUsing(
        message: ByteArray,
        rng: RandomNumberGenerator,
    ): ByteArray {
        val sig = schnorrSignUsing(keyData, message, rng)
        require(sig.size == SCHNORR_SIGNATURE_SIZE)
        return sig
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(taggedCbor().toCborData()))

    // -- CBOR --

    override fun cborTags(): List<Tag> =
        tagsForValues(listOf(TAG_EC_KEY, TAG_EC_KEY_V1))

    /**
     * Encodes as a CBOR map: {2: true, 3: key_bytes}.
     *
     * Key 2 (boolean true) indicates this is a private key.
     * Key 3 is the raw key data as a byte string.
     */
    override fun untaggedCbor(): Cbor {
        val map = CborMap()
        map.insert(Cbor.fromInt(2), Cbor.`true`())
        map.insert(Cbor.fromInt(3), Cbor.fromByteString(keyData))
        return Cbor.fromMap(map)
    }

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ECPrivateKey) return false
        return keyData.contentEquals(other.keyData)
    }

    override fun hashCode(): Int = keyData.contentHashCode()

    // -- toString --

    override fun toString(): String = "ECPrivateKey(${refHexShort()})"

    companion object {
        const val KEY_SIZE: Int = ECDSA_PRIVATE_KEY_SIZE

        /** Creates a new random EC private key using the secure RNG. */
        fun create(): ECPrivateKey {
            val rng = SecureRandomNumberGenerator()
            return createUsing(rng)
        }

        /** Creates a new random EC private key using the given [rng]. */
        fun createUsing(rng: RandomNumberGenerator): ECPrivateKey {
            val data = rng.randomData(KEY_SIZE)
            return ECPrivateKey(data)
        }

        /** Restores an EC private key from exactly [KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): ECPrivateKey {
            if (data.size != KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "EC private key", KEY_SIZE, data.size,
                )
            }
            return ECPrivateKey(data.copyOf())
        }

        /**
         * Derives an EC private key from arbitrary key material.
         *
         * Uses HKDF to deterministically derive a valid secp256k1 key.
         */
        fun deriveFromKeyMaterial(keyMaterial: ByteArray): ECPrivateKey =
            fromData(deriveSigningPrivateKey(keyMaterial))

        /**
         * Creates an EC private key from a hexadecimal string.
         *
         * @throws BcComponentsException.InvalidSize if the decoded bytes
         *   are not exactly [KEY_SIZE] bytes
         */
        fun fromHex(hex: String): ECPrivateKey =
            fromData(hex.hexToByteArray())

        /** Decodes an [ECPrivateKey] from untagged CBOR (a map). */
        fun fromUntaggedCbor(cbor: Cbor): ECPrivateKey {
            val map = cbor.tryMap()
            val isPrivate: Boolean = map.get<Int, Boolean>(2)
                ?: throw BcComponentsException.invalidData(
                    "EC key", "missing private key flag (key 2)",
                )
            if (!isPrivate) {
                throw BcComponentsException.invalidData(
                    "EC key", "expected private key (key 2 = true)",
                )
            }
            val bytes: ByteArray = map.extract<Int, ByteArray>(3)
            return fromData(bytes)
        }

        /** Decodes an [ECPrivateKey] from tagged CBOR. */
        fun fromTaggedCbor(cbor: Cbor): ECPrivateKey =
            CborTaggedUtils.fromTaggedCbor(
                cbor,
                tagsForValues(listOf(TAG_EC_KEY, TAG_EC_KEY_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [ECPrivateKey] from tagged CBOR binary data. */
        fun fromTaggedCborData(data: ByteArray): ECPrivateKey =
            CborTaggedUtils.fromTaggedCborData(
                data,
                tagsForValues(listOf(TAG_EC_KEY, TAG_EC_KEY_V1)),
            ) { fromUntaggedCbor(it) }

        /** Decodes an [ECPrivateKey] from a UR. */
        fun fromUr(ur: UR): ECPrivateKey {
            ur.checkType("eckey")
            return fromUntaggedCbor(ur.cbor)
        }

        /** Decodes an [ECPrivateKey] from a UR string. */
        fun fromUrString(urString: String): ECPrivateKey =
            fromUr(UR.fromUrString(urString))
    }
}
