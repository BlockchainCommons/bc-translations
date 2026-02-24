import Foundation
import BCComponents

// MARK: - High-Level Elision Functions

// An action which obscures (elides, encrypts, or compresses) an envelope.
public enum ObscureAction {
    case elide
    case encrypt(SymmetricKey)
    case compress
}

// Identifies obscuration state when filtering envelope elements.
public enum ObscureType {
    case elided
    case encrypted
    case compressed
}

public extension Envelope {
    /// Returns the elided variant of this envelope.
    ///
    /// Returns the same envelope if it is already elided.
    func elide() -> Envelope {
        switch self {
        case .elided:
            return self
        default:
            return Envelope(elided: self.digest)
        }
    }
}

public extension Envelope {
    /// Returns a version of this envelope with elements in the `target` set elided.
    ///
    /// - Parameters:
    ///   - target: The target set of digests.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elideRemoving(_ target: Swift.Set<Digest>, action: ObscureAction = .elide) -> Envelope {
        elide(target, isRevealing: false, action: action)
    }
    
    /// Returns a version of this envelope with elements in the `target` set elided.
    ///
    /// - Parameters:
    ///   - target: An array of `DigestProvider`s.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elideRemoving(_ target: [DigestProvider], action: ObscureAction = .elide) -> Envelope {
        elide(target, isRevealing: false, action: action)
    }
    
    /// Returns a version of this envelope with the target element elided.
    ///
    /// - Parameters:
    ///   - target: A `DigestProvider`.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elideRemoving(_ target: DigestProvider, action: ObscureAction = .elide) -> Envelope {
        elide(target, isRevealing: false, action: action)
    }
    
    /// Returns a version of this envelope with elements *not* in the `target` set elided.
    ///
    /// - Parameters:
    ///   - target: The target set of digests.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elideRevealing(_ target: Swift.Set<Digest>, action: ObscureAction = .elide) -> Envelope {
        elide(target, isRevealing: true, action: action)
    }
    
    /// Returns a version of this envelope with elements *not* in the `target` set elided.
    ///
    /// - Parameters:
    ///   - target: An array of `DigestProvider`s.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elideRevealing(_ target: [DigestProvider], action: ObscureAction = .elide) -> Envelope {
        elide(target, isRevealing: true, action: action)
    }
    
    /// Returns a version of this envelope with all elements *except* the target element elided.
    ///
    /// - Parameters:
    ///   - target: A `DigestProvider`.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elideRevealing(_ target: DigestProvider, action: ObscureAction = .elide) -> Envelope {
        elide(target, isRevealing: true, action: action)
    }
}

// MARK: - Utility Elision Functions

public extension Envelope {
    // Target Matches   isRevealing     elide
    // ----------------------------------------
    //     false           false        false
    //     false           true         true
    //     true            false        true
    //     true            true         false

    /// Returns an elided version of this envelope.
    ///
    /// - Parameters:
    ///   - target: The target set of digests.
    ///   - isRevealing: If `true`, the target set contains the digests of the elements to
    ///   leave revealed. If it is `false`, the target set contains the digests of the
    ///   elements to elide.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elide(_ target: Swift.Set<Digest>, isRevealing: Bool, action: ObscureAction = .elide) -> Envelope {
        let result: Envelope
        if target.contains(digest) != isRevealing {
            switch action {
            case .elide:
                result = elide()
            case .encrypt(let key):
                let message = key.encryptWithDigest(self.taggedCBOR.cborData, digest: self.digest)
                result = try! Envelope(encryptedMessage: message)
            case .compress:
                result = try! compress()
            }
        } else if case .assertion(let assertion) = self {
            let predicate = assertion.predicate.elide(target, isRevealing: isRevealing, action: action)
            let object = assertion.object.elide(target, isRevealing: isRevealing, action: action)
            let elidedAssertion = Assertion(predicate: predicate, object: object)
            assert(elidedAssertion == assertion)
            result = Envelope(assertion: elidedAssertion)
        } else if case .node(let subject, let assertions, _) = self {
            let elidedSubject = subject.elide(target, isRevealing: isRevealing, action: action)
            assert(elidedSubject.digest == subject.digest)
            let elidedAssertions = assertions.map { assertion in
                let elidedAssertion = assertion.elide(target, isRevealing: isRevealing, action: action)
                assert(elidedAssertion.digest == assertion.digest)
                return elidedAssertion
            }
            result = Envelope(subject: elidedSubject, uncheckedAssertions: elidedAssertions)
        } else if case .wrapped(let envelope, _) = self {
            let elidedEnvelope = envelope.elide(target, isRevealing: isRevealing, action: action)
            assert(elidedEnvelope.digest == envelope.digest)
            result = Envelope(wrapped: elidedEnvelope)
        } else {
            result = self
        }
        assert(result.digest == digest)
        return result
    }
    
    /// Returns an elided version of this envelope.
    ///
    /// - Parameters:
    ///   - target: An array of `DigestProvider`s.
    ///   - isRevealing: If `true`, the target set contains the digests of the elements to
    ///   leave revealed. If it is `false`, the target set contains the digests of the
    ///   elements to elide.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elide(_ target: [DigestProvider], isRevealing: Bool, action: ObscureAction = .elide) -> Envelope {
        elide(Swift.Set(target.map { $0.digest }), isRevealing: isRevealing, action: action)
    }
    
    /// Returns an elided version of this envelope.
    ///
    /// - Parameters:
    ///   - target: A `DigestProvider`.
    ///   - isRevealing: If `true`, the target is the element to leave revealed, eliding
    ///   all others. If it is `false`, the target is the element to elide, leaving all
    ///   others revealed.
    ///   - action: If provided, perform the specified action (encryption or compression) instead of elision.
    ///
    /// - Returns: The elided envelope.
    func elide(_ target: DigestProvider, isRevealing: Bool, action: ObscureAction = .elide) -> Envelope {
        elide([target], isRevealing: isRevealing, action: action)
    }
}

// MARK: - Uneliding an Envelope

public extension Envelope {
    /// Returns the unelided variant of this envelope.
    ///
    /// Throws an exception if the digest of the unelided version does not match.
    func unelide(_ envelope: Envelope) throws -> Envelope {
        guard digest == envelope.digest else {
            throw EnvelopeError.invalidDigest
        }
        return envelope
    }
}

// MARK: - Walk-Based Obscuration Utilities

public extension Envelope {
    /// Returns the set of digests of nodes matching the specified criteria.
    ///
    /// - Parameters:
    ///   - targetDigests: Optional set of digests to filter by. If `nil`, all nodes are considered.
    ///   - obscureTypes: Obscuration types to match. If empty, all matching target nodes are returned.
    func nodesMatching(_ targetDigests: Swift.Set<Digest>? = nil, _ obscureTypes: [ObscureType] = []) -> Swift.Set<Digest> {
        var result: Swift.Set<Digest> = []
        collectNodesMatching(into: &result, targetDigests: targetDigests, obscureTypes: obscureTypes)
        return result
    }
    
    /// Returns this envelope with elided nodes restored from the provided envelopes.
    func walkUnelide(_ envelopes: [Envelope]) -> Envelope {
        var envelopeMap: [Digest: Envelope] = [:]
        for envelope in envelopes {
            envelopeMap[envelope.digest] = envelope
        }
        return walkUnelide(with: envelopeMap)
    }
    
    /// Returns this envelope with nodes in `target` replaced by `replacement`.
    ///
    /// - Throws: `EnvelopeError.invalidFormat` if replacement would produce an invalid assertions array.
    func walkReplace(_ target: Swift.Set<Digest>, with replacement: Envelope) throws -> Envelope {
        if target.contains(digest) {
            return replacement
        }
        
        switch self {
        case .node(let subject, let assertions, _):
            let newSubject = try subject.walkReplace(target, with: replacement)
            let newAssertions = try assertions.map { try $0.walkReplace(target, with: replacement) }
            let subjectUnchanged = newSubject.isIdentical(to: subject)
            let assertionsUnchanged = zip(newAssertions, assertions).allSatisfy { $0.isIdentical(to: $1) }
            if subjectUnchanged && assertionsUnchanged {
                return self
            }
            return try Envelope(subject: newSubject, assertions: newAssertions)
        case .wrapped(let envelope, _):
            let newEnvelope = try envelope.walkReplace(target, with: replacement)
            if newEnvelope.isIdentical(to: envelope) {
                return self
            }
            return Envelope(wrapped: newEnvelope)
        case .assertion(let assertion):
            let newPredicate = try assertion.predicate.walkReplace(target, with: replacement)
            let newObject = try assertion.object.walkReplace(target, with: replacement)
            if newPredicate.isIdentical(to: assertion.predicate) && newObject.isIdentical(to: assertion.object) {
                return self
            }
            return Envelope(assertion: Assertion(predicate: newPredicate, object: newObject))
        default:
            return self
        }
    }
    
    /// Returns this envelope with decryptable encrypted nodes decrypted using `keys`.
    func walkDecrypt(_ keys: [SymmetricKey]) -> Envelope {
        switch self {
        case .encrypted:
            for key in keys {
                if let decrypted = try? decryptSubject(with: key) {
                    return decrypted.walkDecrypt(keys)
                }
            }
            return self
        case .node(let subject, let assertions, _):
            let newSubject = subject.walkDecrypt(keys)
            let newAssertions = assertions.map { $0.walkDecrypt(keys) }
            let subjectUnchanged = newSubject.isIdentical(to: subject)
            let assertionsUnchanged = zip(newAssertions, assertions).allSatisfy { $0.isIdentical(to: $1) }
            if subjectUnchanged && assertionsUnchanged {
                return self
            }
            return Envelope(subject: newSubject, uncheckedAssertions: newAssertions)
        case .wrapped(let envelope, _):
            let newEnvelope = envelope.walkDecrypt(keys)
            if newEnvelope.isIdentical(to: envelope) {
                return self
            }
            return Envelope(wrapped: newEnvelope)
        case .assertion(let assertion):
            let newPredicate = assertion.predicate.walkDecrypt(keys)
            let newObject = assertion.object.walkDecrypt(keys)
            if newPredicate.isIdentical(to: assertion.predicate) && newObject.isIdentical(to: assertion.object) {
                return self
            }
            return Envelope(assertion: Assertion(predicate: newPredicate, object: newObject))
        default:
            return self
        }
    }
    
    /// Returns this envelope with compressed nodes decompressed.
    ///
    /// - Parameter targetDigests: Optional set of digests to filter by. If `nil`, all compressed nodes are considered.
    func walkDecompress(_ targetDigests: Swift.Set<Digest>? = nil) -> Envelope {
        switch self {
        case .compressed:
            let matchesTarget = targetDigests?.contains(digest) ?? true
            if matchesTarget, let decompressed = try? uncompress() {
                return decompressed.walkDecompress(targetDigests)
            }
            return self
        case .node(let subject, let assertions, _):
            let newSubject = subject.walkDecompress(targetDigests)
            let newAssertions = assertions.map { $0.walkDecompress(targetDigests) }
            let subjectUnchanged = newSubject.isIdentical(to: subject)
            let assertionsUnchanged = zip(newAssertions, assertions).allSatisfy { $0.isIdentical(to: $1) }
            if subjectUnchanged && assertionsUnchanged {
                return self
            }
            return Envelope(subject: newSubject, uncheckedAssertions: newAssertions)
        case .wrapped(let envelope, _):
            let newEnvelope = envelope.walkDecompress(targetDigests)
            if newEnvelope.isIdentical(to: envelope) {
                return self
            }
            return Envelope(wrapped: newEnvelope)
        case .assertion(let assertion):
            let newPredicate = assertion.predicate.walkDecompress(targetDigests)
            let newObject = assertion.object.walkDecompress(targetDigests)
            if newPredicate.isIdentical(to: assertion.predicate) && newObject.isIdentical(to: assertion.object) {
                return self
            }
            return Envelope(assertion: Assertion(predicate: newPredicate, object: newObject))
        default:
            return self
        }
    }
}

private extension Envelope {
    func collectNodesMatching(
        into result: inout Swift.Set<Digest>,
        targetDigests: Swift.Set<Digest>?,
        obscureTypes: [ObscureType]
    ) {
        let digestMatches = targetDigests?.contains(digest) ?? true
        if digestMatches {
            if obscureTypes.isEmpty {
                result.insert(digest)
            } else if obscureTypes.contains(where: { matches(obscureType: $0) }) {
                result.insert(digest)
            }
        }
        
        switch self {
        case .node(let subject, let assertions, _):
            subject.collectNodesMatching(into: &result, targetDigests: targetDigests, obscureTypes: obscureTypes)
            for assertion in assertions {
                assertion.collectNodesMatching(into: &result, targetDigests: targetDigests, obscureTypes: obscureTypes)
            }
        case .wrapped(let envelope, _):
            envelope.collectNodesMatching(into: &result, targetDigests: targetDigests, obscureTypes: obscureTypes)
        case .assertion(let assertion):
            assertion.predicate.collectNodesMatching(into: &result, targetDigests: targetDigests, obscureTypes: obscureTypes)
            assertion.object.collectNodesMatching(into: &result, targetDigests: targetDigests, obscureTypes: obscureTypes)
        default:
            break
        }
    }
    
    func walkUnelide(with envelopeMap: [Digest: Envelope]) -> Envelope {
        switch self {
        case .elided:
            return envelopeMap[digest] ?? self
        case .node(let subject, let assertions, _):
            let newSubject = subject.walkUnelide(with: envelopeMap)
            let newAssertions = assertions.map { $0.walkUnelide(with: envelopeMap) }
            let subjectUnchanged = newSubject.isIdentical(to: subject)
            let assertionsUnchanged = zip(newAssertions, assertions).allSatisfy { $0.isIdentical(to: $1) }
            if subjectUnchanged && assertionsUnchanged {
                return self
            }
            return Envelope(subject: newSubject, uncheckedAssertions: newAssertions)
        case .wrapped(let envelope, _):
            let newEnvelope = envelope.walkUnelide(with: envelopeMap)
            if newEnvelope.isIdentical(to: envelope) {
                return self
            }
            return Envelope(wrapped: newEnvelope)
        case .assertion(let assertion):
            let newPredicate = assertion.predicate.walkUnelide(with: envelopeMap)
            let newObject = assertion.object.walkUnelide(with: envelopeMap)
            if newPredicate.isIdentical(to: assertion.predicate) && newObject.isIdentical(to: assertion.object) {
                return self
            }
            return Envelope(assertion: Assertion(predicate: newPredicate, object: newObject))
        default:
            return self
        }
    }
    
    func matches(obscureType: ObscureType) -> Bool {
        switch (obscureType, self) {
        case (.elided, .elided):
            return true
        case (.encrypted, .encrypted):
            return true
        case (.compressed, .compressed):
            return true
        default:
            return false
        }
    }
}
