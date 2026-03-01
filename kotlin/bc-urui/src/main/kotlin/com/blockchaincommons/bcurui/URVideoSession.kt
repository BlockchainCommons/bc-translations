package com.blockchaincommons.bcurui

import android.annotation.SuppressLint
import android.content.Context
import androidx.camera.core.CameraSelector
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.core.content.ContextCompat
import androidx.lifecycle.LifecycleOwner
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.barcode.common.Barcode
import com.google.mlkit.vision.common.InputImage
import java.util.concurrent.Executors

/**
 * Manages a CameraX session for QR code scanning.
 *
 * Scanned QR code strings are delivered to [onCodesScanned].
 */
class URVideoSession(
    private val onCodesScanned: (Set<String>) -> Unit
) {
    private val executor = Executors.newSingleThreadExecutor()
    private val scanner = BarcodeScanning.getClient()
    private var lastFound: Set<String> = emptySet()

    /**
     * Binds the camera preview and QR analysis to the given lifecycle owner.
     *
     * Returns a [PreviewView] to be placed in the view hierarchy.
     */
    fun bind(
        context: Context,
        lifecycleOwner: LifecycleOwner
    ): PreviewView {
        val previewView = PreviewView(context)

        val cameraProviderFuture = ProcessCameraProvider.getInstance(context)
        cameraProviderFuture.addListener({
            val cameraProvider = cameraProviderFuture.get()

            val preview = Preview.Builder().build().also {
                it.surfaceProvider = previewView.surfaceProvider
            }

            val analysis = ImageAnalysis.Builder()
                .setBackpressureStrategy(ImageAnalysis.STRATEGY_KEEP_ONLY_LATEST)
                .build()

            analysis.setAnalyzer(executor) { imageProxy ->
                processImage(imageProxy)
            }

            cameraProvider.unbindAll()
            cameraProvider.bindToLifecycle(
                lifecycleOwner,
                CameraSelector.DEFAULT_BACK_CAMERA,
                preview,
                analysis
            )
        }, ContextCompat.getMainExecutor(context))

        return previewView
    }

    @SuppressLint("UnsafeOptInUsageError")
    private fun processImage(imageProxy: androidx.camera.core.ImageProxy) {
        val mediaImage = imageProxy.image
        if (mediaImage == null) {
            imageProxy.close()
            return
        }

        val inputImage = InputImage.fromMediaImage(
            mediaImage,
            imageProxy.imageInfo.rotationDegrees
        )

        scanner.process(inputImage)
            .addOnSuccessListener { barcodes ->
                val codes = barcodes
                    .filter { it.format == Barcode.FORMAT_QR_CODE }
                    .mapNotNull { it.rawValue }
                    .toSet()

                if (codes.isNotEmpty() && codes != lastFound) {
                    lastFound = codes
                    onCodesScanned(codes)
                }
            }
            .addOnCompleteListener {
                imageProxy.close()
            }
    }
}
