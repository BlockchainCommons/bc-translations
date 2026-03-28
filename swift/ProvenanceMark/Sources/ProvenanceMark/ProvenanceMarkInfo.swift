import Foundation
import BCUR

/// A provenance mark paired with its display-ready identifiers and an optional
/// comment.
///
/// `ProvenanceMarkInfo` wraps a ``ProvenanceMark`` together with its UR string,
/// Bytewords identifier, Bytemoji identifier, and an optional human-readable
/// comment. The wrapper is intended for serialization to JSON and for producing
/// Markdown summaries.
public struct ProvenanceMarkInfo: Sendable {
    /// The UR representation of the mark.
    public let ur: UR

    /// The Bytewords identifier string (with prefix).
    public let bytewords: String

    /// The Bytemoji identifier string (with prefix).
    public let bytemoji: String

    /// An optional human-readable comment. Empty string means no comment.
    public let comment: String

    /// The underlying provenance mark.
    public let mark: ProvenanceMark

    /// Creates a new `ProvenanceMarkInfo` from a mark and an optional comment.
    ///
    /// - Parameters:
    ///   - mark: The provenance mark to wrap.
    ///   - comment: A human-readable comment (pass `""` for none).
    public init(mark: ProvenanceMark, comment: String = "") {
        self.ur = mark.ur()
        self.bytewords = mark.idBytewords(4, prefix: true)
        self.bytemoji = mark.idBytemoji(4, prefix: true)
        self.comment = comment
        self.mark = mark
    }
}

// MARK: - Markdown summary

public extension ProvenanceMarkInfo {
    /// Returns a Markdown-formatted summary of the provenance mark.
    ///
    /// The format is:
    /// ```markdown
    /// ---
    ///
    /// 2025-01-17T01:12:33Z
    ///
    /// #### ur:provenance/...
    ///
    /// #### `🅟 WAVE JUDO LIAR FIGS`
    ///
    /// 🅟 🐝 💨 💕 🍎
    ///
    /// Genesis mark.
    ///
    /// ```
    var markdownSummary: String {
        var lines: [String] = []

        lines.append("---")

        lines.append("")
        lines.append(dateToISO8601(mark.date))

        lines.append("")
        lines.append("#### \(ur)")

        lines.append("")
        lines.append("#### `\(bytewords)`")

        lines.append("")
        lines.append(bytemoji)

        lines.append("")
        if !comment.isEmpty {
            lines.append(comment)
            lines.append("")
        }

        return lines.joined(separator: "\n")
    }
}

// MARK: - Encodable

extension ProvenanceMarkInfo: Encodable {
    private enum CodingKeys: String, CodingKey {
        case ur
        case bytewords
        case bytemoji
        case comment
        case mark
    }

    public func encode(to encoder: any Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)

        // UR is serialized as its string representation.
        try container.encode(ur.urString, forKey: .ur)
        try container.encode(bytewords, forKey: .bytewords)
        try container.encode(bytemoji, forKey: .bytemoji)

        // Comment is skipped when empty.
        if !comment.isEmpty {
            try container.encode(comment, forKey: .comment)
        }

        // Mark is serialized as the full ProvenanceMark JSON.
        try container.encode(mark, forKey: .mark)
    }
}

// MARK: - Decodable

extension ProvenanceMarkInfo: Decodable {
    public init(from decoder: any Decoder) throws {
        let container = try decoder.container(keyedBy: CodingKeys.self)

        let urString = try container.decode(String.self, forKey: .ur)
        let ur = try UR(urString: urString)

        let bytewords = try container.decode(String.self, forKey: .bytewords)
        let bytemoji = try container.decode(String.self, forKey: .bytemoji)
        let comment = try container.decodeIfPresent(String.self, forKey: .comment) ?? ""

        // Reconstruct the mark from the UR (ignoring the serialized `mark`
        // field) to ensure date_bytes and seq_bytes match the original.
        let mark = try ProvenanceMark.fromUR(ur)

        self.ur = ur
        self.bytewords = bytewords
        self.bytemoji = bytemoji
        self.comment = comment
        self.mark = mark
    }
}
