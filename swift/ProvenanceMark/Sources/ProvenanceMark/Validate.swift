import Foundation
import BCUR

// MARK: - ValidationReportFormat

/// Output format for a ``ValidationReport``.
public enum ValidationReportFormat: Sendable {
    /// Human-readable text format.
    case text
    /// Compact JSON (no whitespace).
    case jsonCompact
    /// Pretty-printed JSON (with indentation).
    case jsonPretty
}

// MARK: - ValidationIssue

/// An issue detected while validating a sequence of provenance marks.
public enum ValidationIssue: Error, Sendable, Equatable {
    /// The hash of a mark does not match the expected value computed from the
    /// next mark's revealed key.
    case hashMismatch(expected: [UInt8], actual: [UInt8])

    /// The current mark's hash was not generated from the next mark's key.
    case keyMismatch

    /// A gap in the sequence numbers between consecutive marks.
    case sequenceGap(expected: UInt32, actual: UInt32)

    /// The next mark has a date earlier than the previous mark.
    case dateOrdering(previous: Date, next: Date)

    /// A non-genesis mark was found at sequence number zero.
    case nonGenesisAtZero

    /// A genesis mark has a key that does not equal its chain ID.
    case invalidGenesisKey
}

// MARK: CustomStringConvertible

extension ValidationIssue: CustomStringConvertible {
    public var description: String {
        switch self {
        case .hashMismatch(let expected, let actual):
            return "hash mismatch: expected \(expected.hex), got \(actual.hex)"
        case .keyMismatch:
            return "key mismatch: current hash was not generated from next key"
        case .sequenceGap(let expected, let actual):
            return "sequence number gap: expected \(expected), got \(actual)"
        case .dateOrdering(let previous, let next):
            return "date must be equal or later: previous is \(dateToISO8601(previous)), next is \(dateToISO8601(next))"
        case .nonGenesisAtZero:
            return "non-genesis mark at sequence 0"
        case .invalidGenesisKey:
            return "genesis mark must have key equal to chain ID"
        }
    }
}

// MARK: Encodable

extension ValidationIssue: Encodable {
    private enum CodingKeys: String, CodingKey {
        case type
        case data
    }

    public func encode(to encoder: any Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)

        switch self {
        case .hashMismatch(let expected, let actual):
            try container.encode("HashMismatch", forKey: .type)
            try container.encode(
                ["expected": expected.hex, "actual": actual.hex],
                forKey: .data
            )

        case .keyMismatch:
            try container.encode("KeyMismatch", forKey: .type)

        case .sequenceGap(let expected, let actual):
            try container.encode("SequenceGap", forKey: .type)
            try container.encode(
                ["expected": expected, "actual": actual],
                forKey: .data
            )

        case .dateOrdering(let previous, let next):
            try container.encode("DateOrdering", forKey: .type)
            try container.encode(
                ["previous": dateToISO8601(previous), "next": dateToISO8601(next)],
                forKey: .data
            )

        case .nonGenesisAtZero:
            try container.encode("NonGenesisAtZero", forKey: .type)

        case .invalidGenesisKey:
            try container.encode("InvalidGenesisKey", forKey: .type)
        }
    }
}

// MARK: - FlaggedMark

/// A provenance mark annotated with any validation issues found during
/// sequence checking.
public struct FlaggedMark: Sendable {
    /// The provenance mark.
    public let mark: ProvenanceMark

    /// Validation issues associated with this mark.
    public private(set) var issues: [ValidationIssue]

    /// Creates a flagged mark with no issues.
    public init(mark: ProvenanceMark) {
        self.mark = mark
        self.issues = []
    }

    /// Creates a flagged mark with a single initial issue.
    public init(mark: ProvenanceMark, issue: ValidationIssue) {
        self.mark = mark
        self.issues = [issue]
    }
}

// MARK: Encodable

extension FlaggedMark: Encodable {
    private enum CodingKeys: String, CodingKey {
        case mark
        case issues
    }

    public func encode(to encoder: any Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)
        try container.encode(mark.urString(), forKey: .mark)
        try container.encode(issues, forKey: .issues)
    }
}

// MARK: - SequenceReport

/// A report for a contiguous run of marks within a single chain.
public struct SequenceReport: Sendable {
    /// The sequence number of the first mark in this run.
    public let startSeq: UInt32

    /// The sequence number of the last mark in this run.
    public let endSeq: UInt32

    /// The flagged marks in this run, in sequence order.
    public let marks: [FlaggedMark]
}

// MARK: Encodable

extension SequenceReport: Encodable {
    private enum CodingKeys: String, CodingKey {
        case startSeq = "start_seq"
        case endSeq = "end_seq"
        case marks
    }

    public func encode(to encoder: any Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)
        try container.encode(startSeq, forKey: .startSeq)
        try container.encode(endSeq, forKey: .endSeq)
        try container.encode(marks, forKey: .marks)
    }
}

// MARK: - ChainReport

/// A report for all marks sharing the same chain ID.
public struct ChainReport: Sendable {
    /// The chain ID (raw bytes).
    public let chainId: [UInt8]

    /// Whether a valid genesis mark was found at sequence zero.
    public let hasGenesis: Bool

    /// All marks in this chain, sorted by sequence number.
    public let marks: [ProvenanceMark]

    /// Contiguous sequence runs within this chain.
    public let sequences: [SequenceReport]

    /// The chain ID as a lowercase hex string.
    public var chainIdHex: String {
        chainId.hex
    }
}

// MARK: Encodable

extension ChainReport: Encodable {
    private enum CodingKeys: String, CodingKey {
        case chainId = "chain_id"
        case hasGenesis = "has_genesis"
        case marks
        case sequences
    }

    public func encode(to encoder: any Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)
        try container.encode(chainId.hex, forKey: .chainId)
        try container.encode(hasGenesis, forKey: .hasGenesis)
        // Marks are serialized as UR strings.
        try container.encode(marks.map { $0.urString() }, forKey: .marks)
        try container.encode(sequences, forKey: .sequences)
    }
}

// MARK: - ValidationReport

/// A complete validation report for a collection of provenance marks.
///
/// The report organizes marks by chain ID, detects genesis marks, identifies
/// contiguous sequences, and flags any validation issues found between
/// consecutive marks.
public struct ValidationReport: Sendable {
    /// All deduplicated marks that were validated.
    public let marks: [ProvenanceMark]

    /// Per-chain reports, sorted by chain ID.
    public let chains: [ChainReport]

    /// Whether the report contains any validation issues.
    ///
    /// Returns `true` if there are missing genesis marks, validation issues on
    /// individual marks, multiple chains, or multiple sequences within a chain.
    public var hasIssues: Bool {
        // Missing genesis is an issue.
        if chains.contains(where: { !$0.hasGenesis }) {
            return true
        }

        // Flagged marks with issues.
        if chains.contains(where: { chain in
            chain.sequences.contains(where: { seq in
                seq.marks.contains(where: { !$0.issues.isEmpty })
            })
        }) {
            return true
        }

        // Multiple chains or multiple sequences.
        if chains.count > 1 {
            return true
        }
        if chains.count == 1 && chains[0].sequences.count > 1 {
            return true
        }

        return false
    }

    /// Formats the report in the requested format.
    ///
    /// - Parameter reportFormat: The desired output format.
    /// - Returns: The formatted report string.
    public func format(_ reportFormat: ValidationReportFormat) -> String {
        switch reportFormat {
        case .text:
            return formatText()
        case .jsonCompact:
            return formatJSON(pretty: false)
        case .jsonPretty:
            return formatJSON(pretty: true)
        }
    }

    /// Validates a collection of provenance marks and returns a report.
    ///
    /// The algorithm:
    /// 1. Deduplicates exact duplicates.
    /// 2. Bins marks by chain ID.
    /// 3. Sorts each chain by sequence number.
    /// 4. Detects genesis marks.
    /// 5. Builds contiguous sequence runs, flagging issues at break points.
    /// 6. Sorts chains by chain ID for deterministic output.
    ///
    /// - Parameter marks: The marks to validate.
    /// - Returns: A ``ValidationReport`` describing the results.
    public static func validate(_ marks: [ProvenanceMark]) -> ValidationReport {
        // 1. Deduplicate
        var seen = Swift.Set<ProvenanceMark>()
        var deduplicated: [ProvenanceMark] = []
        for mark in marks {
            if seen.insert(mark).inserted {
                deduplicated.append(mark)
            }
        }

        // 2. Bin by chain ID (Data is Hashable; [UInt8] is not)
        var chainBins: [Data: [ProvenanceMark]] = [:]
        for mark in deduplicated {
            chainBins[Data(mark.chainId), default: []].append(mark)
        }

        // 3. Process each chain
        var chains: [ChainReport] = []
        for (chainIdData, var chainMarks) in chainBins {
            let chainIdBytes = [UInt8](chainIdData)
            // Sort by sequence number
            chainMarks.sort { $0.seq < $1.seq }

            // 4. Check for genesis
            let hasGenesis: Bool
            if let first = chainMarks.first {
                hasGenesis = first.seq == 0 && first.isGenesis
            } else {
                hasGenesis = false
            }

            // 5. Build sequence runs
            let sequences = buildSequenceBins(chainMarks)

            chains.append(ChainReport(
                chainId: chainIdBytes,
                hasGenesis: hasGenesis,
                marks: chainMarks,
                sequences: sequences
            ))
        }

        // 6. Sort chains by chain ID
        chains.sort { $0.chainId.lexicographicallyPrecedes($1.chainId) }

        return ValidationReport(marks: deduplicated, chains: chains)
    }
}

// MARK: - Private helpers

private extension ValidationReport {
    /// Whether the report contains "interesting" information worth displaying
    /// in the text format. A single perfect chain with no issues is not
    /// interesting.
    var isInteresting: Bool {
        if chains.isEmpty {
            return false
        }

        // Missing genesis is always interesting.
        for chain in chains {
            if !chain.hasGenesis {
                return true
            }
        }

        // Single chain with a single perfect sequence is not interesting.
        if chains.count == 1 {
            let chain = chains[0]
            if chain.sequences.count == 1 {
                let seq = chain.sequences[0]
                if seq.marks.allSatisfy({ $0.issues.isEmpty }) {
                    return false
                }
            }
        }

        return true
    }

    func formatText() -> String {
        if !isInteresting {
            return ""
        }

        var lines: [String] = []

        // Summary header
        lines.append("Total marks: \(marks.count)")
        lines.append("Chains: \(chains.count)")
        lines.append("")

        // Per-chain details
        for (chainIdx, chain) in chains.enumerated() {
            let chainIdHex = chain.chainIdHex
            let shortChainId = chainIdHex.count > 8
                ? String(chainIdHex.prefix(8))
                : chainIdHex

            lines.append("Chain \(chainIdx + 1): \(shortChainId)")

            if !chain.hasGenesis {
                lines.append("  Warning: No genesis mark found")
            }

            for seq in chain.sequences {
                for flagged in seq.marks {
                    let shortId = flagged.mark.identifier
                    let seqNum = flagged.mark.seq

                    var annotations: [String] = []

                    if flagged.mark.isGenesis {
                        annotations.append("genesis mark")
                    }

                    for issue in flagged.issues {
                        let annotation: String
                        switch issue {
                        case .sequenceGap(let expected, _):
                            annotation = "gap: \(expected) missing"
                        case .dateOrdering(let previous, let next):
                            annotation = "date \(dateToISO8601(previous)) < \(dateToISO8601(next))"
                        case .hashMismatch:
                            annotation = "hash mismatch"
                        case .keyMismatch:
                            annotation = "key mismatch"
                        case .nonGenesisAtZero:
                            annotation = "non-genesis at seq 0"
                        case .invalidGenesisKey:
                            annotation = "invalid genesis key"
                        }
                        annotations.append(annotation)
                    }

                    if annotations.isEmpty {
                        lines.append("  \(seqNum): \(shortId)")
                    } else {
                        lines.append("  \(seqNum): \(shortId) (\(annotations.joined(separator: ", ")))")
                    }
                }
            }

            lines.append("")
        }

        // Trim trailing whitespace (matches Rust behavior).
        var result = lines.joined(separator: "\n")
        while result.hasSuffix("\n") || result.hasSuffix(" ") {
            result.removeLast()
        }
        return result
    }

    func formatJSON(pretty: Bool) -> String {
        let encoder = JSONEncoder()
        encoder.outputFormatting = pretty
            ? [.sortedKeys, .prettyPrinted, .withoutEscapingSlashes]
            : [.sortedKeys, .withoutEscapingSlashes]
        guard let data = try? encoder.encode(self) else {
            return ""
        }
        return String(data: data, encoding: .utf8) ?? ""
    }

    static func buildSequenceBins(_ marks: [ProvenanceMark]) -> [SequenceReport] {
        var sequences: [SequenceReport] = []
        var currentSequence: [FlaggedMark] = []

        for (i, mark) in marks.enumerated() {
            if i == 0 {
                currentSequence.append(FlaggedMark(mark: mark))
            } else {
                let prev = marks[i - 1]

                do {
                    try prev.validatePrecedes(mark)
                    // Continues the current sequence.
                    currentSequence.append(FlaggedMark(mark: mark))
                } catch {
                    // Break: save the current sequence and start a new one.
                    if !currentSequence.isEmpty {
                        sequences.append(createSequenceReport(currentSequence))
                    }
                    let issue: ValidationIssue
                    if case .validation(let vi) = error {
                        issue = vi
                    } else {
                        issue = .keyMismatch
                    }
                    currentSequence = [FlaggedMark(mark: mark, issue: issue)]
                }
            }
        }

        if !currentSequence.isEmpty {
            sequences.append(createSequenceReport(currentSequence))
        }

        return sequences
    }

    static func createSequenceReport(_ marks: [FlaggedMark]) -> SequenceReport {
        let startSeq = marks.first?.mark.seq ?? 0
        let endSeq = marks.last?.mark.seq ?? 0
        return SequenceReport(startSeq: startSeq, endSeq: endSeq, marks: marks)
    }
}

// MARK: - Encodable

extension ValidationReport: Encodable {
    private enum CodingKeys: String, CodingKey {
        case marks
        case chains
    }

    public func encode(to encoder: any Encoder) throws {
        var container = encoder.container(keyedBy: CodingKeys.self)
        // Marks are serialized as UR strings.
        try container.encode(marks.map { $0.urString() }, forKey: .marks)
        try container.encode(chains, forKey: .chains)
    }
}

// MARK: - ProvenanceMark convenience

public extension ProvenanceMark {
    /// Validates a collection of provenance marks.
    ///
    /// This is a convenience wrapper around ``ValidationReport/validate(_:)``.
    ///
    /// - Parameter marks: The marks to validate.
    /// - Returns: A ``ValidationReport`` describing the results.
    static func validate(_ marks: [ProvenanceMark]) -> ValidationReport {
        ValidationReport.validate(marks)
    }
}
