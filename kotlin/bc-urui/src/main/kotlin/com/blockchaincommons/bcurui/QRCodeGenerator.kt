package com.blockchaincommons.bcurui

import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.Paint
import android.graphics.Rect
import com.google.zxing.BarcodeFormat
import com.google.zxing.EncodeHintType
import com.google.zxing.qrcode.QRCodeWriter
import com.google.zxing.qrcode.encoder.Encoder
import kotlin.math.floor
import kotlin.math.max
import kotlin.math.roundToInt

/** Generates a QR code bitmap from the given data bytes, optionally with a logo overlay. */
fun makeQRCodeBitmap(
    message: ByteArray,
    correctionLevel: QRCorrectionLevel = QRCorrectionLevel.Medium,
    size: Int = 512,
    foregroundColor: Int = Color.BLACK,
    backgroundColor: Int = Color.TRANSPARENT,
    logo: QRLogo? = null
): Bitmap {
    val effectiveCorrection = if (logo != null) QRCorrectionLevel.Quartile else correctionLevel

    val hints = mapOf(
        EncodeHintType.ERROR_CORRECTION to effectiveCorrection.zxing,
        EncodeHintType.MARGIN to 0
    )

    // Get module count from ZXing's Encoder for logo sizing calculations
    val moduleCount = if (logo != null) {
        val qrCode = Encoder.encode(
            String(message, Charsets.UTF_8),
            effectiveCorrection.zxing,
            hints
        )
        qrCode.matrix.width
    } else {
        0
    }

    // Calculate module-aligned compositing size when logo is present
    val compositingSize = if (logo != null && moduleCount > 0) {
        val pixelsPerModule = max(1, size / moduleCount)
        moduleCount * pixelsPerModule
    } else {
        size
    }

    val matrix = QRCodeWriter().encode(
        String(message, Charsets.UTF_8),
        BarcodeFormat.QR_CODE,
        compositingSize,
        compositingSize,
        hints
    )

    val bitmap = Bitmap.createBitmap(matrix.width, matrix.height, Bitmap.Config.ARGB_8888)
    for (x in 0 until matrix.width) {
        for (y in 0 until matrix.height) {
            bitmap.setPixel(x, y, if (matrix.get(x, y)) foregroundColor else backgroundColor)
        }
    }

    if (logo == null || moduleCount == 0) {
        return if (compositingSize != size) {
            Bitmap.createScaledBitmap(bitmap, size, size, false)
        } else {
            bitmap
        }
    }

    // Composite logo onto QR code
    val layout = LogoLayout(moduleCount, logo.requestedFraction)
    if (layout.logoModules < 3) {
        return if (compositingSize != size) {
            Bitmap.createScaledBitmap(bitmap, size, size, false)
        } else {
            bitmap
        }
    }

    val pixelsPerModule = max(1, compositingSize / moduleCount)
    val canvas = Canvas(bitmap)

    // 1. Clear center area (logo + 1-module border on each side)
    val clearPixels = layout.clearedModules * pixelsPerModule
    val clearOrigin = (compositingSize - clearPixels) / 2
    val clearColor = if (Color.alpha(backgroundColor) < 3) Color.WHITE else backgroundColor
    val clearPaint = Paint().apply { color = clearColor; style = Paint.Style.FILL }
    canvas.drawRect(
        clearOrigin.toFloat(),
        clearOrigin.toFloat(),
        (clearOrigin + clearPixels).toFloat(),
        (clearOrigin + clearPixels).toFloat(),
        clearPaint
    )

    // 2. Draw logo centered within the cleared area
    val logoPixels = layout.logoModules * pixelsPerModule
    val logoOrigin = (compositingSize - logoPixels) / 2
    val logoSrc = Rect(0, 0, logo.bitmap.width, logo.bitmap.height)
    val logoDst = Rect(logoOrigin, logoOrigin, logoOrigin + logoPixels, logoOrigin + logoPixels)
    val logoPaint = Paint().apply { isFilterBitmap = true }
    canvas.drawBitmap(logo.bitmap, logoSrc, logoDst, logoPaint)

    // Scale to requested size if compositing size differs
    return if (compositingSize != size) {
        Bitmap.createScaledBitmap(bitmap, size, size, false)
    } else {
        bitmap
    }
}

/** Calculates logo and clearing dimensions in QR modules. */
internal class LogoLayout(moduleCount: Int, requestedFraction: Float) {
    val logoModules: Int
    val clearedModules: Int

    init {
        // Calculate logo size in modules
        var logo = (moduleCount * requestedFraction).roundToInt()
        // Make odd for symmetry
        if (logo % 2 == 0) logo++
        // Add 1-module border on each side
        var cleared = logo + 2
        // Cap: cleared area must not exceed 40% of QR width
        val maxCleared = floor(moduleCount * 0.40).toInt()
        if (cleared > maxCleared) {
            cleared = maxCleared
            logo = cleared - 2
        }
        // Ensure logo has odd module count
        if (logo % 2 == 0) logo--

        this.logoModules = maxOf(0, logo)
        this.clearedModules = maxOf(0, cleared)
    }
}
