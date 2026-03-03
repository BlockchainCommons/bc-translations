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

private fun colorForState(state: FragmentState): Color = when (state) {
    FragmentState.Off -> Color(0xFF0000FF) // Blue
    FragmentState.On -> Color(0xFF3366CC)  // Lighter blue
    FragmentState.Highlighted -> Color(0xFF66AAFF) // Light blue
}
