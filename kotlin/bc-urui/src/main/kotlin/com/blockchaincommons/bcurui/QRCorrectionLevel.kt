package com.blockchaincommons.bcurui

import com.google.zxing.qrcode.decoder.ErrorCorrectionLevel

/** QR code error correction levels. */
enum class QRCorrectionLevel(val zxing: ErrorCorrectionLevel) {
    Low(ErrorCorrectionLevel.L),
    Medium(ErrorCorrectionLevel.M),
    Quartile(ErrorCorrectionLevel.Q),
    High(ErrorCorrectionLevel.H)
}
