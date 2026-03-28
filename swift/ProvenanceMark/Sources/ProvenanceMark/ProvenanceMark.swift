import Foundation
import DCBOR
import BCUR
import BCTags
import BCEnvelope

/// A cryptographic provenance mark that can be chained to form a verifiable
/// sequence of marks.
///
/// Each mark contains a key, hash, chain ID, sequence number, date, and
/// optional info field. Marks are linked by revealing keys in subsequent marks,
/// allowing verification that a sequence is authentic and unbroken.
public struct ProvenanceMark: Sendable {
    private let _seq: UInt32
    private let _date: Date
    private let _resolution: ProvenanceMarkResolution
    private let _chainId: [UInt8]
    private let _key: [UInt8]
    private let _hash: [UInt8]
    private let _infoBytes: [UInt8]
    private let _seqBytes: [UInt8]
    private let _dateBytes: [UInt8]
}

// MARK: - Accessors

public extension ProvenanceMark {
    /// The resolution of this provenance mark.
    var resolution: ProvenanceMarkResolution { _resolution }

    /// The key bytes.
    var key: [UInt8] { _key }

    /// The hash bytes.
    var hash: [UInt8] { _hash }

    /// The chain ID bytes.
    var chainId: [UInt8] { _chainId }

    /// The serialized sequence number bytes.
    var seqBytes: [UInt8] { _seqBytes }

    /// The serialized date bytes.
    var dateBytes: [UInt8] { _dateBytes }

    /// The sequence number.
    var seq: UInt32 { _seq }

    /// The date.
    var date: Date { _date }

    /// The full obfuscated message bytes.
    ///
    /// The message is the key concatenated with the obfuscated payload
    /// (chain ID, hash, sequence bytes, date bytes, and info bytes).
    var message: [UInt8] {
        let payload = _chainId + _hash + _seqBytes + _dateBytes + _infoBytes
        return _key + CryptoUtils.obfuscate(key: _key, message: payload)
    }

    /// The decoded CBOR info, or `nil` if the info field is empty.
    var info: CBOR? {
        guard !_infoBytes.isEmpty else { return nil }
        return try? CBOR(Data(_infoBytes))
    }
}

// MARK: - Constructors

public extension ProvenanceMark {
    /// Creates a new provenance mark.
    ///
    /// - Parameters:
    ///   - resolution: The resolution level.
    ///   - key: The key bytes (must be `resolution.linkLength` bytes).
    ///   - nextKey: The next key bytes (must be `resolution.linkLength` bytes).
    ///   - chainId: The chain ID bytes (must be `resolution.linkLength` bytes).
    ///   - seq: The sequence number.
    ///   - date: The date.
    ///   - info: Optional CBOR-encodable info to embed in the mark.
    /// - Throws: `ProvenanceMarkError` if any field has an invalid length.
    init(
        resolution: ProvenanceMarkResolution,
        key: [UInt8],
        nextKey: [UInt8],
        chainId: [UInt8],
        seq: UInt32,
        date: Date,
        info: (any CBOREncodable)? = nil
    ) throws {
        guard key.count == resolution.linkLength else {
            throw ProvenanceMarkError.invalidKeyLength(
                expected: resolution.linkLength, actual: key.count)
        }
        guard nextKey.count == resolution.linkLength else {
            throw ProvenanceMarkError.invalidNextKeyLength(
                expected: resolution.linkLength, actual: nextKey.count)
        }
        guard chainId.count == resolution.linkLength else {
            throw ProvenanceMarkError.invalidChainIdLength(
                expected: resolution.linkLength, actual: chainId.count)
        }

        let dateBytes = try resolution.serializeDate(date)
        let seqBytes = try resolution.serializeSeq(seq)

        // Re-deserialize the date to normalize it to the resolution's precision.
        let normalizedDate = try resolution.deserializeDate(dateBytes)

        let infoBytes: [UInt8]
        if let info {
            infoBytes = Array(info.cborData)
        } else {
            infoBytes = []
        }

        let hash = Self.makeHash(
            resolution: resolution,
            key: key,
            nextKey: nextKey,
            chainId: chainId,
            seqBytes: seqBytes,
            dateBytes: dateBytes,
            infoBytes: infoBytes
        )

        self._resolution = resolution
        self._key = key
        self._hash = hash
        self._chainId = chainId
        self._seqBytes = seqBytes
        self._dateBytes = dateBytes
        self._infoBytes = infoBytes
        self._seq = seq
        self._date = normalizedDate
    }

    /// Creates a provenance mark from an obfuscated message.
    ///
    /// - Parameters:
    ///   - resolution: The resolution level.
    ///   - message: The full message bytes (key + obfuscated payload).
    /// - Throws: `ProvenanceMarkError` if the message is too short or contains
    ///   invalid data.
    init(resolution: ProvenanceMarkResolution, message: [UInt8]) throws {
        guard message.count >= resolution.fixedLength else {
            throw ProvenanceMarkError.invalidMessageLength(
                expected: resolution.fixedLength, actual: message.count)
        }

        let key = Array(message[resolution.keyRange])
        let payload = CryptoUtils.obfuscate(
            key: key,
            message: Array(message[resolution.linkLength...])
        )
        let chainId = Array(payload[resolution.chainIdRange])
        let hash = Array(payload[resolution.hashRange])
        let seqBytes = Array(payload[resolution.seqBytesRange])
        let seq = try resolution.deserializeSeq(seqBytes)
        let dateBytes = Array(payload[resolution.dateBytesRange])
        let date = try resolution.deserializeDate(dateBytes)

        let infoBytes = Array(payload[resolution.infoRangeStart...])
        if !infoBytes.isEmpty {
            do {
                _ = try CBOR(Data(infoBytes))
            } catch {
                throw ProvenanceMarkError.invalidInfoCbor
            }
        }

        self._resolution = resolution
        self._key = key
        self._hash = hash
        self._chainId = chainId
        self._seqBytes = seqBytes
        self._dateBytes = dateBytes
        self._infoBytes = infoBytes
        self._seq = seq
        self._date = date
    }
}

// MARK: - Hash Computation

extension ProvenanceMark {
    /// Computes the hash for a provenance mark from its component fields.
    static func makeHash(
        resolution: ProvenanceMarkResolution,
        key: [UInt8],
        nextKey: [UInt8],
        chainId: [UInt8],
        seqBytes: [UInt8],
        dateBytes: [UInt8],
        infoBytes: [UInt8]
    ) -> [UInt8] {
        var buf = [UInt8]()
        buf.append(contentsOf: key)
        buf.append(contentsOf: nextKey)
        buf.append(contentsOf: chainId)
        buf.append(contentsOf: seqBytes)
        buf.append(contentsOf: dateBytes)
        buf.append(contentsOf: infoBytes)
        return CryptoUtils.sha256Prefix(buf, length: resolution.linkLength)
    }
}

// MARK: - Identifiers

public extension ProvenanceMark {
    /// A 32-byte identifier derived from the hash and fingerprint.
    ///
    /// The first `hash.count` bytes are the stored hash (backward compatible).
    /// Remaining bytes come from `fingerprint` (SHA-256 of CBOR).
    var id: [UInt8] {
        var result = [UInt8](repeating: 0, count: 32)
        let n = _hash.count
        result.replaceSubrange(0..<n, with: _hash)
        if n < 32 {
            let fp = fingerprint
            result.replaceSubrange(n..<32, with: fp.prefix(32 - n))
        }
        return result
    }

    /// The full 64-character hex string of the identifier.
    var idHex: String {
        id.hex
    }

    /// The first `wordCount` bytes of the identifier as upper-case ByteWords.
    ///
    /// - Precondition: `wordCount` must be in `4...32`.
    func idBytewords(_ wordCount: Int, prefix: Bool) -> String {
        precondition((4...32).contains(wordCount), "wordCount must be 4...32, got \(wordCount)")
        let s = Bytewords.encodeToWords(Array(id.prefix(wordCount))).uppercased()
        return prefix ? "\u{1F15F} \(s)" : s
    }

    /// The first `wordCount` bytes of the identifier as Bytemoji.
    ///
    /// - Precondition: `wordCount` must be in `4...32`.
    func idBytemoji(_ wordCount: Int, prefix: Bool) -> String {
        precondition((4...32).contains(wordCount), "wordCount must be 4...32, got \(wordCount)")
        let s = Bytewords.encodeToBytemojis(Array(id.prefix(wordCount))).uppercased()
        return prefix ? "\u{1F15F} \(s)" : s
    }

    /// The first `wordCount` bytes of the identifier as upper-case minimal
    /// ByteWords.
    ///
    /// - Precondition: `wordCount` must be in `4...32`.
    func idBytewordsMinimal(_ wordCount: Int, prefix: Bool) -> String {
        precondition((4...32).contains(wordCount), "wordCount must be 4...32, got \(wordCount)")
        let s = Bytewords.encodeToMinimalBytewords(Array(id.prefix(wordCount))).uppercased()
        return prefix ? "\u{1F15F} \(s)" : s
    }
}

// MARK: - Disambiguation

public extension ProvenanceMark {
    /// Returns disambiguated upper-case ByteWords identifiers for a set of marks.
    ///
    /// Non-colliding marks get 4-word identifiers. Only marks whose 4-byte
    /// prefixes collide are extended with additional words.
    static func disambiguatedIdBytewords(
        _ marks: [ProvenanceMark],
        prefix: Bool
    ) -> [String] {
        let hashes = marks.map { $0.id }
        let lengths = minimalNoncollidingPrefixLengths(hashes)
        return zip(hashes, lengths).map { hash, len in
            let s = Bytewords.encodeToWords(Array(hash.prefix(len))).uppercased()
            return prefix ? "\u{1F15F} \(s)" : s
        }
    }

    /// Returns disambiguated Bytemoji identifiers for a set of marks.
    ///
    /// Non-colliding marks get 4-emoji identifiers. Only marks whose 4-byte
    /// prefixes collide are extended with additional emojis.
    static func disambiguatedIdBytemoji(
        _ marks: [ProvenanceMark],
        prefix: Bool
    ) -> [String] {
        let hashes = marks.map { $0.id }
        let lengths = minimalNoncollidingPrefixLengths(hashes)
        return zip(hashes, lengths).map { hash, len in
            let s = Bytewords.encodeToBytemojis(Array(hash.prefix(len))).uppercased()
            return prefix ? "\u{1F15F} \(s)" : s
        }
    }

    private static func minimalNoncollidingPrefixLengths(
        _ hashes: [[UInt8]]
    ) -> [Int] {
        var lengths = [Int](repeating: 4, count: hashes.count)

        var groups: [[UInt8]: [Int]] = [:]
        for (i, hash) in hashes.enumerated() {
            let key = Array(hash.prefix(4))
            groups[key, default: []].append(i)
        }

        for (_, indices) in groups where indices.count > 1 {
            resolveCollisionGroup(hashes, indices: indices, lengths: &lengths)
        }

        return lengths
    }

    private static func resolveCollisionGroup(
        _ hashes: [[UInt8]],
        indices initialIndices: [Int],
        lengths: inout [Int]
    ) {
        var unresolved = initialIndices

        for prefixLen in 5...32 {
            var subGroups: [[UInt8]: [Int]] = [:]
            for i in unresolved {
                let key = Array(hashes[i].prefix(prefixLen))
                subGroups[key, default: []].append(i)
            }

            var nextUnresolved: [Int] = []
            for (_, subIndices) in subGroups {
                if subIndices.count == 1 {
                    lengths[subIndices[0]] = prefixLen
                } else {
                    nextUnresolved.append(contentsOf: subIndices)
                }
            }

            if nextUnresolved.isEmpty { return }
            unresolved = nextUnresolved
        }

        for i in unresolved {
            lengths[i] = 32
        }
    }
}

// MARK: - Validation

public extension ProvenanceMark {
    /// Whether this mark is a genesis mark (sequence 0 with key equal to chain ID).
    var isGenesis: Bool {
        _seq == 0 && _key == _chainId
    }

    /// Returns `true` if this mark validly precedes the given next mark.
    func precedes(_ next: ProvenanceMark) -> Bool {
        do {
            try validatePrecedes(next)
            return true
        } catch {
            return false
        }
    }

    /// Validates that this mark precedes the given next mark.
    ///
    /// - Throws: `ProvenanceMarkError.validation(_)` describing the issue if
    ///   validation fails.
    func validatePrecedes(_ next: ProvenanceMark) throws(ProvenanceMarkError) {
        // `next` can't be a genesis
        if next._seq == 0 {
            throw .validation(.nonGenesisAtZero)
        }
        if next._key == next._chainId {
            throw .validation(.invalidGenesisKey)
        }
        // `next` must have the next highest sequence number
        if _seq != next._seq - 1 {
            throw .validation(.sequenceGap(expected: _seq + 1, actual: next._seq))
        }
        // `next` must have an equal or later date
        if _date > next._date {
            throw .validation(.dateOrdering(previous: _date, next: next._date))
        }
        // `next` must reveal the key that was used to generate this mark's hash
        let expectedHash = Self.makeHash(
            resolution: _resolution,
            key: _key,
            nextKey: next._key,
            chainId: _chainId,
            seqBytes: _seqBytes,
            dateBytes: _dateBytes,
            infoBytes: _infoBytes
        )
        if _hash != expectedHash {
            throw .validation(.hashMismatch(expected: expectedHash, actual: _hash))
        }
    }

    /// Validates that a sequence of marks forms a valid chain.
    ///
    /// - Parameter marks: The marks to validate, in order.
    /// - Returns: `true` if the sequence is valid.
    static func isSequenceValid(_ marks: [ProvenanceMark]) -> Bool {
        guard marks.count >= 2 else { return false }
        if marks[0].seq == 0 && !marks[0].isGenesis {
            return false
        }
        for i in 0..<(marks.count - 1) {
            if !marks[i].precedes(marks[i + 1]) {
                return false
            }
        }
        return true
    }
}

// MARK: - Bytewords Serialization

public extension ProvenanceMark {
    /// Encodes this mark's message as bytewords in the given style.
    ///
    /// - Parameter style: The bytewords encoding style (defaults to `.standard`).
    func bytewords(style: BytewordsStyle = .standard) -> String {
        Bytewords.encode(message, style: style)
    }

    /// Decodes a provenance mark from a bytewords string.
    ///
    /// - Parameters:
    ///   - resolution: The resolution level.
    ///   - bytewords: The bytewords-encoded string.
    /// - Throws: `ProvenanceMarkError` on decode failure.
    init(
        resolution: ProvenanceMarkResolution,
        bytewords: String
    ) throws {
        let decoded: [UInt8]
        do {
            decoded = try Bytewords.decode(bytewords, style: .standard)
        } catch {
            throw ProvenanceMarkError.bytewords(String(describing: error))
        }
        try self.init(resolution: resolution, message: decoded)
    }
}

// MARK: - URL Encoding

public extension ProvenanceMark {
    /// This mark's CBOR data encoded as a minimal bytewords string.
    var urlEncoding: String {
        Bytewords.encode(Array(cborData), style: .minimal)
    }

    /// Decodes a provenance mark from a minimal bytewords URL encoding.
    ///
    /// - Parameter urlEncoding: The minimal bytewords string.
    /// - Throws: `ProvenanceMarkError` on decode failure.
    init(urlEncoding: String) throws {
        let cborBytes: [UInt8]
        do {
            cborBytes = try Bytewords.decode(urlEncoding, style: .minimal)
        } catch {
            throw ProvenanceMarkError.bytewords(String(describing: error))
        }
        let cbor: CBOR
        do {
            cbor = try CBOR(Data(cborBytes))
        } catch {
            throw ProvenanceMarkError.cbor(String(describing: error))
        }
        do {
            try self.init(taggedCBOR: cbor)
        } catch {
            throw ProvenanceMarkError.cbor(String(describing: error))
        }
    }
}

// MARK: - URL

public extension ProvenanceMark {
    /// Creates a URL by appending this mark as a `provenance` query parameter.
    ///
    /// - Parameter base: The base URL string.
    /// - Returns: The URL with the provenance query parameter appended.
    func url(base: String) -> URL {
        var components = URLComponents(string: base)!
        var items = components.queryItems ?? []
        items.append(URLQueryItem(name: "provenance", value: urlEncoding))
        components.queryItems = items
        return components.url!
    }

    /// Decodes a provenance mark from a URL's `provenance` query parameter.
    ///
    /// - Parameter url: The URL containing the provenance query parameter.
    /// - Throws: `ProvenanceMarkError.missingUrlParameter` if the parameter is
    ///   absent, or other errors on decode failure.
    init(url: URL) throws {
        guard let components = URLComponents(url: url, resolvingAgainstBaseURL: false),
              let queryItems = components.queryItems,
              let item = queryItems.first(where: { $0.name == "provenance" }),
              let value = item.value
        else {
            throw ProvenanceMarkError.missingUrlParameter(parameter: "provenance")
        }
        try self.init(urlEncoding: value)
    }
}

// MARK: - Fingerprint

public extension ProvenanceMark {
    /// The SHA-256 fingerprint of this mark's CBOR-encoded data.
    var fingerprint: [UInt8] {
        CryptoUtils.sha256(Array(cborData))
    }
}

// MARK: - Equatable, Hashable

extension ProvenanceMark: Equatable {
    public static func == (lhs: ProvenanceMark, rhs: ProvenanceMark) -> Bool {
        lhs._resolution == rhs._resolution && lhs.message == rhs.message
    }
}

extension ProvenanceMark: Hashable {
    public func hash(into hasher: inout Hasher) {
        hasher.combine(_resolution)
        hasher.combine(message)
    }
}

// MARK: - CustomStringConvertible, CustomDebugStringConvertible

extension ProvenanceMark: CustomStringConvertible {
    public var description: String {
        "ProvenanceMark(\(idHex))"
    }
}

extension ProvenanceMark: CustomDebugStringConvertible {
    public var debugDescription: String {
        var components = [
            "key: \(_key.hex)",
            "hash: \(_hash.hex)",
            "chainID: \(_chainId.hex)",
            "seq: \(_seq)",
            "date: \(dateToISO8601(_date))",
        ]

        if let info {
            components.append("info: \(info.diagnostic())")
        }

        return "ProvenanceMark(\(components.joined(separator: ", ")))"
    }
}

// MARK: - CBOR Tagged Codable

extension ProvenanceMark: CBORTaggedCodable {
    public static var cborTags: [Tag] { [.provenanceMark] }

    public var untaggedCBOR: CBOR {
        CBOR.array([_resolution.cbor, CBOR.bytes(Data(message))])
    }

    public init(untaggedCBOR cbor: CBOR) throws {
        guard case .array(let elements) = cbor, elements.count == 2 else {
            throw CBORError.wrongType
        }
        let resolution = try ProvenanceMarkResolution(cbor: elements[0])
        let messageData = try Data(cbor: elements[1])
        do {
            try self.init(resolution: resolution, message: Array(messageData))
        } catch {
            throw CBORError.invalidFormat
        }
    }
}

// MARK: - URCodable

extension ProvenanceMark: URCodable { }

// MARK: - Envelope

public extension Envelope {
    /// Creates an envelope containing a provenance mark.
    init(_ mark: ProvenanceMark) {
        self.init(mark.taggedCBOR)
    }
}

public extension ProvenanceMark {
    /// Extracts a provenance mark from an envelope.
    ///
    /// - Parameter envelope: The envelope containing a provenance mark as its
    ///   leaf subject.
    /// - Throws: `ProvenanceMarkError` if the envelope does not contain a valid
    ///   provenance mark.
    init(envelope: Envelope) throws {
        guard let leaf = envelope.leaf else {
            throw ProvenanceMarkError.envelope("expected leaf envelope")
        }
        do {
            try self.init(taggedCBOR: leaf)
        } catch {
            throw ProvenanceMarkError.cbor(String(describing: error))
        }
    }
}

// MARK: - Tag Registration

public extension ProvenanceMark {
    /// Registers the provenance mark tag summarizer in the given tags store.
    ///
    /// This allows BCEnvelope's formatting system to display provenance marks
    /// with their identifier when encountered in envelope output.
    ///
    /// - Parameter tagsStore: The tags store to register the summarizer in.
    @MainActor
    static func registerTags(in tagsStore: TagsStore) {
        tagsStore.setSummarizer(.provenanceMark) { payload, _ in
            guard let untaggedCBOR = payload as? CBOR else {
                throw ProvenanceMarkError.cbor("expected CBOR payload")
            }
            let mark = try ProvenanceMark(untaggedCBOR: untaggedCBOR)
            return mark.description
        }
    }

    /// Registers the provenance mark tag summarizer in the global tags store.
    @MainActor
    static func registerTags() {
        registerTags(in: globalTags)
    }
}

// MARK: - Codable (JSON)

extension ProvenanceMark: Codable {
    private enum CodingKeys: String, CodingKey {
        case seq
        case date
        case resolution = "res"
        case chainID
        case key
        case hash
        case infoBytes
    }

    public func encode(to encoder: Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)
        try container.encode(_seq, forKey: .seq)
        try container.encode(dateToISO8601(_date), forKey: .date)
        try container.encode(_resolution, forKey: .resolution)
        try container.encode(Data(_chainId).base64EncodedString(), forKey: .chainID)
        try container.encode(Data(_key).base64EncodedString(), forKey: .key)
        try container.encode(Data(_hash).base64EncodedString(), forKey: .hash)
        if !_infoBytes.isEmpty {
            try container.encode(Data(_infoBytes).hex, forKey: .infoBytes)
        }
    }

    public init(from decoder: Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)

        let resolution = try container.decode(ProvenanceMarkResolution.self, forKey: .resolution)

        let keyBase64 = try container.decode(String.self, forKey: .key)
        guard let keyData = Data(base64Encoded: keyBase64) else {
            throw ProvenanceMarkError.base64("invalid base64 for key")
        }
        let key = Array(keyData)

        let hashBase64 = try container.decode(String.self, forKey: .hash)
        guard let hashData = Data(base64Encoded: hashBase64) else {
            throw ProvenanceMarkError.base64("invalid base64 for hash")
        }
        let hash = Array(hashData)

        let chainIdBase64 = try container.decode(String.self, forKey: .chainID)
        guard let chainIdData = Data(base64Encoded: chainIdBase64) else {
            throw ProvenanceMarkError.base64("invalid base64 for chainID")
        }
        let chainId = Array(chainIdData)

        let infoBytesHex = try container.decodeIfPresent(String.self, forKey: .infoBytes) ?? ""
        let infoBytes: [UInt8]
        if infoBytesHex.isEmpty {
            infoBytes = []
        } else {
            guard let parsed = [UInt8](hex: infoBytesHex) else {
                throw ProvenanceMarkError.cbor("invalid hex for infoBytes")
            }
            infoBytes = parsed
        }

        let seq = try container.decode(UInt32.self, forKey: .seq)
        let dateString = try container.decode(String.self, forKey: .date)
        let date = try dateFromISO8601(dateString)

        let seqBytes = try resolution.serializeSeq(seq)
        let dateBytes = try resolution.serializeDate(date)

        self._resolution = resolution
        self._key = key
        self._hash = hash
        self._chainId = chainId
        self._seqBytes = seqBytes
        self._dateBytes = dateBytes
        self._infoBytes = infoBytes
        self._seq = seq
        self._date = date
    }
}
