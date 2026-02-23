@file:OptIn(ExperimentalStdlibApi::class)
package com.blockchaincommons.bcenvelope

/**
 * Returns the CBOR hex dump of this envelope.
 */
fun Envelope.hex(): String = taggedCbor().toCborData().toHexString()
