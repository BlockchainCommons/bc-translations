import Foundation

public func memzero<T>(_ values: inout [T]) {
    values.withUnsafeMutableBytes { rawBuffer in
        rawBuffer.initializeMemory(as: UInt8.self, repeating: 0)
    }
}

public func memzeroVecVecU8(_ values: inout [[UInt8]]) {
    for index in values.indices {
        memzero(&values[index])
    }
}
