package com.blockchaincommons.bcurui

import com.blockchaincommons.bcur.UR

/** The result of processing a scanned QR code. */
sealed class URScanResult {
    /** A complete UR was decoded. */
    data class Ur(val ur: UR) : URScanResult()

    /** A non-UR QR code was read. */
    data class Other(val code: String) : URScanResult()

    /** A part of a multi-part QR code was read. */
    data class Progress(val progress: URScanProgress) : URScanResult()

    /** A part of a multi-part QR code was rejected. */
    data object Reject : URScanResult()

    /** An error occurred that aborted the scan session. */
    data class Failure(val error: Throwable) : URScanResult()
}

/** Progress information for a multi-part UR scan. */
data class URScanProgress(
    val estimatedPercentComplete: Double,
    val fragmentStates: List<FragmentState>
)
