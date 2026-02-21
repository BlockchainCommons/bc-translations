import Foundation
import BCRand

@inline(__always)
func requireLength(_ data: Data, expected: Int, name: String) {
    precondition(
        data.count == expected,
        "\(name) must be \(expected) bytes, got \(data.count)"
    )
}

extension Data {
    init(hex: String) {
        self.init()
        self.reserveCapacity(hex.count / 2)

        var index = hex.startIndex
        while index < hex.endIndex {
            let nextIndex = hex.index(index, offsetBy: 2)
            let byteString = hex[index..<nextIndex]
            let byte = UInt8(byteString, radix: 16)!
            self.append(byte)
            index = nextIndex
        }
    }

    var hexString: String {
        map { String(format: "%02x", $0) }.joined()
    }
}
