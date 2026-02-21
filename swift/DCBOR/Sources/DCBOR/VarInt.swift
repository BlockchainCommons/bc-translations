import Foundation

enum MajorType: Int {
    case unsigned
    case negative
    case bytes
    case text
    case array
    case map
    case tagged
    case simple
}

func typeBits(_ t: MajorType) -> UInt8 {
    UInt8(t.rawValue << 5)
}

protocol EncodeVarInt {
    func encodeVarInt(_ majorType: MajorType) -> Data
    func encodeInt(_ majorType: MajorType) -> Data
}

extension UInt8: EncodeVarInt {
    func encodeVarInt(_ majorType: MajorType) -> Data {
        if self <= 23 {
            return Data([self | typeBits(majorType)])
        } else {
            return encodeInt(majorType)
        }
    }
    
    func encodeInt(_ majorType: MajorType) -> Data {
        return Data([
            0x18 | typeBits(majorType),
            self
        ])
    }
}

extension UInt16: EncodeVarInt {
    func encodeVarInt(_ majorType: MajorType) -> Data {
        if self <= UInt8.max {
            return UInt8(self).encodeVarInt(majorType)
        } else {
            return encodeInt(majorType)
        }
    }
    
    func encodeInt(_ majorType: MajorType) -> Data {
        return Data([
            0x19 | typeBits(majorType),
            UInt8(truncatingIfNeeded: self >> 8), UInt8(truncatingIfNeeded: self)
        ])
    }
}

extension UInt32: EncodeVarInt {
    func encodeVarInt(_ majorType: MajorType) -> Data {
        if self <= UInt16.max {
            return UInt16(self).encodeVarInt(majorType)
        } else {
            return encodeInt(majorType)
        }
    }
    
    func encodeInt(_ majorType: MajorType) -> Data {
        return Data([
            0x1a | typeBits(majorType),
            UInt8(truncatingIfNeeded: self >> 24), UInt8(truncatingIfNeeded: self >> 16),
            UInt8(truncatingIfNeeded: self >> 8), UInt8(truncatingIfNeeded: self)
        ])
    }
}

extension UInt64: EncodeVarInt {
    func encodeVarInt(_ majorType: MajorType) -> Data {
        if self <= UInt32.max {
            return UInt32(self).encodeVarInt(majorType)
        } else {
            return encodeInt(majorType)
        }
    }
    
    func encodeInt(_ majorType: MajorType) -> Data {
        return Data([
            0x1b | typeBits(majorType),
            UInt8(truncatingIfNeeded: self >> 56), UInt8(truncatingIfNeeded: self >> 48),
            UInt8(truncatingIfNeeded: self >> 40), UInt8(truncatingIfNeeded: self >> 32),
            UInt8(truncatingIfNeeded: self >> 24), UInt8(truncatingIfNeeded: self >> 16),
            UInt8(truncatingIfNeeded: self >> 8), UInt8(truncatingIfNeeded: self)
        ])
    }
}

extension UInt: EncodeVarInt {
    func encodeVarInt(_ majorType: MajorType) -> Data {
        UInt64(self).encodeVarInt(majorType)
    }
    
    func encodeInt(_ majorType: MajorType) -> Data {
        UInt64(self).encodeInt(majorType)
    }
}


extension Int: EncodeVarInt {
    func encodeVarInt(_ majorType: MajorType) -> Data {
        UInt64(self).encodeVarInt(majorType)
    }
    
    func encodeInt(_ majorType: MajorType) -> Data {
        UInt64(self).encodeInt(majorType)
    }
}
