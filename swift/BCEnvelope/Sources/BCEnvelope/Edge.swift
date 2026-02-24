import Foundation
import BCComponents

extension EnvelopeError {
    static let edgeMissingIsA = EnvelopeError("edgeMissingIsA")
    static let edgeMissingSource = EnvelopeError("edgeMissingSource")
    static let edgeMissingTarget = EnvelopeError("edgeMissingTarget")
    static let edgeDuplicateIsA = EnvelopeError("edgeDuplicateIsA")
    static let edgeDuplicateSource = EnvelopeError("edgeDuplicateSource")
    static let edgeDuplicateTarget = EnvelopeError("edgeDuplicateTarget")
    static let edgeUnexpectedAssertion = EnvelopeError("edgeUnexpectedAssertion")
    static let nonexistentEdge = EnvelopeError("nonexistentEdge")
    static let ambiguousEdge = EnvelopeError("ambiguousEdge")
}

/// A container for edge envelopes on a document.
public struct Edges: Sendable {
    private var envelopes: [Digest: Envelope]

    public init() {
        self.envelopes = [:]
    }

    public mutating func add(_ edgeEnvelope: Envelope) {
        envelopes[edgeEnvelope.digest] = edgeEnvelope
    }

    public func get(_ digest: Digest) -> Envelope? {
        envelopes[digest]
    }

    @discardableResult
    public mutating func remove(_ digest: Digest) -> Envelope? {
        envelopes.removeValue(forKey: digest)
    }

    public mutating func clear() {
        envelopes.removeAll()
    }

    public func isEmpty() -> Bool {
        envelopes.isEmpty
    }

    public func len() -> Int {
        envelopes.count
    }

    public func iter() -> [(Digest, Envelope)] {
        envelopes.map { ($0.key, $0.value) }.sorted { $0.0 < $1.0 }
    }

    public func addToEnvelope(_ envelope: Envelope) -> Envelope {
        iter().reduce(envelope) { partial, item in
            partial.addAssertion(.edge, item.1)
        }
    }

    public static func tryFromEnvelope(_ envelope: Envelope) throws -> Edges {
        let edgeEnvelopes = try envelope.edges()
        var result = Edges()
        edgeEnvelopes.forEach { edge in
            result.envelopes[edge.digest] = edge
        }
        return result
    }
}

/// A protocol for types that can store edge envelopes.
public protocol Edgeable {
    var edges: Edges { get set }
}

public extension Edgeable {
    mutating func addEdge(_ edgeEnvelope: Envelope) {
        edges.add(edgeEnvelope)
    }

    func getEdge(_ digest: Digest) -> Envelope? {
        edges.get(digest)
    }

    @discardableResult
    mutating func removeEdge(_ digest: Digest) -> Envelope? {
        edges.remove(digest)
    }

    mutating func clearEdges() {
        edges.clear()
    }

    func hasEdges() -> Bool {
        !edges.isEmpty()
    }
}

public extension Envelope {
    func addEdgeEnvelope(_ edge: Envelope) -> Envelope {
        addAssertion(.edge, edge)
    }

    func edges() throws -> [Envelope] {
        objects(forPredicate: .edge)
    }

    func validateEdge() throws {
        let inner = try edgeInnerEnvelope()

        var seenIsA = false
        var seenSource = false
        var seenTarget = false

        for assertion in inner.assertions {
            guard
                let predicate = assertion.predicate,
                let knownValue = predicate.knownValue
            else {
                throw EnvelopeError.edgeUnexpectedAssertion
            }

            switch knownValue.value {
            case KnownValue.isARaw:
                if seenIsA {
                    throw EnvelopeError.edgeDuplicateIsA
                }
                seenIsA = true
            case KnownValue.sourceRaw:
                if seenSource {
                    throw EnvelopeError.edgeDuplicateSource
                }
                seenSource = true
            case KnownValue.targetRaw:
                if seenTarget {
                    throw EnvelopeError.edgeDuplicateTarget
                }
                seenTarget = true
            default:
                throw EnvelopeError.edgeUnexpectedAssertion
            }
        }

        if !seenIsA {
            throw EnvelopeError.edgeMissingIsA
        }
        if !seenSource {
            throw EnvelopeError.edgeMissingSource
        }
        if !seenTarget {
            throw EnvelopeError.edgeMissingTarget
        }
    }

    func edgeIsA() throws -> Envelope {
        try edgeInnerEnvelope().object(forPredicate: .isA)
    }

    func edgeSource() throws -> Envelope {
        try edgeInnerEnvelope().object(forPredicate: .source)
    }

    func edgeTarget() throws -> Envelope {
        try edgeInnerEnvelope().object(forPredicate: .target)
    }

    func edgeSubject() throws -> Envelope {
        try edgeInnerEnvelope().subject
    }

    func edgesMatching(
        isA: Envelope? = nil,
        source: Envelope? = nil,
        target: Envelope? = nil,
        subject: Envelope? = nil
    ) throws -> [Envelope] {
        var matching: [Envelope] = []

        for edge in try edges() {
            if let isA {
                guard let edgeIsA = try? edge.edgeIsA(), edgeIsA.isEquivalent(to: isA) else {
                    continue
                }
            }
            if let source {
                guard let edgeSource = try? edge.edgeSource(), edgeSource.isEquivalent(to: source) else {
                    continue
                }
            }
            if let target {
                guard let edgeTarget = try? edge.edgeTarget(), edgeTarget.isEquivalent(to: target) else {
                    continue
                }
            }
            if let subject {
                guard let edgeSubject = try? edge.edgeSubject(), edgeSubject.isEquivalent(to: subject) else {
                    continue
                }
            }
            matching.append(edge)
        }

        return matching
    }
}

private extension Envelope {
    func edgeInnerEnvelope() throws -> Envelope {
        if subject.isWrapped {
            return try subject.unwrap()
        } else {
            return self
        }
    }
}
