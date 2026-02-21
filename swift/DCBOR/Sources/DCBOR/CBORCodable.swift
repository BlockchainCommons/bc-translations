import Foundation

/// A type that can be encoded to or decoded from CBOR.
public protocol CBORCodable: CBOREncodable & CBORDecodable { }
