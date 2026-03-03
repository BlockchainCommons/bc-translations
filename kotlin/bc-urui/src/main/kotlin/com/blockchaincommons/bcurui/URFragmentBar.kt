package com.blockchaincommons.bcurui

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Row
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.runtime.Composable
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp

/** Displays which fragments of a multi-part UR are currently displayed or being captured. */
@Composable
fun URFragmentBar(
    states: List<FragmentState>,
    modifier: Modifier = Modifier
) {
    Row(
        modifier = modifier
            .fillMaxWidth()
            .height(14.dp)
            .clip(RoundedCornerShape(50))
    ) {
        for (state in states) {
            Box(
                modifier = Modifier
                    .weight(1f)
                    .height(14.dp)
                    .background(colorForState(state))
            )
        }
    }
}

// Colors match iOS system blue (#007AFF) desaturated by blending toward white.
private fun colorForState(state: FragmentState): Color = when (state) {
    FragmentState.Off -> Color(0xFF007AFF)  // iOS system blue
    FragmentState.On -> Color(0xFF409CFF)   // 25% toward white
    FragmentState.Highlighted -> Color(0xFF80BDFF) // 50% toward white
}
