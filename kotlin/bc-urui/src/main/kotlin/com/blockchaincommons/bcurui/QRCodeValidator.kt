package com.blockchaincommons.bcurui

import com.google.zxing.EncodeHintType
import com.google.zxing.qrcode.encoder.Encoder

/** Default maximum QR module count for reliable phone scanning (QR version 25). */
const val DEFAULT_MAX_QR_MODULES = 117

/** Get the QR module count for a message at a given correction level. */
fun qrModuleCount(
    message: ByteArray,
    correctionLevel: QRCorrectionLevel = QRCorrectionLevel.Medium
): Int {
    val hints = mapOf(EncodeHintType.MARGIN to 0)
    val qrCode = Encoder.encode(
        String(message, Charsets.UTF_8),
        correctionLevel.zxing,
        hints
    )
    return qrCode.matrix.width
}

/**
 * Validate that a QR module count is within a density limit.
 *
 * @throws QRGenerationException.QRCodeTooDense if [moduleCount] exceeds [maxModules].
 */
fun checkQRDensity(
    moduleCount: Int,
    maxModules: Int = DEFAULT_MAX_QR_MODULES
) {
    if (moduleCount > maxModules) {
        throw QRGenerationException.QRCodeTooDense(moduleCount, maxModules)
    }
}
