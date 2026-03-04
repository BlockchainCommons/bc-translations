package com.blockchaincommons.bcurui

import android.annotation.SuppressLint
import android.content.Context
import android.graphics.RectF
import androidx.camera.core.CameraSelector
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.ImageProxy
import androidx.camera.core.Preview
import androidx.camera.lifecycle.ProcessCameraProvider
import androidx.camera.view.PreviewView
import androidx.core.content.ContextCompat
import androidx.lifecycle.LifecycleOwner
import com.google.android.gms.tasks.Tasks
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.barcode.common.Barcode
import com.google.mlkit.vision.common.InputImage
import com.google.mlkit.vision.text.TextRecognition
import com.google.mlkit.vision.text.TextRecognizer
import com.google.mlkit.vision.text.latin.TextRecognizerOptions
import java.util.concurrent.Executors

/**
 * Manages a CameraX session for QR code scanning and optional text recognition.
 *
 * Scanned QR code strings are delivered to [onCodesScanned].
 * When [onTextRecognized] is provided, recognized text blocks are also delivered.
 */
class URVideoSession(
    private val onTextRecognized: ((List<URRecognizedText>) -> Unit)? = null,
    private val onCodesScanned: (Set<String>) -> Unit
) {
    private val executor = Executors.newSingleThreadExecutor()
    private val scanner = BarcodeScanning.getClient()
    private val textRecognizer: TextRecognizer? =
        if (onTextRecognized != null) TextRecognition.getClient(TextRecognizerOptions.DEFAULT_OPTIONS)
        else null
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
    private fun processImage(imageProxy: ImageProxy) {
        val mediaImage = imageProxy.image
        if (mediaImage == null) {
            imageProxy.close()
            return
        }

        val inputImage = InputImage.fromMediaImage(
            mediaImage,
            imageProxy.imageInfo.rotationDegrees
        )

        val barcodeTask = scanner.process(inputImage)
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

        val recognizer = textRecognizer
        if (recognizer != null) {
            val imageWidth = inputImage.width.toFloat()
            val imageHeight = inputImage.height.toFloat()

            val textTask = recognizer.process(inputImage)
                .addOnSuccessListener { visionText ->
                    if (visionText.textBlocks.isNotEmpty() && imageWidth > 0 && imageHeight > 0) {
                        val texts = visionText.textBlocks.mapNotNull { block ->
                            val box = block.boundingBox ?: return@mapNotNull null
                            URRecognizedText(
                                text = block.text,
                                boundingBox = RectF(
                                    box.left / imageWidth,
                                    box.top / imageHeight,
                                    box.right / imageWidth,
                                    box.bottom / imageHeight
                                ),
                                confidence = 1.0f // ML Kit doesn't expose block-level confidence
                            )
                        }
                        if (texts.isNotEmpty()) {
                            onTextRecognized?.invoke(texts)
                        }
                    }
                }

            // Close the proxy only after both tasks complete.
            Tasks.whenAllComplete(barcodeTask, textTask)
                .addOnCompleteListener { imageProxy.close() }
        } else {
            // Text recognition not requested — close after barcode completes.
            barcodeTask.addOnCompleteListener { imageProxy.close() }
        }
    }
}
