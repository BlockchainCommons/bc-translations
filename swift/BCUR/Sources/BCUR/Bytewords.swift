import Foundation

/// Bytewords encoding and decoding with CRC32 checksums.
public enum Bytewords {
    /// Encodes bytes into a bytewords string in the selected style.
    public static func encode(_ data: some DataProtocol, style: BytewordsStyle) -> String {
        let payload = Array(data)
        let checksum = Crc32.checksum(payload).bigEndianBytes
        let bytes = payload + checksum

        let words: [String]
        switch style {
        case .standard, .uri:
            words = bytes.map { BytewordsConstants.words[Int($0)] }
        case .minimal:
            words = bytes.map { BytewordsConstants.minimals[Int($0)] }
        }

        switch style {
        case .standard:
            return words.joined(separator: " ")
        case .uri:
            return words.joined(separator: "-")
        case .minimal:
            return words.joined()
        }
    }

    /// Decodes a bytewords string back to bytes and validates checksum.
    public static func decode(_ encoded: String, style: BytewordsStyle) throws -> [UInt8] {
        do {
            return try decodeRaw(encoded, style: style)
        } catch let error as BytewordsCodecError {
            throw URError(bytewords: error)
        }
    }

    /// Encodes an arbitrary byte slice as space-separated bytewords.
    public static func encodeToWords(_ data: [UInt8]) -> String {
        data.map { BytewordsConstants.words[Int($0)] }.joined(separator: " ")
    }

    /// Encodes an arbitrary byte slice as space-separated bytemojis.
    public static func encodeToBytemojis(_ data: [UInt8]) -> String {
        data.map { BytewordsConstants.bytemojis[Int($0)] }.joined(separator: " ")
    }

    /// Encodes a 4-byte identifier as space-separated bytewords.
    public static func identifier(_ data: [UInt8]) -> String {
        precondition(data.count == 4, "Expected exactly 4 bytes")
        return encodeToWords(data)
    }

    /// Encodes a 4-byte identifier as space-separated bytemojis.
    public static func bytemojiIdentifier(_ data: [UInt8]) -> String {
        precondition(data.count == 4, "Expected exactly 4 bytes")
        return encodeToBytemojis(data)
    }

    /// Returns `true` if `word` (lowercase) is a valid byteword.
    public static func isValidWord(_ word: String) -> Bool {
        BytewordsConstants.wordIndexes[word] != nil
    }

    /// Returns `true` if `emoji` is one of the 256 bytemojis.
    public static func isValidBytemoji(_ emoji: String) -> Bool {
        BytewordsConstants.bytemojiSet.contains(emoji)
    }

    /// Canonicalizes a byteword token (2–4 ASCII letters, case-insensitive) to its
    /// full 4-letter lowercase form. Returns `nil` if the token is not a valid byteword
    /// or any of its short forms.
    ///
    /// Accepted forms:
    /// - 4 letters: full byteword (e.g., "wolf" or "WOLF")
    /// - 2 letters: first+last abbreviation (e.g., "wf")
    /// - 3 letters: first-three (e.g., "wol") or last-three (e.g., "olf")
    public static func canonicalizeByteword(_ token: String) -> String? {
        let lower = token.lowercased()
        switch lower.count {
        case 4:
            return BytewordsConstants.wordIndexes[lower] != nil ? lower : nil
        case 2:
            return BytewordsConstants.firstLastToWord[lower]
        case 3:
            return BytewordsConstants.firstThreeToWord[lower]
                ?? BytewordsConstants.lastThreeToWord[lower]
        default:
            return nil
        }
    }

    static func decodeRaw(_ encoded: String, style: BytewordsStyle) throws -> [UInt8] {
        guard encoded.isASCII else {
            throw BytewordsCodecError.nonAscii
        }

        let bytes: [UInt8]
        switch style {
        case .minimal:
            bytes = try decodeMinimal(encoded)
        case .standard:
            bytes = try decodeFromIndex(
                encoded.split(separator: " ", omittingEmptySubsequences: false).map(String.init),
                indexes: BytewordsConstants.wordIndexes
            )
        case .uri:
            bytes = try decodeFromIndex(
                encoded.split(separator: "-", omittingEmptySubsequences: false).map(String.init),
                indexes: BytewordsConstants.wordIndexes
            )
        }

        return try stripChecksum(bytes)
    }

    private static func decodeMinimal(_ encoded: String) throws -> [UInt8] {
        guard encoded.count % 2 == 0 else {
            throw BytewordsCodecError.invalidLength
        }

        var result: [UInt8] = []
        result.reserveCapacity(encoded.count / 2)

        var index = encoded.startIndex
        while index < encoded.endIndex {
            let next = encoded.index(index, offsetBy: 2)
            let key = String(encoded[index..<next])
            guard let value = BytewordsConstants.minimalIndexes[key] else {
                throw BytewordsCodecError.invalidWord
            }
            result.append(value)
            index = next
        }

        return result
    }

    private static func decodeFromIndex(
        _ keys: [String],
        indexes: [String: UInt8]
    ) throws -> [UInt8] {
        try keys.map { key in
            guard let value = indexes[key] else {
                throw BytewordsCodecError.invalidWord
            }
            return value
        }
    }

    private static func stripChecksum(_ data: [UInt8]) throws -> [UInt8] {
        guard data.count >= 4 else {
            throw BytewordsCodecError.invalidChecksum
        }

        let payloadLength = data.count - 4
        let payload = Array(data[..<payloadLength])
        let checksum = Array(data[payloadLength...])
        let expected = Crc32.checksum(payload).bigEndianBytes

        guard checksum == expected else {
            throw BytewordsCodecError.invalidChecksum
        }

        return payload
    }
}

private extension String {
    var isASCII: Bool {
        unicodeScalars.allSatisfy { $0.isASCII }
    }
}
