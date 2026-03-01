package com.blockchaincommons.bcurui

import androidx.compose.runtime.Composable
import androidx.compose.runtime.remember
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.viewinterop.AndroidView
import androidx.lifecycle.compose.LocalLifecycleOwner

/**
 * A Composable that displays camera preview and scans QR codes.
 *
 * Wraps [URVideoSession] in an AndroidView for integration
 * with Jetpack Compose layouts.
 */
@Composable
fun URVideo(
    videoSession: URVideoSession,
    modifier: Modifier = Modifier
) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current

    AndroidView(
        factory = { ctx ->
            videoSession.bind(ctx, lifecycleOwner)
        },
        modifier = modifier
    )
}
