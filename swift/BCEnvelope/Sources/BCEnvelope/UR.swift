import Foundation
import BCComponents

extension Envelope: URCodable { }

public extension Envelope {
    init(ur: UR) throws {
        self = try Self.fromUR(ur)
    }

    init(urString: String) throws {
        self = try Self.fromURString(urString)
    }

    var ur: UR {
        ur()
    }

    var urString: String {
        urString()
    }
}
