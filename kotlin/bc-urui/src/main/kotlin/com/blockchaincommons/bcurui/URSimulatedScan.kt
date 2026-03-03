package com.blockchaincommons.bcurui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.DisposableEffect
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp

/**
 * Displays a simulated camera scan by showing the QR codes being "scanned"
 * as visual feedback while [URSimulatedScanState] feeds parts to a [URScanState].
 */
@Composable
fun URSimulatedScan(
    state: URSimulatedScanState,
    showFragmentBar: Boolean = true,
    modifier: Modifier = Modifier,
) {
    val scope = rememberCoroutineScope()

    DisposableEffect(state) {
        state.run(scope)
        onDispose { state.stop() }
    }

    Column(modifier = modifier.fillMaxWidth()) {
        Box(
            modifier = Modifier
                .weight(1f)
                .fillMaxWidth()
                .background(Color.Black),
            contentAlignment = Alignment.Center
        ) {
            URQRCode(
                data = state.currentPart,
                modifier = Modifier.padding(16.dp),
                foregroundColor = Color.White,
                backgroundColor = Color.Black
            )
        }

        if (showFragmentBar) {
            URFragmentBar(states = state.fragmentStates)

            Text(
                text = "Simulated Scan",
                fontSize = 12.sp,
                color = Color.Gray,
                textAlign = TextAlign.Center,
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(top = 4.dp),
            )
        }
    }
}
