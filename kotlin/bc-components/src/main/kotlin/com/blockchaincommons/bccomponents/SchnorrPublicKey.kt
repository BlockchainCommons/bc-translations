package com.blockchaincommons.bccomponents

import com.blockchaincommons.bccrypto.SCHNORR_PUBLIC_KEY_SIZE
import com.blockchaincommons.bccrypto.SCHNORR_SIGNATURE_SIZE
import com.blockchaincommons.bccrypto.schnorrVerify as cryptoSchnorrVerify

/**
 * A Schnorr (x-only) elliptic curve public key.
 *
 * A [SchnorrPublicKey] is a 32-byte x-only public key used with the BIP-340
 * Schnorr signature scheme. Unlike compressed ECDSA public keys (33 bytes)
 * that include a prefix byte indicating the parity of the y-coordinate,
 * Schnorr public keys only contain the x-coordinate of the elliptic curve
 * point.
 *
 * Schnorr signatures offer several advantages over traditional ECDSA
 * signatures, including linearity (enabling key and signature aggregation),
 * non-malleability, smaller size, better privacy, and provable security.
 */
class SchnorrPublicKey private constructor(private val keyData: ByteArray) :
    ECKeyBase,
    ReferenceProvider {

    init {
        require(keyData.size == KEY_SIZE) {
            "Schnorr public key must be exactly $KEY_SIZE bytes, got ${keyData.size}"
        }
    }

    // -- ECKeyBase --

    override fun data(): ByteArray = keyData.copyOf()

    /**
     * Verifies a BIP-340 Schnorr signature for the given message.
     *
     * @param signature a 64-byte Schnorr signature
     * @param message the message that was signed
     * @return `true` if the signature is valid, `false` otherwise
     */
    fun schnorrVerify(signature: ByteArray, message: ByteArray): Boolean {
        require(signature.size == SCHNORR_SIGNATURE_SIZE) {
            "Schnorr signature must be exactly $SCHNORR_SIGNATURE_SIZE bytes, got ${signature.size}"
        }
        return cryptoSchnorrVerify(keyData, signature, message)
    }

    // -- ReferenceProvider --

    override fun reference(): Reference =
        Reference.fromDigest(Digest.fromImage(keyData))

    // -- equals / hashCode --

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is SchnorrPublicKey) return false
        return keyData.contentEquals(other.keyData)
    }

    override fun hashCode(): Int = keyData.contentHashCode()

    // -- toString --

    override fun toString(): String = "SchnorrPublicKey(${refHexShort()})"

    companion object {
        const val KEY_SIZE: Int = SCHNORR_PUBLIC_KEY_SIZE

        /** Restores a Schnorr public key from exactly [KEY_SIZE] bytes. */
        fun fromData(data: ByteArray): SchnorrPublicKey {
            if (data.size != KEY_SIZE) {
                throw BcComponentsException.invalidSize(
                    "Schnorr public key", KEY_SIZE, data.size,
                )
            }
            return SchnorrPublicKey(data.copyOf())
        }

        /**
         * Creates a Schnorr public key from a hexadecimal string.
         *
         * @throws BcComponentsException.InvalidSize if the decoded bytes
         *   are not exactly [KEY_SIZE] bytes
         */
        fun fromHex(hex: String): SchnorrPublicKey =
            fromData(hex.hexToByteArray())
    }
}
