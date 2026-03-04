package com.blockchaincommons.bcurui

import android.annotation.SuppressLint
import android.content.Context
import android.graphics.RectF
import android.hardware.camera2.CameraMetadata
import android.hardware.camera2.CaptureRequest
import androidx.camera.camera2.interop.Camera2Interop
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
    private val textRecognitionRotations: Set<Int> = setOf(0),
    private val onCodesScanned: (Set<String>) -> Unit
) {
    private val executor = Executors.newSingleThreadExecutor()
    private val scanner = BarcodeScanning.getClient()

    /// The aspect ratio (width / height) of the camera image in the current
    /// device orientation. Used by overlays to correct for resizeAspectFill crop.
    @Volatile
    var orientedImageAspectRatio: Float = 1.0f
        private set

    private val textRecognizer: TextRecognizer? =
        if (onTextRecognized != null) TextRecognition.getClient(TextRecognizerOptions.DEFAULT_OPTIONS)
        else null
    private var lastFound: Set<String> = emptySet()

    /**
     * Binds the camera preview and QR analysis to the given lifecycle owner.
     *
     * Returns a [PreviewView] to be placed in the view hierarchy.
     */
    @SuppressLint("UnsafeOptInUsageError", "RestrictedApi")
    fun bind(
        context: Context,
        lifecycleOwner: LifecycleOwner
    ): PreviewView {
        val previewView = PreviewView(context)

        val cameraProviderFuture = ProcessCameraProvider.getInstance(context)
        cameraProviderFuture.addListener({
            val cameraProvider = cameraProviderFuture.get()

            val previewBuilder = Preview.Builder()
            Camera2Interop.Extender(previewBuilder)
                .setCaptureRequestOption(
                    CaptureRequest.CONTROL_AF_MODE,
                    CameraMetadata.CONTROL_AF_MODE_MACRO
                )
            val preview = previewBuilder.build().also {
                it.surfaceProvider = previewView.surfaceProvider
            }

            val analysisBuilder = ImageAnalysis.Builder()
                .setBackpressureStrategy(ImageAnalysis.STRATEGY_KEEP_ONLY_LATEST)
            Camera2Interop.Extender(analysisBuilder)
                .setCaptureRequestOption(
                    CaptureRequest.CONTROL_AF_MODE,
                    CameraMetadata.CONTROL_AF_MODE_MACRO
                )
            val analysis = analysisBuilder.build()

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

    @SuppressLint("UnsafeOptInUsageError", "RestrictedApi")
    private fun processImage(imageProxy: ImageProxy) {
        val mediaImage = imageProxy.image
        if (mediaImage == null) {
            imageProxy.close()
            return
        }

        val baseDegrees = imageProxy.imageInfo.rotationDegrees

        // Update oriented aspect ratio for overlay crop correction.
        orientedImageAspectRatio = if (baseDegrees == 90 || baseDegrees == 270) {
            imageProxy.height.toFloat() / imageProxy.width.toFloat()
        } else {
            imageProxy.width.toFloat() / imageProxy.height.toFloat()
        }

        val baseInputImage = InputImage.fromMediaImage(mediaImage, baseDegrees)

        val barcodeTask = scanner.process(baseInputImage)
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
            val allTexts = mutableListOf<URRecognizedText>()
            val textTasks = textRecognitionRotations.map { extraRotation ->
                val totalRotation = (baseDegrees + extraRotation) % 360
                val rotatedInput = InputImage.fromMediaImage(mediaImage, totalRotation)
                val imageWidth = rotatedInput.width.toFloat()
                val imageHeight = rotatedInput.height.toFloat()

                recognizer.process(rotatedInput)
                    .addOnSuccessListener { visionText ->
                        if (visionText.textBlocks.isNotEmpty() && imageWidth > 0 && imageHeight > 0) {
                            val texts = visionText.textBlocks.mapNotNull { block ->
                                val box = block.boundingBox ?: return@mapNotNull null
                                val blockConfidence = block.lines
                                    .mapNotNull { line -> line.confidence?.takeIf { it > 0f } }
                                    .minOrNull() ?: 1.0f
                                if (blockConfidence < 0.5f) return@mapNotNull null
                                val normalizedBox = RectF(
                                    box.left / imageWidth,
                                    box.top / imageHeight,
                                    box.right / imageWidth,
                                    box.bottom / imageHeight
                                )
                                val displayBox = transformToDisplay(normalizedBox, extraRotation)
                                URRecognizedText(
                                    text = block.text,
                                    boundingBox = displayBox,
                                    confidence = blockConfidence,
                                    rotation = extraRotation
                                )
                            }
                            synchronized(allTexts) {
                                allTexts.addAll(texts)
                            }
                        }
                    }
            }

            // Close the proxy only after barcode + all text tasks complete.
            Tasks.whenAllComplete(listOf(barcodeTask) + textTasks)
                .addOnCompleteListener {
                    imageProxy.close()
                    val snapshot = synchronized(allTexts) { allTexts.toList() }
                    if (snapshot.isNotEmpty()) {
                        onTextRecognized?.invoke(snapshot)
                    }
                }
        } else {
            // Text recognition not requested — close after barcode completes.
            barcodeTask.addOnCompleteListener { imageProxy.close() }
        }
    }

    companion object {
        /**
         * Transforms a normalized bounding box from a rotated coordinate space
         * back to display coordinates.
         *
         * ML Kit returns bounding boxes relative to the (rotated) image.
         * This maps them back so they overlay correctly on the camera preview.
         */
        fun transformToDisplay(box: RectF, rotation: Int): RectF {
            val vx = box.left
            val vy = box.top
            val vw = box.right - box.left
            val vh = box.bottom - box.top

            return when (((rotation % 360) + 360) % 360) {
                0 -> box
                90 -> RectF(vy, 1f - vx - vw, vy + vh, 1f - vx)
                180 -> RectF(1f - vx - vw, 1f - vy - vh, 1f - vx, 1f - vy)
                270 -> RectF(1f - vy - vh, vx, 1f - vy, vx + vw)
                else -> box
            }
        }
    }
}
