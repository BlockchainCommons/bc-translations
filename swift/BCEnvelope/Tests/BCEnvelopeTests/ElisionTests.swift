import Testing
import BCComponents
import BCEnvelope
import WolfBase

struct ElisionTests {
    static let basicEnvelope = Envelope("Hello.")
    static let assertionEnvelope = Envelope("knows", "Bob")

    static let singleAssertionEnvelope = Envelope("Alice")
        .addAssertion("knows", "Bob")
    static let doubleAssertionEnvelope = singleAssertionEnvelope
        .addAssertion("knows", "Carol")

    init() async {
        await addKnownTags()
    }

    @Test func testEnvelopeElision() throws {
        let e1 = Self.basicEnvelope

        let e2 = e1.elide()
        #expect(e1.isEquivalent(to: e2))
        #expect(!e1.isIdentical(to: e2))

        #expect(e2.format() ==
        """
        ELIDED
        """
        )

        #expect(e2.diagnostic() ==
        """
        200(   / envelope /
           h'8cc96cdb771176e835114a0f8936690b41cfed0df22d014eedd64edaea945d59'
        )
        """
        )

        let e3 = try e2.unelide(e1)
        #expect(e3.isEquivalent(to: e1))
        #expect(e3.format() ==
        """
        "Hello."
        """
        )
    }

    @Test func testSingleAssertionRemoveElision() throws {
        // The original Envelope
        let e1 = Self.singleAssertionEnvelope
        #expect(e1.format() ==
        """
        "Alice" [
            "knows": "Bob"
        ]
        """
        )

        // Elide the entire envelope
        let e2 = try e1.elideRemoving(e1).checkEncoding()
        #expect(e2.format() ==
        """
        ELIDED
        """
        )

        // Elide just the envelope's subject
        let e3 = try e1.elideRemoving(Envelope("Alice")).checkEncoding()
        #expect(e3.format() ==
        """
        ELIDED [
            "knows": "Bob"
        ]
        """
        )

        // Elide just the assertion's predicate
        let e4 = try e1.elideRemoving(Envelope("knows")).checkEncoding()
        #expect(e4.format() ==
        """
        "Alice" [
            ELIDED: "Bob"
        ]
        """
        )

        // Elide just the assertion's object
        let e5 = try e1.elideRemoving(Envelope("Bob")).checkEncoding()
        #expect(e5.format() ==
        """
        "Alice" [
            "knows": ELIDED
        ]
        """
        )

        // Elide the entire assertion
        let e6 = try e1.elideRemoving(Self.assertionEnvelope).checkEncoding()
        #expect(e6.format() ==
        """
        "Alice" [
            ELIDED
        ]
        """
        )
    }

    @Test func testDoubleAssertionRemoveElision() throws {
        // The original Envelope
        let e1 = Self.doubleAssertionEnvelope
        #expect(e1.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "knows": "Carol"
        ]
        """
        )

        // Elide the entire envelope
        let e2 = try e1.elideRemoving(e1).checkEncoding()
        #expect(e2.format() ==
        """
        ELIDED
        """
        )

        // Elide just the envelope's subject
        let e3 = try e1.elideRemoving(Envelope("Alice")).checkEncoding()
        #expect(e3.format() ==
        """
        ELIDED [
            "knows": "Bob"
            "knows": "Carol"
        ]
        """
        )

        // Elide just the assertion's predicate
        let e4 = try e1.elideRemoving(Envelope("knows")).checkEncoding()
        #expect(e4.format() ==
        """
        "Alice" [
            ELIDED: "Bob"
            ELIDED: "Carol"
        ]
        """
        )

        // Elide just the assertion's object
        let e5 = try e1.elideRemoving(Envelope("Bob")).checkEncoding()
        #expect(e5.format() ==
        """
        "Alice" [
            "knows": "Carol"
            "knows": ELIDED
        ]
        """
        )

        // Elide the entire assertion
        let e6 = try e1.elideRemoving(Self.assertionEnvelope).checkEncoding()
        #expect(e6.format() ==
        """
        "Alice" [
            "knows": "Carol"
            ELIDED
        ]
        """
        )
    }

    @Test func testSingleAssertionRevealElision() throws {
        // The original Envelope
        let e1 = Self.singleAssertionEnvelope
        #expect(e1.format() ==
        """
        "Alice" [
            "knows": "Bob"
        ]
        """
        )

        // Elide revealing nothing
        let e2 = try e1.elideRevealing([]).checkEncoding()
        #expect(e2.format() ==
        """
        ELIDED
        """
        )

        // Reveal just the envelope's structure
        let e3 = try e1.elideRevealing(e1).checkEncoding()
        #expect(e3.format() ==
        """
        ELIDED [
            ELIDED
        ]
        """
        )

        // Reveal just the envelope's subject
        let e4 = try e1.elideRevealing([e1, Envelope("Alice")]).checkEncoding()
        #expect(e4.format() ==
        """
        "Alice" [
            ELIDED
        ]
        """
        )

        // Reveal just the assertion's structure.
        let e5 = try e1.elideRevealing([e1, Self.assertionEnvelope]).checkEncoding()
        #expect(e5.format() ==
        """
        ELIDED [
            ELIDED: ELIDED
        ]
        """
        )

        // Reveal just the assertion's predicate
        let e6 = try e1.elideRevealing([e1, Self.assertionEnvelope, Envelope("knows")]).checkEncoding()
        #expect(e6.format() ==
        """
        ELIDED [
            "knows": ELIDED
        ]
        """
        )

        // Reveal just the assertion's object
        let e7 = try e1.elideRevealing([e1, Self.assertionEnvelope, Envelope("Bob")]).checkEncoding()
        #expect(e7.format() ==
        """
        ELIDED [
            ELIDED: "Bob"
        ]
        """
        )
    }

    @Test func testDoubleAssertionRevealElision() throws {
        // The original Envelope
        let e1 = Self.doubleAssertionEnvelope
        #expect(e1.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "knows": "Carol"
        ]
        """
        )

        // Elide revealing nothing
        let e2 = try e1.elideRevealing([]).checkEncoding()
        #expect(e2.format() ==
        """
        ELIDED
        """
        )

        // Reveal just the envelope's structure
        let e3 = try e1.elideRevealing(e1).checkEncoding()
        #expect(e3.format() ==
        """
        ELIDED [
            ELIDED (2)
        ]
        """
        )

        // Reveal just the envelope's subject
        let e4 = try e1.elideRevealing([e1, Envelope("Alice")]).checkEncoding()
        #expect(e4.format() ==
        """
        "Alice" [
            ELIDED (2)
        ]
        """
        )

        // Reveal just the assertion's structure.
        let e5 = try e1.elideRevealing([e1, Self.assertionEnvelope]).checkEncoding()
        #expect(e5.format() ==
        """
        ELIDED [
            ELIDED: ELIDED
            ELIDED
        ]
        """
        )

        // Reveal just the assertion's predicate
        let e6 = try e1.elideRevealing([e1, Self.assertionEnvelope, Envelope("knows")]).checkEncoding()
        #expect(e6.format() ==
        """
        ELIDED [
            "knows": ELIDED
            ELIDED
        ]
        """
        )

        // Reveal just the assertion's object
        let e7 = try e1.elideRevealing([e1, Self.assertionEnvelope, Envelope("Bob")]).checkEncoding()
        #expect(e7.format() ==
        """
        ELIDED [
            ELIDED: "Bob"
            ELIDED
        ]
        """
        )
    }

    @Test func testDigests() throws {
        let e1 = Self.doubleAssertionEnvelope
        #expect(e1.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "knows": "Carol"
        ]
        """
        )

        let e2 = try e1.elideRevealing(e1.digests(levelLimit: 0)).checkEncoding()
        #expect(e2.format() ==
        """
        ELIDED
        """
        )

        let e3 = try e1.elideRevealing(e1.digests(levelLimit: 1)).checkEncoding()
        #expect(e3.format() ==
        """
        "Alice" [
            ELIDED (2)
        ]
        """
        )

        let e4 = try e1.elideRevealing(e1.digests(levelLimit: 2)).checkEncoding()
        #expect(e4.format() ==
        """
        "Alice" [
            ELIDED: ELIDED
            ELIDED: ELIDED
        ]
        """
        )

        let e5 = try e1.elideRevealing(e1.digests(levelLimit: 3)).checkEncoding()
        #expect(e5.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "knows": "Carol"
        ]
        """
        )
    }

    @Test func testTargetedReveal() throws {
        let e1 = Self.doubleAssertionEnvelope
            .addAssertion("livesAt", "123 Main St.")
        #expect(e1.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "knows": "Carol"
            "livesAt": "123 Main St."
        ]
        """
        )

        var target: Swift.Set<Digest> = []
        // Reveal the Envelope structure
        target.formUnion(e1.digests(levelLimit: 1))
        // Reveal everything about the subject
        target.formUnion(e1.subject.deepDigests)
        // Reveal everything about one of the assertions
        target.formUnion(Self.assertionEnvelope.deepDigests)
        // Reveal the specific `livesAt` assertion
        target.formUnion(try e1.assertion(withPredicate: "livesAt").deepDigests)
        let e2 = try e1.elideRevealing(target).checkEncoding()
        #expect(e2.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "livesAt": "123 Main St."
            ELIDED
        ]
        """
        )
    }

    @Test func testTargetedRemove() throws {
        let e1 = Self.doubleAssertionEnvelope
            .addAssertion("livesAt", "123 Main St.")
        #expect(e1.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "knows": "Carol"
            "livesAt": "123 Main St."
        ]
        """
        )

        var target2: Swift.Set<Digest> = []
        // Hide one of the assertions
        target2.formUnion(Self.assertionEnvelope.digests(levelLimit: 1))
        let e2 = try e1.elideRemoving(target2).checkEncoding()
        #expect(e2.format() ==
        """
        "Alice" [
            "knows": "Carol"
            "livesAt": "123 Main St."
            ELIDED
        ]
        """
        )

        var target3: Swift.Set<Digest> = []
        // Hide one of the assertions by finding its predicate
        target3.formUnion(try e1.assertion(withPredicate: "livesAt").deepDigests)
        let e3 = try e1.elideRemoving(target3).checkEncoding()
        #expect(e3.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "knows": "Carol"
            ELIDED
        ]
        """
        )
        
        // Semantically equivalent
        #expect(e1.isEquivalent(to: e3))
        
        // Structurally different
        #expect(!e1.isIdentical(to: e3))
    }
    
    @Test func testWalkReplaceBasic() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let charlie = Envelope("Charlie")
        
        let envelope = alice
            .addAssertion("knows", bob)
            .addAssertion("likes", bob)
        
        #expect(envelope.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "likes": "Bob"
        ]
        """
        )
        
        let target: Swift.Set<Digest> = [bob.digest]
        let modified = try envelope.walkReplace(target, with: charlie)
        
        #expect(modified.format() ==
        """
        "Alice" [
            "knows": "Charlie"
            "likes": "Charlie"
        ]
        """
        )
        
        #expect(!modified.isEquivalent(to: envelope))
    }
    
    @Test func testWalkReplaceSubject() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let carol = Envelope("Carol")
        
        let envelope = alice.addAssertion("knows", bob)
        
        #expect(envelope.format() ==
        """
        "Alice" [
            "knows": "Bob"
        ]
        """
        )
        
        let target: Swift.Set<Digest> = [alice.digest]
        let modified = try envelope.walkReplace(target, with: carol)
        
        #expect(modified.format() ==
        """
        "Carol" [
            "knows": "Bob"
        ]
        """
        )
    }
    
    @Test func testWalkReplaceNested() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let charlie = Envelope("Charlie")
        
        let inner = bob.addAssertion("friend", bob)
        let envelope = alice.addAssertion("knows", inner)
        
        #expect(envelope.format() ==
        """
        "Alice" [
            "knows": "Bob" [
                "friend": "Bob"
            ]
        ]
        """
        )
        
        let target: Swift.Set<Digest> = [bob.digest]
        let modified = try envelope.walkReplace(target, with: charlie)
        
        #expect(modified.format() ==
        """
        "Alice" [
            "knows": "Charlie" [
                "friend": "Charlie"
            ]
        ]
        """
        )
    }
    
    @Test func testWalkReplaceWrapped() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let charlie = Envelope("Charlie")
        
        let wrapped = bob.wrap()
        let envelope = alice.addAssertion("data", wrapped)
        
        #expect(envelope.format() ==
        """
        "Alice" [
            "data": {
                "Bob"
            }
        ]
        """
        )
        
        let target: Swift.Set<Digest> = [bob.digest]
        let modified = try envelope.walkReplace(target, with: charlie)
        
        #expect(modified.format() ==
        """
        "Alice" [
            "data": {
                "Charlie"
            }
        ]
        """
        )
    }
    
    @Test func testWalkReplaceNoMatch() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let charlie = Envelope("Charlie")
        let dave = Envelope("Dave")
        
        let envelope = alice.addAssertion("knows", bob)
        
        #expect(envelope.format() ==
        """
        "Alice" [
            "knows": "Bob"
        ]
        """
        )
        
        let target: Swift.Set<Digest> = [dave.digest]
        let modified = try envelope.walkReplace(target, with: charlie)
        
        #expect(modified.format() ==
        """
        "Alice" [
            "knows": "Bob"
        ]
        """
        )
        
        #expect(modified.isIdentical(to: envelope))
    }
    
    @Test func testWalkReplaceMultipleTargets() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let carol = Envelope("Carol")
        let replacement = Envelope("REDACTED")
        
        let envelope = alice
            .addAssertion("knows", bob)
            .addAssertion("likes", carol)
        
        #expect(envelope.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "likes": "Carol"
        ]
        """
        )
        
        let target: Swift.Set<Digest> = [bob.digest, carol.digest]
        let modified = try envelope.walkReplace(target, with: replacement)
        
        #expect(modified.format() ==
        """
        "Alice" [
            "knows": "REDACTED"
            "likes": "REDACTED"
        ]
        """
        )
    }
    
    @Test func testWalkReplaceElided() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let charlie = Envelope("Charlie")
        
        let envelope = alice
            .addAssertion("knows", bob)
            .addAssertion("likes", bob)
        
        #expect(envelope.format() ==
        """
        "Alice" [
            "knows": "Bob"
            "likes": "Bob"
        ]
        """
        )
        
        let elided = envelope.elideRemoving(bob)
        
        #expect(elided.format() ==
        """
        "Alice" [
            "knows": ELIDED
            "likes": ELIDED
        ]
        """
        )
        
        let target: Swift.Set<Digest> = [bob.digest]
        let modified = try elided.walkReplace(target, with: charlie)
        
        #expect(modified.format() ==
        """
        "Alice" [
            "knows": "Charlie"
            "likes": "Charlie"
        ]
        """
        )
        
        #expect(!modified.isEquivalent(to: envelope))
        #expect(!modified.isEquivalent(to: elided))
    }
    
    @Test func testWalkReplaceAssertionWithNonAssertionFails() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let charlie = Envelope("Charlie")
        
        let envelope = alice.addAssertion("knows", bob)
        
        let knowsAssertion = try envelope.assertion(withPredicate: "knows")
        let target: Swift.Set<Digest> = [knowsAssertion.digest]
        
        var threw = false
        do {
            _ = try envelope.walkReplace(target, with: charlie)
        } catch {
            threw = true
            #expect(String(describing: error) == EnvelopeError.invalidFormat.description)
        }
        #expect(threw)
    }
}
