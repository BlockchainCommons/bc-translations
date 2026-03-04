package com.blockchaincommons.bcurui

import android.graphics.RectF

/**
 * A block of text recognized by the camera's text recognition pipeline.
 *
 * Bounding boxes use normalized coordinates (0-1) with a top-left origin.
 */
data class URRecognizedText(
    /** The recognized text string. */
    val text: String,

    /** Normalized bounding box (0-1, top-left origin) of the text in the camera frame. */
    val boundingBox: RectF,

    /** Recognition confidence (0-1). */
    val confidence: Float,

    /** The rotation (in degrees, 0/90/180/270) at which the text was detected. */
    val rotation: Int = 0
)
