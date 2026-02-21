import Foundation

extension Array: CBOREncodable where Element: CBOREncodable {
    public var cbor: CBOR {
        .array(self.map { $0.cbor })
    }

    public var cborData: Data {
        var buf = self.count.encodeVarInt(.array)
        for element in self {
            buf += element.cborData
        }
        return buf
    }
}

extension Array: CBORDecodable where Element: CBORDecodable {
    public init(cbor: CBOR) throws {
        switch cbor {
        case .array(let array):
            self = try array.map { try Element(cbor: $0) }
        default:
            throw CBORError.wrongType
        }
    }
}

extension Array: CBORCodable where Element: CBORCodable { }

public extension Array where Element == any CBOREncodable {
    var cbor: CBOR {
        .array(self.map { $0.cbor })
    }
    
    var cborData: Data {
        var buf = self.count.encodeVarInt(.array)
        for element in self {
            buf += element.cborData
        }
        return buf
    }
}
