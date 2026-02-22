package com.blockchaincommons.sskr

internal data class SskrShare(
    val identifier: Int,
    val groupIndex: Int,
    val groupThreshold: Int,
    val groupCount: Int,
    val memberIndex: Int,
    val memberThreshold: Int,
    val value: Secret,
)
