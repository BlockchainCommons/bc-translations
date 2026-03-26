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
	Unit                  = newKnownValueWithStaticName(UnitRaw, "")
	IsA                   = newKnownValueWithStaticName(IsARaw, "isA")
	ID                    = newKnownValueWithStaticName(IDRaw, "id")
	Signed                = newKnownValueWithStaticName(SignedRaw, "signed")
	Note                  = newKnownValueWithStaticName(NoteRaw, "note")
	HasRecipient          = newKnownValueWithStaticName(HasRecipientRaw, "hasRecipient")
	SSKRShare             = newKnownValueWithStaticName(SSKRShareRaw, "sskrShare")
	Controller            = newKnownValueWithStaticName(ControllerRaw, "controller")
	Key                   = newKnownValueWithStaticName(KeyRaw, "key")
	DereferenceVia        = newKnownValueWithStaticName(DereferenceViaRaw, "dereferenceVia")
	Entity                = newKnownValueWithStaticName(EntityRaw, "entity")
	Name                  = newKnownValueWithStaticName(NameRaw, "name")
	Language              = newKnownValueWithStaticName(LanguageRaw, "language")
	Issuer                = newKnownValueWithStaticName(IssuerRaw, "issuer")
	Holder                = newKnownValueWithStaticName(HolderRaw, "holder")
	Salt                  = newKnownValueWithStaticName(SaltRaw, "salt")
	Date                  = newKnownValueWithStaticName(DateRaw, "date")
	UnknownValue          = newKnownValueWithStaticName(UnknownValueRaw, "Unknown")
	VersionValue          = newKnownValueWithStaticName(VersionValueRaw, "version")
	HasSecret             = newKnownValueWithStaticName(HasSecretRaw, "hasSecret")
	DiffEdits             = newKnownValueWithStaticName(DiffEditsRaw, "edits")
	ValidFrom             = newKnownValueWithStaticName(ValidFromRaw, "validFrom")
	ValidUntil            = newKnownValueWithStaticName(ValidUntilRaw, "validUntil")
	Position              = newKnownValueWithStaticName(PositionRaw, "position")
	Nickname              = newKnownValueWithStaticName(NicknameRaw, "nickname")
	Value                 = newKnownValueWithStaticName(ValueRaw, "value")
	Attestation           = newKnownValueWithStaticName(AttestationRaw, "attestation")
	VerifiableAt          = newKnownValueWithStaticName(VerifiableAtRaw, "verifiableAt")
	Attachment            = newKnownValueWithStaticName(AttachmentRaw, "attachment")
	Vendor                = newKnownValueWithStaticName(VendorRaw, "vendor")
	ConformsTo            = newKnownValueWithStaticName(ConformsToRaw, "conformsTo")
	Allow                 = newKnownValueWithStaticName(AllowRaw, "allow")
	Deny                  = newKnownValueWithStaticName(DenyRaw, "deny")
	Endpoint              = newKnownValueWithStaticName(EndpointRaw, "endpoint")
	Delegate              = newKnownValueWithStaticName(DelegateRaw, "delegate")
	Provenance            = newKnownValueWithStaticName(ProvenanceRaw, "provenance")
	PrivateKey            = newKnownValueWithStaticName(PrivateKeyRaw, "privateKey")
	Service               = newKnownValueWithStaticName(ServiceRaw, "service")
	Capability            = newKnownValueWithStaticName(CapabilityRaw, "capability")
	ProvenanceGenerator   = newKnownValueWithStaticName(ProvenanceGeneratorRaw, "provenanceGenerator")
	PrivilegeAll          = newKnownValueWithStaticName(PrivilegeAllRaw, "All")
	PrivilegeAuth         = newKnownValueWithStaticName(PrivilegeAuthRaw, "Authorize")
	PrivilegeSign         = newKnownValueWithStaticName(PrivilegeSignRaw, "Sign")
	PrivilegeEncrypt      = newKnownValueWithStaticName(PrivilegeEncryptRaw, "Encrypt")
	PrivilegeElide        = newKnownValueWithStaticName(PrivilegeElideRaw, "Elide")
	PrivilegeIssue        = newKnownValueWithStaticName(PrivilegeIssueRaw, "Issue")
	PrivilegeAccess       = newKnownValueWithStaticName(PrivilegeAccessRaw, "Access")
	PrivilegeDelegate     = newKnownValueWithStaticName(PrivilegeDelegateRaw, "Delegate")
	PrivilegeVerify       = newKnownValueWithStaticName(PrivilegeVerifyRaw, "Verify")
	PrivilegeUpdate       = newKnownValueWithStaticName(PrivilegeUpdateRaw, "Update")
	PrivilegeTransfer     = newKnownValueWithStaticName(PrivilegeTransferRaw, "Transfer")
	PrivilegeElect        = newKnownValueWithStaticName(PrivilegeElectRaw, "Elect")
	PrivilegeBurn         = newKnownValueWithStaticName(PrivilegeBurnRaw, "Burn")
	PrivilegeRevoke       = newKnownValueWithStaticName(PrivilegeRevokeRaw, "Revoke")
	Body                  = newKnownValueWithStaticName(BodyRaw, "body")
	Result                = newKnownValueWithStaticName(ResultRaw, "result")
	Error                 = newKnownValueWithStaticName(ErrorRaw, "error")
	OKValue               = newKnownValueWithStaticName(OKValueRaw, "OK")
	ProcessingValue       = newKnownValueWithStaticName(ProcessingValueRaw, "Processing")
	Sender                = newKnownValueWithStaticName(SenderRaw, "sender")
	SenderContinuation    = newKnownValueWithStaticName(SenderContinuationRaw, "senderContinuation")
	RecipientContinuation = newKnownValueWithStaticName(RecipientContinuationRaw, "recipientContinuation")
	Content               = newKnownValueWithStaticName(ContentRaw, "content")
	SeedType              = newKnownValueWithStaticName(SeedTypeRaw, "Seed")
	PrivateKeyType        = newKnownValueWithStaticName(PrivateKeyTypeRaw, "PrivateKey")
	PublicKeyType         = newKnownValueWithStaticName(PublicKeyTypeRaw, "PublicKey")
	MasterKeyType         = newKnownValueWithStaticName(MasterKeyTypeRaw, "MasterKey")
	Asset                 = newKnownValueWithStaticName(AssetRaw, "asset")
	BitcoinValue          = newKnownValueWithStaticName(BitcoinValueRaw, "Bitcoin")
	EthereumValue         = newKnownValueWithStaticName(EthereumValueRaw, "Ethereum")
	TezosValue            = newKnownValueWithStaticName(TezosValueRaw, "Tezos")
	Network               = newKnownValueWithStaticName(NetworkRaw, "network")
	MainNetValue          = newKnownValueWithStaticName(MainNetValueRaw, "MainNet")
	TestNetValue          = newKnownValueWithStaticName(TestNetValueRaw, "TestNet")
	BIP32KeyType          = newKnownValueWithStaticName(BIP32KeyTypeRaw, "BIP32Key")
	ChainCode             = newKnownValueWithStaticName(ChainCodeRaw, "chainCode")
	DerivationPathType    = newKnownValueWithStaticName(DerivationPathTypeRaw, "DerivationPath")
	ParentPath            = newKnownValueWithStaticName(ParentPathRaw, "parentPath")
	ChildrenPath          = newKnownValueWithStaticName(ChildrenPathRaw, "childrenPath")
	ParentFingerprint     = newKnownValueWithStaticName(ParentFingerprintRaw, "parentFingerprint")
	PSBTType              = newKnownValueWithStaticName(PSBTTypeRaw, "PSBT")
	OutputDescriptorType  = newKnownValueWithStaticName(OutputDescriptorTypeRaw, "OutputDescriptor")
	OutputDescriptor      = newKnownValueWithStaticName(OutputDescriptorRaw, "outputDescriptor")
	Graph                 = newKnownValueWithStaticName(GraphRaw, "Graph")
	SourceTargetGraph     = newKnownValueWithStaticName(SourceTargetGraphRaw, "SourceTargetGraph")
	ParentChildGraph      = newKnownValueWithStaticName(ParentChildGraphRaw, "ParentChildGraph")
	Digraph               = newKnownValueWithStaticName(DigraphRaw, "Digraph")
	AcyclicGraph          = newKnownValueWithStaticName(AcyclicGraphRaw, "AcyclicGraph")
	Multigraph            = newKnownValueWithStaticName(MultigraphRaw, "Multigraph")
	Pseudograph           = newKnownValueWithStaticName(PseudographRaw, "Pseudograph")
	GraphFragment         = newKnownValueWithStaticName(GraphFragmentRaw, "GraphFragment")
	DAG                   = newKnownValueWithStaticName(DAGRaw, "DAG")
	Tree                  = newKnownValueWithStaticName(TreeRaw, "Tree")
	Forest                = newKnownValueWithStaticName(ForestRaw, "Forest")
	CompoundGraph         = newKnownValueWithStaticName(CompoundGraphRaw, "CompoundGraph")
	Hypergraph            = newKnownValueWithStaticName(HypergraphRaw, "Hypergraph")
	Dihypergraph          = newKnownValueWithStaticName(DihypergraphRaw, "Dihypergraph")
	Node                  = newKnownValueWithStaticName(NodeRaw, "node")
	Edge                  = newKnownValueWithStaticName(EdgeRaw, "edge")
	Source                = newKnownValueWithStaticName(SourceRaw, "source")
	Target                = newKnownValueWithStaticName(TargetRaw, "target")
	Parent                = newKnownValueWithStaticName(ParentRaw, "parent")
	Child                 = newKnownValueWithStaticName(ChildRaw, "child")
	Self                  = newKnownValueWithStaticName(SelfRaw, "Self")

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
	for _, value := range result.Values() {
		store.Insert(value)
	}
	return store
}
