package knownvalues

import "testing"

type registryExpectation struct {
	raw   uint64
	value KnownValue
	name  string
}

var expectedRegistry = []registryExpectation{
	{raw: UnitRaw, value: Unit, name: ""},
	{raw: IsARaw, value: IsA, name: "isA"},
	{raw: IDRaw, value: ID, name: "id"},
	{raw: SignedRaw, value: Signed, name: "signed"},
	{raw: NoteRaw, value: Note, name: "note"},
	{raw: HasRecipientRaw, value: HasRecipient, name: "hasRecipient"},
	{raw: SSKRShareRaw, value: SSKRShare, name: "sskrShare"},
	{raw: ControllerRaw, value: Controller, name: "controller"},
	{raw: KeyRaw, value: Key, name: "key"},
	{raw: DereferenceViaRaw, value: DereferenceVia, name: "dereferenceVia"},
	{raw: EntityRaw, value: Entity, name: "entity"},
	{raw: NameRaw, value: Name, name: "name"},
	{raw: LanguageRaw, value: Language, name: "language"},
	{raw: IssuerRaw, value: Issuer, name: "issuer"},
	{raw: HolderRaw, value: Holder, name: "holder"},
	{raw: SaltRaw, value: Salt, name: "salt"},
	{raw: DateRaw, value: Date, name: "date"},
	{raw: UnknownValueRaw, value: UnknownValue, name: "Unknown"},
	{raw: VersionValueRaw, value: VersionValue, name: "version"},
	{raw: HasSecretRaw, value: HasSecret, name: "hasSecret"},
	{raw: DiffEditsRaw, value: DiffEdits, name: "edits"},
	{raw: ValidFromRaw, value: ValidFrom, name: "validFrom"},
	{raw: ValidUntilRaw, value: ValidUntil, name: "validUntil"},
	{raw: PositionRaw, value: Position, name: "position"},
	{raw: NicknameRaw, value: Nickname, name: "nickname"},
	{raw: ValueRaw, value: Value, name: "value"},
	{raw: AttestationRaw, value: Attestation, name: "attestation"},
	{raw: VerifiableAtRaw, value: VerifiableAt, name: "verifiableAt"},
	{raw: AttachmentRaw, value: Attachment, name: "attachment"},
	{raw: VendorRaw, value: Vendor, name: "vendor"},
	{raw: ConformsToRaw, value: ConformsTo, name: "conformsTo"},
	{raw: AllowRaw, value: Allow, name: "allow"},
	{raw: DenyRaw, value: Deny, name: "deny"},
	{raw: EndpointRaw, value: Endpoint, name: "endpoint"},
	{raw: DelegateRaw, value: Delegate, name: "delegate"},
	{raw: ProvenanceRaw, value: Provenance, name: "provenance"},
	{raw: PrivateKeyRaw, value: PrivateKey, name: "privateKey"},
	{raw: ServiceRaw, value: Service, name: "service"},
	{raw: CapabilityRaw, value: Capability, name: "capability"},
	{raw: ProvenanceGeneratorRaw, value: ProvenanceGenerator, name: "provenanceGenerator"},
	{raw: PrivilegeAllRaw, value: PrivilegeAll, name: "All"},
	{raw: PrivilegeAuthRaw, value: PrivilegeAuth, name: "Authorize"},
	{raw: PrivilegeSignRaw, value: PrivilegeSign, name: "Sign"},
	{raw: PrivilegeEncryptRaw, value: PrivilegeEncrypt, name: "Encrypt"},
	{raw: PrivilegeElideRaw, value: PrivilegeElide, name: "Elide"},
	{raw: PrivilegeIssueRaw, value: PrivilegeIssue, name: "Issue"},
	{raw: PrivilegeAccessRaw, value: PrivilegeAccess, name: "Access"},
	{raw: PrivilegeDelegateRaw, value: PrivilegeDelegate, name: "Delegate"},
	{raw: PrivilegeVerifyRaw, value: PrivilegeVerify, name: "Verify"},
	{raw: PrivilegeUpdateRaw, value: PrivilegeUpdate, name: "Update"},
	{raw: PrivilegeTransferRaw, value: PrivilegeTransfer, name: "Transfer"},
	{raw: PrivilegeElectRaw, value: PrivilegeElect, name: "Elect"},
	{raw: PrivilegeBurnRaw, value: PrivilegeBurn, name: "Burn"},
	{raw: PrivilegeRevokeRaw, value: PrivilegeRevoke, name: "Revoke"},
	{raw: BodyRaw, value: Body, name: "body"},
	{raw: ResultRaw, value: Result, name: "result"},
	{raw: ErrorRaw, value: Error, name: "error"},
	{raw: OKValueRaw, value: OKValue, name: "OK"},
	{raw: ProcessingValueRaw, value: ProcessingValue, name: "Processing"},
	{raw: SenderRaw, value: Sender, name: "sender"},
	{raw: SenderContinuationRaw, value: SenderContinuation, name: "senderContinuation"},
	{raw: RecipientContinuationRaw, value: RecipientContinuation, name: "recipientContinuation"},
	{raw: ContentRaw, value: Content, name: "content"},
	{raw: SeedTypeRaw, value: SeedType, name: "Seed"},
	{raw: PrivateKeyTypeRaw, value: PrivateKeyType, name: "PrivateKey"},
	{raw: PublicKeyTypeRaw, value: PublicKeyType, name: "PublicKey"},
	{raw: MasterKeyTypeRaw, value: MasterKeyType, name: "MasterKey"},
	{raw: AssetRaw, value: Asset, name: "asset"},
	{raw: BitcoinValueRaw, value: BitcoinValue, name: "Bitcoin"},
	{raw: EthereumValueRaw, value: EthereumValue, name: "Ethereum"},
	{raw: TezosValueRaw, value: TezosValue, name: "Tezos"},
	{raw: NetworkRaw, value: Network, name: "network"},
	{raw: MainNetValueRaw, value: MainNetValue, name: "MainNet"},
	{raw: TestNetValueRaw, value: TestNetValue, name: "TestNet"},
	{raw: BIP32KeyTypeRaw, value: BIP32KeyType, name: "BIP32Key"},
	{raw: ChainCodeRaw, value: ChainCode, name: "chainCode"},
	{raw: DerivationPathTypeRaw, value: DerivationPathType, name: "DerivationPath"},
	{raw: ParentPathRaw, value: ParentPath, name: "parentPath"},
	{raw: ChildrenPathRaw, value: ChildrenPath, name: "childrenPath"},
	{raw: ParentFingerprintRaw, value: ParentFingerprint, name: "parentFingerprint"},
	{raw: PSBTTypeRaw, value: PSBTType, name: "PSBT"},
	{raw: OutputDescriptorTypeRaw, value: OutputDescriptorType, name: "OutputDescriptor"},
	{raw: OutputDescriptorRaw, value: OutputDescriptor, name: "outputDescriptor"},
	{raw: GraphRaw, value: Graph, name: "Graph"},
	{raw: SourceTargetGraphRaw, value: SourceTargetGraph, name: "SourceTargetGraph"},
	{raw: ParentChildGraphRaw, value: ParentChildGraph, name: "ParentChildGraph"},
	{raw: DigraphRaw, value: Digraph, name: "Digraph"},
	{raw: AcyclicGraphRaw, value: AcyclicGraph, name: "AcyclicGraph"},
	{raw: MultigraphRaw, value: Multigraph, name: "Multigraph"},
	{raw: PseudographRaw, value: Pseudograph, name: "Pseudograph"},
	{raw: GraphFragmentRaw, value: GraphFragment, name: "GraphFragment"},
	{raw: DAGRaw, value: DAG, name: "DAG"},
	{raw: TreeRaw, value: Tree, name: "Tree"},
	{raw: ForestRaw, value: Forest, name: "Forest"},
	{raw: CompoundGraphRaw, value: CompoundGraph, name: "CompoundGraph"},
	{raw: HypergraphRaw, value: Hypergraph, name: "Hypergraph"},
	{raw: DihypergraphRaw, value: Dihypergraph, name: "Dihypergraph"},
	{raw: NodeRaw, value: Node, name: "node"},
	{raw: EdgeRaw, value: Edge, name: "edge"},
	{raw: SourceRaw, value: Source, name: "source"},
	{raw: TargetRaw, value: Target, name: "target"},
	{raw: ParentRaw, value: Parent, name: "parent"},
	{raw: ChildRaw, value: Child, name: "child"},
	{raw: SelfRaw, value: Self, name: "Self"},
}

func TestRegistryConstantsMatchRustInventory(t *testing.T) {
	if got, want := len(expectedRegistry), 104; got != want {
		t.Fatalf("expectedRegistry length mismatch: got %d want %d", got, want)
	}
	if got, want := len(allKnownValues), 104; got != want {
		t.Fatalf("allKnownValues length mismatch: got %d want %d", got, want)
	}

	for _, entry := range expectedRegistry {
		if got, want := entry.raw, entry.value.Value(); got != want {
			t.Fatalf("raw/value mismatch for %q: got raw=%d value=%d", entry.name, got, want)
		}
		if got, want := entry.value.Name(), entry.name; got != want {
			t.Fatalf("name mismatch for raw=%d: got %q want %q", entry.raw, got, want)
		}
		if got, ok := entry.value.AssignedName(); !ok || got != entry.name {
			t.Fatalf("AssignedName mismatch for raw=%d: got %q ok=%t want %q", entry.raw, got, ok, entry.name)
		}
	}
}

func TestRegistrySmokeMatchesRustTest1(t *testing.T) {
	resetGlobalStateForTesting()
	t.Cleanup(resetGlobalStateForTesting)
	mustSetEmptyGlobalConfig(t)

	if got, want := IsA.Value(), uint64(1); got != want {
		t.Fatalf("IsA.Value mismatch: got %d want %d", got, want)
	}
	if got, want := IsA.Name(), "isA"; got != want {
		t.Fatalf("IsA.Name mismatch: got %q want %q", got, want)
	}

	knownValues := KnownValues.Get()
	got, ok := knownValues.KnownValueNamed("isA")
	if !ok || got.Value() != 1 {
		t.Fatalf("global store lookup mismatch: got %v ok=%t", got, ok)
	}
}

func TestGlobalRegistryPreservesRustInitializerOmissions(t *testing.T) {
	resetGlobalStateForTesting()
	t.Cleanup(resetGlobalStateForTesting)
	mustSetEmptyGlobalConfig(t)

	knownValues := KnownValues.Get()
	if _, ok := knownValues.KnownValueNamed("value"); ok {
		t.Fatalf("global registry should omit Value to match Rust source")
	}
	if _, ok := knownValues.KnownValueNamed("Self"); ok {
		t.Fatalf("global registry should omit Self to match Rust source")
	}
	if got, ok := knownValues.KnownValueNamed("isA"); !ok || got.Value() != 1 {
		t.Fatalf("expected hardcoded registry lookup to succeed: got %v ok=%t", got, ok)
	}
}
