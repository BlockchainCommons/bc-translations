package com.blockchaincommons.bcur

import java.util.zip.CRC32 as JavaCrc32

/**
 * CRC32/ISO-HDLC checksum.
 *
 * Java's [java.util.zip.CRC32] uses the same CRC-32/ISO-HDLC algorithm
 * (polynomial 0xEDB88320 reflected) as the Rust `crc` crate.
 */
internal object Crc32 {
    fun checksum(data: ByteArray): UInt {
        val crc = JavaCrc32()
        crc.update(data)
        return crc.value.toUInt()
    }
}

/** Encodes a [UInt] as 4 big-endian bytes. */
internal fun UInt.toBytesBigEndian(): ByteArray = byteArrayOf(
    (this shr 24).toByte(),
    (this shr 16).toByte(),
    (this shr 8).toByte(),
    this.toByte()
)
