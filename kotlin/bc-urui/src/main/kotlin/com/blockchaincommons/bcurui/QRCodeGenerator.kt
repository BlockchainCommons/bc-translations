package com.blockchaincommons.bcurui

import android.graphics.Bitmap
import android.graphics.Color
import com.google.zxing.BarcodeFormat
import com.google.zxing.EncodeHintType
import com.google.zxing.qrcode.QRCodeWriter

/** Generates a QR code bitmap from the given data bytes. */
fun makeQRCodeBitmap(
    message: ByteArray,
    correctionLevel: QRCorrectionLevel = QRCorrectionLevel.Medium,
    size: Int = 512,
    foregroundColor: Int = Color.BLACK,
    backgroundColor: Int = Color.TRANSPARENT
): Bitmap {
    val hints = mapOf(
        EncodeHintType.ERROR_CORRECTION to correctionLevel.zxing,
        EncodeHintType.MARGIN to 0
    )
    val matrix = QRCodeWriter().encode(
        String(message, Charsets.UTF_8),
        BarcodeFormat.QR_CODE,
        size,
        size,
        hints
    )
    val bitmap = Bitmap.createBitmap(matrix.width, matrix.height, Bitmap.Config.ARGB_8888)
    for (x in 0 until matrix.width) {
        for (y in 0 until matrix.height) {
            bitmap.setPixel(x, y, if (matrix.get(x, y)) foregroundColor else backgroundColor)
        }
    }
    return bitmap
}
