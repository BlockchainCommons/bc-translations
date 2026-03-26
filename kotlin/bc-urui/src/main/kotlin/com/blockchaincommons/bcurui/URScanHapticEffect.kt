package com.blockchaincommons.bcurui

import android.content.Context
import android.media.AudioManager
import android.os.Build
import android.os.VibrationEffect
import android.os.Vibrator
import android.os.VibratorManager
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.remember
import androidx.compose.ui.platform.LocalContext

enum class URHapticEvent { Progress, Success, Failure }

/**
 * Produces haptic feedback for scan events using the Vibrator API directly.
 *
 * Unlike View.performHapticFeedback(), the Vibrator API is not gated by
 * the system "Touch feedback" setting — matching how keyboards and iOS's
 * UIImpactFeedbackGenerator work. Falls back to sound on devices without
 * a vibrator.
 */
@Composable
fun URScanHapticEffect(scanState: URScanState) {
    if (!scanState.hapticFeedback) return

    val context = LocalContext.current
    val vibrator = remember { resolveVibrator(context) }
    val hasVibrator = remember { vibrator?.hasVibrator() == true }

    LaunchedEffect(scanState) {
        scanState.hapticEvents.collect { event ->
            if (hasVibrator) {
                vibrator?.vibrate(vibrationEffect(event))
            } else {
                val audioManager = context.getSystemService(Context.AUDIO_SERVICE) as? AudioManager
                val soundEffect = when (event) {
                    URHapticEvent.Progress -> AudioManager.FX_KEY_CLICK
                    URHapticEvent.Success -> AudioManager.FX_KEYPRESS_STANDARD
                    URHapticEvent.Failure -> AudioManager.FX_KEYPRESS_INVALID
                }
                audioManager?.playSoundEffect(soundEffect, 1.0f)
            }
        }
    }
}

private fun resolveVibrator(context: Context): Vibrator? {
    return if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
        val manager = context.getSystemService(Context.VIBRATOR_MANAGER_SERVICE) as? VibratorManager
        manager?.defaultVibrator
    } else {
        @Suppress("DEPRECATION")
        context.getSystemService(Context.VIBRATOR_SERVICE) as? Vibrator
    }
}

private fun vibrationEffect(event: URHapticEvent): VibrationEffect {
    // Use createOneShot for maximum device compatibility.
    // Predefined effects (EFFECT_TICK etc.) require an LRA actuator
    // and silently produce nothing on devices with basic ERM motors.
    return when (event) {
        URHapticEvent.Progress -> VibrationEffect.createOneShot(40, 150)
        URHapticEvent.Success -> VibrationEffect.createOneShot(40, 200)
        URHapticEvent.Failure -> VibrationEffect.createOneShot(80, 255)
    }
}
