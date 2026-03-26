namespace BlockchainCommons.KnownValues;

/// <summary>
/// A lazily initialized singleton that holds the global registry of known
/// values.
/// </summary>
public sealed class LazyKnownValues
{
    private readonly object _sync = new();
    private readonly Func<KnownValuesStore> _factory;
    private KnownValuesStore? _data;

    internal LazyKnownValues(Func<KnownValuesStore> factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Gets the global <see cref="KnownValuesStore"/>, initializing it if
    /// necessary.
    /// </summary>
    public KnownValuesStore Get()
    {
        if (_data is not null)
        {
            return _data;
        }

        lock (_sync)
        {
            _data ??= _factory();
            return _data;
        }
    }

    internal void ResetForTesting()
    {
        lock (_sync)
        {
            _data = null;
        }
    }
}

/// <summary>
/// Registry constants and global accessors for the standard Blockchain Commons
/// known values.
/// </summary>
public static class KnownValuesRegistry
{
    public const ulong UnitRaw = 0ul;
    public static readonly KnownValue Unit = KnownValue.NewWithStaticName(UnitRaw, "");
    public const ulong IsARaw = 1ul;
    public static readonly KnownValue IsA = KnownValue.NewWithStaticName(IsARaw, "isA");
    public const ulong IdRaw = 2ul;
    public static readonly KnownValue Id = KnownValue.NewWithStaticName(IdRaw, "id");
    public const ulong SignedRaw = 3ul;
    public static readonly KnownValue Signed = KnownValue.NewWithStaticName(SignedRaw, "signed");
    public const ulong NoteRaw = 4ul;
    public static readonly KnownValue Note = KnownValue.NewWithStaticName(NoteRaw, "note");
    public const ulong HasRecipientRaw = 5ul;
    public static readonly KnownValue HasRecipient = KnownValue.NewWithStaticName(HasRecipientRaw, "hasRecipient");
    public const ulong SSKRShareRaw = 6ul;
    public static readonly KnownValue SSKRShare = KnownValue.NewWithStaticName(SSKRShareRaw, "sskrShare");
    public const ulong ControllerRaw = 7ul;
    public static readonly KnownValue Controller = KnownValue.NewWithStaticName(ControllerRaw, "controller");
    public const ulong KeyRaw = 8ul;
    public static readonly KnownValue Key = KnownValue.NewWithStaticName(KeyRaw, "key");
    public const ulong DereferenceViaRaw = 9ul;
    public static readonly KnownValue DereferenceVia = KnownValue.NewWithStaticName(DereferenceViaRaw, "dereferenceVia");
    public const ulong EntityRaw = 10ul;
    public static readonly KnownValue Entity = KnownValue.NewWithStaticName(EntityRaw, "entity");
    public const ulong NameRaw = 11ul;
    public static readonly KnownValue Name = KnownValue.NewWithStaticName(NameRaw, "name");
    public const ulong LanguageRaw = 12ul;
    public static readonly KnownValue Language = KnownValue.NewWithStaticName(LanguageRaw, "language");
    public const ulong IssuerRaw = 13ul;
    public static readonly KnownValue Issuer = KnownValue.NewWithStaticName(IssuerRaw, "issuer");
    public const ulong HolderRaw = 14ul;
    public static readonly KnownValue Holder = KnownValue.NewWithStaticName(HolderRaw, "holder");
    public const ulong SaltRaw = 15ul;
    public static readonly KnownValue Salt = KnownValue.NewWithStaticName(SaltRaw, "salt");
    public const ulong DateRaw = 16ul;
    public static readonly KnownValue Date = KnownValue.NewWithStaticName(DateRaw, "date");
    public const ulong UnknownValueRaw = 17ul;
    public static readonly KnownValue UnknownValue = KnownValue.NewWithStaticName(UnknownValueRaw, "Unknown");
    public const ulong VersionValueRaw = 18ul;
    public static readonly KnownValue VersionValue = KnownValue.NewWithStaticName(VersionValueRaw, "version");
    public const ulong HasSecretRaw = 19ul;
    public static readonly KnownValue HasSecret = KnownValue.NewWithStaticName(HasSecretRaw, "hasSecret");
    public const ulong DiffEditsRaw = 20ul;
    public static readonly KnownValue DiffEdits = KnownValue.NewWithStaticName(DiffEditsRaw, "edits");
    public const ulong ValidFromRaw = 21ul;
    public static readonly KnownValue ValidFrom = KnownValue.NewWithStaticName(ValidFromRaw, "validFrom");
    public const ulong ValidUntilRaw = 22ul;
    public static readonly KnownValue ValidUntil = KnownValue.NewWithStaticName(ValidUntilRaw, "validUntil");
    public const ulong PositionRaw = 23ul;
    public static readonly KnownValue Position = KnownValue.NewWithStaticName(PositionRaw, "position");
    public const ulong NicknameRaw = 24ul;
    public static readonly KnownValue Nickname = KnownValue.NewWithStaticName(NicknameRaw, "nickname");
    public const ulong ValueRaw = 25ul;
    public static readonly KnownValue Value = KnownValue.NewWithStaticName(ValueRaw, "value");
    public const ulong AttestationRaw = 26ul;
    public static readonly KnownValue Attestation = KnownValue.NewWithStaticName(AttestationRaw, "attestation");
    public const ulong VerifiableAtRaw = 27ul;
    public static readonly KnownValue VerifiableAt = KnownValue.NewWithStaticName(VerifiableAtRaw, "verifiableAt");
    public const ulong AttachmentRaw = 50ul;
    public static readonly KnownValue Attachment = KnownValue.NewWithStaticName(AttachmentRaw, "attachment");
    public const ulong VendorRaw = 51ul;
    public static readonly KnownValue Vendor = KnownValue.NewWithStaticName(VendorRaw, "vendor");
    public const ulong ConformsToRaw = 52ul;
    public static readonly KnownValue ConformsTo = KnownValue.NewWithStaticName(ConformsToRaw, "conformsTo");
    public const ulong AllowRaw = 60ul;
    public static readonly KnownValue Allow = KnownValue.NewWithStaticName(AllowRaw, "allow");
    public const ulong DenyRaw = 61ul;
    public static readonly KnownValue Deny = KnownValue.NewWithStaticName(DenyRaw, "deny");
    public const ulong EndpointRaw = 62ul;
    public static readonly KnownValue Endpoint = KnownValue.NewWithStaticName(EndpointRaw, "endpoint");
    public const ulong DelegateRaw = 63ul;
    public static readonly KnownValue Delegate = KnownValue.NewWithStaticName(DelegateRaw, "delegate");
    public const ulong ProvenanceRaw = 64ul;
    public static readonly KnownValue Provenance = KnownValue.NewWithStaticName(ProvenanceRaw, "provenance");
    public const ulong PrivateKeyRaw = 65ul;
    public static readonly KnownValue PrivateKey = KnownValue.NewWithStaticName(PrivateKeyRaw, "privateKey");
    public const ulong ServiceRaw = 66ul;
    public static readonly KnownValue Service = KnownValue.NewWithStaticName(ServiceRaw, "service");
    public const ulong CapabilityRaw = 67ul;
    public static readonly KnownValue Capability = KnownValue.NewWithStaticName(CapabilityRaw, "capability");
    public const ulong ProvenanceGeneratorRaw = 68ul;
    public static readonly KnownValue ProvenanceGenerator = KnownValue.NewWithStaticName(ProvenanceGeneratorRaw, "provenanceGenerator");
    public const ulong PrivilegeAllRaw = 70ul;
    public static readonly KnownValue PrivilegeAll = KnownValue.NewWithStaticName(PrivilegeAllRaw, "All");
    public const ulong PrivilegeAuthRaw = 71ul;
    public static readonly KnownValue PrivilegeAuth = KnownValue.NewWithStaticName(PrivilegeAuthRaw, "Authorize");
    public const ulong PrivilegeSignRaw = 72ul;
    public static readonly KnownValue PrivilegeSign = KnownValue.NewWithStaticName(PrivilegeSignRaw, "Sign");
    public const ulong PrivilegeEncryptRaw = 73ul;
    public static readonly KnownValue PrivilegeEncrypt = KnownValue.NewWithStaticName(PrivilegeEncryptRaw, "Encrypt");
    public const ulong PrivilegeElideRaw = 74ul;
    public static readonly KnownValue PrivilegeElide = KnownValue.NewWithStaticName(PrivilegeElideRaw, "Elide");
    public const ulong PrivilegeIssueRaw = 75ul;
    public static readonly KnownValue PrivilegeIssue = KnownValue.NewWithStaticName(PrivilegeIssueRaw, "Issue");
    public const ulong PrivilegeAccessRaw = 76ul;
    public static readonly KnownValue PrivilegeAccess = KnownValue.NewWithStaticName(PrivilegeAccessRaw, "Access");
    public const ulong PrivilegeDelegateRaw = 80ul;
    public static readonly KnownValue PrivilegeDelegate = KnownValue.NewWithStaticName(PrivilegeDelegateRaw, "Delegate");
    public const ulong PrivilegeVerifyRaw = 81ul;
    public static readonly KnownValue PrivilegeVerify = KnownValue.NewWithStaticName(PrivilegeVerifyRaw, "Verify");
    public const ulong PrivilegeUpdateRaw = 82ul;
    public static readonly KnownValue PrivilegeUpdate = KnownValue.NewWithStaticName(PrivilegeUpdateRaw, "Update");
    public const ulong PrivilegeTransferRaw = 83ul;
    public static readonly KnownValue PrivilegeTransfer = KnownValue.NewWithStaticName(PrivilegeTransferRaw, "Transfer");
    public const ulong PrivilegeElectRaw = 84ul;
    public static readonly KnownValue PrivilegeElect = KnownValue.NewWithStaticName(PrivilegeElectRaw, "Elect");
    public const ulong PrivilegeBurnRaw = 85ul;
    public static readonly KnownValue PrivilegeBurn = KnownValue.NewWithStaticName(PrivilegeBurnRaw, "Burn");
    public const ulong PrivilegeRevokeRaw = 86ul;
    public static readonly KnownValue PrivilegeRevoke = KnownValue.NewWithStaticName(PrivilegeRevokeRaw, "Revoke");
    public const ulong BodyRaw = 100ul;
    public static readonly KnownValue Body = KnownValue.NewWithStaticName(BodyRaw, "body");
    public const ulong ResultRaw = 101ul;
    public static readonly KnownValue Result = KnownValue.NewWithStaticName(ResultRaw, "result");
    public const ulong ErrorRaw = 102ul;
    public static readonly KnownValue Error = KnownValue.NewWithStaticName(ErrorRaw, "error");
    public const ulong OkValueRaw = 103ul;
    public static readonly KnownValue OkValue = KnownValue.NewWithStaticName(OkValueRaw, "OK");
    public const ulong ProcessingValueRaw = 104ul;
    public static readonly KnownValue ProcessingValue = KnownValue.NewWithStaticName(ProcessingValueRaw, "Processing");
    public const ulong SenderRaw = 105ul;
    public static readonly KnownValue Sender = KnownValue.NewWithStaticName(SenderRaw, "sender");
    public const ulong SenderContinuationRaw = 106ul;
    public static readonly KnownValue SenderContinuation = KnownValue.NewWithStaticName(SenderContinuationRaw, "senderContinuation");
    public const ulong RecipientContinuationRaw = 107ul;
    public static readonly KnownValue RecipientContinuation = KnownValue.NewWithStaticName(RecipientContinuationRaw, "recipientContinuation");
    public const ulong ContentRaw = 108ul;
    public static readonly KnownValue Content = KnownValue.NewWithStaticName(ContentRaw, "content");
    public const ulong SeedTypeRaw = 200ul;
    public static readonly KnownValue SeedType = KnownValue.NewWithStaticName(SeedTypeRaw, "Seed");
    public const ulong PrivateKeyTypeRaw = 201ul;
    public static readonly KnownValue PrivateKeyType = KnownValue.NewWithStaticName(PrivateKeyTypeRaw, "PrivateKey");
    public const ulong PublicKeyTypeRaw = 202ul;
    public static readonly KnownValue PublicKeyType = KnownValue.NewWithStaticName(PublicKeyTypeRaw, "PublicKey");
    public const ulong MasterKeyTypeRaw = 203ul;
    public static readonly KnownValue MasterKeyType = KnownValue.NewWithStaticName(MasterKeyTypeRaw, "MasterKey");
    public const ulong AssetRaw = 300ul;
    public static readonly KnownValue Asset = KnownValue.NewWithStaticName(AssetRaw, "asset");
    public const ulong BitcoinValueRaw = 301ul;
    public static readonly KnownValue BitcoinValue = KnownValue.NewWithStaticName(BitcoinValueRaw, "Bitcoin");
    public const ulong EthereumValueRaw = 302ul;
    public static readonly KnownValue EthereumValue = KnownValue.NewWithStaticName(EthereumValueRaw, "Ethereum");
    public const ulong TezosValueRaw = 303ul;
    public static readonly KnownValue TezosValue = KnownValue.NewWithStaticName(TezosValueRaw, "Tezos");
    public const ulong NetworkRaw = 400ul;
    public static readonly KnownValue Network = KnownValue.NewWithStaticName(NetworkRaw, "network");
    public const ulong MainNetValueRaw = 401ul;
    public static readonly KnownValue MainNetValue = KnownValue.NewWithStaticName(MainNetValueRaw, "MainNet");
    public const ulong TestNetValueRaw = 402ul;
    public static readonly KnownValue TestNetValue = KnownValue.NewWithStaticName(TestNetValueRaw, "TestNet");
    public const ulong BIP32KeyTypeRaw = 500ul;
    public static readonly KnownValue BIP32KeyType = KnownValue.NewWithStaticName(BIP32KeyTypeRaw, "BIP32Key");
    public const ulong ChainCodeRaw = 501ul;
    public static readonly KnownValue ChainCode = KnownValue.NewWithStaticName(ChainCodeRaw, "chainCode");
    public const ulong DerivationPathTypeRaw = 502ul;
    public static readonly KnownValue DerivationPathType = KnownValue.NewWithStaticName(DerivationPathTypeRaw, "DerivationPath");
    public const ulong ParentPathRaw = 503ul;
    public static readonly KnownValue ParentPath = KnownValue.NewWithStaticName(ParentPathRaw, "parentPath");
    public const ulong ChildrenPathRaw = 504ul;
    public static readonly KnownValue ChildrenPath = KnownValue.NewWithStaticName(ChildrenPathRaw, "childrenPath");
    public const ulong ParentFingerprintRaw = 505ul;
    public static readonly KnownValue ParentFingerprint = KnownValue.NewWithStaticName(ParentFingerprintRaw, "parentFingerprint");
    public const ulong PSBTTypeRaw = 506ul;
    public static readonly KnownValue PSBTType = KnownValue.NewWithStaticName(PSBTTypeRaw, "PSBT");
    public const ulong OutputDescriptorTypeRaw = 507ul;
    public static readonly KnownValue OutputDescriptorType = KnownValue.NewWithStaticName(OutputDescriptorTypeRaw, "OutputDescriptor");
    public const ulong OutputDescriptorRaw = 508ul;
    public static readonly KnownValue OutputDescriptor = KnownValue.NewWithStaticName(OutputDescriptorRaw, "outputDescriptor");
    public const ulong GraphRaw = 600ul;
    public static readonly KnownValue Graph = KnownValue.NewWithStaticName(GraphRaw, "Graph");
    public const ulong SourceTargetGraphRaw = 601ul;
    public static readonly KnownValue SourceTargetGraph = KnownValue.NewWithStaticName(SourceTargetGraphRaw, "SourceTargetGraph");
    public const ulong ParentChildGraphRaw = 602ul;
    public static readonly KnownValue ParentChildGraph = KnownValue.NewWithStaticName(ParentChildGraphRaw, "ParentChildGraph");
    public const ulong DigraphRaw = 603ul;
    public static readonly KnownValue Digraph = KnownValue.NewWithStaticName(DigraphRaw, "Digraph");
    public const ulong AcyclicGraphRaw = 604ul;
    public static readonly KnownValue AcyclicGraph = KnownValue.NewWithStaticName(AcyclicGraphRaw, "AcyclicGraph");
    public const ulong MultigraphRaw = 605ul;
    public static readonly KnownValue Multigraph = KnownValue.NewWithStaticName(MultigraphRaw, "Multigraph");
    public const ulong PseudographRaw = 606ul;
    public static readonly KnownValue Pseudograph = KnownValue.NewWithStaticName(PseudographRaw, "Pseudograph");
    public const ulong GraphFragmentRaw = 607ul;
    public static readonly KnownValue GraphFragment = KnownValue.NewWithStaticName(GraphFragmentRaw, "GraphFragment");
    public const ulong DAGRaw = 608ul;
    public static readonly KnownValue DAG = KnownValue.NewWithStaticName(DAGRaw, "DAG");
    public const ulong TreeRaw = 609ul;
    public static readonly KnownValue Tree = KnownValue.NewWithStaticName(TreeRaw, "Tree");
    public const ulong ForestRaw = 610ul;
    public static readonly KnownValue Forest = KnownValue.NewWithStaticName(ForestRaw, "Forest");
    public const ulong CompoundGraphRaw = 611ul;
    public static readonly KnownValue CompoundGraph = KnownValue.NewWithStaticName(CompoundGraphRaw, "CompoundGraph");
    public const ulong HypergraphRaw = 612ul;
    public static readonly KnownValue Hypergraph = KnownValue.NewWithStaticName(HypergraphRaw, "Hypergraph");
    public const ulong DihypergraphRaw = 613ul;
    public static readonly KnownValue Dihypergraph = KnownValue.NewWithStaticName(DihypergraphRaw, "Dihypergraph");
    public const ulong NodeRaw = 700ul;
    public static readonly KnownValue Node = KnownValue.NewWithStaticName(NodeRaw, "node");
    public const ulong EdgeRaw = 701ul;
    public static readonly KnownValue Edge = KnownValue.NewWithStaticName(EdgeRaw, "edge");
    public const ulong SourceRaw = 702ul;
    public static readonly KnownValue Source = KnownValue.NewWithStaticName(SourceRaw, "source");
    public const ulong TargetRaw = 703ul;
    public static readonly KnownValue Target = KnownValue.NewWithStaticName(TargetRaw, "target");
    public const ulong ParentRaw = 704ul;
    public static readonly KnownValue Parent = KnownValue.NewWithStaticName(ParentRaw, "parent");
    public const ulong ChildRaw = 705ul;
    public static readonly KnownValue Child = KnownValue.NewWithStaticName(ChildRaw, "child");
    public const ulong SelfRaw = 706ul;
    public static readonly KnownValue Self = KnownValue.NewWithStaticName(SelfRaw, "Self");

    internal static readonly KnownValue[] AllKnownValues =
    [
        Unit,
        IsA,
        Id,
        Signed,
        Note,
        HasRecipient,
        SSKRShare,
        Controller,
        Key,
        DereferenceVia,
        Entity,
        Name,
        Language,
        Issuer,
        Holder,
        Salt,
        Date,
        UnknownValue,
        VersionValue,
        HasSecret,
        DiffEdits,
        ValidFrom,
        ValidUntil,
        Position,
        Nickname,
        Value,
        Attestation,
        VerifiableAt,
        Attachment,
        Vendor,
        ConformsTo,
        Allow,
        Deny,
        Endpoint,
        Delegate,
        Provenance,
        PrivateKey,
        Service,
        Capability,
        ProvenanceGenerator,
        PrivilegeAll,
        PrivilegeAuth,
        PrivilegeSign,
        PrivilegeEncrypt,
        PrivilegeElide,
        PrivilegeIssue,
        PrivilegeAccess,
        PrivilegeDelegate,
        PrivilegeVerify,
        PrivilegeUpdate,
        PrivilegeTransfer,
        PrivilegeElect,
        PrivilegeBurn,
        PrivilegeRevoke,
        Body,
        Result,
        Error,
        OkValue,
        ProcessingValue,
        Sender,
        SenderContinuation,
        RecipientContinuation,
        Content,
        SeedType,
        PrivateKeyType,
        PublicKeyType,
        MasterKeyType,
        Asset,
        BitcoinValue,
        EthereumValue,
        TezosValue,
        Network,
        MainNetValue,
        TestNetValue,
        BIP32KeyType,
        ChainCode,
        DerivationPathType,
        ParentPath,
        ChildrenPath,
        ParentFingerprint,
        PSBTType,
        OutputDescriptorType,
        OutputDescriptor,
        Graph,
        SourceTargetGraph,
        ParentChildGraph,
        Digraph,
        AcyclicGraph,
        Multigraph,
        Pseudograph,
        GraphFragment,
        DAG,
        Tree,
        Forest,
        CompoundGraph,
        Hypergraph,
        Dihypergraph,
        Node,
        Edge,
        Source,
        Target,
        Parent,
        Child,
        Self,
    ];

    // Preserve the current Rust implementation exactly: VALUE and SELF are
    // public constants but are not inserted into the lazy global registry.
    internal static readonly KnownValue[] DefaultRegistryValues =
    [
        Unit,
        IsA,
        Id,
        Signed,
        Note,
        HasRecipient,
        SSKRShare,
        Controller,
        Key,
        DereferenceVia,
        Entity,
        Name,
        Language,
        Issuer,
        Holder,
        Salt,
        Date,
        UnknownValue,
        VersionValue,
        HasSecret,
        DiffEdits,
        ValidFrom,
        ValidUntil,
        Position,
        Nickname,
        Attestation,
        VerifiableAt,
        Attachment,
        Vendor,
        ConformsTo,
        Allow,
        Deny,
        Endpoint,
        Delegate,
        Provenance,
        PrivateKey,
        Service,
        Capability,
        ProvenanceGenerator,
        PrivilegeAll,
        PrivilegeAuth,
        PrivilegeSign,
        PrivilegeEncrypt,
        PrivilegeElide,
        PrivilegeIssue,
        PrivilegeAccess,
        PrivilegeDelegate,
        PrivilegeVerify,
        PrivilegeUpdate,
        PrivilegeTransfer,
        PrivilegeElect,
        PrivilegeBurn,
        PrivilegeRevoke,
        Body,
        Result,
        Error,
        OkValue,
        ProcessingValue,
        Sender,
        SenderContinuation,
        RecipientContinuation,
        Content,
        SeedType,
        PrivateKeyType,
        PublicKeyType,
        MasterKeyType,
        Asset,
        BitcoinValue,
        EthereumValue,
        TezosValue,
        Network,
        MainNetValue,
        TestNetValue,
        BIP32KeyType,
        ChainCode,
        DerivationPathType,
        ParentPath,
        ChildrenPath,
        ParentFingerprint,
        PSBTType,
        OutputDescriptorType,
        OutputDescriptor,
        Graph,
        SourceTargetGraph,
        ParentChildGraph,
        Digraph,
        AcyclicGraph,
        Multigraph,
        Pseudograph,
        GraphFragment,
        DAG,
        Tree,
        Forest,
        CompoundGraph,
        Hypergraph,
        Dihypergraph,
        Node,
        Edge,
        Source,
        Target,
        Parent,
        Child,
    ];

    /// <summary>
    /// The global registry of known values.
    /// </summary>
    public static readonly LazyKnownValues KnownValues =
        new(CreateDefaultStore);

    private static KnownValuesStore CreateDefaultStore()
    {
        var store = new KnownValuesStore(DefaultRegistryValues);
        var config = DirectoryLoader.GetAndLockConfig();
        var result = DirectoryLoader.LoadFromConfig(config);

        foreach (var value in result.GetValues())
        {
            store.Insert(value);
        }

        return store;
    }

    internal static void ResetForTesting() => KnownValues.ResetForTesting();
}
