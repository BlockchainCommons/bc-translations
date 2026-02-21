import Foundation

extension Data {
    var hex: String {
        reduce(into: "") { $0 += String(format: "%02x", $1) }
    }

    var utf8: String? {
        String(data: self, encoding: .utf8)
    }

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

extension String {
    var utf8Data: Data {
        data(using: .utf8)!
    }
}
