package com.blockchaincommons.bcurui

import com.blockchaincommons.bcur.MultipartDecoder
import com.blockchaincommons.bcur.UR
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue
import kotlinx.coroutines.flow.MutableSharedFlow
import kotlinx.coroutines.flow.SharedFlow
import kotlinx.coroutines.flow.asSharedFlow

/**
 * Tracks and reports state of ongoing multi-part UR capture.
 *
 * Feed scanned QR code strings via [receiveCodes]. Observe [lastResult]
 * for decoded URs, progress updates, or errors.
 */
class URScanState(
    val hapticFeedback: Boolean = true,
) {
    /** The most recent scan result. */
    var lastResult: URScanResult? by mutableStateOf(null)
        private set

    private val _scanEvents = MutableSharedFlow<URHapticEvent>(extraBufferCapacity = 64)
    val scanEvents: SharedFlow<URHapticEvent> = _scanEvents.asSharedFlow()

    @Deprecated("Use scanEvents", ReplaceWith("scanEvents"))
    val hapticEvents: SharedFlow<URHapticEvent> get() = scanEvents

    private var decoder = MultipartDecoder()
    private var hasReceivedFirstPart: Boolean = false

    fun restart() {
        decoder = MultipartDecoder()
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
        _scanEvents.tryEmit(URHapticEvent.Failure)
    }

    /**
     * Signals a successful non-QR scan result (e.g., text recognition match).
     *
     * Emits success haptic feedback but does not set [lastResult] — the host
     * manages its own result state for non-QR outcomes.
     */
    fun completeWithSuccess() {
        _scanEvents.tryEmit(URHapticEvent.Success)
    }

    /**
     * Signals a failed non-QR scan result.
     *
     * Sets [lastResult] to [URScanResult.Failure] and emits failure haptic feedback.
     */
    fun completeWithFailure(error: Throwable) {
        lastResult = URScanResult.Failure(error)
        _scanEvents.tryEmit(URHapticEvent.Failure)
    }

    private val progress: URScanProgress
        get() {
            val count = decoder.expectedFragmentCount.takeIf { it > 0 } ?: 1
            val decodedCount = decoder.decodedFragmentCount
            val percent = minOf(
                (decodedCount + decoder.bufferContribution) / count, 1.0
            )
            val filledCount = (percent * count).toInt()
            val states = (0 until count).map { i ->
                when {
                    i < filledCount -> FragmentState.Highlighted
                    i == filledCount -> FragmentState.On
                    else -> FragmentState.Off
                }
            }
            return URScanProgress(estimatedPercentComplete = percent, fragmentStates = states)
        }

    private fun processCode(code: String) {
        if (lastResult is URScanResult.Ur) return

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
                _scanEvents.tryEmit(URHapticEvent.Success)
                return
            }

            // Multi-part UR
            if (!hasReceivedFirstPart) {
                hasReceivedFirstPart = true
            }

            decoder.receive(trimmed)

            if (decoder.isComplete) {
                val ur = decoder.message()
                if (ur != null) {
                    lastResult = URScanResult.Ur(ur)
                    _scanEvents.tryEmit(URHapticEvent.Success)
                }
            } else {
                lastResult = URScanResult.Progress(progress)
                if (hasReceivedFirstPart) {
                    _scanEvents.tryEmit(URHapticEvent.Progress)
                }
            }
        } catch (e: Exception) {
            if (hasReceivedFirstPart) {
                lastResult = URScanResult.Reject
            } else {
                lastResult = URScanResult.Failure(e)
                _scanEvents.tryEmit(URHapticEvent.Failure)
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

}
