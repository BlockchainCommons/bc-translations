import Foundation

public extension CBOR {
    /// Returns the CBOR value in CBOR diagnostic notation.
    ///
    /// - Parameters:
    ///   - annotate: If `true`, add additional notes and context.
    ///   - knownTags: If `annotate` is `true`, uses the name of these tags rather than their number.
    func diagnostic(annotate: Bool = true, tags: TagsStoreProtocol? = globalTags) -> String {
        diagItem(
            annotate: annotate,
            knownTags: tags,
            summarize: false,
            flat: false
        ).format(annotate: annotate)
    }

    /// Returns the CBOR value in a single-line diagnostic form.
    var diagnosticFlat: String {
        diagItem(
            annotate: false,
            knownTags: globalTags,
            summarize: false,
            flat: true
        ).flatString()
    }

    /// Returns the CBOR value in annotated diagnostic notation.
    var diagnosticAnnotated: String {
        diagnostic(annotate: true, tags: globalTags)
    }

    /// Returns a short human-readable summary string.
    var summary: String {
        diagItem(
            annotate: false,
            knownTags: globalTags,
            summarize: true,
            flat: true
        ).flatString()
    }
}

extension Array where Element == String {
    func joined(itemSeparator: String = "", pairSeparator: String? = nil) -> String {
        let pairSeparator = pairSeparator ?? itemSeparator
        var result = ""
        for (index, item) in self.enumerated() {
            result += item
            if index != count - 1 {
                if index & 1 != 0 {
                    result += itemSeparator
                } else {
                    result += pairSeparator
                }
            }
        }
        return result
    }
}

enum DiagItem {
    case item(String)
    case group(begin: String, end: String, items: [DiagItem], isPairs: Bool, comment: String?)
    
    func format(level: Int = 0, separator: String = "", annotate: Bool = false) -> String {
        switch self {
        case .item(let string):
            return formatLine(level: level, string: string, separator: separator)
        case .group:
            if containsGroup || totalStringsLength > 20 || greatestStringsLength > 20 {
                return multilineComposition(level: level, separator: separator, annotate: annotate)
            } else {
                return singleLineComposition(level: level, separator: separator, annotate: annotate)
            }
        }
    }
    
    private func formatLine(level: Int, string: String, separator: String = "", comment: String? = nil) -> String {
        var result = String(repeating: " ", count: level * 3) + string + separator
        if let comment {
            result += "   / \(comment) /"
        }
        return result
    }
    
    func singleLineComposition(level: Int, separator: String, annotate: Bool) -> String {
        let string: String
        let comment: String?
        switch self {
        case .item(let s):
            string = s
            comment = nil
        case .group(let begin, let end, let items, let isPairs, let comm):
            let components = items.map { item -> String in
                switch item {
                case .item(let string):
                    return string
                case .group:
                    return "<group>"
                }
            }
            let pairSeparator = isPairs ? ": " : ", "
            string = components.joined(itemSeparator: ", ", pairSeparator: pairSeparator).flanked(begin, end)
            comment = comm
        }
        return formatLine(level: level, string: string, separator: separator, comment: comment)
    }
    
    func multilineComposition(level: Int, separator: String, annotate: Bool) -> String {
        switch self {
        case .item(let string):
            return string
        case .group(let begin, let end, let items, let isPairs, let comment):
            var lines: [String] = []
            lines.append(formatLine(level: level, string: begin, comment: comment))
            for (index, item) in items.enumerated() {
                let separator = index == items.count - 1 ? "" : (isPairs && index & 1 == 0 ? ":" : ",")
                lines.append(item.format(level: level + 1, separator: separator, annotate: annotate))
            }
            lines.append(formatLine(level: level, string: end, separator: separator))
            return lines.joined(separator: "\n")
        }
    }
    
    var totalStringsLength: Int {
        switch self {
        case .item(let string):
            return string.count
        case .group(_, _, let items, _, _):
            return items.reduce(into: 0) { result, item in
                result += item.totalStringsLength
            }
        }
    }
    
    var greatestStringsLength: Int {
        switch self {
        case .item(let string):
            return string.count
        case .group(_, _, let items, _, _):
            return items.reduce(into: 0) { result, item in
                result = max(result, item.totalStringsLength)
            }
        }
    }
    
    var isGroup: Bool {
        if case .group = self {
            return true
        } else {
            return false
        }
    }
    
    var containsGroup: Bool {
        switch self {
        case .item:
            return false
        case .group(_, _, let items, _, _):
            return items.first { $0.isGroup } != nil
        }
    }

    func flatString() -> String {
        switch self {
        case .item(let string):
            return string
        case .group(let begin, let end, let items, let isPairs, _):
            let pairSeparator = isPairs ? ": " : ", "
            let components = items.map { $0.flatString() }
            return components
                .joined(itemSeparator: ", ", pairSeparator: pairSeparator)
                .flanked(begin, end)
        }
    }
}

extension CBOR {
    func diagItem(
        annotate: Bool = false,
        knownTags: TagsStoreProtocol?,
        summarize: Bool = false,
        flat: Bool = false
    ) -> DiagItem {
        switch self {
        case .unsigned, .negative, .bytes, .text, .simple:
            return .item(description)
        case .tagged(let tag, let item):
            if summarize, let summarizer = knownTags?.summarizer(for: tag) {
                do {
                    return .item(try summarizer(item, flat))
                } catch {
                    return .item("<error: \(error)>")
                }
            }

            let diagItem: DiagItem
            if annotate && tag == 1 {
                if let n = try? Double(cbor: item) {
                    diagItem = .item(ISO8601DateFormatter().string(from: Date(timeIntervalSince1970: TimeInterval(n))))
                } else {
                    diagItem = item.diagItem(
                        annotate: annotate,
                        knownTags: knownTags,
                        summarize: summarize,
                        flat: flat
                    )
                }
            } else {
                diagItem = item.diagItem(
                    annotate: annotate,
                    knownTags: knownTags,
                    summarize: summarize,
                    flat: flat
                )
            }
            return .group(
                begin: String(tag.value) + "(",
                end: ")",
                items: [diagItem],
                isPairs: false,
                comment: knownTags?.assignedName(for: tag)
            )
        case .array(let a):
            return .group(
                begin: "[",
                end: "]",
                items: a.map {
                    $0.diagItem(
                        annotate: annotate,
                        knownTags: knownTags,
                        summarize: summarize,
                        flat: flat
                    )
                },
                isPairs: false,
                comment: nil
            )
        case .map(let m):
            return .group(
                begin: "{",
                end: "}",
                items: m.map { (key, value) in [
                    key.diagItem(
                        annotate: annotate,
                        knownTags: knownTags,
                        summarize: summarize,
                        flat: flat
                    ),
                    value.diagItem(
                        annotate: annotate,
                        knownTags: knownTags,
                        summarize: summarize,
                        flat: flat
                    )
                ]}.flatMap { $0 },
                isPairs: true,
                comment: nil
            )
        }
    }
}
