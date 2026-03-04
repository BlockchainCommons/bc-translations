package com.blockchaincommons.bcurui

/** Exceptions related to QR code generation parameters. */
sealed class QRGenerationException(message: String) : Exception(message) {
    /** The QR code's module count exceeds the recommended scanning limit. */
    class QRCodeTooDense(val moduleCount: Int, val maxModules: Int) :
        QRGenerationException(
            "QR code too dense: $moduleCount modules exceeds limit of $maxModules"
        )

    /** Fewer frames were requested than the message has fountain-coded fragments. */
    class InsufficientFrames(val requested: Int, val fragments: Int) :
        QRGenerationException(
            "Insufficient frames: $requested requested but message requires at least $fragments fragments"
        )
}
