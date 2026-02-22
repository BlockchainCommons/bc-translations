package com.blockchaincommons.sskr

/**
 * A secret to be split into shares.
 *
 * The data is defensively copied on construction and never exposed by reference.
 *
 * @param data The raw secret bytes. Must be [MIN_SECRET_LEN]..[MAX_SECRET_LEN] bytes
 *   long with an even length.
 * @throws SskrException if the data length is invalid.
 */
class Secret(data: ByteArray) {
    private val bytes: ByteArray = data.copyOf()

    init {
        val len = bytes.size
        if (len < MIN_SECRET_LEN) {
            throw SskrException.SecretTooShort()
        }
        if (len > MAX_SECRET_LEN) {
            throw SskrException.SecretTooLong()
        }
        if ((len and 1) != 0) {
            throw SskrException.SecretLengthNotEven()
        }
    }

    /** The length of the secret in bytes. */
    val length: Int
        get() = bytes.size

    /** Whether the secret is empty. */
    val isEmpty: Boolean
        get() = length == 0

    /** Returns a defensive copy of the secret bytes. */
    fun toByteArray(): ByteArray = bytes.copyOf()

    /** Returns the backing array without copying (internal use only). */
    internal fun dataRef(): ByteArray = bytes

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is Secret) return false
        return bytes.contentEquals(other.bytes)
    }

    override fun hashCode(): Int = bytes.contentHashCode()
}
