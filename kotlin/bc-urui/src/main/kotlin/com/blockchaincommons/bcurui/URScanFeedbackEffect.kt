package com.blockchaincommons.bcurui

import android.media.AudioAttributes
import android.media.SoundPool
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableIntStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberUpdatedState
import androidx.compose.runtime.setValue
import androidx.compose.ui.platform.LocalContext

@Composable
fun URScanFeedbackEffect(scanState: URScanState, config: URScanFeedbackConfig) {
    val context = LocalContext.current

    val vibrator = remember { resolveVibrator(context) }
    val hasVibrator = remember { vibrator?.hasVibrator() == true }

    val soundPool = remember(
        config.soundEnabled,
        config.clickSoundResId,
        config.successSoundResId,
        config.failureSoundResId,
    ) {
        if (!config.soundEnabled) return@remember null
        if (config.clickSoundResId == null &&
            config.successSoundResId == null &&
            config.failureSoundResId == null
        ) return@remember null
        SoundPool.Builder()
            .setMaxStreams(4)
            .setAudioAttributes(
                AudioAttributes.Builder()
                    .setUsage(AudioAttributes.USAGE_MEDIA)
                    .setContentType(AudioAttributes.CONTENT_TYPE_SONIFICATION)
                    .build(),
            )
            .build()
    }

    var clickId by remember { mutableIntStateOf(0) }
    var successId by remember { mutableIntStateOf(0) }
    var failureId by remember { mutableIntStateOf(0) }

    DisposableEffect(soundPool) {
        if (soundPool != null) {
            config.clickSoundResId?.let { clickId = soundPool.load(context, it, 1) }
            config.successSoundResId?.let { successId = soundPool.load(context, it, 1) }
            config.failureSoundResId?.let { failureId = soundPool.load(context, it, 1) }
        }
        onDispose {
            soundPool?.release()
            clickId = 0
            successId = 0
            failureId = 0
        }
    }

    val currentConfig by rememberUpdatedState(config)
    val currentSoundPool by rememberUpdatedState(soundPool)
    LaunchedEffect(scanState) {
        scanState.scanEvents.collect { event ->
            if (currentConfig.hapticEnabled && hasVibrator) {
                vibrator?.vibrate(vibrationEffect(event))
            }
            if (currentConfig.soundEnabled) {
                val id = when (event) {
                    URHapticEvent.Progress -> clickId
                    URHapticEvent.Success -> successId
                    URHapticEvent.Failure -> failureId
                }
                if (id != 0) {
                    currentSoundPool?.play(id, 1f, 1f, 1, 0, 1f)
                }
            }
        }
    }
}
