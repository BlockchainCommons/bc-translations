package com.blockchaincommons.bcenvelope

import com.blockchaincommons.dcbor.*

/**
 * Returns the CBOR diagnostic notation for this envelope, with annotations.
 */
fun Envelope.diagnosticAnnotated(): String = withFormatContext { context ->
    taggedCbor().diagnosticOpt(
        DiagFormatOpts(
            annotate = true,
            tags = TagsStoreOpt.Custom(context.tags()),
        )
    )
}

/**
 * Returns the CBOR diagnostic notation for this envelope.
 */
fun Envelope.diagnostic(): String = taggedCbor().diagnostic()
