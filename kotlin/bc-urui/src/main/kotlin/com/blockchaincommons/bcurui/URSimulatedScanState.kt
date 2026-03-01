package com.blockchaincommons.bcurui

import com.blockchaincommons.bcur.MultipartEncoder
import com.blockchaincommons.bcur.UR
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Job
import kotlinx.coroutines.delay
import kotlinx.coroutines.launch
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.getValue
import androidx.compose.runtime.setValue

/**
 * Simulates scanning a UR by encoding it into fountain-coded parts and
 * feeding the part strings to a [URScanState] on a timer.
 *
 * This exercises the full encode→decode round-trip without camera hardware,
 * making it suitable for use in Android Emulator or automated testing.
 */
class URSimulatedScanState(
    val ur: UR,
    val scanState: URScanState,
    val maxFragmentLen: Int,
    val secondsPerFrame: Double = 0.5,
    val startFragment: StartFragment = StartFragment.First
) {
    /** The current QR part as uppercase UTF-8 bytes (suitable for QR rendering). */
    var currentPart: ByteArray by mutableStateOf(ByteArray(0))
        private set

    /** Fragment state indicators for the progress bar. */
    var fragmentStates: List<FragmentState> by mutableStateOf(listOf(FragmentState.Off))
        private set

    /** Whether the simulated scan loop is currently running. */
    var isRunning: Boolean by mutableStateOf(false)
        private set

    private var encoder: MultipartEncoder = MultipartEncoder(ur, maxFragmentLen)
    private var partsCount: Int = encoder.partCount
    private var currentSequence: Int = 0
    private var timerJob: Job? = null

    init {
        advanceToStart()
    }

    /** Starts the simulated scan loop. */
    fun run(scope: CoroutineScope) {
        timerJob?.cancel()
        isRunning = true
        timerJob = scope.launch {
            while (true) {
                emitAndDeliver()

                // Auto-stop on successful decode
                val result = scanState.lastResult
                if (result is URScanResult.Ur) {
                    isRunning = false
                    return@launch
                }

                delay((secondsPerFrame * 1000).toLong())
            }
        }
    }

    /** Stops the simulated scan loop. */
    fun stop() {
        timerJob?.cancel()
        timerJob = null
        isRunning = false
    }

    /** Restarts the encoder and scan loop from the configured start fragment. */
    fun restart(scope: CoroutineScope) {
        stop()
        scanState.restart()
        encoder = MultipartEncoder(ur, maxFragmentLen)
        partsCount = encoder.partCount
        currentSequence = 0
        advanceToStart()
        run(scope)
    }

    private fun advanceToStart() {
        val skipCount = when (startFragment) {
            is StartFragment.First -> 0
            is StartFragment.Index -> {
                val i = startFragment.index
                require(i in 0 until partsCount) { "Start index $i out of range [0, $partsCount)" }
                i
            }
            is StartFragment.Random -> (0 until partsCount).random()
        }

        repeat(skipCount) {
            encoder.nextPart()
        }
    }

    private fun emitAndDeliver() {
        val partString = encoder.nextPart()
        currentSequence = encoder.currentIndex
        currentPart = partString.uppercase().toByteArray(Charsets.UTF_8)

        // Update fragment state display (same logic as URDisplayState)
        fragmentStates = if (currentSequence <= partsCount) {
            val fragmentIndex = currentSequence - 1
            (0 until partsCount).map { i ->
                if (i == fragmentIndex) FragmentState.On else FragmentState.Off
            }
        } else {
            List(partsCount) { FragmentState.On }
        }

        // Deliver to scan state
        scanState.receiveCodes(setOf(partString))
    }
}
