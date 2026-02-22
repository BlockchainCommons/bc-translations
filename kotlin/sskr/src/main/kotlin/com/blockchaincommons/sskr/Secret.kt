package com.blockchaincommons.sskr

/** A secret to be split into shares. */
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

    /** Returns the length of the secret. */
    val length: Int
        get() = bytes.size

    /** Returns `true` if the secret is empty. */
    val isEmpty: Boolean
        get() = length == 0

    /** Returns a copy of the secret data. */
    fun data(): ByteArray = bytes.copyOf()

    internal fun dataRef(): ByteArray = bytes

    override fun equals(other: Any?): Boolean {
        if (this === other) {
            return true
        }
        if (other !is Secret) {
            return false
        }
        return bytes.contentEquals(other.bytes)
    }

    override fun hashCode(): Int = bytes.contentHashCode()
}
