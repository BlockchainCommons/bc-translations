import Foundation

/// A type that can be encoded as CBOR.
///
/// ## Conforming Native Types
///
/// In addition to types defined in this package like ``Map``, and ``Tagged``, the following
/// native types also conform to ``CBOREncodable``:
///
/// * `Array where Element: CBOREncodable`
/// * `Bool`
/// * `Data`
/// * `Date`
/// * `Int`
/// * `Int8`
/// * `Int16`
/// * `Int32`
/// * `Int64`
/// * `String`
/// * `UInt`
/// * `UInt8`
/// * `UInt16`
/// * `UInt32`
/// * `UInt64`
public protocol CBOREncodable {
    /// Returns the value in CBOR symbolic representation.
    var cbor: CBOR { get }

    /// Returns the value in CBOR binary representation.
    var cborData: Data { get }
}

public extension CBOREncodable {
    var cborData: Data {
        cbor.cborData
    }
}
