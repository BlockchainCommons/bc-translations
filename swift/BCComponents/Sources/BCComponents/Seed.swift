import BCUR
import BCTags
import BCRand
import DCBOR
import Foundation

public struct Seed: Equatable, Hashable, Sendable {
    public static let minSeedLength = 16

    private var bytes: Data
    private var nameValue: String
    private var noteValue: String
    private var creationDateValue: Date?

    public init(
        data: some DataProtocol,
        name: String = "",
        note: String = "",
        creationDate: Date? = nil
    ) throws(BCComponentsError) {
        let data = Data(data)
        if data.count < Self.minSeedLength {
            throw .dataTooShort(
                dataType: "seed",
                minimum: Self.minSeedLength,
                actual: data.count
            )
        }

        self.bytes = data
        self.nameValue = name
        self.noteValue = note
        self.creationDateValue = creationDate
    }

    public static func new() -> Seed {
        try! newWithLen(Self.minSeedLength)
    }

    public static func newWithLen(_ count: Int) throws(BCComponentsError) -> Seed {
        var rng = SecureRandomNumberGenerator()
        return try newWithLenUsing(count, rng: &rng)
    }

    public static func newWithLenUsing<G: BCRandomNumberGenerator>(
        _ count: Int,
        rng: inout G
    ) throws(BCComponentsError) -> Seed {
        try Seed(data: rngRandomData(&rng, count: count))
    }

    public static func newOpt(
        _ data: some DataProtocol,
        name: String? = nil,
        note: String? = nil,
        creationDate: Date? = nil
    ) throws(BCComponentsError) -> Seed {
        try Seed(
            data: data,
            name: name ?? "",
            note: note ?? "",
            creationDate: creationDate
        )
    }

    public func asBytes() -> Data {
        bytes
    }

    public func name() -> String {
        nameValue
    }

    public mutating func setName(_ name: String) {
        nameValue = name
    }

    public func note() -> String {
        noteValue
    }

    public mutating func setNote(_ note: String) {
        noteValue = note
    }

    public func creationDate() -> Date? {
        creationDateValue
    }

    public mutating func setCreationDate(_ creationDate: Date?) {
        creationDateValue = creationDate
    }
}

extension Seed: PrivateKeyDataProvider {
    public func privateKeyData() -> Data {
        bytes
    }
}

extension Seed: CBORTaggedEncodable {
    public static var cborTags: [Tag] {
        [Tag.seed, Tag.seedV1]
    }

    public var untaggedCBOR: CBOR {
        var map = Map()
        map.insert(1, bytes)
        if let creationDateValue {
            map.insert(2, creationDateValue)
        }
        if !nameValue.isEmpty {
            map.insert(3, nameValue)
        }
        if !noteValue.isEmpty {
            map.insert(4, noteValue)
        }
        return .map(map)
    }
}

extension Seed: CBORTaggedDecodable {
    public init(untaggedCBOR: CBOR) throws {
        guard case .map(let map) = untaggedCBOR else {
            throw CBORError.wrongType
        }
        guard let dataCBOR = map.get(1) else {
            throw BCComponentsError.invalidData(
                dataType: "seed",
                reason: "missing seed data"
            )
        }

        let data = try byteString(dataCBOR)
        let creationDate = try map.get(2).map { try Date(cbor: $0) }
        let name = try map.get(3).map { try String(cbor: $0) } ?? ""
        let note = try map.get(4).map { try String(cbor: $0) } ?? ""

        try self.init(
            data: data,
            name: name,
            note: note,
            creationDate: creationDate
        )
    }
}

extension Seed: URCodable {}
