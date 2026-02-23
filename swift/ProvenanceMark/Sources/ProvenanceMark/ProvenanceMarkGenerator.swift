import Foundation
import DCBOR
import BCRand
import BCEnvelope

/// A stateful generator that produces a chain of `ProvenanceMark` values.
///
/// Each generator is initialized with a resolution and a seed. Successive calls
/// to `next(date:info:)` produce cryptographically linked marks whose key
/// derivation is driven by a deterministic Xoshiro256** PRNG seeded from the
/// initial state.
///
/// The generator is `Codable` (JSON-serializable) so that its state can be
/// persisted and restored between sessions.
public struct ProvenanceMarkGenerator: Sendable, Equatable, Codable {
    // MARK: - Stored properties

    /// The resolution of the marks produced by this generator.
    public let resolution: ProvenanceMarkResolution

    /// The seed used to initialize this generator.
    public let seed: ProvenanceSeed

    /// The chain identifier (truncated SHA-256 of the seed, length = `resolution.linkLength`).
    public let chainId: [UInt8]

    /// The sequence number of the next mark to be generated.
    public private(set) var nextSeq: UInt32

    /// The current RNG state, used to derive keys for subsequent marks.
    public private(set) var rngState: RngState

    // MARK: - CodingKeys

    private enum CodingKeys: String, CodingKey {
        case resolution = "res"
        case seed
        case chainId = "chainID"
        case nextSeq = "nextSeq"
        case rngState = "rngState"
    }

    // MARK: - Codable (chain ID as base64)

    public func encode(to encoder: Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)
        try container.encode(resolution, forKey: .resolution)
        try container.encode(seed, forKey: .seed)
        try container.encode(Data(chainId).base64EncodedString(), forKey: .chainId)
        try container.encode(nextSeq, forKey: .nextSeq)
        try container.encode(rngState, forKey: .rngState)
    }

    public init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)
        self.resolution = try container.decode(ProvenanceMarkResolution.self, forKey: .resolution)
        self.seed = try container.decode(ProvenanceSeed.self, forKey: .seed)
        let base64String = try container.decode(String.self, forKey: .chainId)
        guard let data = Data(base64Encoded: base64String) else {
            throw DecodingError.dataCorruptedError(
                forKey: .chainId,
                in: container,
                debugDescription: "Invalid base64 string for chainID")
        }
        self.chainId = [UInt8](data)
        self.nextSeq = try container.decode(UInt32.self, forKey: .nextSeq)
        self.rngState = try container.decode(RngState.self, forKey: .rngState)
    }

    // MARK: - Initializers

    /// Creates a generator from its raw components, validating the chain ID length.
    ///
    /// - Parameters:
    ///   - resolution: The mark resolution.
    ///   - seed: The provenance seed.
    ///   - chainId: The chain identifier bytes (must match `resolution.linkLength`).
    ///   - nextSeq: The next sequence number.
    ///   - rngState: The RNG state snapshot.
    /// - Throws: `ProvenanceMarkError.invalidChainIdLength` if `chainId.count`
    ///   does not equal `resolution.linkLength`.
    public init(
        res resolution: ProvenanceMarkResolution,
        seed: ProvenanceSeed,
        chainId: [UInt8],
        nextSeq: UInt32,
        rngState: RngState
    ) throws(ProvenanceMarkError) {
        guard chainId.count == resolution.linkLength else {
            throw .invalidChainIdLength(expected: resolution.linkLength, actual: chainId.count)
        }
        self.resolution = resolution
        self.seed = seed
        self.chainId = chainId
        self.nextSeq = nextSeq
        self.rngState = rngState
    }

    /// Creates a new generator from a seed.
    ///
    /// The chain ID is derived as the first `resolution.linkLength` bytes of
    /// `SHA-256(seed)`, and the initial RNG state is `SHA-256(SHA-256(seed))`.
    ///
    /// - Parameters:
    ///   - resolution: The mark resolution.
    ///   - seed: The provenance seed.
    public init(res resolution: ProvenanceMarkResolution, seed: ProvenanceSeed) {
        let digest1 = CryptoUtils.sha256(seed.bytes)
        let chainId = Array(digest1.prefix(resolution.linkLength))
        let digest2 = CryptoUtils.sha256(digest1)
        // The chain ID length is guaranteed to match resolution.linkLength.
        try! self.init(
            res: resolution,
            seed: seed,
            chainId: chainId,
            nextSeq: 0,
            rngState: RngState(bytes: digest2)
        )
    }

    /// Creates a new generator from a passphrase.
    ///
    /// The seed is derived from the passphrase via HKDF key extension.
    ///
    /// - Parameters:
    ///   - resolution: The mark resolution.
    ///   - passphrase: A passphrase string.
    public init(res resolution: ProvenanceMarkResolution, passphrase: String) {
        let seed = ProvenanceSeed(passphrase: passphrase)
        self.init(res: resolution, seed: seed)
    }

    /// Creates a new generator using the given random number generator.
    ///
    /// - Parameters:
    ///   - resolution: The mark resolution.
    ///   - rng: A random number generator conforming to `BCRandomNumberGenerator`.
    public init(res resolution: ProvenanceMarkResolution, using rng: inout some BCRandomNumberGenerator) {
        let seed = ProvenanceSeed(using: &rng)
        self.init(res: resolution, seed: seed)
    }

    /// Creates a new generator with a cryptographically random seed.
    ///
    /// - Parameter resolution: The mark resolution.
    public init(res resolution: ProvenanceMarkResolution) {
        let seed = ProvenanceSeed()
        self.init(res: resolution, seed: seed)
    }

    // MARK: - Mark generation

    /// Generates the next provenance mark in the chain.
    ///
    /// Each call advances the internal state (sequence number and RNG state)
    /// so that the next call produces a cryptographically linked successor.
    ///
    /// - Parameters:
    ///   - date: The date to embed in the mark.
    ///   - info: Optional CBOR-encodable metadata to embed in the mark.
    /// - Returns: The generated `ProvenanceMark`.
    public mutating func next(date: Date, info: CBOR? = nil) -> ProvenanceMark {
        let rngData = rngState.bytes
        var rng = Xoshiro256StarStar(data: rngData)

        let seq = nextSeq
        nextSeq += 1

        let key: [UInt8]
        if seq == 0 {
            key = chainId
        } else {
            // The randomness generated by the PRNG should be portable across
            // implementations.
            key = rng.nextBytes(count: resolution.linkLength)
            rngState = RngState(bytes: rng.data)
        }

        var nextRng = rng
        let nextKey = nextRng.nextBytes(count: resolution.linkLength)

        return try! ProvenanceMark(
            res: resolution,
            key: key,
            nextKey: nextKey,
            chainId: chainId,
            seq: seq,
            date: date,
            info: info
        )
    }
}

// MARK: - CustomStringConvertible

extension ProvenanceMarkGenerator: CustomStringConvertible {
    public var description: String {
        "ProvenanceMarkGenerator(chainID: \(chainId.hex), resolution: \(resolution), seed: \(seed.hex), nextSeq: \(nextSeq), rngState: \(rngState.hex))"
    }
}

// MARK: - Envelope encoding

public extension ProvenanceMarkGenerator {
    /// Encodes this generator as an `Envelope`.
    ///
    /// The envelope subject is the chain ID as a CBOR byte string, with
    /// assertions for the resolution, seed, next sequence number, and RNG state.
    var envelope: Envelope {
        Envelope(Data(chainId))
            .addType("provenance-generator")
            .addAssertion("res", resolution.cbor)
            .addAssertion("seed", seed.cbor)
            .addAssertion("next-seq", nextSeq)
            .addAssertion("rng-state", rngState.cbor)
    }

    /// Decodes a `ProvenanceMarkGenerator` from an `Envelope`.
    ///
    /// - Parameter envelope: The envelope to decode.
    /// - Throws: `ProvenanceMarkError` if the envelope does not contain a valid
    ///   provenance generator.
    init(envelope: Envelope) throws {
        do {
            try envelope.checkType("provenance-generator")

            let chainIdData: Data = try envelope.extractSubject(Data.self)
            let chainId = [UInt8](chainIdData)

            let expectedAssertionCount = 5
            let assertionCount = envelope.assertions.count
            guard assertionCount == expectedAssertionCount else {
                throw ProvenanceMarkError.extraKeys(
                    expected: expectedAssertionCount,
                    actual: assertionCount)
            }

            let resCBOR = try envelope.object(forPredicate: "res").leaf!
            let resolution = try ProvenanceMarkResolution(cbor: resCBOR)

            let seedCBOR = try envelope.object(forPredicate: "seed").leaf!
            let seed = try ProvenanceSeed(cbor: seedCBOR)

            let nextSeq: UInt32 = try envelope.extractObject(
                UInt32.self, forPredicate: "next-seq")

            let rngStateCBOR = try envelope.object(forPredicate: "rng-state").leaf!
            let rngState = try RngState(cbor: rngStateCBOR)

            try self.init(
                res: resolution,
                seed: seed,
                chainId: chainId,
                nextSeq: nextSeq,
                rngState: rngState
            )
        } catch let error as ProvenanceMarkError {
            throw error
        } catch {
            throw ProvenanceMarkError.envelope(String(describing: error))
        }
    }
}
