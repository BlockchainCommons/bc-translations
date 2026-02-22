package com.blockchaincommons.bccomponents

/**
 * A type that can provide a single unique digest characterizing its contents.
 *
 * Implementations produce a cryptographic [Digest] (SHA-256) that uniquely
 * identifies the content of the implementing type.
 *
 * Use cases include data integrity verification, content-addressable storage,
 * and comparing objects by content rather than identity.
 */
interface DigestProvider {
    /** Returns a digest that uniquely characterizes this object's content. */
    fun digest(): Digest
}

/**
 * Extension that lets any [ByteArray] act as a [DigestProvider] by hashing
 * its contents.
 */
fun ByteArray.toDigest(): Digest = Digest.fromImage(this)
