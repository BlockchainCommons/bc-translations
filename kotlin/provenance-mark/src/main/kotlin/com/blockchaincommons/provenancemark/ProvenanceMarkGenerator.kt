package com.blockchaincommons.provenancemark

import com.blockchaincommons.bcenvelope.Envelope
import com.blockchaincommons.bcenvelope.addType
import com.blockchaincommons.bcenvelope.checkType
import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.dcbor.Cbor
import com.blockchaincommons.dcbor.CborDate

class ProvenanceMarkGenerator private constructor(
    private val res: ProvenanceMarkResolution,
    private val seed: ProvenanceSeed,
    private val chainId: ByteArray,
    private var nextSeq: UInt,
    private var rngState: RngState,
) {
    init {
        if (chainId.size != res.linkLength()) {
            throw ProvenanceMarkException.InvalidChainIdLength(res.linkLength(), chainId.size)
        }
    }

    fun res(): ProvenanceMarkResolution = res

    fun seed(): ProvenanceSeed = seed

    fun chainId(): ByteArray = chainId.copyOf()

    fun nextSeq(): UInt = nextSeq

    fun rngState(): RngState = rngState

    fun next(date: CborDate, info: Any? = null): ProvenanceMark {
        val rng = Xoshiro256StarStar.fromData(rngState.toBytes())

        val seq = nextSeq
        nextSeq += 1u

        val key = if (seq == 0u) {
            chainId.copyOf()
        } else {
            val generated = rng.nextBytes(res.linkLength())
            rngState = RngState.fromBytes(rng.toData())
            generated
        }

        val nextRng = rng.copy()
        val nextKey = nextRng.nextBytes(res.linkLength())

        return ProvenanceMark.new(
            res = res,
            key = key,
            nextKey = nextKey,
            chainId = chainId,
            seq = seq,
            date = date,
            info = info,
        )
    }

    fun toEnvelope(): Envelope {
        return Envelope.from(Cbor.fromByteString(chainId()))
            .addType("provenance-generator")
            .addAssertion("res", res.toCbor())
            .addAssertion("seed", seed.toCbor())
            .addAssertion("next-seq", nextSeq.toLong())
            .addAssertion("rng-state", rngState.toCbor())
    }

    fun toJson(): String {
        val fields = linkedMapOf<String, Any>(
            "res" to res.code,
            "seed" to seed.toBase64(),
            "chainID" to chainId.toBase64(),
            "nextSeq" to nextSeq.toLong(),
            "rngState" to rngState.toBase64(),
        )
        return JsonSupport.mapper.writeValueAsString(fields)
    }

    override fun toString(): String =
        "ProvenanceMarkGenerator(chainID: ${chainId.toHex()}, res: $res, seed: ${seed.hex()}, nextSeq: $nextSeq, rngState: $rngState)"

    override fun equals(other: Any?): Boolean {
        if (this === other) return true
        if (other !is ProvenanceMarkGenerator) return false
        return res == other.res &&
            seed == other.seed &&
            chainId.contentEquals(other.chainId) &&
            nextSeq == other.nextSeq &&
            rngState == other.rngState
    }

    override fun hashCode(): Int {
        var result = res.hashCode()
        result = 31 * result + seed.hashCode()
        result = 31 * result + chainId.contentHashCode()
        result = 31 * result + nextSeq.hashCode()
        result = 31 * result + rngState.hashCode()
        return result
    }

    companion object {
        fun newWithSeed(res: ProvenanceMarkResolution, seed: ProvenanceSeed): ProvenanceMarkGenerator {
            val digest1 = CryptoUtils.sha256(seed.toBytes())
            val chainId = digest1.copyOfRange(0, res.linkLength())
            val digest2 = CryptoUtils.sha256(digest1)
            return new(
                res = res,
                seed = seed,
                chainId = chainId,
                nextSeq = 0u,
                rngState = RngState.fromBytes(digest2),
            )
        }

        fun newWithPassphrase(res: ProvenanceMarkResolution, passphrase: String): ProvenanceMarkGenerator {
            val seed = ProvenanceSeed.newWithPassphrase(passphrase)
            return newWithSeed(res, seed)
        }

        fun newUsing(res: ProvenanceMarkResolution, rng: RandomNumberGenerator): ProvenanceMarkGenerator {
            val seed = ProvenanceSeed.newUsing(rng)
            return newWithSeed(res, seed)
        }

        fun newRandom(res: ProvenanceMarkResolution): ProvenanceMarkGenerator {
            val seed = ProvenanceSeed.new()
            return newWithSeed(res, seed)
        }

        fun new(
            res: ProvenanceMarkResolution,
            seed: ProvenanceSeed,
            chainId: ByteArray,
            nextSeq: UInt,
            rngState: RngState,
        ): ProvenanceMarkGenerator {
            if (chainId.size != res.linkLength()) {
                throw ProvenanceMarkException.InvalidChainIdLength(res.linkLength(), chainId.size)
            }
            return ProvenanceMarkGenerator(
                res = res,
                seed = seed,
                chainId = chainId.copyOf(),
                nextSeq = nextSeq,
                rngState = rngState,
            )
        }

        fun fromEnvelope(envelope: Envelope): ProvenanceMarkGenerator {
            envelope.checkType("provenance-generator")
            val chainId = envelope.subject().tryByteString()
            val assertionCount = envelope.assertions().size
            val expectedKeyCount = 5
            if (assertionCount != expectedKeyCount) {
                throw ProvenanceMarkException.ExtraKeys(expectedKeyCount, assertionCount)
            }

            val res = ProvenanceMarkResolution.fromCbor(
                envelope.objectForPredicate("res").tryLeaf()
            )
            val seed = ProvenanceSeed.fromCbor(
                envelope.objectForPredicate("seed").tryLeaf()
            )
            val nextSeq = envelope.objectForPredicate("next-seq").tryLeaf().tryUInt()
            val rngState = RngState.fromCbor(
                envelope.objectForPredicate("rng-state").tryLeaf()
            )

            return new(res, seed, chainId, nextSeq, rngState)
        }

        fun fromJson(json: String): ProvenanceMarkGenerator {
            val node = JsonSupport.mapper.readTree(json)
            val res = ProvenanceMarkResolution.fromCode(node.path("res").asInt())
            val seed = ProvenanceSeed.fromBase64(node.path("seed").asText())
            val chainId = node.path("chainID").asText().fromBase64()
            val nextSeqLong = node.path("nextSeq").asLong()
            if (nextSeqLong < 0 || nextSeqLong > UInt.MAX_VALUE.toLong()) {
                throw ProvenanceMarkException.IntegerConversion("nextSeq out of range for u32: $nextSeqLong")
            }
            val nextSeq = nextSeqLong.toUInt()
            val rngState = RngState.fromBase64(node.path("rngState").asText())
            return new(
                res = res,
                seed = seed,
                chainId = chainId,
                nextSeq = nextSeq,
                rngState = rngState,
            )
        }
    }
}
