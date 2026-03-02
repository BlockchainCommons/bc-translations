package com.blockchaincommons.bcurui

import android.graphics.Bitmap
import android.graphics.Canvas
import android.graphics.Color
import android.graphics.RectF
import com.caverock.androidsvg.SVG

/**
 * A logo image to overlay on the center of a QR code.
 *
 * The logo is pre-rendered at construction time and cached as a [Bitmap].
 * During QR compositing, it is scaled to fit the calculated logo area.
 */
class QRLogo private constructor(
    internal val bitmap: Bitmap,
    /** The desired logo width as a fraction of the QR code width (0.0..1.0). */
    val requestedFraction: Float
) {
    companion object {
        private const val RENDER_SIZE = 512

        /**
         * Create a logo from raw SVG data bytes.
         *
         * The SVG is rasterized once at 512x512 and cached for reuse across QR frames.
         *
         * @param svgData Raw SVG file bytes.
         * @param fraction Desired logo width as a fraction of the QR code width. Default 0.25.
         */
        fun fromSVG(svgData: ByteArray, fraction: Float = 0.25f): QRLogo {
            return fromSVG(String(svgData, Charsets.UTF_8), fraction)
        }

        /**
         * Create a logo from an SVG string.
         *
         * @param svgString SVG markup as a string.
         * @param fraction Desired logo width as a fraction of the QR code width. Default 0.25.
         */
        fun fromSVG(svgString: String, fraction: Float = 0.25f): QRLogo {
            val clamped = fraction.coerceIn(0.01f, 0.99f)
            val svg = SVG.getFromString(svgString)
            val bitmap = renderSVG(svg, RENDER_SIZE)
            return QRLogo(bitmap, clamped)
        }

        /**
         * Create a logo from a pre-rendered bitmap.
         *
         * @param bitmap A [Bitmap] to use as the logo.
         * @param fraction Desired logo width as a fraction of the QR code width. Default 0.25.
         */
        fun fromBitmap(bitmap: Bitmap, fraction: Float = 0.25f): QRLogo {
            val clamped = fraction.coerceIn(0.01f, 0.99f)
            return QRLogo(bitmap, clamped)
        }

        private fun renderSVG(svg: SVG, size: Int): Bitmap {
            val bitmap = Bitmap.createBitmap(size, size, Bitmap.Config.ARGB_8888)
            val canvas = Canvas(bitmap)

            // Scale SVG to fill the target size, preserving aspect ratio
            val svgWidth = if (svg.documentWidth > 0) svg.documentWidth else size.toFloat()
            val svgHeight = if (svg.documentHeight > 0) svg.documentHeight else size.toFloat()
            val scale = minOf(size / svgWidth, size / svgHeight)
            val scaledWidth = svgWidth * scale
            val scaledHeight = svgHeight * scale
            val offsetX = (size - scaledWidth) / 2
            val offsetY = (size - scaledHeight) / 2

            svg.renderToCanvas(
                canvas,
                RectF(offsetX, offsetY, offsetX + scaledWidth, offsetY + scaledHeight)
            )
            return bitmap
        }
    }
}
