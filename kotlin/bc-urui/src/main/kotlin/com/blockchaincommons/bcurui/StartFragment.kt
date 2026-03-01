package com.blockchaincommons.bcurui

/** Specifies which fountain-coded fragment to begin simulated scanning from. */
sealed class StartFragment {
    /** Start from the first fragment (sequence 1). */
    data object First : StartFragment()

    /** Start from a specific fragment index (0-based). */
    data class Index(val index: Int) : StartFragment()

    /** Start from a random fragment index. */
    data object Random : StartFragment()
}
