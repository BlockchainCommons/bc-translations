import Foundation
import DCBOR
@testable import BCUR

func hexToBytes(_ hex: String) -> [UInt8] {
    var bytes: [UInt8] = []
    bytes.reserveCapacity(hex.count / 2)

    var index = hex.startIndex
    while index < hex.endIndex {
        let next = hex.index(index, offsetBy: 2)
        let byte = UInt8(hex[index..<next], radix: 16)!
        bytes.append(byte)
        index = next
    }

    return bytes
}

func bytesToHex(_ bytes: [UInt8]) -> String {
    bytes.map { String(format: "%02x", $0) }.joined()
}

func makeMessageUR(length: Int, seed: String) -> [UInt8] {
    let message = Xoshiro256.makeMessage(seed: seed, size: length)
    return Array(Data(message).cborData)
}
