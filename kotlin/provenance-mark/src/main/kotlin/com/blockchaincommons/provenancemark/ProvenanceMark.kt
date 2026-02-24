package com.blockchaincommons.provenancemark

import com.blockchaincommons.bcenvelope.Envelope
import com.blockchaincommons.bcenvelope.FormatContext
import com.blockchaincommons.bcur.Bytewords
import com.blockchaincommons.bcur.BytewordsStyle
import com.blockchaincommons.bcur.UR
import com.blockchaincommons.bcur.UREncodable
import com.blockchaincommons.bcur.URException
import com.blockchaincommons.bctags.TAG_NAME_PROVENANCE_MARK
import com.blockchaincommons.bctags.TAG_PROVENANCE_MARK
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate
import com.blockchaincommons.dcbor.CborTaggedEncodable
import com.blockchaincommons.dcbor.CborTaggedUtils
import com.blockchaincommons.dcbor.Tag
import com.blockchaincommons.dcbor.tagsForValues
import java.net.URI
import java.net.URL

class ProvenanceMark private constructor(
    private val res: ProvenanceMarkResolution,
    private val key: ByteArray,
    private val hash: ByteArray,
    private val chainId: ByteArray,
    private val seqBytes: ByteArray,
    private val dateBytes: ByteArray,
    private val infoBytes: ByteArray,
    private val seq: UInt,
    private val date: CborDate,
) : CborTaggedEncodable, UREncodable {

    fun res(): ProvenanceMarkResolution = res

    fun key(): ByteArray = key.copyOf()

    fun hash(): ByteArray = hash.copyOf()

    fun chainId(): ByteArray = chainId.copyOf()

    fun seqBytes(): ByteArray = seqBytes.copyOf()

    fun dateBytes(): ByteArray = dateBytes.copyOf()

    fun seq(): UInt = seq

    fun date(): CborDate = date

    fun message(): ByteArray {
        val payload = chainId + hash + seqBytes + dateBytes + infoBytes
        return key + CryptoUtils.obfuscate(key, payload)
    }

    fun info(): Cbor? {
        if (infoBytes.isEmpty()) return null
        return Cbor.tryFromData(infoBytes)
    }

    fun identifier(): String = hash.copyOfRange(0, 4).toHex()

    fun bytewordsIdentifier(prefix: Boolean): String {
        val s = Bytewords.identifier(hash.copyOfRange(0, 4)).uppercase()
        return if (prefix) "🅟 $s" else s
    }

    fun bytemojiIdentifier(prefix: Boolean): String {
        val s = Bytewords.bytemojiIdentifier(hash.copyOfRange(0, 4)).uppercase()
        return if (prefix) "🅟 $s" else s
    }

    fun bytewordsMinimalIdentifier(prefix: Boolean): String {
        val full = Bytewords.identifier(hash.copyOfRange(0, 4))
        val words = full.trim().split(Regex("\\s+"))
        val out = StringBuilder(8)

        if (words.size == 4) {
            for (word in words) {
                if (word.isEmpty()) continue
                out.append(word.first().uppercaseChar())
                out.append(word.last().uppercaseChar())
            }
        }

        if (out.length != 8) {
            out.clear()
            val compact = full.filter { it.isLetter() }.uppercase()
            for (chunk in compact.chunked(4)) {
                if (chunk.length == 4) {
                    out.append(chunk.first())
                    out.append(chunk.last())
                }
            }
        }

        val result = out.toString()
        return if (prefix) "🅟 $result" else result
    }

    fun precedes(next: ProvenanceMark): Boolean {
        return try {
            precedesOpt(next)
            true
        } catch (_: Exception) {
            false
        }
    }

    fun precedesOpt(next: ProvenanceMark) {
        if (next.seq == 0u) {
            throw ProvenanceMarkException.Validation(ValidationIssue.NonGenesisAtZero)
        }
        if (next.key.contentEquals(next.chainId)) {
            throw ProvenanceMarkException.Validation(ValidationIssue.InvalidGenesisKey)
        }
        if (seq != next.seq - 1u) {
            throw ProvenanceMarkException.Validation(
                ValidationIssue.SequenceGap(expected = seq + 1u, actual = next.seq)
            )
        }
        if (date.instant.isAfter(next.date.instant)) {
            throw ProvenanceMarkException.Validation(
                ValidationIssue.DateOrdering(previous = date, next = next.date)
            )
        }

        val expectedHash = makeHash(
            res = res,
            key = key,
            nextKey = next.key,
            chainId = chainId,
            seqBytes = seqBytes,
            dateBytes = dateBytes,
            infoBytes = infoBytes,
        )
        if (!hash.contentEquals(expectedHash)) {
            throw ProvenanceMarkException.Validation(
                ValidationIssue.HashMismatch(expected = expectedHash, actual = hash.copyOf())
            )
        }
    }

    fun isGenesis(): Boolean = seq == 0u && key.contentEquals(chainId)

    fun toBytewordsWithStyle(style: BytewordsStyle): String {
        return Bytewords.encode(message(), style)
    }

    fun toBytewords(): String = toBytewordsWithStyle(BytewordsStyle.Standard)

    fun toUrlEncoding(): String {
        return Bytewords.encode(taggedCborData(), BytewordsStyle.Minimal)
    }

    fun toUrl(base: String): URL {
        val baseUri = try {
            URI(base)
        } catch (e: Exception) {
            throw ProvenanceMarkException.Url(e.message ?: "invalid URL")
        }

        val encoded = toUrlEncoding()
        val newQuery = if (baseUri.rawQuery.isNullOrEmpty()) {
            "provenance=$encoded"
        } else {
            "${baseUri.rawQuery}&provenance=$encoded"
        }

        return try {
            URI(
                baseUri.scheme,
                baseUri.rawAuthority,
                baseUri.rawPath,
                newQuery,
                baseUri.rawFragment,
            ).toURL()
        } catch (e: Exception) {
            throw ProvenanceMarkException.Url(e.message ?: "invalid URL")
        }
    }

    fun toJson(): String {
        val fields = linkedMapOf<String, Any>(
            "seq" to seq.toLong(),
            "date" to dateToIso8601(date),
            "res" to res.code,
            "chain_id" to chainId.toBase64(),
            "key" to key.toBase64(),
            "hash" to hash.toBase64(),
        )
        if (infoBytes.isNotEmpty()) {
            fields["info_bytes"] = infoBytes.toBase64()
        }
        return JsonSupport.mapper.writeValueAsString(fields)
    }

    fun debugString(): String {
        val components = mutableListOf(
            "key: ${key.toHex()}",
            "hash: ${hash.toHex()}",
            "chainID: ${chainId.toHex()}",
            "seq: $seq",
            "date: ${dateToIso8601(date)}",
        )

        val infoCbor = info()
        if (infoCbor != null) {
            components.add("info: ${infoCbor.diagnostic()}")
        }

        return "ProvenanceMark(${components.joinToString(", ")})"
    }

    fun fingerprint(): ByteArray = CryptoUtils.sha256(taggedCborData())

    override fun cborTags(): List<Tag> = cborTagsStatic()

    override fun untaggedCbor(): Cbor {
        return Cbor.fromArray(
            listOf(
                res.toCbor(),
                Cbor.fromByteString(message()),
            )
        )
    }

    fun toEnvelope(): Envelope = Envelope.from(taggedCbor())

    override fun toString(): String = "ProvenanceMark(${identifier()})"

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ProvenanceMark) return false
        return res == other.res && message().contentEquals(other.message())
    }

    override fun hashCode(): Int {
        var result = res.hashCode()
        result = 31 * result + message().contentHashCode()
        return result
    }

    companion object {
        private fun makeHash(
            res: ProvenanceMarkResolution,
            key: ByteArray,
            nextKey: ByteArray,
            chainId: ByteArray,
            seqBytes: ByteArray,
            dateBytes: ByteArray,
            infoBytes: ByteArray,
        ): ByteArray {
            val bytes = key + nextKey + chainId + seqBytes + dateBytes + infoBytes
            return CryptoUtils.sha256Prefix(bytes, res.linkLength())
        }

        fun new(
            res: ProvenanceMarkResolution,
            key: ByteArray,
            nextKey: ByteArray,
            chainId: ByteArray,
            seq: UInt,
            date: CborDate,
            info: Any? = null,
        ): ProvenanceMark {
            if (key.size != res.linkLength()) {
                throw ProvenanceMarkException.InvalidKeyLength(res.linkLength(), key.size)
            }
            if (nextKey.size != res.linkLength()) {
                throw ProvenanceMarkException.InvalidNextKeyLength(res.linkLength(), nextKey.size)
            }
            if (chainId.size != res.linkLength()) {
                throw ProvenanceMarkException.InvalidChainIdLength(res.linkLength(), chainId.size)
            }

            val dateBytes = res.serializeDate(date)
            val seqBytes = res.serializeSeq(seq)
            val normalizedDate = res.deserializeDate(dateBytes)
            val infoBytes = if (info == null) byteArrayOf() else anyToCborData(info)

            val hash = makeHash(
                res = res,
                key = key,
                nextKey = nextKey,
                chainId = chainId,
                seqBytes = seqBytes,
                dateBytes = dateBytes,
                infoBytes = infoBytes,
            )

            return ProvenanceMark(
                res = res,
                key = key.copyOf(),
                hash = hash,
                chainId = chainId.copyOf(),
                seqBytes = seqBytes,
                dateBytes = dateBytes,
                infoBytes = infoBytes,
                seq = seq,
                date = normalizedDate,
            )
        }

        fun fromMessage(res: ProvenanceMarkResolution, message: ByteArray): ProvenanceMark {
            if (message.size < res.fixedLength()) {
                throw ProvenanceMarkException.InvalidMessageLength(res.fixedLength(), message.size)
            }

            val key = message.sliceArray(res.keyRange())
            val payload = CryptoUtils.obfuscate(key, message.copyOfRange(res.linkLength(), message.size))
            val hash = payload.sliceArray(res.hashRange())
            val chainId = payload.sliceArray(res.chainIdRange())
            val seqBytes = payload.sliceArray(res.seqBytesRange())
            val seq = res.deserializeSeq(seqBytes)
            val dateBytes = payload.sliceArray(res.dateBytesRange())
            val date = res.deserializeDate(dateBytes)
            val infoStart = res.infoRangeStart()
            val infoBytes = if (infoStart < payload.size) payload.copyOfRange(infoStart, payload.size) else byteArrayOf()
            if (infoBytes.isNotEmpty()) {
                try {
                    Cbor.tryFromData(infoBytes)
                } catch (_: Exception) {
                    throw ProvenanceMarkException.InvalidInfoCbor()
                }
            }

            return ProvenanceMark(
                res = res,
                key = key,
                hash = hash,
                chainId = chainId,
                seqBytes = seqBytes,
                dateBytes = dateBytes,
                infoBytes = infoBytes,
                seq = seq,
                date = date,
            )
        }

        fun fromBytewords(res: ProvenanceMarkResolution, value: String): ProvenanceMark {
            val message = try {
                Bytewords.decode(value, BytewordsStyle.Standard)
            } catch (e: URException) {
                throw ProvenanceMarkException.Bytewords(e.message ?: "decode failure")
            }
            return fromMessage(res, message)
        }

        fun fromUrlEncoding(value: String): ProvenanceMark {
            val cborData = try {
                Bytewords.decode(value, BytewordsStyle.Minimal)
            } catch (e: URException) {
                throw ProvenanceMarkException.Bytewords(e.message ?: "decode failure")
            }
            val cbor = try {
                Cbor.tryFromData(cborData)
            } catch (e: Exception) {
                throw ProvenanceMarkException.Cbor(e.message ?: "invalid CBOR")
            }
            return fromTaggedCbor(cbor)
        }

        fun fromUrl(url: URL): ProvenanceMark {
            val value = queryValue(url, "provenance")
                ?: throw ProvenanceMarkException.MissingUrlParameter("provenance")
            return fromUrlEncoding(value)
        }

        fun fromJson(json: String): ProvenanceMark {
            val node = JsonSupport.mapper.readTree(json)
            val res = ProvenanceMarkResolution.fromCode(node.path("res").asInt())
            val key = node.path("key").asText().fromBase64()
            val hash = node.path("hash").asText().fromBase64()
            val chainId = node.path("chain_id").asText().fromBase64()
            val seqLong = node.path("seq").asLong()
            if (seqLong < 0 || seqLong > UInt.MAX_VALUE.toLong()) {
                throw ProvenanceMarkException.IntegerConversion("sequence out of range for u32: $seqLong")
            }
            val seq = seqLong.toUInt()
            val date = dateFromIso8601(node.path("date").asText())
            val infoBytes = if (node.has("info_bytes")) node.path("info_bytes").asText().fromBase64() else byteArrayOf()
            val seqBytes = res.serializeSeq(seq)
            val dateBytes = res.serializeDate(date)

            return ProvenanceMark(
                res = res,
                key = key,
                hash = hash,
                chainId = chainId,
                seqBytes = seqBytes,
                dateBytes = dateBytes,
                infoBytes = infoBytes,
                seq = seq,
                date = date,
            )
        }

        fun cborTagsStatic(): List<Tag> = tagsForValues(listOf(TAG_PROVENANCE_MARK))

        fun fromUntaggedCbor(cbor: Cbor): ProvenanceMark {
            val values = try {
                cbor.tryArray()
            } catch (e: Exception) {
                throw ProvenanceMarkException.Cbor(e.message ?: "expected array")
            }
            if (values.size != 2) {
                throw ProvenanceMarkException.Cbor("Invalid provenance mark length")
            }
            val res = ProvenanceMarkResolution.fromCbor(values[0])
            val message = try {
                values[1].tryByteStringData()
            } catch (e: Exception) {
                throw ProvenanceMarkException.Cbor(e.message ?: "invalid message bytes")
            }
            return fromMessage(res, message)
        }

        fun fromTaggedCbor(cbor: Cbor): ProvenanceMark {
            return CborTaggedUtils.fromTaggedCbor(cbor, cborTagsStatic()) {
                fromUntaggedCbor(it)
            }
        }

        fun fromTaggedCborData(data: ByteArray): ProvenanceMark {
            val cbor = Cbor.tryFromData(data)
            return fromTaggedCbor(cbor)
        }

        fun fromUr(ur: UR): ProvenanceMark {
            ur.checkType(TAG_NAME_PROVENANCE_MARK)
            return fromUntaggedCbor(ur.cbor)
        }

        fun fromUrString(urString: String): ProvenanceMark = fromUr(UR.fromUrString(urString))

        fun fromEnvelope(envelope: Envelope): ProvenanceMark {
            return try {
                val leaf = envelope.subject().tryLeaf()
                fromTaggedCbor(leaf)
            } catch (e: Exception) {
                throw ProvenanceMarkException.Cbor("envelope error: ${e.message}")
            }
        }

        fun isSequenceValid(marks: List<ProvenanceMark>): Boolean {
            if (marks.size < 2) return false
            if (marks[0].seq == 0u && !marks[0].isGenesis()) return false
            return marks.zipWithNext().all { (a, b) -> a.precedes(b) }
        }

        fun validate(marks: List<ProvenanceMark>): ValidationReport = ValidationReport.validate(marks)
    }
}

fun registerTagsIn(context: FormatContext) {
    com.blockchaincommons.bcenvelope.registerTagsIn(context)
    context.tags().setSummarizer(TAG_PROVENANCE_MARK) { untaggedCbor, _ ->
        fromUntaggedCbor(untaggedCbor).toString()
    }
}

fun registerTags() {
    com.blockchaincommons.bcenvelope.withFormatContext { context ->
        registerTagsIn(context)
    }
}

private fun fromUntaggedCbor(cbor: Cbor): ProvenanceMark = ProvenanceMark.fromUntaggedCbor(cbor)
