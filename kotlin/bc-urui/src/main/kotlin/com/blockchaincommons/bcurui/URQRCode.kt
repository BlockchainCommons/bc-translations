package com.blockchaincommons.bcurui

import android.graphics.Bitmap
import androidx.compose.foundation.Image
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.graphics.asImageBitmap
import androidx.compose.ui.graphics.toArgb
import androidx.compose.ui.layout.ContentScale

/** Displays a (possibly animated) QR code, optionally with a centered logo overlay. */
@Composable
fun URQRCode(
    data: ByteArray,
    modifier: Modifier = Modifier,
    foregroundColor: Color = Color.Black,
    backgroundColor: Color = Color.Transparent,
    logo: QRLogo? = null
) {
    if (data.isEmpty()) return

    val bitmap = remember(data, foregroundColor, backgroundColor, logo) {
        makeQRCodeBitmap(
            message = data,
            correctionLevel = if (logo != null) QRCorrectionLevel.High else QRCorrectionLevel.Low,
            foregroundColor = foregroundColor.toArgb(),
            backgroundColor = backgroundColor.toArgb(),
            logo = logo
        )
    }

    Image(
        bitmap = bitmap.asImageBitmap(),
        contentDescription = "QR Code",
        modifier = modifier.aspectRatio(1f),
        contentScale = ContentScale.Fit
    )
}
