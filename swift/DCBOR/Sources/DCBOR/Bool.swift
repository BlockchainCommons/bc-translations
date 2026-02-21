import Foundation

extension Bool: CBORCodable {
    public static let cborFalse = CBOR.simple(.false)
    public static let cborTrue = CBOR.simple(.true)
    public static let cborFalseEncoded = 20.encodeVarInt(.simple)
    public static let cborTrueEncoded = 21.encodeVarInt(.simple)
    
    public var cbor: CBOR {
        self ? Self.cborTrue : Self.cborFalse
    }
    
    public var cborData: Data {
        self ? Self.cborTrueEncoded : Self.cborFalseEncoded
    }
    
    public init(cbor: CBOR) throws {
        switch cbor {
        case Self.cborFalse:
            self = false
        case Self.cborTrue:
            self = true
        default:
            throw CBORError.wrongType
        }
    }
}
