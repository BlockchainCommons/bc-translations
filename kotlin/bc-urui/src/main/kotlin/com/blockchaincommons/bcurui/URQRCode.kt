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

/** Displays a (possibly animated) QR code. Empty data renders nothing. */
@Composable
fun URQRCode(
    data: ByteArray,
    modifier: Modifier = Modifier,
    foregroundColor: Color = Color.Black,
    backgroundColor: Color = Color.Transparent
) {
    if (data.isEmpty()) return

    val bitmap = remember(data, foregroundColor, backgroundColor) {
        makeQRCodeBitmap(
            message = data,
            correctionLevel = QRCorrectionLevel.Low,
            foregroundColor = foregroundColor.toArgb(),
            backgroundColor = backgroundColor.toArgb()
        )
    }

    Image(
        bitmap = bitmap.asImageBitmap(),
        contentDescription = "QR Code",
        modifier = modifier.aspectRatio(1f),
        contentScale = ContentScale.Fit
    )
}
