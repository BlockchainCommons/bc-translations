import Foundation

func toHex(byte: UInt8) -> String {
    String(format: "%02x", byte)
}

func toHex(data: Data) -> String {
    data.reduce(into: "") {
        $0 += toHex(byte: $1)
    }
}

func toUTF8(data: Data) -> String? {
    String(data: data, encoding: .utf8)
}

extension Data {
    var hex: String {
        toHex(data: self)
    }

    var utf8: String? {
        toUTF8(data: self)
    }
}

extension Data {
    init<A>(of a: A) {
        let d = Swift.withUnsafeBytes(of: a) {
            Data($0)
        }
        self = d
    }

    init?(hexString: String) {
        let hex = hexString.trimmingCharacters(in: .whitespacesAndNewlines)
        guard hex.count % 2 == 0 else {
            return nil
        }
        var data = Data(capacity: hex.count / 2)
        var index = hex.startIndex
        while index < hex.endIndex {
            let next = hex.index(index, offsetBy: 2)
            let byte = hex[index..<next]
            guard let value = UInt8(byte, radix: 16) else {
                return nil
            }
            data.append(value)
            index = next
        }
        self = data
    }
}

extension StringProtocol {
    func flanked(_ leading: String, _ trailing: String) -> String {
        leading + self + trailing
    }

    func flanked(_ around: String) -> String {
        around + self + around
    }
}

func toData(utf8: String) -> Data {
    utf8.data(using: .utf8)!
}

extension String {
    var utf8Data: Data {
        toData(utf8: self)
    }
}
