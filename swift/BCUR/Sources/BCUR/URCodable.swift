import DCBOR

/// A type that can be encoded to a UR.
public protocol UREncodable: CBORTaggedEncodable { }

public extension UREncodable {
    /// Returns the UR representation of this value.
    func ur() -> UR {
        guard let tag = Self.cborTags.first else {
            preconditionFailure("At least one CBOR tag is required.")
        }
        guard let name = tag.name else {
            preconditionFailure(
                "CBOR tag \(tag.value) must have a name. Did you call registerTags()?"
            )
        }
        guard let ur = try? UR(name, untaggedCBOR) else {
            preconditionFailure("Invalid UR type derived from CBOR tag name: \(name)")
        }
        return ur
    }

    /// Returns the UR string representation of this value.
    func urString() -> String {
        ur().string
    }
}

/// A type that can be decoded from a UR.
public protocol URDecodable: CBORTaggedDecodable { }

public extension URDecodable {
    /// Decodes a value from a UR.
    static func fromUR(_ ur: UR) throws -> Self {
        guard let tag = Self.cborTags.first else {
            preconditionFailure("At least one CBOR tag is required.")
        }
        guard let name = tag.name else {
            preconditionFailure(
                "CBOR tag \(tag.value) must have a name. Did you call registerTags()?"
            )
        }

        try ur.checkType(name)
        return try Self(untaggedCBOR: ur.cbor)
    }

    /// Decodes a value from a UR string.
    static func fromURString(_ urString: String) throws -> Self {
        try fromUR(UR.fromURString(urString))
    }
}

/// A type that can be both encoded to and decoded from a UR.
public protocol URCodable: UREncodable, URDecodable { }
