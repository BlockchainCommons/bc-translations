import Foundation
import NumberKit

func decodeFixedWidthInteger<T: FixedWidthInteger>(_ cbor: CBOR) throws -> T {
    let result: T
    switch cbor {
    case .unsigned(let n):
        guard let n = T(exactly: n) else {
            throw CBORError.outOfRange
        }
        result = n
    case .negative(let n):
        let b = -1 - BigInt(n)
        guard let n = T(exactly: b) else {
            throw CBORError.outOfRange
        }
        result = n
    default:
        throw CBORError.wrongType
    }
    return result
}

extension UInt8: CBORCodable {
    public var cbor: CBOR {
        .unsigned(UInt64(self))
    }
    
    public var cborData: Data {
        self.encodeVarInt(.unsigned)
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension UInt16: CBORCodable {
    public var cbor: CBOR {
        .unsigned(UInt64(self))
    }
    
    public var cborData: Data {
        self.encodeVarInt(.unsigned)
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension UInt32: CBORCodable {
    public var cbor: CBOR {
        .unsigned(UInt64(self))
    }

    public var cborData: Data {
        self.encodeVarInt(.unsigned)
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension UInt64: CBORCodable {
    public var cbor: CBOR {
        .unsigned(UInt64(self))
    }
    
    public var cborData: Data {
        self.encodeVarInt(.unsigned)
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension UInt: CBORCodable {
    public var cbor: CBOR {
        .unsigned(UInt64(self))
    }
    
    public var cborData: Data {
        self.encodeVarInt(.unsigned)
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension Int8: CBORCodable {
    public var cbor: CBOR {
        if self < 0 {
            let b = Int16(self)
            let a = UInt64(-1 - b)
            return .negative(a)
        } else {
            return .unsigned(UInt64(self))
        }
    }
    
    public var cborData: Data {
        if self < 0 {
            let b = Int16(self)
            let a = UInt8(-1 - b)
            return a.encodeVarInt(.negative)
        } else {
            let a = UInt8(self)
            return a.encodeVarInt(.unsigned)
        }
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension Int16: CBORCodable {
    public var cbor: CBOR {
        if self < 0 {
            let b = Int32(self)
            let a = UInt64(-1 - b)
            return .negative(a)
        } else {
            return .unsigned(UInt64(self))
        }
    }
    
    public var cborData: Data {
        if self < 0 {
            let b = Int32(self)
            let a = UInt16(-1 - b)
            return a.encodeVarInt(.negative)
        } else {
            let a = UInt16(self)
            return a.encodeVarInt(.unsigned)
        }
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension Int32: CBORCodable {
    public var cbor: CBOR {
        if self < 0 {
            let b = Int64(self)
            let a = UInt64(-1 - b)
            return .negative(a)
        } else {
            return .unsigned(UInt64(self))
        }
    }
    
    public var cborData: Data {
        if self < 0 {
            let b = Int64(self)
            let a = UInt32(-b - 1)
            return a.encodeVarInt(.negative)
        } else {
            let a = UInt32(self)
            return a.encodeVarInt(.unsigned)
        }
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension Int64: CBORCodable {
    public var cbor: CBOR {
        if self < 0 {
            if self == Int64.min {
                return .negative(UInt64(Int64.max))
            } else {
                return .negative(UInt64(-1 - self))
            }
        } else {
            return .unsigned(UInt64(self))
        }
    }
    
    public var cborData: Data {
        if self < 0 {
            if self == Int64.min {
                return UInt64(Int64.max).encodeVarInt(.negative)
            } else {
                let a = UInt64(-self - 1)
                return a.encodeVarInt(.negative)
            }
        } else {
            let a = UInt32(self)
            return a.encodeVarInt(.unsigned)
        }
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}

extension Int: CBORCodable {
    public var cbor: CBOR {
        if self < 0 {
            if self == Int.min {
                return .negative(UInt64(Int.max))
            } else {
                return .negative(UInt64(-1 - self))
            }
        } else {
            return .unsigned(UInt64(self))
        }
    }

    public var cborData: Data {
        Int64(self).cborData
    }
    
    public init(cbor: CBOR) throws {
        self = try decodeFixedWidthInteger(cbor)
    }
}
