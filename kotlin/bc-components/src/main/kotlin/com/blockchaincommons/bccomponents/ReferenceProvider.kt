package com.blockchaincommons.bccomponents

/**
 * A type that can provide a globally unique cryptographic reference to itself.
 *
 * The reference is derived from a digest of the object's serialized form,
 * ensuring that it uniquely identifies the object's contents.
 */
interface ReferenceProvider {
    /** Returns a cryptographic reference that uniquely identifies this object. */
    fun reference(): Reference

    /** Returns the reference data as a hexadecimal string. */
    fun refHex(): String = reference().refHex()

    /** Returns the first four bytes of the reference. */
    fun refDataShort(): ByteArray = reference().refDataShort()

    /** Returns the first four bytes of the reference as a hexadecimal string. */
    fun refHexShort(): String = reference().refHexShort()

    /**
     * Returns the first four bytes of the reference as upper-case ByteWords.
     *
     * @param prefix an optional prefix to prepend
     */
    fun refBytewords(prefix: String? = null): String =
        reference().bytewordsIdentifier(prefix)

    /**
     * Returns the first four bytes of the reference as Bytemoji.
     *
     * @param prefix an optional prefix to prepend
     */
    fun refBytemoji(prefix: String? = null): String =
        reference().bytemojiIdentifier(prefix)
}
