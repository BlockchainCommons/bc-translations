package com.blockchaincommons.bcurui

import android.content.Context
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.runtime.Composable

enum class URHapticEvent { Progress, Success, Failure }

/**
 * Produces haptic feedback for scan events using the Vibrator API directly.
 *
 * Unlike View.performHapticFeedback(), the Vibrator API is not gated by
 * the system "Touch feedback" setting — matching how keyboards and iOS's
 * UIImpactFeedbackGenerator work. Falls back to sound on devices without
 * a vibrator.
 */
@Deprecated("Use URScanFeedbackEffect", ReplaceWith("URScanFeedbackEffect(scanState, URScanFeedbackConfig.hapticOnly(scanState.hapticFeedback))"))
@Composable
fun URScanHapticEffect(scanState: URScanState) {
    URScanFeedbackEffect(scanState, URScanFeedbackConfig.hapticOnly(scanState.hapticFeedback))
}

internal fun resolveVibrator(context: Context): Vibrator? {
    return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
        val manager = context.getSystemService(Context.VIBRATOR_MANAGER_SERVICE) as? VibratorManager
        manager?.defaultVibrator
    } else {
        @Suppress("DEPRECATION")
        context.getSystemService(Context.VIBRATOR_SERVICE) as? Vibrator
    }
}

internal fun vibrationEffect(event: URHapticEvent): VibrationEffect {
    // Use createOneShot for maximum device compatibility.
    // Predefined effects (EFFECT_TICK etc.) require an LRA actuator
    // and silently produce nothing on devices with basic ERM motors.
    return when (event) {
        URHapticEvent.Progress -> VibrationEffect.createOneShot(40, 150)
        URHapticEvent.Success -> VibrationEffect.createOneShot(40, 200)
        URHapticEvent.Failure -> VibrationEffect.createOneShot(80, 255)
    }
}
