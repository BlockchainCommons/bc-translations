import Foundation
import NumberKit

public extension CBOR {
    /// Returns the encoded hexadecimal representation of this CBOR.
    var hex: String {
        cborData.hex
    }

    /// Returns the encoded hexadecimal representation in annotated form.
    var hexAnnotated: String {
        hex(annotate: true, tags: globalTags)
    }
    
    /// Returns the encoded hexadecimal representation of this CBOR.
    ///
    /// - Parameters:
    ///   - annotate: If `true`, add additional notes and context, otherwise just return a
    ///   straight hexadecimal encoding.
    ///   - knownTags: If `annotate` is `true`, uses the name of these tags rather than their number.
    func hex(annotate: Bool = true, tags: TagsStoreProtocol? = nil) -> String {
        guard annotate else {
            return cborData.hex
        }
        let items = dumpItems(level: 0, knownTags: tags)
        let noteColumn = items.reduce(into: 0) { largest, item in
            largest = max(largest, item.formatFirstColumn().count)
        }
        let lines = items.map { $0.format(noteColumn: noteColumn) }
        return lines.joined(separator: "\n")
    }

}

struct DumpItem {
    let level: Int
    let data: [Data]
    let note: String?
    
    init(level: Int, data: [Data], note: String? = nil) {
        self.level = level
        self.data = data
        self.note = note
    }

    init(level: Int, data: Data, note: String? = nil) {
        self.init(level: level, data: [data], note: note)
    }
    
    func format(noteColumn: Int) -> String {
        let column1 = formatFirstColumn()
        let column2: String
        let padding: String
        if let note = note {
            let paddingCount = max(1, min(40, noteColumn) - column1.count + 1)
            padding = String(repeating: " ", count: paddingCount)
            column2 = "# " + note
        } else {
            padding = ""
            column2 = ""
        }
        return column1 + padding + column2
    }
    
    func formatFirstColumn() -> String {
        let indent = String(repeating: " ", count: level * 3)
        let hex = data.map { $0.hex }.filter { !$0.isEmpty }.joined(separator: " ")
        return indent + hex
    }
}

extension CBOR {
    func dumpItems(level: Int, knownTags: TagsStoreProtocol?) -> [DumpItem] {
        switch self {
        case .unsigned(let n):
            return [DumpItem(level: level, data: self.cborData, note: "unsigned(\(n))")]
        case .negative(let n):
            return [DumpItem(level: level, data: self.cborData, note: "negative(\(-1 - BigInt(n)))")]
        case .bytes(let d):
            var items = [
                DumpItem(level: level, data: d.count.encodeVarInt(.bytes), note: "bytes(\(d.count))")
            ]
            if !d.isEmpty {
                let note = d.utf8?.sanitized?.flanked("\"")
                items.append(DumpItem(level: level + 1, data: d, note: note))
            }
            return items
        case .text(let s):
            let header = s.count.encodeVarInt(.text)
            let headerData = [Data(of: header.first!), header.dropFirst()]
            return [
                DumpItem(level: level, data: headerData, note: "text(\(s.utf8Data.count))"),
                DumpItem(level: level + 1, data: s.utf8Data, note: s.flanked("\""))
            ]
        case .simple(let v):
            let data = v.cborData
            let note = v.description
            return [
                DumpItem(level: level, data: data, note: note)
            ]
        case .tagged(let tag, let item):
            let header = tag.value.encodeVarInt(.tagged)
            let headerData = [Data(of: header.first!), header.dropFirst()]
            var noteComponents: [String] = [ "tag(\(tag.value))" ]
            if let name = knownTags?.assignedName(for: tag) {
                noteComponents.append(name)
            }
            let tagNote = noteComponents.joined(separator: " ")
            return [
                [
                    DumpItem(level: level, data: headerData, note: tagNote)
                ],
                item.dumpItems(level: level + 1, knownTags: knownTags)
            ].flatMap { $0 }
        case .array(let array):
            let header = array.count.encodeVarInt(.array)
            let headerData = [Data(of: header.first!), header.dropFirst()]
            return [
                [
                    DumpItem(level: level, data: headerData, note: "array(\(array.count))")
                ],
                array.flatMap { $0.dumpItems(level: level + 1, knownTags: knownTags) }
            ].flatMap { $0 }
        case .map(let m):
            let header = m.count.encodeVarInt(.map)
            let headerData = [Data(of: header.first!), header.dropFirst()]
            return [
                [
                    DumpItem(level: level, data: headerData, note: "map(\(m.count))")
                ],
                m.entries.flatMap {
                    [
                        $0.key.dumpItems(level: level + 1, knownTags: knownTags),
                        $0.value.dumpItems(level: level + 1, knownTags: knownTags)
                    ].flatMap { $0 }
                }
            ].flatMap { $0 }
        }
    }
}

extension Character {
    var isPrintable: Bool {
        !isASCII || (32...126).contains(asciiValue!)
    }
}

extension String {
    var sanitized: String? {
        var hasPrintable = false
        let s = self.map { c -> Character in
            if c.isPrintable {
                hasPrintable = true
                return c
            } else {
                return "."
            }
        }
        return !hasPrintable ? nil : String(s)
    }
}
