package com.blockchaincommons.dcbor

/**
 * Options for annotated hex dump of CBOR data.
 */
data class HexFormatOpts(
    val annotate: Boolean = false,
    val tags: TagsStoreOpt = TagsStoreOpt.Global
) {
    fun annotate(value: Boolean) = copy(annotate = value)
    fun context(tags: TagsStoreOpt) = copy(tags = tags)
}

// Hex formatting extensions for Cbor
fun Cbor.hexOpt(opts: HexFormatOpts): String {
    if (!opts.annotate) return hex
    val items = dumpItems(0, opts)
    val noteColumn = items.fold(0) { largest, item ->
        maxOf(largest, item.formatFirstColumn().length)
    }
    // Round up to nearest multiple of 4
    val paddedColumn = ((noteColumn + 4) and 3.inv()) - 1
    return items.joinToString("\n") { it.format(paddedColumn) }
}

private fun Cbor.dumpItems(level: Int, opts: HexFormatOpts): List<DumpItem> {
    return when (val case = cborCase) {
        is CborCase.Unsigned -> listOf(
            DumpItem(level, listOf(toCborData()), "unsigned(${case.value})")
        )
        is CborCase.Negative -> listOf(
            DumpItem(level, listOf(toCborData()), "negative(${negativeValueToString(case.value)})")
        )
        is CborCase.CborByteString -> {
            val data = case.value.toByteArray()
            val header = Varint.encode(data.size.toULong(), MajorType.ByteString)
            val items = mutableListOf(DumpItem(level, listOf(header), "bytes(${data.size})"))
            if (data.isNotEmpty()) {
                val note = try {
                    val decoder = Charsets.UTF_8.newDecoder()
                        .onMalformedInput(java.nio.charset.CodingErrorAction.REPORT)
                        .onUnmappableCharacter(java.nio.charset.CodingErrorAction.REPORT)
                    val s = decoder.decode(java.nio.ByteBuffer.wrap(data)).toString()
                    StringUtil.sanitized(s)?.let { "\"$it\"" }
                } catch (_: Exception) { null }
                items.add(DumpItem(level + 1, listOf(data), note))
            }
            items
        }
        is CborCase.Text -> {
            val utf8Data = case.value.toByteArray(Charsets.UTF_8)
            val header = Varint.encode(utf8Data.size.toULong(), MajorType.Text)
            val headerData = if (header.size > 1) {
                listOf(byteArrayOf(header[0]), header.copyOfRange(1, header.size))
            } else {
                listOf(header)
            }
            listOf(
                DumpItem(level, headerData, "text(${utf8Data.size})"),
                DumpItem(level + 1, listOf(utf8Data), "\"${case.value}\"")
            )
        }
        is CborCase.CborSimple -> {
            val data = case.value.toCborData()
            listOf(DumpItem(level, listOf(data), case.value.displayDescription))
        }
        is CborCase.Tagged -> {
            val header = Varint.encode(case.tag.value, MajorType.Tagged)
            val headerData = if (header.size > 1) {
                listOf(byteArrayOf(header[0]), header.copyOfRange(1, header.size))
            } else {
                listOf(header)
            }
            val noteComponents = mutableListOf("tag(${case.tag.value})")
            when (opts.tags) {
                is TagsStoreOpt.None -> {}
                is TagsStoreOpt.Global -> GlobalTags.withTags { store ->
                    store.assignedNameForTag(case.tag)?.let { noteComponents.add(it) }
                }
                is TagsStoreOpt.Custom -> {
                    opts.tags.store.assignedNameForTag(case.tag)?.let { noteComponents.add(it) }
                }
            }
            listOf(DumpItem(level, headerData, noteComponents.joinToString(" "))) +
                case.item.dumpItems(level + 1, opts)
        }
        is CborCase.Array -> {
            val header = Varint.encode(case.value.size.toULong(), MajorType.Array)
            val headerData = if (header.size > 1) {
                listOf(byteArrayOf(header[0]), header.copyOfRange(1, header.size))
            } else {
                listOf(header)
            }
            listOf(DumpItem(level, headerData, "array(${case.value.size})")) +
                case.value.flatMap { it.dumpItems(level + 1, opts) }
        }
        is CborCase.CborMap -> {
            val size = case.value.size
            val header = Varint.encode(size.toULong(), MajorType.Map)
            val headerData = if (header.size > 1) {
                listOf(byteArrayOf(header[0]), header.copyOfRange(1, header.size))
            } else {
                listOf(header)
            }
            listOf(DumpItem(level, headerData, "map($size)")) +
                case.value.toList().flatMap { (key, value) ->
                    key.dumpItems(level + 1, opts) + value.dumpItems(level + 1, opts)
                }
        }
    }
}

private class DumpItem(
    val level: Int,
    val data: List<ByteArray>,
    val note: String?
) {
    fun format(noteColumn: Int): String {
        val column1 = formatFirstColumn()
        return if (note != null) {
            val paddingCount = maxOf(1, minOf(39, noteColumn) - column1.length + 1)
            val padding = " ".repeat(paddingCount)
            "$column1${padding}# $note"
        } else {
            column1
        }
    }

    fun formatFirstColumn(): String {
        val indent = " ".repeat(level * 4)
        val hex = data.map { it.toHexString() }.filter { it.isNotEmpty() }.joinToString(" ")
        return "$indent$hex"
    }
}
