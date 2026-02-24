import Testing
import BCComponents
import BCEnvelope
import WolfBase
import Foundation

struct ObscuringTests {
    /// This tests the transformation of different kinds of "obscured" envelopes into
    /// others. Some transformations are allowed, some are idempotent (return the same
    /// result), and some throw errors.
    ///
    /// | Operation > | Encrypt | Elide      | Compress   |
    /// |:------------|:--------|:-----------|:-----------|
    /// | Encrypted   | ERROR   | OK         | ERROR      |
    /// | Elided      | ERROR   | IDEMPOTENT | ERROR      |
    /// | Compressed  | OK      | OK         | IDEMPOTENT |
    ///
    @Test func testObscuring() throws {
        let key = SymmetricKey()
        
        let envelope = Envelope(plaintextHello)
        #expect(!envelope.isObscured)
        
        let encrypted = try envelope.encryptSubject(with: key)
        #expect(encrypted.isObscured)

        let elided = envelope.elide()
        #expect(elided.isObscured)

        let compressed = try envelope.compress()
        #expect(compressed.isObscured)

        
        // ENCRYPTION
        
        // Cannot encrypt an encrypted envelope.
        //
        // If allowed, would result in an envelope with the same digest but
        // double-encrypted, possibly with a different key, which is probably not what's
        // intended. If you want to double-encrypt then wrap the encrypted envelope first,
        // which will change its digest.
        #expect(throws: (any Swift.Error).self) { try encrypted.encryptSubject(with: key) }
        
        // Cannot encrypt an elided envelope.
        //
        // Elided envelopes have no data to encrypt.
        #expect(throws: (any Swift.Error).self) { try elided.encryptSubject(with: key) }
        
        // OK to encrypt a compressed envelope.
        guard case .encrypted = try compressed.encryptSubject(with: key) else {
            Issue.record()
            return
        }
        
        
        // ELISION
        
        // OK to elide an encrypted envelope.
        guard case .elided = encrypted.elide() else {
            Issue.record()
            return
        }
        
        // Eliding an elided envelope is idempotent.
        guard case .elided = elided.elide() else {
            Issue.record()
            return
        }
        
        // OK to elide a compressed envelope.
        guard case .elided = compressed.elide() else {
            Issue.record()
            return
        }
        
        
        // COMPRESSION
        
        // Cannot compress an encrypted envelope.
        //
        // Encrypted envelopes cannot become smaller because encrypted data looks random,
        // and random data is not compressible.
        #expect(throws: (any Swift.Error).self) { try encrypted.compress() }
        
        // Cannot compress an elided envelope.
        //
        // Elided envelopes have no data to compress.
        #expect(throws: (any Swift.Error).self) { try elided.compress() }
        
        // Compressing a compressed envelope is idempotent.
        guard case .compressed = try compressed.compress() else {
            Issue.record()
            return
        }
    }
    
    @Test func testNodesMatching() throws {
        let envelope = Envelope("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("age", 30)
            .addAssertion("city", "Boston")
        
        let knowsAssertion = try envelope.assertion(withPredicate: "knows")
        let knowsDigest = knowsAssertion.digest
        
        let ageAssertion = try envelope.assertion(withPredicate: "age")
        let ageDigest = ageAssertion.digest
        
        let elideTarget: Swift.Set<Digest> = [knowsDigest]
        let compressTarget: Swift.Set<Digest> = [ageDigest]
        
        var obscured = envelope.elideRemoving(elideTarget)
        obscured = obscured.elideRemoving(compressTarget, action: .compress)
        
        #expect(obscured.format() ==
        """
        "Alice" [
            "city": "Boston"
            COMPRESSED
            ELIDED
        ]
        """
        )
        
        let elidedNodes = obscured.nodesMatching(nil, [.elided])
        #expect(elidedNodes.contains(knowsDigest))
        
        let compressedNodes = obscured.nodesMatching(nil, [.compressed])
        #expect(compressedNodes.contains(ageDigest))
        
        let targetFilter: Swift.Set<Digest> = [knowsDigest]
        let filtered = obscured.nodesMatching(targetFilter, [.elided])
        #expect(filtered.count == 1)
        #expect(filtered.contains(knowsDigest))
        
        let allInTarget = obscured.nodesMatching(elideTarget, [])
        #expect(allInTarget.count == 1)
        #expect(allInTarget.contains(knowsDigest))
        
        let noMatchTarget: Swift.Set<Digest> = [Digest.fromImage(Data("nonexistent".utf8))]
        let noMatches = obscured.nodesMatching(noMatchTarget, [.elided])
        #expect(noMatches.isEmpty)
    }
    
    @Test func testWalkUnelide() throws {
        let alice = Envelope("Alice")
        let bob = Envelope("Bob")
        let carol = Envelope("Carol")
        
        let envelope = Envelope("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("friend", "Carol")
        
        let elided = envelope
            .elideRemoving(alice)
            .elideRemoving(bob)
        
        #expect(elided.format() ==
        """
        ELIDED [
            "friend": "Carol"
            "knows": ELIDED
        ]
        """
        )
        
        let restored = elided.walkUnelide([alice, bob, carol])
        
        #expect(restored.format() ==
        """
        "Alice" [
            "friend": "Carol"
            "knows": "Bob"
        ]
        """
        )
        
        let partial = elided.walkUnelide([alice])
        #expect(partial.format() ==
        """
        "Alice" [
            "friend": "Carol"
            "knows": ELIDED
        ]
        """
        )
        
        let unchanged = elided.walkUnelide([])
        #expect(unchanged.isIdentical(to: elided))
    }
    
    @Test func testWalkDecrypt() throws {
        let key1 = SymmetricKey()
        let key2 = SymmetricKey()
        let key3 = SymmetricKey()
        
        let envelope = Envelope("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("age", 30)
            .addAssertion("city", "Boston")
        
        let knowsAssertion = try envelope.assertion(withPredicate: "knows")
        let ageAssertion = try envelope.assertion(withPredicate: "age")
        
        let encrypt1Target: Swift.Set<Digest> = [knowsAssertion.digest]
        let encrypt2Target: Swift.Set<Digest> = [ageAssertion.digest]
        
        let encrypted = envelope
            .elideRemoving(encrypt1Target, action: .encrypt(key1))
            .elideRemoving(encrypt2Target, action: .encrypt(key2))
        
        #expect(encrypted.format() ==
        """
        "Alice" [
            "city": "Boston"
            ENCRYPTED (2)
        ]
        """
        )
        
        let decrypted = encrypted.walkDecrypt([key1, key2])
        #expect(decrypted.format() ==
        """
        "Alice" [
            "age": 30
            "city": "Boston"
            "knows": "Bob"
        ]
        """
        )
        
        let partial = encrypted.walkDecrypt([key1])
        #expect(!partial.isIdentical(to: encrypted))
        #expect(partial.isEquivalent(to: envelope))
        #expect(partial.format() ==
        """
        "Alice" [
            "city": "Boston"
            "knows": "Bob"
            ENCRYPTED
        ]
        """
        )
        
        let unchanged = encrypted.walkDecrypt([key3])
        #expect(unchanged.isIdentical(to: encrypted))
    }
    
    @Test func testWalkDecompress() throws {
        let envelope = Envelope("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("bio", String(repeating: "A", count: 1000))
            .addAssertion("description", String(repeating: "B", count: 1000))
        
        let bioAssertion = try envelope.assertion(withPredicate: "bio")
        let descriptionAssertion = try envelope.assertion(withPredicate: "description")
        
        let bioDigest = bioAssertion.digest
        let descriptionDigest = descriptionAssertion.digest
        
        let compressTarget: Swift.Set<Digest> = [bioDigest, descriptionDigest]
        let compressed = envelope.elideRemoving(compressTarget, action: .compress)
        
        #expect(compressed.format() ==
        """
        "Alice" [
            "knows": "Bob"
            COMPRESSED (2)
        ]
        """
        )
        
        let decompressed = compressed.walkDecompress(nil)
        #expect(decompressed.isEquivalent(to: envelope))
        
        let target: Swift.Set<Digest> = [bioDigest]
        let partial = compressed.walkDecompress(target)
        #expect(!partial.isIdentical(to: compressed))
        #expect(partial.isEquivalent(to: envelope))
        
        let stillCompressed = partial.nodesMatching(nil, [.compressed])
        #expect(stillCompressed.contains(descriptionDigest))
        #expect(!stillCompressed.contains(bioDigest))
        
        let noMatch: Swift.Set<Digest> = [Digest.fromImage(Data("nonexistent".utf8))]
        let unchanged = compressed.walkDecompress(noMatch)
        #expect(unchanged.isIdentical(to: compressed))
    }
    
    @Test func testMixedObscurationOperations() throws {
        let key = SymmetricKey()
        
        let envelope = Envelope("Alice")
            .addAssertion("knows", "Bob")
            .addAssertion("age", 30)
            .addAssertion("bio", String(repeating: "A", count: 1000))
        
        let knowsAssertion = try envelope.assertion(withPredicate: "knows")
        let ageAssertion = try envelope.assertion(withPredicate: "age")
        let bioAssertion = try envelope.assertion(withPredicate: "bio")
        
        let knowsDigest = knowsAssertion.digest
        let ageDigest = ageAssertion.digest
        let bioDigest = bioAssertion.digest
        
        let elideTarget: Swift.Set<Digest> = [knowsDigest]
        let encryptTarget: Swift.Set<Digest> = [ageDigest]
        let compressTarget: Swift.Set<Digest> = [bioDigest]
        
        let obscured = envelope
            .elideRemoving(elideTarget)
            .elideRemoving(encryptTarget, action: .encrypt(key))
            .elideRemoving(compressTarget, action: .compress)
        
        let elided = obscured.nodesMatching(nil, [.elided])
        let encrypted = obscured.nodesMatching(nil, [.encrypted])
        let compressed = obscured.nodesMatching(nil, [.compressed])
        
        #expect(elided.contains(knowsDigest))
        #expect(encrypted.contains(ageDigest))
        #expect(compressed.contains(bioDigest))
        
        let restored = obscured
            .walkUnelide([knowsAssertion])
            .walkDecrypt([key])
            .walkDecompress(nil)
        
        #expect(restored.isEquivalent(to: envelope))
    }
}
