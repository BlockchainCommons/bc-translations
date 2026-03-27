package com.blockchaincommons.bcurui

data class URScanFeedbackConfig(
    val hapticEnabled: Boolean = true,
    val soundEnabled: Boolean = false,
    val clickSoundResId: Int? = null,
    val successSoundResId: Int? = null,
    val failureSoundResId: Int? = null,
) {
    companion object {
        val DEFAULT = URScanFeedbackConfig()
        fun hapticOnly(enabled: Boolean) = URScanFeedbackConfig(hapticEnabled = enabled)
    }
}
