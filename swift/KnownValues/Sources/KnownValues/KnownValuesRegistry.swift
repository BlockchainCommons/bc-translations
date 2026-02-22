import Foundation

// For definitions see:
// https://github.com/BlockchainCommons/Research/blob/master/papers/bcr-2023-002-known-value.md#appendix-a-registry

// MARK: - Known Value Constants

extension KnownValue {

    // MARK: General

    public static let unit              = KnownValue(value: 0, name: "")
    public static let isA               = KnownValue(value: 1, name: "isA")
    public static let id                = KnownValue(value: 2, name: "id")
    public static let signed            = KnownValue(value: 3, name: "signed")
    public static let note              = KnownValue(value: 4, name: "note")
    public static let hasRecipient      = KnownValue(value: 5, name: "hasRecipient")
    public static let sskrShare         = KnownValue(value: 6, name: "sskrShare")
    public static let controller        = KnownValue(value: 7, name: "controller")
    public static let key               = KnownValue(value: 8, name: "key")
    public static let dereferenceVia    = KnownValue(value: 9, name: "dereferenceVia")
    public static let entity            = KnownValue(value: 10, name: "entity")
    public static let `name`            = KnownValue(value: 11, name: "name")
    public static let language          = KnownValue(value: 12, name: "language")
    public static let issuer            = KnownValue(value: 13, name: "issuer")
    public static let holder            = KnownValue(value: 14, name: "holder")
    public static let salt              = KnownValue(value: 15, name: "salt")
    public static let date              = KnownValue(value: 16, name: "date")
    public static let unknownValue      = KnownValue(value: 17, name: "Unknown")
    public static let versionValue      = KnownValue(value: 18, name: "version")
    public static let hasSecret         = KnownValue(value: 19, name: "hasSecret")
    public static let diffEdits         = KnownValue(value: 20, name: "edits")
    public static let validFrom         = KnownValue(value: 21, name: "validFrom")
    public static let validUntil        = KnownValue(value: 22, name: "validUntil")
    public static let position          = KnownValue(value: 23, name: "position")
    public static let nickname          = KnownValue(value: 24, name: "nickname")
    public static let `value`           = KnownValue(value: 25, name: "value")
    public static let attestation       = KnownValue(value: 26, name: "attestation")
    public static let verifiableAt      = KnownValue(value: 27, name: "verifiableAt")

    // MARK: Attachments

    public static let attachment        = KnownValue(value: 50, name: "attachment")
    public static let vendor            = KnownValue(value: 51, name: "vendor")
    public static let conformsTo        = KnownValue(value: 52, name: "conformsTo")

    // MARK: XID Documents

    public static let allow             = KnownValue(value: 60, name: "allow")
    public static let deny              = KnownValue(value: 61, name: "deny")
    public static let endpoint          = KnownValue(value: 62, name: "endpoint")
    public static let delegate          = KnownValue(value: 63, name: "delegate")
    public static let provenance        = KnownValue(value: 64, name: "provenance")
    public static let privateKey        = KnownValue(value: 65, name: "privateKey")
    public static let service           = KnownValue(value: 66, name: "service")
    public static let capability        = KnownValue(value: 67, name: "capability")
    public static let provenanceGenerator = KnownValue(value: 68, name: "provenanceGenerator")

    // MARK: XID Privileges

    public static let privilegeAll      = KnownValue(value: 70, name: "All")
    public static let privilegeAuth     = KnownValue(value: 71, name: "Authorize")
    public static let privilegeSign     = KnownValue(value: 72, name: "Sign")
    public static let privilegeEncrypt  = KnownValue(value: 73, name: "Encrypt")
    public static let privilegeElide    = KnownValue(value: 74, name: "Elide")
    public static let privilegeIssue    = KnownValue(value: 75, name: "Issue")
    public static let privilegeAccess   = KnownValue(value: 76, name: "Access")
    public static let privilegeDelegate = KnownValue(value: 80, name: "Delegate")
    public static let privilegeVerify   = KnownValue(value: 81, name: "Verify")
    public static let privilegeUpdate   = KnownValue(value: 82, name: "Update")
    public static let privilegeTransfer = KnownValue(value: 83, name: "Transfer")
    public static let privilegeElect    = KnownValue(value: 84, name: "Elect")
    public static let privilegeBurn     = KnownValue(value: 85, name: "Burn")
    public static let privilegeRevoke   = KnownValue(value: 86, name: "Revoke")

    // MARK: Expression and Function Calls

    public static let body              = KnownValue(value: 100, name: "body")
    public static let result            = KnownValue(value: 101, name: "result")
    public static let error             = KnownValue(value: 102, name: "error")
    public static let okValue           = KnownValue(value: 103, name: "OK")
    public static let processingValue   = KnownValue(value: 104, name: "Processing")
    public static let sender            = KnownValue(value: 105, name: "sender")
    public static let senderContinuation = KnownValue(value: 106, name: "senderContinuation")
    public static let recipientContinuation = KnownValue(value: 107, name: "recipientContinuation")
    public static let content           = KnownValue(value: 108, name: "content")

    // MARK: Cryptography

    public static let seedType          = KnownValue(value: 200, name: "Seed")
    public static let privateKeyType    = KnownValue(value: 201, name: "PrivateKey")
    public static let publicKeyType     = KnownValue(value: 202, name: "PublicKey")
    public static let masterKeyType     = KnownValue(value: 203, name: "MasterKey")

    // MARK: Cryptocurrency Assets

    public static let asset             = KnownValue(value: 300, name: "asset")
    public static let bitcoinValue      = KnownValue(value: 301, name: "Bitcoin")
    public static let ethereumValue     = KnownValue(value: 302, name: "Ethereum")
    public static let tezosValue        = KnownValue(value: 303, name: "Tezos")

    // MARK: Cryptocurrency Networks

    public static let network           = KnownValue(value: 400, name: "network")
    public static let mainNetValue      = KnownValue(value: 401, name: "MainNet")
    public static let testNetValue      = KnownValue(value: 402, name: "TestNet")

    // MARK: Bitcoin

    public static let bip32KeyType      = KnownValue(value: 500, name: "BIP32Key")
    public static let chainCode         = KnownValue(value: 501, name: "chainCode")
    public static let derivationPathType = KnownValue(value: 502, name: "DerivationPath")
    public static let parentPath        = KnownValue(value: 503, name: "parentPath")
    public static let childrenPath      = KnownValue(value: 504, name: "childrenPath")
    public static let parentFingerprint = KnownValue(value: 505, name: "parentFingerprint")
    public static let psbtType          = KnownValue(value: 506, name: "PSBT")
    public static let outputDescriptorType = KnownValue(value: 507, name: "OutputDescriptor")
    public static let outputDescriptor  = KnownValue(value: 508, name: "outputDescriptor")

    // MARK: Graphs

    public static let graph             = KnownValue(value: 600, name: "Graph")
    public static let sourceTargetGraph = KnownValue(value: 601, name: "SourceTargetGraph")
    public static let parentChildGraph  = KnownValue(value: 602, name: "ParentChildGraph")
    public static let digraph           = KnownValue(value: 603, name: "Digraph")
    public static let acyclicGraph      = KnownValue(value: 604, name: "AcyclicGraph")
    public static let multigraph        = KnownValue(value: 605, name: "Multigraph")
    public static let pseudograph       = KnownValue(value: 606, name: "Pseudograph")
    public static let graphFragment     = KnownValue(value: 607, name: "GraphFragment")
    public static let dag               = KnownValue(value: 608, name: "DAG")
    public static let tree              = KnownValue(value: 609, name: "Tree")
    public static let forest            = KnownValue(value: 610, name: "Forest")
    public static let compoundGraph     = KnownValue(value: 611, name: "CompoundGraph")
    public static let hypergraph        = KnownValue(value: 612, name: "Hypergraph")
    public static let dihypergraph      = KnownValue(value: 613, name: "Dihypergraph")
    public static let node              = KnownValue(value: 700, name: "node")
    public static let edge              = KnownValue(value: 701, name: "edge")
    public static let source            = KnownValue(value: 702, name: "source")
    public static let target            = KnownValue(value: 703, name: "target")
    public static let parent            = KnownValue(value: 704, name: "parent")
    public static let child             = KnownValue(value: 705, name: "child")
    public static let `self`            = KnownValue(value: 706, name: "Self")
}

// MARK: - Raw Value Constants

extension KnownValue {
    public static let unitRaw: UInt64              = 0
    public static let isARaw: UInt64               = 1
    public static let idRaw: UInt64                = 2
    public static let signedRaw: UInt64            = 3
    public static let noteRaw: UInt64              = 4
    public static let hasRecipientRaw: UInt64      = 5
    public static let sskrShareRaw: UInt64         = 6
    public static let controllerRaw: UInt64        = 7
    public static let keyRaw: UInt64               = 8
    public static let dereferenceViaRaw: UInt64    = 9
    public static let entityRaw: UInt64            = 10
    public static let nameRaw: UInt64              = 11
    public static let languageRaw: UInt64          = 12
    public static let issuerRaw: UInt64            = 13
    public static let holderRaw: UInt64            = 14
    public static let saltRaw: UInt64              = 15
    public static let dateRaw: UInt64              = 16
    public static let unknownValueRaw: UInt64      = 17
    public static let versionValueRaw: UInt64      = 18
    public static let hasSecretRaw: UInt64         = 19
    public static let diffEditsRaw: UInt64         = 20
    public static let validFromRaw: UInt64         = 21
    public static let validUntilRaw: UInt64        = 22
    public static let positionRaw: UInt64          = 23
    public static let nicknameRaw: UInt64          = 24
    public static let valueRaw: UInt64             = 25
    public static let attestationRaw: UInt64       = 26
    public static let verifiableAtRaw: UInt64      = 27
    public static let attachmentRaw: UInt64        = 50
    public static let vendorRaw: UInt64            = 51
    public static let conformsToRaw: UInt64        = 52
    public static let allowRaw: UInt64             = 60
    public static let denyRaw: UInt64              = 61
    public static let endpointRaw: UInt64          = 62
    public static let delegateRaw: UInt64          = 63
    public static let provenanceRaw: UInt64        = 64
    public static let privateKeyRaw: UInt64        = 65
    public static let serviceRaw: UInt64           = 66
    public static let capabilityRaw: UInt64        = 67
    public static let provenanceGeneratorRaw: UInt64 = 68
    public static let privilegeAllRaw: UInt64      = 70
    public static let privilegeAuthRaw: UInt64     = 71
    public static let privilegeSignRaw: UInt64     = 72
    public static let privilegeEncryptRaw: UInt64  = 73
    public static let privilegeElideRaw: UInt64    = 74
    public static let privilegeIssueRaw: UInt64    = 75
    public static let privilegeAccessRaw: UInt64   = 76
    public static let privilegeDelegateRaw: UInt64 = 80
    public static let privilegeVerifyRaw: UInt64   = 81
    public static let privilegeUpdateRaw: UInt64   = 82
    public static let privilegeTransferRaw: UInt64 = 83
    public static let privilegeElectRaw: UInt64    = 84
    public static let privilegeBurnRaw: UInt64     = 85
    public static let privilegeRevokeRaw: UInt64   = 86
    public static let bodyRaw: UInt64              = 100
    public static let resultRaw: UInt64            = 101
    public static let errorRaw: UInt64             = 102
    public static let okValueRaw: UInt64           = 103
    public static let processingValueRaw: UInt64   = 104
    public static let senderRaw: UInt64            = 105
    public static let senderContinuationRaw: UInt64 = 106
    public static let recipientContinuationRaw: UInt64 = 107
    public static let contentRaw: UInt64           = 108
    public static let seedTypeRaw: UInt64          = 200
    public static let privateKeyTypeRaw: UInt64    = 201
    public static let publicKeyTypeRaw: UInt64     = 202
    public static let masterKeyTypeRaw: UInt64     = 203
    public static let assetRaw: UInt64             = 300
    public static let bitcoinValueRaw: UInt64      = 301
    public static let ethereumValueRaw: UInt64     = 302
    public static let tezosValueRaw: UInt64        = 303
    public static let networkRaw: UInt64           = 400
    public static let mainNetValueRaw: UInt64      = 401
    public static let testNetValueRaw: UInt64      = 402
    public static let bip32KeyTypeRaw: UInt64      = 500
    public static let chainCodeRaw: UInt64         = 501
    public static let derivationPathTypeRaw: UInt64 = 502
    public static let parentPathRaw: UInt64        = 503
    public static let childrenPathRaw: UInt64      = 504
    public static let parentFingerprintRaw: UInt64 = 505
    public static let psbtTypeRaw: UInt64          = 506
    public static let outputDescriptorTypeRaw: UInt64 = 507
    public static let outputDescriptorRaw: UInt64  = 508
    public static let graphRaw: UInt64             = 600
    public static let sourceTargetGraphRaw: UInt64 = 601
    public static let parentChildGraphRaw: UInt64  = 602
    public static let digraphRaw: UInt64           = 603
    public static let acyclicGraphRaw: UInt64      = 604
    public static let multigraphRaw: UInt64        = 605
    public static let pseudographRaw: UInt64       = 606
    public static let graphFragmentRaw: UInt64     = 607
    public static let dagRaw: UInt64               = 608
    public static let treeRaw: UInt64              = 609
    public static let forestRaw: UInt64            = 610
    public static let compoundGraphRaw: UInt64     = 611
    public static let hypergraphRaw: UInt64        = 612
    public static let dihypergraphRaw: UInt64      = 613
    public static let nodeRaw: UInt64              = 700
    public static let edgeRaw: UInt64              = 701
    public static let sourceRaw: UInt64            = 702
    public static let targetRaw: UInt64            = 703
    public static let parentRaw: UInt64            = 704
    public static let childRaw: UInt64             = 705
    public static let selfRaw: UInt64              = 706
}

// MARK: - Global Registry

extension KnownValuesStore {
    /// The global registry of Known Values, lazily initialized on first access.
    ///
    /// Populated with all hardcoded known value constants plus any values
    /// loaded from configured directories (default: `~/.known-values/`).
    ///
    /// To customize the search paths, call
    /// ``DirectoryConfig/setDirectoryConfig(_:)`` or
    /// ``DirectoryConfig/addSearchPaths(_:)`` **before** accessing this
    /// property.
    public static let shared: KnownValuesStore = {
        var store = KnownValuesStore([
            .unit, .isA, .id, .signed, .note, .hasRecipient,
            .sskrShare, .controller, .key, .dereferenceVia,
            .entity, .name, .language, .issuer, .holder,
            .salt, .date, .unknownValue, .versionValue, .hasSecret,
            .diffEdits, .validFrom, .validUntil, .position, .nickname,
            .attestation, .verifiableAt,
            .attachment, .vendor, .conformsTo,
            .allow, .deny, .endpoint, .delegate, .provenance,
            .privateKey, .service, .capability, .provenanceGenerator,
            .privilegeAll, .privilegeAuth, .privilegeSign,
            .privilegeEncrypt, .privilegeElide, .privilegeIssue,
            .privilegeAccess, .privilegeDelegate, .privilegeVerify,
            .privilegeUpdate, .privilegeTransfer, .privilegeElect,
            .privilegeBurn, .privilegeRevoke,
            .body, .result, .error, .okValue, .processingValue,
            .sender, .senderContinuation, .recipientContinuation,
            .content,
            .seedType, .privateKeyType, .publicKeyType, .masterKeyType,
            .asset, .bitcoinValue, .ethereumValue, .tezosValue,
            .network, .mainNetValue, .testNetValue,
            .bip32KeyType, .chainCode, .derivationPathType,
            .parentPath, .childrenPath, .parentFingerprint,
            .psbtType, .outputDescriptorType, .outputDescriptor,
            .graph, .sourceTargetGraph, .parentChildGraph,
            .digraph, .acyclicGraph, .multigraph, .pseudograph,
            .graphFragment, .dag, .tree, .forest,
            .compoundGraph, .hypergraph, .dihypergraph,
            .node, .edge, .source, .target, .parent, .child,
        ])

        // Load additional values from configured directories.
        // Values from directories override hardcoded values when codepoints
        // match.
        let config = ConfigState.shared.getAndLock()
        let loadResult = DirectoryLoader.loadFromConfig(config)
        for value in loadResult.values.values {
            store.insert(value)
        }

        return store
    }()
}
