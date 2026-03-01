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
 * Tracks state of ongoing display of (possibly multi-part) UR.
 *
 * Drives animated multi-part UR QR code display by cycling through
 * fountain-coded parts on a timer.
 */
class URDisplayState(
    val ur: UR,
    val maxFragmentLen: Int
) {
    var framesPerSecond: Double = 10.0

    /** The current QR part as uppercase UTF-8 bytes (suitable for QR rendering). */
    var part: ByteArray by mutableStateOf(ByteArray(0))
        private set

    /** Fragment state indicators for the progress bar. */
    var fragmentStates: List<FragmentState> by mutableStateOf(listOf(FragmentState.Off))
        private set

    val isSinglePart: Boolean get() = partsCount == 1
    val seqNum: Int get() = currentSequence
    val seqLen: Int get() = partsCount

    private var encoder: MultipartEncoder = MultipartEncoder(ur, maxFragmentLen)
    private var partsCount: Int = encoder.partCount
    private var currentSequence: Int = 0
    private var timerJob: Job? = null

    init {
        emitNextPart()
    }

    fun restart() {
        stop()
        encoder = MultipartEncoder(ur, maxFragmentLen)
        partsCount = encoder.partCount
        currentSequence = 0
        emitNextPart()
    }

    fun run(scope: CoroutineScope) {
        if (isSinglePart) return
        stop()
        timerJob = scope.launch {
            while (true) {
                delay((1000.0 / framesPerSecond).toLong())
                emitNextPart()
            }
        }
    }

    fun stop() {
        timerJob?.cancel()
        timerJob = null
    }

    private fun emitNextPart() {
        val partString = encoder.nextPart()
        currentSequence = encoder.currentIndex
        part = partString.uppercase().toByteArray(Charsets.UTF_8)

        // For sequences 1..partsCount, the fountain encoder produces simple
        // (single-fragment) parts where fragment index = sequence - 1.
        // For sequences > partsCount, mixed parts are emitted; we show all
        // fragments as "on" since the exact mix indexes are internal to bc-ur.
        fragmentStates = if (currentSequence <= partsCount) {
            val fragmentIndex = currentSequence - 1
            (0 until partsCount).map { i ->
                if (i == fragmentIndex) FragmentState.On else FragmentState.Off
            }
        } else {
            List(partsCount) { FragmentState.On }
        }
    }
}
