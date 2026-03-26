package knownvalues

import "sync"

// LazyKnownValues lazily initializes the global known values registry.
type LazyKnownValues struct {
	once    sync.Once
	factory func() *KnownValuesStore
	data    *KnownValuesStore
}

func newLazyKnownValues(factory func() *KnownValuesStore) LazyKnownValues {
	return LazyKnownValues{factory: factory}
}

// Get returns the lazily initialized global known values store.
func (l *LazyKnownValues) Get() *KnownValuesStore {
	l.once.Do(func() {
		if l.factory != nil {
			l.data = l.factory()
		}
	})
	return l.data
}

const (
	UnitRaw                  uint64 = 0
	IsARaw                   uint64 = 1
	IDRaw                    uint64 = 2
	SignedRaw                uint64 = 3
	NoteRaw                  uint64 = 4
	HasRecipientRaw          uint64 = 5
	SSKRShareRaw             uint64 = 6
	ControllerRaw            uint64 = 7
	KeyRaw                   uint64 = 8
	DereferenceViaRaw        uint64 = 9
	EntityRaw                uint64 = 10
	NameRaw                  uint64 = 11
	LanguageRaw              uint64 = 12
	IssuerRaw                uint64 = 13
	HolderRaw                uint64 = 14
	SaltRaw                  uint64 = 15
	DateRaw                  uint64 = 16
	UnknownValueRaw          uint64 = 17
	VersionValueRaw          uint64 = 18
	HasSecretRaw             uint64 = 19
	DiffEditsRaw             uint64 = 20
	ValidFromRaw             uint64 = 21
	ValidUntilRaw            uint64 = 22
	PositionRaw              uint64 = 23
	NicknameRaw              uint64 = 24
	ValueRaw                 uint64 = 25
	AttestationRaw           uint64 = 26
	VerifiableAtRaw          uint64 = 27
	AttachmentRaw            uint64 = 50
	VendorRaw                uint64 = 51
	ConformsToRaw            uint64 = 52
	AllowRaw                 uint64 = 60
	DenyRaw                  uint64 = 61
	EndpointRaw              uint64 = 62
	DelegateRaw              uint64 = 63
	ProvenanceRaw            uint64 = 64
	PrivateKeyRaw            uint64 = 65
	ServiceRaw               uint64 = 66
	CapabilityRaw            uint64 = 67
	ProvenanceGeneratorRaw   uint64 = 68
	PrivilegeAllRaw          uint64 = 70
	PrivilegeAuthRaw         uint64 = 71
	PrivilegeSignRaw         uint64 = 72
	PrivilegeEncryptRaw      uint64 = 73
	PrivilegeElideRaw        uint64 = 74
	PrivilegeIssueRaw        uint64 = 75
	PrivilegeAccessRaw       uint64 = 76
	PrivilegeDelegateRaw     uint64 = 80
	PrivilegeVerifyRaw       uint64 = 81
	PrivilegeUpdateRaw       uint64 = 82
	PrivilegeTransferRaw     uint64 = 83
	PrivilegeElectRaw        uint64 = 84
	PrivilegeBurnRaw         uint64 = 85
	PrivilegeRevokeRaw       uint64 = 86
	BodyRaw                  uint64 = 100
	ResultRaw                uint64 = 101
	ErrorRaw                 uint64 = 102
	OKValueRaw               uint64 = 103
	ProcessingValueRaw       uint64 = 104
	SenderRaw                uint64 = 105
	SenderContinuationRaw    uint64 = 106
	RecipientContinuationRaw uint64 = 107
	ContentRaw               uint64 = 108
	SeedTypeRaw              uint64 = 200
	PrivateKeyTypeRaw        uint64 = 201
	PublicKeyTypeRaw         uint64 = 202
	MasterKeyTypeRaw         uint64 = 203
	AssetRaw                 uint64 = 300
	BitcoinValueRaw          uint64 = 301
	EthereumValueRaw         uint64 = 302
	TezosValueRaw            uint64 = 303
	NetworkRaw               uint64 = 400
	MainNetValueRaw          uint64 = 401
	TestNetValueRaw          uint64 = 402
	BIP32KeyTypeRaw          uint64 = 500
	ChainCodeRaw             uint64 = 501
	DerivationPathTypeRaw    uint64 = 502
	ParentPathRaw            uint64 = 503
	ChildrenPathRaw          uint64 = 504
	ParentFingerprintRaw     uint64 = 505
	PSBTTypeRaw              uint64 = 506
	OutputDescriptorTypeRaw  uint64 = 507
	OutputDescriptorRaw      uint64 = 508
	GraphRaw                 uint64 = 600
	SourceTargetGraphRaw     uint64 = 601
	ParentChildGraphRaw      uint64 = 602
	DigraphRaw               uint64 = 603
	AcyclicGraphRaw          uint64 = 604
	MultigraphRaw            uint64 = 605
	PseudographRaw           uint64 = 606
	GraphFragmentRaw         uint64 = 607
	DAGRaw                   uint64 = 608
	TreeRaw                  uint64 = 609
	ForestRaw                uint64 = 610
	CompoundGraphRaw         uint64 = 611
	HypergraphRaw            uint64 = 612
	DihypergraphRaw          uint64 = 613
	NodeRaw                  uint64 = 700
	EdgeRaw                  uint64 = 701
	SourceRaw                uint64 = 702
	TargetRaw                uint64 = 703
	ParentRaw                uint64 = 704
	ChildRaw                 uint64 = 705
	SelfRaw                  uint64 = 706
)

var (
	Unit                  = NewKnownValueWithStaticName(UnitRaw, "")
	IsA                   = NewKnownValueWithStaticName(IsARaw, "isA")
	ID                    = NewKnownValueWithStaticName(IDRaw, "id")
	Signed                = NewKnownValueWithStaticName(SignedRaw, "signed")
	Note                  = NewKnownValueWithStaticName(NoteRaw, "note")
	HasRecipient          = NewKnownValueWithStaticName(HasRecipientRaw, "hasRecipient")
	SSKRShare             = NewKnownValueWithStaticName(SSKRShareRaw, "sskrShare")
	Controller            = NewKnownValueWithStaticName(ControllerRaw, "controller")
	Key                   = NewKnownValueWithStaticName(KeyRaw, "key")
	DereferenceVia        = NewKnownValueWithStaticName(DereferenceViaRaw, "dereferenceVia")
	Entity                = NewKnownValueWithStaticName(EntityRaw, "entity")
	Name                  = NewKnownValueWithStaticName(NameRaw, "name")
	Language              = NewKnownValueWithStaticName(LanguageRaw, "language")
	Issuer                = NewKnownValueWithStaticName(IssuerRaw, "issuer")
	Holder                = NewKnownValueWithStaticName(HolderRaw, "holder")
	Salt                  = NewKnownValueWithStaticName(SaltRaw, "salt")
	Date                  = NewKnownValueWithStaticName(DateRaw, "date")
	UnknownValue          = NewKnownValueWithStaticName(UnknownValueRaw, "Unknown")
	VersionValue          = NewKnownValueWithStaticName(VersionValueRaw, "version")
	HasSecret             = NewKnownValueWithStaticName(HasSecretRaw, "hasSecret")
	DiffEdits             = NewKnownValueWithStaticName(DiffEditsRaw, "edits")
	ValidFrom             = NewKnownValueWithStaticName(ValidFromRaw, "validFrom")
	ValidUntil            = NewKnownValueWithStaticName(ValidUntilRaw, "validUntil")
	Position              = NewKnownValueWithStaticName(PositionRaw, "position")
	Nickname              = NewKnownValueWithStaticName(NicknameRaw, "nickname")
	Value                 = NewKnownValueWithStaticName(ValueRaw, "value")
	Attestation           = NewKnownValueWithStaticName(AttestationRaw, "attestation")
	VerifiableAt          = NewKnownValueWithStaticName(VerifiableAtRaw, "verifiableAt")
	Attachment            = NewKnownValueWithStaticName(AttachmentRaw, "attachment")
	Vendor                = NewKnownValueWithStaticName(VendorRaw, "vendor")
	ConformsTo            = NewKnownValueWithStaticName(ConformsToRaw, "conformsTo")
	Allow                 = NewKnownValueWithStaticName(AllowRaw, "allow")
	Deny                  = NewKnownValueWithStaticName(DenyRaw, "deny")
	Endpoint              = NewKnownValueWithStaticName(EndpointRaw, "endpoint")
	Delegate              = NewKnownValueWithStaticName(DelegateRaw, "delegate")
	Provenance            = NewKnownValueWithStaticName(ProvenanceRaw, "provenance")
	PrivateKey            = NewKnownValueWithStaticName(PrivateKeyRaw, "privateKey")
	Service               = NewKnownValueWithStaticName(ServiceRaw, "service")
	Capability            = NewKnownValueWithStaticName(CapabilityRaw, "capability")
	ProvenanceGenerator   = NewKnownValueWithStaticName(ProvenanceGeneratorRaw, "provenanceGenerator")
	PrivilegeAll          = NewKnownValueWithStaticName(PrivilegeAllRaw, "All")
	PrivilegeAuth         = NewKnownValueWithStaticName(PrivilegeAuthRaw, "Authorize")
	PrivilegeSign         = NewKnownValueWithStaticName(PrivilegeSignRaw, "Sign")
	PrivilegeEncrypt      = NewKnownValueWithStaticName(PrivilegeEncryptRaw, "Encrypt")
	PrivilegeElide        = NewKnownValueWithStaticName(PrivilegeElideRaw, "Elide")
	PrivilegeIssue        = NewKnownValueWithStaticName(PrivilegeIssueRaw, "Issue")
	PrivilegeAccess       = NewKnownValueWithStaticName(PrivilegeAccessRaw, "Access")
	PrivilegeDelegate     = NewKnownValueWithStaticName(PrivilegeDelegateRaw, "Delegate")
	PrivilegeVerify       = NewKnownValueWithStaticName(PrivilegeVerifyRaw, "Verify")
	PrivilegeUpdate       = NewKnownValueWithStaticName(PrivilegeUpdateRaw, "Update")
	PrivilegeTransfer     = NewKnownValueWithStaticName(PrivilegeTransferRaw, "Transfer")
	PrivilegeElect        = NewKnownValueWithStaticName(PrivilegeElectRaw, "Elect")
	PrivilegeBurn         = NewKnownValueWithStaticName(PrivilegeBurnRaw, "Burn")
	PrivilegeRevoke       = NewKnownValueWithStaticName(PrivilegeRevokeRaw, "Revoke")
	Body                  = NewKnownValueWithStaticName(BodyRaw, "body")
	Result                = NewKnownValueWithStaticName(ResultRaw, "result")
	Error                 = NewKnownValueWithStaticName(ErrorRaw, "error")
	OKValue               = NewKnownValueWithStaticName(OKValueRaw, "OK")
	ProcessingValue       = NewKnownValueWithStaticName(ProcessingValueRaw, "Processing")
	Sender                = NewKnownValueWithStaticName(SenderRaw, "sender")
	SenderContinuation    = NewKnownValueWithStaticName(SenderContinuationRaw, "senderContinuation")
	RecipientContinuation = NewKnownValueWithStaticName(RecipientContinuationRaw, "recipientContinuation")
	Content               = NewKnownValueWithStaticName(ContentRaw, "content")
	SeedType              = NewKnownValueWithStaticName(SeedTypeRaw, "Seed")
	PrivateKeyType        = NewKnownValueWithStaticName(PrivateKeyTypeRaw, "PrivateKey")
	PublicKeyType         = NewKnownValueWithStaticName(PublicKeyTypeRaw, "PublicKey")
	MasterKeyType         = NewKnownValueWithStaticName(MasterKeyTypeRaw, "MasterKey")
	Asset                 = NewKnownValueWithStaticName(AssetRaw, "asset")
	BitcoinValue          = NewKnownValueWithStaticName(BitcoinValueRaw, "Bitcoin")
	EthereumValue         = NewKnownValueWithStaticName(EthereumValueRaw, "Ethereum")
	TezosValue            = NewKnownValueWithStaticName(TezosValueRaw, "Tezos")
	Network               = NewKnownValueWithStaticName(NetworkRaw, "network")
	MainNetValue          = NewKnownValueWithStaticName(MainNetValueRaw, "MainNet")
	TestNetValue          = NewKnownValueWithStaticName(TestNetValueRaw, "TestNet")
	BIP32KeyType          = NewKnownValueWithStaticName(BIP32KeyTypeRaw, "BIP32Key")
	ChainCode             = NewKnownValueWithStaticName(ChainCodeRaw, "chainCode")
	DerivationPathType    = NewKnownValueWithStaticName(DerivationPathTypeRaw, "DerivationPath")
	ParentPath            = NewKnownValueWithStaticName(ParentPathRaw, "parentPath")
	ChildrenPath          = NewKnownValueWithStaticName(ChildrenPathRaw, "childrenPath")
	ParentFingerprint     = NewKnownValueWithStaticName(ParentFingerprintRaw, "parentFingerprint")
	PSBTType              = NewKnownValueWithStaticName(PSBTTypeRaw, "PSBT")
	OutputDescriptorType  = NewKnownValueWithStaticName(OutputDescriptorTypeRaw, "OutputDescriptor")
	OutputDescriptor      = NewKnownValueWithStaticName(OutputDescriptorRaw, "outputDescriptor")
	Graph                 = NewKnownValueWithStaticName(GraphRaw, "Graph")
	SourceTargetGraph     = NewKnownValueWithStaticName(SourceTargetGraphRaw, "SourceTargetGraph")
	ParentChildGraph      = NewKnownValueWithStaticName(ParentChildGraphRaw, "ParentChildGraph")
	Digraph               = NewKnownValueWithStaticName(DigraphRaw, "Digraph")
	AcyclicGraph          = NewKnownValueWithStaticName(AcyclicGraphRaw, "AcyclicGraph")
	Multigraph            = NewKnownValueWithStaticName(MultigraphRaw, "Multigraph")
	Pseudograph           = NewKnownValueWithStaticName(PseudographRaw, "Pseudograph")
	GraphFragment         = NewKnownValueWithStaticName(GraphFragmentRaw, "GraphFragment")
	DAG                   = NewKnownValueWithStaticName(DAGRaw, "DAG")
	Tree                  = NewKnownValueWithStaticName(TreeRaw, "Tree")
	Forest                = NewKnownValueWithStaticName(ForestRaw, "Forest")
	CompoundGraph         = NewKnownValueWithStaticName(CompoundGraphRaw, "CompoundGraph")
	Hypergraph            = NewKnownValueWithStaticName(HypergraphRaw, "Hypergraph")
	Dihypergraph          = NewKnownValueWithStaticName(DihypergraphRaw, "Dihypergraph")
	Node                  = NewKnownValueWithStaticName(NodeRaw, "node")
	Edge                  = NewKnownValueWithStaticName(EdgeRaw, "edge")
	Source                = NewKnownValueWithStaticName(SourceRaw, "source")
	Target                = NewKnownValueWithStaticName(TargetRaw, "target")
	Parent                = NewKnownValueWithStaticName(ParentRaw, "parent")
	Child                 = NewKnownValueWithStaticName(ChildRaw, "child")
	Self                  = NewKnownValueWithStaticName(SelfRaw, "Self")

	allKnownValues = []KnownValue{
		Unit,
		IsA,
		ID,
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
		OKValue,
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
	}

	defaultKnownValues = []KnownValue{
		Unit,
		IsA,
		ID,
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
		OKValue,
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
	}

	// KnownValues is the global lazily initialized registry.
	KnownValues = newLazyKnownValues(newDefaultKnownValuesStore)
)

func newDefaultKnownValuesStore() *KnownValuesStore {
	store := NewKnownValuesStore(defaultKnownValues...)
	config := getAndLockConfig()
	result := LoadFromConfig(config)
	for _, value := range result.IntoValues() {
		store.Insert(value)
	}
	return store
}
