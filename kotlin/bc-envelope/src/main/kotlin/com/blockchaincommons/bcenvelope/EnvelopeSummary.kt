package com.blockchaincommons.bcenvelope

import com.blockchaincommons.dcbor.*
import com.blockchaincommons.knownvalues.KnownValuesStore

/** Returns a short summary of the envelope's content. */
fun Envelope.summary(maxLength: Int, context: FormatContext): String {
    return when (val c = case()) {
        is EnvelopeCase.Node -> "NODE"
        is EnvelopeCase.Leaf -> c.cbor.envelopeSummary(
            maxLength,
            FormatContextOpt.Custom(context),
        )
        is EnvelopeCase.Wrapped -> "WRAPPED"
        is EnvelopeCase.AssertionCase -> "ASSERTION"
        is EnvelopeCase.Elided -> "ELIDED"
        is EnvelopeCase.KnownValueCase -> {
            val kv = KnownValuesStore.knownValueForRawValue(
                c.value.value,
                context.knownValues(),
            )
            kv.toString().flankedBy("'", "'")
        }
        is EnvelopeCase.Encrypted -> "ENCRYPTED"
        is EnvelopeCase.CompressedCase -> "COMPRESSED"
    }
}

/** Returns a CBOR summary for envelope notation. */
fun Cbor.envelopeSummary(maxLength: Int, context: FormatContextOpt): String {
    return when (val case = this.cborCase) {
        is CborCase.Unsigned -> case.value.toString()
        is CborCase.Negative -> (-1L - case.value.toLong()).toString()
        is CborCase.CborByteString -> "Bytes(${case.value.size})"
        is CborCase.Text -> {
            val s = if (case.value.length > maxLength) {
                case.value.take(maxLength) + "\u2026"
            } else {
                case.value
            }
            s.replace("\n", "\\n").flankedBy("\"", "\"")
        }
        is CborCase.CborSimple -> case.value.name
        is CborCase.Array, is CborCase.CborMap, is CborCase.Tagged -> {
            when (context) {
                is FormatContextOpt.None -> diagnosticWithOpts(
                    summarize = true,
                    tagsOpt = TagsStoreOpt.None,
                )
                is FormatContextOpt.Global -> withFormatContext { ctx ->
                    diagnosticWithOpts(
                        summarize = true,
                        tagsOpt = TagsStoreOpt.Custom(ctx.tags()),
                    )
                }
                is FormatContextOpt.Custom -> diagnosticWithOpts(
                    summarize = true,
                    tagsOpt = TagsStoreOpt.Custom(context.context.tags()),
                )
            }
        }
    }
}

/** Helper: produce diagnostic output with specific summarize and tags options. */
private fun Cbor.diagnosticWithOpts(
    summarize: Boolean = false,
    annotate: Boolean = false,
    tagsOpt: TagsStoreOpt = TagsStoreOpt.None,
): String {
    val opts = DiagFormatOpts(
        annotate = annotate,
        summarize = summarize,
        flat = summarize,
        tags = tagsOpt,
    )
    return diagnosticOpt(opts)
}
