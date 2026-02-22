import Testing
@testable import BCTags

private typealias CborTag = BCTags.Tag

@Suite("Tag Constants")
struct TagConstantsTests {
    static let expectedTags: [(UInt64, String)] = [
        (32, "url"),
        (37, "uuid"),
        (24, "encoded-cbor"),
        (200, "envelope"),
        (201, "leaf"),
        (262, "json"),
        (40000, "known-value"),
        (40001, "digest"),
        (40002, "encrypted"),
        (40003, "compressed"),
        (40004, "request"),
        (40005, "response"),
        (40006, "function"),
        (40007, "parameter"),
        (40008, "placeholder"),
        (40009, "replacement"),
        (40010, "agreement-private-key"),
        (40011, "agreement-public-key"),
        (40012, "arid"),
        (40013, "crypto-prvkeys"),
        (40014, "nonce"),
        (40015, "password"),
        (40016, "crypto-prvkey-base"),
        (40017, "crypto-pubkeys"),
        (40018, "salt"),
        (40019, "crypto-sealed"),
        (40020, "signature"),
        (40021, "signing-private-key"),
        (40022, "signing-public-key"),
        (40023, "crypto-key"),
        (40024, "xid"),
        (40025, "reference"),
        (40026, "event"),
        (40027, "encrypted-key"),
        (40100, "mlkem-private-key"),
        (40101, "mlkem-public-key"),
        (40102, "mlkem-ciphertext"),
        (40103, "mldsa-private-key"),
        (40104, "mldsa-public-key"),
        (40105, "mldsa-signature"),
        (40300, "seed"),
        (40303, "hdkey"),
        (40304, "keypath"),
        (40305, "coin-info"),
        (40306, "eckey"),
        (40307, "address"),
        (40308, "output-descriptor"),
        (40309, "sskr"),
        (40310, "psbt"),
        (40311, "account-descriptor"),
        (40800, "ssh-private"),
        (40801, "ssh-public"),
        (40802, "ssh-signature"),
        (40803, "ssh-certificate"),
        (1347571542, "provenance"),
        (300, "crypto-seed"),
        (306, "crypto-eckey"),
        (309, "crypto-sskr"),
        (303, "crypto-hdkey"),
        (304, "crypto-keypath"),
        (305, "crypto-coin-info"),
        (307, "crypto-output"),
        (310, "crypto-psbt"),
        (311, "crypto-account"),
        (400, "output-script-hash"),
        (401, "output-witness-script-hash"),
        (402, "output-public-key"),
        (403, "output-public-key-hash"),
        (404, "output-witness-public-key-hash"),
        (405, "output-combo"),
        (406, "output-multisig"),
        (407, "output-sorted-multisig"),
        (408, "output-raw-script"),
        (409, "output-taproot"),
        (410, "output-cosigner"),
    ]

    static let actualTags: [(UInt64, String)] = [
        (CborTag.uri.value, CborTag.uri.name!),
        (CborTag.uuid.value, CborTag.uuid.name!),
        (CborTag.encodedCBOR.value, CborTag.encodedCBOR.name!),
        (CborTag.envelope.value, CborTag.envelope.name!),
        (CborTag.leaf.value, CborTag.leaf.name!),
        (CborTag.json.value, CborTag.json.name!),
        (CborTag.knownValue.value, CborTag.knownValue.name!),
        (CborTag.digest.value, CborTag.digest.name!),
        (CborTag.encrypted.value, CborTag.encrypted.name!),
        (CborTag.compressed.value, CborTag.compressed.name!),
        (CborTag.request.value, CborTag.request.name!),
        (CborTag.response.value, CborTag.response.name!),
        (CborTag.function.value, CborTag.function.name!),
        (CborTag.parameter.value, CborTag.parameter.name!),
        (CborTag.placeholder.value, CborTag.placeholder.name!),
        (CborTag.replacement.value, CborTag.replacement.name!),
        (CborTag.x25519PrivateKey.value, CborTag.x25519PrivateKey.name!),
        (CborTag.x25519PublicKey.value, CborTag.x25519PublicKey.name!),
        (CborTag.arid.value, CborTag.arid.name!),
        (CborTag.privateKeys.value, CborTag.privateKeys.name!),
        (CborTag.nonce.value, CborTag.nonce.name!),
        (CborTag.password.value, CborTag.password.name!),
        (CborTag.privateKeyBase.value, CborTag.privateKeyBase.name!),
        (CborTag.publicKeys.value, CborTag.publicKeys.name!),
        (CborTag.salt.value, CborTag.salt.name!),
        (CborTag.sealedMessage.value, CborTag.sealedMessage.name!),
        (CborTag.signature.value, CborTag.signature.name!),
        (CborTag.signingPrivateKey.value, CborTag.signingPrivateKey.name!),
        (CborTag.signingPublicKey.value, CborTag.signingPublicKey.name!),
        (CborTag.symmetricKey.value, CborTag.symmetricKey.name!),
        (CborTag.xid.value, CborTag.xid.name!),
        (CborTag.reference.value, CborTag.reference.name!),
        (CborTag.event.value, CborTag.event.name!),
        (CborTag.encryptedKey.value, CborTag.encryptedKey.name!),
        (CborTag.mlkemPrivateKey.value, CborTag.mlkemPrivateKey.name!),
        (CborTag.mlkemPublicKey.value, CborTag.mlkemPublicKey.name!),
        (CborTag.mlkemCiphertext.value, CborTag.mlkemCiphertext.name!),
        (CborTag.mldsaPrivateKey.value, CborTag.mldsaPrivateKey.name!),
        (CborTag.mldsaPublicKey.value, CborTag.mldsaPublicKey.name!),
        (CborTag.mldsaSignature.value, CborTag.mldsaSignature.name!),
        (CborTag.seed.value, CborTag.seed.name!),
        (CborTag.hdKey.value, CborTag.hdKey.name!),
        (CborTag.derivationPath.value, CborTag.derivationPath.name!),
        (CborTag.useInfo.value, CborTag.useInfo.name!),
        (CborTag.ecKey.value, CborTag.ecKey.name!),
        (CborTag.address.value, CborTag.address.name!),
        (CborTag.outputDescriptor.value, CborTag.outputDescriptor.name!),
        (CborTag.sskrShare.value, CborTag.sskrShare.name!),
        (CborTag.psbt.value, CborTag.psbt.name!),
        (CborTag.accountDescriptor.value, CborTag.accountDescriptor.name!),
        (CborTag.sshTextPrivateKey.value, CborTag.sshTextPrivateKey.name!),
        (CborTag.sshTextPublicKey.value, CborTag.sshTextPublicKey.name!),
        (CborTag.sshTextSignature.value, CborTag.sshTextSignature.name!),
        (CborTag.sshTextCertificate.value, CborTag.sshTextCertificate.name!),
        (CborTag.provenanceMark.value, CborTag.provenanceMark.name!),
        (CborTag.seedV1.value, CborTag.seedV1.name!),
        (CborTag.ecKeyV1.value, CborTag.ecKeyV1.name!),
        (CborTag.sskrShareV1.value, CborTag.sskrShareV1.name!),
        (CborTag.hdKeyV1.value, CborTag.hdKeyV1.name!),
        (CborTag.derivationPathV1.value, CborTag.derivationPathV1.name!),
        (CborTag.useInfoV1.value, CborTag.useInfoV1.name!),
        (CborTag.outputDescriptorV1.value, CborTag.outputDescriptorV1.name!),
        (CborTag.psbtV1.value, CborTag.psbtV1.name!),
        (CborTag.accountV1.value, CborTag.accountV1.name!),
        (CborTag.outputScriptHash.value, CborTag.outputScriptHash.name!),
        (CborTag.outputWitnessScriptHash.value, CborTag.outputWitnessScriptHash.name!),
        (CborTag.outputPublicKey.value, CborTag.outputPublicKey.name!),
        (CborTag.outputPublicKeyHash.value, CborTag.outputPublicKeyHash.name!),
        (CborTag.outputWitnessPublicKeyHash.value, CborTag.outputWitnessPublicKeyHash.name!),
        (CborTag.outputCombo.value, CborTag.outputCombo.name!),
        (CborTag.outputMultisig.value, CborTag.outputMultisig.name!),
        (CborTag.outputSortedMultisig.value, CborTag.outputSortedMultisig.name!),
        (CborTag.outputRawScript.value, CborTag.outputRawScript.name!),
        (CborTag.outputTaproot.value, CborTag.outputTaproot.name!),
        (CborTag.outputCosigner.value, CborTag.outputCosigner.name!),
    ]

    @Test("All 75 tag constants match expected values")
    func tagConstantsMatchExpectedValues() {
        #expect(Self.expectedTags.count == Self.actualTags.count)
        #expect(Self.expectedTags.count == 75)

        for i in 0..<Self.expectedTags.count {
            #expect(Self.expectedTags[i].0 == Self.actualTags[i].0,
                "Tag \(i) value mismatch: expected \(Self.expectedTags[i].0), got \(Self.actualTags[i].0)")
            #expect(Self.expectedTags[i].1 == Self.actualTags[i].1,
                "Tag \(i) name mismatch: expected \(Self.expectedTags[i].1), got \(Self.actualTags[i].1)")
        }
    }

    @Test("All tag values are unique")
    func tagValuesAreUnique() {
        let values = Set(Self.expectedTags.map { $0.0 })
        #expect(values.count == Self.expectedTags.count)
    }

    @Test("All tag names are unique")
    func tagNamesAreUnique() {
        let names = Set(Self.expectedTags.map { $0.1 })
        #expect(names.count == Self.expectedTags.count)
    }
}

@Suite("Tag Behavior")
struct TagBehaviorTests {
    @Test("Tags with same value are equal")
    func tagEquality() {
        let a = CborTag(42, "foo")
        let b = CborTag(42, "bar")
        #expect(a == b)
    }

    @Test("Tags with different values are not equal")
    func tagInequality() {
        let a = CborTag(42, "foo")
        let b = CborTag(43, "foo")
        #expect(a != b)
    }

    @Test("Tags hash by value")
    func tagHashing() {
        let a = CborTag(42, "foo")
        let b = CborTag(42, "bar")
        #expect(a.hashValue == b.hashValue)
    }

    @Test("Tag description uses name when available")
    func tagDescriptionWithName() {
        let tag = CborTag(42, "foo")
        #expect(tag.description == "foo")
    }

    @Test("Tag description uses value when no name")
    func tagDescriptionWithoutName() {
        let tag = CborTag(42)
        #expect(tag.description == "42")
    }

    @Test("Tag integer literal")
    func tagIntegerLiteral() {
        let tag: CborTag = 42
        #expect(tag.value == 42)
        #expect(tag.name == nil)
    }

    @Test("Tag with multiple names uses first as preferred")
    func tagMultipleNames() {
        let tag = CborTag(42, ["primary", "secondary"])
        #expect(tag.name == "primary")
        #expect(tag.names == ["primary", "secondary"])
        #expect(tag.description == "primary")
    }
}

@Suite("TagsStore")
struct TagsStoreTests {
    @Test("Store lookup by value")
    func lookupByValue() {
        let store = TagsStore([CborTag(42, "foo")])
        let found = store.tag(for: 42 as UInt64)
        #expect(found != nil)
        #expect(found?.name == "foo")
    }

    @Test("Store lookup by name")
    func lookupByName() {
        let store = TagsStore([CborTag(42, "foo")])
        let found = store.tag(for: "foo")
        #expect(found != nil)
        #expect(found?.value == 42)
    }

    @Test("Store assigned name")
    func assignedName() {
        let store = TagsStore([CborTag(42, "foo")])
        let name = store.assignedName(for: CborTag(42))
        #expect(name == "foo")
    }

    @Test("Store name fallback")
    func nameFallback() {
        let store = TagsStore([CborTag(42, "foo")])
        let name = store.name(for: CborTag(99))
        #expect(name == "99")
    }

    @Test("Store iterates in numeric order")
    func iterationOrder() {
        let store = TagsStore([CborTag(3, "c"), CborTag(1, "a"), CborTag(2, "b")])
        let values = store.map { $0.value }
        #expect(values == [1, 2, 3])
    }

    @Test("Store summarizer registration and lookup")
    @MainActor
    func summarizerLookup() throws {
        let store = TagsStore([CborTag(42, "foo")])
        store.setSummarizer(CborTag(42)) { payload, _ in
            let text = payload as? String ?? ""
            return "S(\(text))"
        }
        let summarizer = store.summarizer(for: CborTag(42))
        #expect(summarizer != nil)
        #expect(try summarizer?("hello", true) == "S(hello)")
        #expect(store.summarizer(for: CborTag(99)) == nil)
    }

    @Test("Free function name(for:knownTags:)")
    func freeNameFunction() {
        let store = TagsStore([CborTag(42, "foo")])
        #expect(BCTags.name(for: CborTag(42), knownTags: store) == "foo")
        #expect(BCTags.name(for: CborTag(99), knownTags: store) == "99")
        #expect(BCTags.name(for: CborTag(42), knownTags: nil) == "42")
    }
}

@Suite("Registration")
struct RegistrationTests {
    @Test("registerTagsIn populates store with all tags")
    @MainActor
    func registerTagsInPopulatesStore() {
        let store = TagsStore()
        registerTagsIn(store)

        // Verify dcbor base date tag is registered
        let dateTag = store.tag(for: 1 as UInt64)
        #expect(dateTag != nil)
        #expect(dateTag?.name == "date")

        // Verify all 75 BC tags are registered by value and name
        for (value, name) in TagConstantsTests.expectedTags {
            let byValue = store.tag(for: value)
            #expect(byValue != nil, "Tag \(value) (\(name)) not found by value")
            #expect(byValue?.name == name)

            let byName = store.tag(for: name)
            #expect(byName != nil, "Tag \(name) not found by name")
            #expect(byName?.value == value)
        }
    }

    @Test("registerTags populates global store")
    @MainActor
    func registerTagsPopulatesGlobalStore() {
        registerTags()

        let envelopeTag = globalTags.tag(for: CborTag.envelope.value)
        #expect(envelopeTag != nil)
        #expect(envelopeTag?.name == "envelope")

        let dateTag = globalTags.tag(for: 1 as UInt64)
        #expect(dateTag != nil)
        #expect(dateTag?.name == "date")
    }
}
