package com.blockchaincommons.bcurui

import com.blockchaincommons.bcur.MultipartDecoder
import com.blockchaincommons.bcur.UR
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue

/**
 * Tracks and reports state of ongoing multi-part UR capture.
 *
 * Feed scanned QR code strings via [receiveCodes]. Observe [lastResult]
 * for decoded URs, progress updates, or errors.
 */
class URScanState {
    /** The most recent scan result. */
    var lastResult: URScanResult? by mutableStateOf(null)
        private set

    private var decoder = MultipartDecoder()
    private var expectedFragmentCount: Int? = null
    private var receivedCount: Int = 0
    private var hasReceivedFirstPart: Boolean = false

    fun restart() {
        decoder = MultipartDecoder()
        expectedFragmentCount = null
        receivedCount = 0
        hasReceivedFirstPart = false
        lastResult = null
    }

    fun receiveCodes(codes: Set<String>) {
        for (code in codes) {
            processCode(code)
        }
    }

    fun receiveError(error: Throwable) {
        lastResult = URScanResult.Failure(error)
    }

    private val progress: URScanProgress
        get() {
            val count = expectedFragmentCount ?: 1
            val percent = if (count > 0) minOf(receivedCount.toDouble() / count, 1.0) else 0.0
            val states = (0 until count).map { i ->
                when {
                    i < receivedCount -> FragmentState.Highlighted
                    i == receivedCount -> FragmentState.On
                    else -> FragmentState.Off
                }
            }
            return URScanProgress(estimatedPercentComplete = percent, fragmentStates = states)
        }

    private fun processCode(code: String) {
        val trimmed = code.trim()

        if (!trimmed.lowercase().startsWith("ur:")) {
            lastResult = URScanResult.Other(code)
            return
        }

        try {
            // Try single-part UR first
            if (isSinglePartUR(trimmed)) {
                val ur = UR.fromUrString(trimmed)
                lastResult = URScanResult.Ur(ur)
                return
            }

            // Multi-part UR
            if (!hasReceivedFirstPart) {
                expectedFragmentCount = extractFragmentCount(trimmed)
                hasReceivedFirstPart = true
            }

            decoder.receive(trimmed)
            receivedCount++

            if (decoder.isComplete) {
                val ur = decoder.message()
                if (ur != null) {
                    lastResult = URScanResult.Ur(ur)
                }
            } else {
                lastResult = URScanResult.Progress(progress)
            }
        } catch (e: Exception) {
            if (hasReceivedFirstPart) {
                lastResult = URScanResult.Reject
            } else {
                lastResult = URScanResult.Failure(e)
                restart()
            }
        }
    }

    /** Checks whether a UR string is single-part (no sequence component). */
    private fun isSinglePartUR(ur: String): Boolean {
        val withoutScheme = ur.removePrefix("ur:").removePrefix("UR:")
        val components = withoutScheme.split('/')
        if (components.size >= 2) {
            val secondPart = components[1]
            val dashParts = secondPart.split('-')
            if (dashParts.size == 2 &&
                dashParts[0].all { it.isDigit() } &&
                dashParts[1].all { it.isDigit() }
            ) {
                return false
            }
        }
        return true
    }

    /** Extracts the total fragment count from a multipart UR sequence ID. */
    private fun extractFragmentCount(ur: String): Int? {
        val withoutScheme = ur.removePrefix("ur:").removePrefix("UR:")
        val components = withoutScheme.split('/')
        if (components.size < 2) return null
        val seqParts = components[1].split('-')
        if (seqParts.size != 2) return null
        return seqParts[1].toIntOrNull()
    }
}
