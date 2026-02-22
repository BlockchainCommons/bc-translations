import Foundation

/// Internal UR string encoding/decoding helpers.
internal enum UREncoding {
    enum Kind {
        case singlePart
        case multiPart
    }

    static func encode(_ data: [UInt8], urType: String) -> String {
        let body = Bytewords.encode(data, style: .minimal)
        return "ur:\(urType)/\(body)"
    }

    static func decode(_ value: String) throws -> (Kind, [UInt8]) {
        guard value.hasPrefix("ur:") else {
            throw URCodecError.invalidScheme
        }

        let stripScheme = String(value.dropFirst(3))
        guard let firstSlash = stripScheme.firstIndex(of: "/") else {
            throw URCodecError.typeUnspecified
        }

        let type = String(stripScheme[..<firstSlash])
        guard type.allSatisfy({ $0.isURCodecTypeCharacter }) else {
            throw URCodecError.invalidCharacters
        }

        let rest = String(stripScheme[stripScheme.index(after: firstSlash)...])

        do {
            if let lastSlash = rest.lastIndex(of: "/") {
                let indices = String(rest[..<lastSlash])
                let payload = String(rest[rest.index(after: lastSlash)...])

                let parts = indices.split(separator: "-", omittingEmptySubsequences: false)
                guard parts.count == 2,
                      UInt16(parts[0]) != nil,
                      UInt16(parts[1]) != nil else {
                    throw URCodecError.invalidIndices
                }

                let decoded = try Bytewords.decodeRaw(payload, style: .minimal)
                return (.multiPart, decoded)
            }

            let decoded = try Bytewords.decodeRaw(rest, style: .minimal)
            return (.singlePart, decoded)
        } catch let error as BytewordsCodecError {
            throw URCodecError.bytewords(error)
        }
    }
}

private extension Character {
    var isURCodecTypeCharacter: Bool {
        if isASCII,
           let scalar = unicodeScalars.first,
           CharacterSet.alphanumerics.contains(scalar) {
            return true
        }
        return self == "-"
    }
}
