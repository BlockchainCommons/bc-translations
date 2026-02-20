package com.blockchaincommons.bccrypto

fun memzero(data: ByteArray) {
    data.fill(0)
}

fun memzeroAll(data: List<ByteArray>) {
    data.forEach { memzero(it) }
}
