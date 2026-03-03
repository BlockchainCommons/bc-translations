package com.blockchaincommons.bcurui

import android.os.Build
import android.view.HapticFeedbackConstants
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.platform.LocalView

enum class URHapticEvent { Progress, Success, Failure }

@Composable
fun URScanHapticEffect(scanState: URScanState) {
    if (!scanState.hapticFeedback) return

    val view = LocalView.current

    LaunchedEffect(scanState) {
        scanState.hapticEvents.collect { event ->
            val feedbackConstant = when (event) {
                URHapticEvent.Progress -> HapticFeedbackConstants.CLOCK_TICK
                URHapticEvent.Success -> {
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
                        HapticFeedbackConstants.CONFIRM
                    } else {
                        HapticFeedbackConstants.CONTEXT_CLICK
                    }
                }
                URHapticEvent.Failure -> {
                    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.R) {
                        HapticFeedbackConstants.REJECT
                    } else {
                        HapticFeedbackConstants.LONG_PRESS
                    }
                }
            }
            view.performHapticFeedback(feedbackConstant)
        }
    }
}
