import Foundation

let longMessage = Data(
    "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it."
        .utf8
)

extension Data {
    init(testHex hex: String) {
        self.init()
        self.reserveCapacity(hex.count / 2)

        var index = hex.startIndex
        while index < hex.endIndex {
            let next = hex.index(index, offsetBy: 2)
            let byte = UInt8(hex[index..<next], radix: 16)!
            self.append(byte)
            index = next
        }
    }
}
