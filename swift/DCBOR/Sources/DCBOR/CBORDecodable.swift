import Foundation

/// A type that can be decoded from CBOR
public protocol CBORDecodable {
    /// Creates an instance of this type from CBOR symbolic representation.
    init(cbor: CBOR) throws
    
    /// Creates an instance of this type from encoded CBOR binary data.
    init(cborData: Data) throws
}

public extension CBORDecodable {
    init(cborData: Data) throws {
        self = try Self.init(cbor: CBOR(cborData))
    }
    
    init?(cbor: CBOR?) throws {
        guard let cbor else {
            return nil
        }
        try self.init(cbor: cbor)
    }
}
