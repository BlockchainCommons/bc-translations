import Foundation
import BCComponents

public extension Envelope {
    static func `false`() -> Envelope {
        Envelope(false)
    }
    
    static func `true`() -> Envelope {
        Envelope(true)
    }
    
    var isFalse: Bool {
        (try? extractSubject(Bool.self)) == false
    }
    
    var isTrue: Bool {
        (try? extractSubject(Bool.self)) == true
    }
    
    var isBool: Bool {
        (try? extractSubject(Bool.self)) != nil
    }
}

public extension Envelope {
    /// Constructs a unit envelope (`''` known value).
    static func unit() -> Envelope {
        Envelope(.unit)
    }
    
    /// `true` if the subject of the envelope is the unit value.
    var isSubjectUnit: Bool {
        (try? extractSubject(KnownValue.self)) == .unit
    }
    
    @discardableResult
    func checkSubjectUnit() throws -> Envelope {
        guard isSubjectUnit else {
            throw EnvelopeError.invalidFormat
        }
        return self
    }
}

public extension Envelope {
    /// Sets a `'position'` assertion on the envelope.
    func setPosition(_ position: Int) throws -> Envelope {
        guard position >= 0 else {
            throw EnvelopeError.invalidFormat
        }
        
        let positionAssertions = assertions(withPredicate: .position)
        guard positionAssertions.count <= 1 else {
            throw EnvelopeError.invalidFormat
        }
        
        let envelopeWithoutPosition: Envelope
        if let positionAssertion = positionAssertions.first {
            envelopeWithoutPosition = removeAssertion(positionAssertion)
        } else {
            envelopeWithoutPosition = self
        }
        
        return envelopeWithoutPosition.addAssertion(.position, position)
    }
    
    /// Returns the value of the `'position'` assertion.
    func position() throws -> Int {
        let positionEnvelope = try object(forPredicate: .position)
        if let position = try? positionEnvelope.extractSubject(Int.self) {
            return position
        }
        if let position = try? positionEnvelope.extractSubject(UInt.self) {
            return Int(position)
        }
        if let position = try? positionEnvelope.extractSubject(UInt64.self) {
            return Int(position)
        }
        throw EnvelopeError.invalidFormat
    }
    
    /// Removes the `'position'` assertion if present.
    func removePosition() throws -> Envelope {
        let positionAssertions = assertions(withPredicate: .position)
        guard positionAssertions.count <= 1 else {
            throw EnvelopeError.invalidFormat
        }
        if let positionAssertion = positionAssertions.first {
            return removeAssertion(positionAssertion)
        } else {
            return self
        }
    }
}
