import Foundation

/// Metadata associated with a signature.
public struct SignatureMetadata: Sendable {
    private let storedAssertions: [Assertion]

    public init() {
        self.storedAssertions = []
    }

    public init(assertions: [Assertion]) {
        self.storedAssertions = assertions
    }

    public var assertions: [Assertion] {
        storedAssertions
    }

    public func addAssertion(_ assertion: Assertion) -> SignatureMetadata {
        SignatureMetadata(assertions: storedAssertions + [assertion])
    }

    public func withAssertion(_ predicate: Any, _ object: Any) -> SignatureMetadata {
        addAssertion(Assertion(predicate: predicate, object: object))
    }

    public var hasAssertions: Bool {
        !storedAssertions.isEmpty
    }
}

extension SignatureMetadata: Equatable {
}
