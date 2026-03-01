package com.blockchaincommons.bcurui

/** The display state of a single fragment in a multi-part UR. */
enum class FragmentState {
    /** Not yet displayed or received. */
    Off,
    /** Currently being displayed or received. */
    On,
    /** Just received / highlighted. */
    Highlighted
}
