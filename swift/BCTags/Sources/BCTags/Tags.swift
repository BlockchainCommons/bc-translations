// CBOR Tags Registry
//
// https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2020-006-urtypes.md
//
// As of August 13 2022, the IANA registry of CBOR tags
// (https://www.iana.org/assignments/cbor-tags/cbor-tags.xhtml)
// has the following low-numbered values available:
//
// One byte encoding: 6-15, 19-20
// Two byte encoding: 48-51, 53, 55-60, 62, 88-95, 99, 102, 105-109, 113-119, 128-255
//
// Tags in the range 0-23 require "standards action" for the IANA to recognize.
// Tags in the range 24-32767 require a specification to reserve.
// Tags in the range 24-255 only require two bytes to encode.
// Higher numbered tags are first-come, first-served.

// MARK: - IANA-registered tags

public extension Tag {
    static let uri = Tag(32, "url")
    static let uuid = Tag(37, "uuid")
}

// MARK: - Core Envelope tags

public extension Tag {
    /// See https://www.rfc-editor.org/rfc/rfc8949.html#name-encoded-cbor-data-item
    static let encodedCBOR = Tag(24, "encoded-cbor")
    /// Registered in https://www.iana.org/assignments/cbor-tags/cbor-tags.xhtml
    static let envelope = Tag(200, "envelope")
    static let leaf = Tag(201, "leaf")
    static let json = Tag(262, "json")
}

// MARK: - Envelope extension tags

public extension Tag {
    static let knownValue = Tag(40000, "known-value")
    static let digest = Tag(40001, "digest")
    static let encrypted = Tag(40002, "encrypted")
    static let compressed = Tag(40003, "compressed")
}

// MARK: - Distributed Function Call tags

public extension Tag {
    static let request = Tag(40004, "request")
    static let response = Tag(40005, "response")
    static let function = Tag(40006, "function")
    static let parameter = Tag(40007, "parameter")
    static let placeholder = Tag(40008, "placeholder")
    static let replacement = Tag(40009, "replacement")
}

// MARK: - Cryptographic key and identity tags

public extension Tag {
    static let x25519PrivateKey = Tag(40010, "agreement-private-key")
    static let x25519PublicKey = Tag(40011, "agreement-public-key")
    static let arid = Tag(40012, "arid")
    static let privateKeys = Tag(40013, "crypto-prvkeys")
    static let nonce = Tag(40014, "nonce")
    static let password = Tag(40015, "password")
    static let privateKeyBase = Tag(40016, "crypto-prvkey-base")
    static let publicKeys = Tag(40017, "crypto-pubkeys")
    static let salt = Tag(40018, "salt")
    static let sealedMessage = Tag(40019, "crypto-sealed")
    static let signature = Tag(40020, "signature")
    static let signingPrivateKey = Tag(40021, "signing-private-key")
    static let signingPublicKey = Tag(40022, "signing-public-key")
    static let symmetricKey = Tag(40023, "crypto-key")
    static let xid = Tag(40024, "xid")
    static let reference = Tag(40025, "reference")
    static let event = Tag(40026, "event")
    static let encryptedKey = Tag(40027, "encrypted-key")
}

// MARK: - Post-quantum cryptography tags

public extension Tag {
    static let mlkemPrivateKey = Tag(40100, "mlkem-private-key")
    static let mlkemPublicKey = Tag(40101, "mlkem-public-key")
    static let mlkemCiphertext = Tag(40102, "mlkem-ciphertext")
    static let mldsaPrivateKey = Tag(40103, "mldsa-private-key")
    static let mldsaPublicKey = Tag(40104, "mldsa-public-key")
    static let mldsaSignature = Tag(40105, "mldsa-signature")
}

// MARK: - Cryptocurrency and key management tags

public extension Tag {
    static let seed = Tag(40300, "seed")
    static let hdKey = Tag(40303, "hdkey")
    static let derivationPath = Tag(40304, "keypath")
    static let useInfo = Tag(40305, "coin-info")
    static let ecKey = Tag(40306, "eckey")
    static let address = Tag(40307, "address")
    static let outputDescriptor = Tag(40308, "output-descriptor")
    static let sskrShare = Tag(40309, "sskr")
    static let psbt = Tag(40310, "psbt")
    static let accountDescriptor = Tag(40311, "account-descriptor")
}

// MARK: - SSH tags

public extension Tag {
    static let sshTextPrivateKey = Tag(40800, "ssh-private")
    static let sshTextPublicKey = Tag(40801, "ssh-public")
    static let sshTextSignature = Tag(40802, "ssh-signature")
    static let sshTextCertificate = Tag(40803, "ssh-certificate")
}

// MARK: - Provenance

public extension Tag {
    static let provenanceMark = Tag(1347571542, "provenance")
}

// MARK: - Deprecated tags
//
// The following tags are deprecated and should not be used in new code.
// They are in the range of CBOR tags requiring "Specification" action by IANA.
// Most have been replaced by "First Come First Served" tags in the 40000+ range.

public extension Tag {
    static let seedV1 = Tag(300, "crypto-seed")
    static let ecKeyV1 = Tag(306, "crypto-eckey")
    static let sskrShareV1 = Tag(309, "crypto-sskr")
    static let hdKeyV1 = Tag(303, "crypto-hdkey")
    static let derivationPathV1 = Tag(304, "crypto-keypath")
    static let useInfoV1 = Tag(305, "crypto-coin-info")
    static let outputDescriptorV1 = Tag(307, "crypto-output")
    static let psbtV1 = Tag(310, "crypto-psbt")
    static let accountV1 = Tag(311, "crypto-account")
}

// MARK: - Output descriptor subtypes

public extension Tag {
    static let outputScriptHash = Tag(400, "output-script-hash")
    static let outputWitnessScriptHash = Tag(401, "output-witness-script-hash")
    static let outputPublicKey = Tag(402, "output-public-key")
    static let outputPublicKeyHash = Tag(403, "output-public-key-hash")
    static let outputWitnessPublicKeyHash = Tag(404, "output-witness-public-key-hash")
    static let outputCombo = Tag(405, "output-combo")
    static let outputMultisig = Tag(406, "output-multisig")
    static let outputSortedMultisig = Tag(407, "output-sorted-multisig")
    static let outputRawScript = Tag(408, "output-raw-script")
    static let outputTaproot = Tag(409, "output-taproot")
    static let outputCosigner = Tag(410, "output-cosigner")
}

// MARK: - dcbor base tags

/// The date tag from dcbor (tag 1, "date"). Registered as a base tag.
public extension Tag {
    static let date = Tag(1, "date")
}

// MARK: - Registration

/// All bc-tags tag definitions in registration order.
private let bcTagDefinitions: [Tag] = [
    .uri,
    .uuid,
    .encodedCBOR,
    .envelope,
    .leaf,
    .json,
    .knownValue,
    .digest,
    .encrypted,
    .compressed,
    .request,
    .response,
    .function,
    .parameter,
    .placeholder,
    .replacement,
    .event,
    .seedV1,
    .ecKeyV1,
    .sskrShareV1,
    .seed,
    .ecKey,
    .sskrShare,
    .x25519PrivateKey,
    .x25519PublicKey,
    .arid,
    .privateKeys,
    .nonce,
    .password,
    .privateKeyBase,
    .publicKeys,
    .salt,
    .sealedMessage,
    .signature,
    .signingPrivateKey,
    .signingPublicKey,
    .symmetricKey,
    .xid,
    .reference,
    .encryptedKey,
    .mlkemPrivateKey,
    .mlkemPublicKey,
    .mlkemCiphertext,
    .mldsaPrivateKey,
    .mldsaPublicKey,
    .mldsaSignature,
    .hdKeyV1,
    .derivationPathV1,
    .useInfoV1,
    .outputDescriptorV1,
    .psbtV1,
    .accountV1,
    .hdKey,
    .derivationPath,
    .useInfo,
    .address,
    .outputDescriptor,
    .psbt,
    .accountDescriptor,
    .sshTextPrivateKey,
    .sshTextPublicKey,
    .sshTextSignature,
    .sshTextCertificate,
    .outputScriptHash,
    .outputWitnessScriptHash,
    .outputPublicKey,
    .outputPublicKeyHash,
    .outputWitnessPublicKeyHash,
    .outputCombo,
    .outputMultisig,
    .outputSortedMultisig,
    .outputRawScript,
    .outputTaproot,
    .outputCosigner,
    .provenanceMark,
]

/// Registers the dcbor base tags followed by all Blockchain Commons tags
/// in the provided tags store.
@MainActor
public func registerTagsIn(_ tagsStore: TagsStore) {
    // Register dcbor base tags first
    tagsStore.insert(.date)

    // Register all bc-tags
    tagsStore.insertAll(bcTagDefinitions)
}

/// Registers the dcbor base tags and all Blockchain Commons tags
/// in the global tag store.
@MainActor
public func registerTags() {
    registerTagsIn(globalTags)
}
