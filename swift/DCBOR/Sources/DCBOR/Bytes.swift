import Foundation

extension Data: CBORCodable {
    public var cbor: CBOR {
        .bytes(self)
    }
    
    public var cborData: Data {
        count.encodeVarInt(.bytes) + self
    }
    
    public init(cbor: CBOR) throws {
        switch cbor {
        case .bytes(let data):
            self = data
        default:
            throw CBORError.wrongType
        }
    }
}
