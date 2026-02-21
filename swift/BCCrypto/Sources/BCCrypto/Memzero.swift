import Foundation

/// Overwrites the contents of an array with zeros.
///
/// Used to securely erase sensitive cryptographic material from memory.
///
/// - Parameter values: The array whose elements will be zeroed.
public func memzero<T>(_ values: inout [T]) {
    _ = values.withUnsafeMutableBytes { rawBuffer in
        rawBuffer.initializeMemory(as: UInt8.self, repeating: 0)
    }
}

/// Overwrites the contents of a nested array of byte arrays with zeros.
///
/// Applies ``memzero(_:)-5cvck`` to each inner array.
///
/// - Parameter values: The nested array whose inner arrays will be zeroed.
public func memzero(_ values: inout [[UInt8]]) {
    for index in values.indices {
        memzero(&values[index])
    }
}
