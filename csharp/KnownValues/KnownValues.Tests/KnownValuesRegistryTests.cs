namespace BlockchainCommons.KnownValues.Tests;

public sealed class KnownValuesRegistryTests : IDisposable
{
    private static readonly (ulong ExpectedValue, string ExpectedName, ulong RawConstant, KnownValue KnownValueConstant)[] ExpectedRegistry =
    [
        (0ul, "", KnownValuesRegistry.UnitRaw, KnownValuesRegistry.Unit),
        (1ul, "isA", KnownValuesRegistry.IsARaw, KnownValuesRegistry.IsA),
        (2ul, "id", KnownValuesRegistry.IdRaw, KnownValuesRegistry.Id),
        (3ul, "signed", KnownValuesRegistry.SignedRaw, KnownValuesRegistry.Signed),
        (4ul, "note", KnownValuesRegistry.NoteRaw, KnownValuesRegistry.Note),
        (5ul, "hasRecipient", KnownValuesRegistry.HasRecipientRaw, KnownValuesRegistry.HasRecipient),
        (6ul, "sskrShare", KnownValuesRegistry.SSKRShareRaw, KnownValuesRegistry.SSKRShare),
        (7ul, "controller", KnownValuesRegistry.ControllerRaw, KnownValuesRegistry.Controller),
        (8ul, "key", KnownValuesRegistry.KeyRaw, KnownValuesRegistry.Key),
        (9ul, "dereferenceVia", KnownValuesRegistry.DereferenceViaRaw, KnownValuesRegistry.DereferenceVia),
        (10ul, "entity", KnownValuesRegistry.EntityRaw, KnownValuesRegistry.Entity),
        (11ul, "name", KnownValuesRegistry.NameRaw, KnownValuesRegistry.Name),
        (12ul, "language", KnownValuesRegistry.LanguageRaw, KnownValuesRegistry.Language),
        (13ul, "issuer", KnownValuesRegistry.IssuerRaw, KnownValuesRegistry.Issuer),
        (14ul, "holder", KnownValuesRegistry.HolderRaw, KnownValuesRegistry.Holder),
        (15ul, "salt", KnownValuesRegistry.SaltRaw, KnownValuesRegistry.Salt),
        (16ul, "date", KnownValuesRegistry.DateRaw, KnownValuesRegistry.Date),
        (17ul, "Unknown", KnownValuesRegistry.UnknownValueRaw, KnownValuesRegistry.UnknownValue),
        (18ul, "version", KnownValuesRegistry.VersionValueRaw, KnownValuesRegistry.VersionValue),
        (19ul, "hasSecret", KnownValuesRegistry.HasSecretRaw, KnownValuesRegistry.HasSecret),
        (20ul, "edits", KnownValuesRegistry.DiffEditsRaw, KnownValuesRegistry.DiffEdits),
        (21ul, "validFrom", KnownValuesRegistry.ValidFromRaw, KnownValuesRegistry.ValidFrom),
        (22ul, "validUntil", KnownValuesRegistry.ValidUntilRaw, KnownValuesRegistry.ValidUntil),
        (23ul, "position", KnownValuesRegistry.PositionRaw, KnownValuesRegistry.Position),
        (24ul, "nickname", KnownValuesRegistry.NicknameRaw, KnownValuesRegistry.Nickname),
        (25ul, "value", KnownValuesRegistry.ValueRaw, KnownValuesRegistry.Value),
        (26ul, "attestation", KnownValuesRegistry.AttestationRaw, KnownValuesRegistry.Attestation),
        (27ul, "verifiableAt", KnownValuesRegistry.VerifiableAtRaw, KnownValuesRegistry.VerifiableAt),
        (50ul, "attachment", KnownValuesRegistry.AttachmentRaw, KnownValuesRegistry.Attachment),
        (51ul, "vendor", KnownValuesRegistry.VendorRaw, KnownValuesRegistry.Vendor),
        (52ul, "conformsTo", KnownValuesRegistry.ConformsToRaw, KnownValuesRegistry.ConformsTo),
        (60ul, "allow", KnownValuesRegistry.AllowRaw, KnownValuesRegistry.Allow),
        (61ul, "deny", KnownValuesRegistry.DenyRaw, KnownValuesRegistry.Deny),
        (62ul, "endpoint", KnownValuesRegistry.EndpointRaw, KnownValuesRegistry.Endpoint),
        (63ul, "delegate", KnownValuesRegistry.DelegateRaw, KnownValuesRegistry.Delegate),
        (64ul, "provenance", KnownValuesRegistry.ProvenanceRaw, KnownValuesRegistry.Provenance),
        (65ul, "privateKey", KnownValuesRegistry.PrivateKeyRaw, KnownValuesRegistry.PrivateKey),
        (66ul, "service", KnownValuesRegistry.ServiceRaw, KnownValuesRegistry.Service),
        (67ul, "capability", KnownValuesRegistry.CapabilityRaw, KnownValuesRegistry.Capability),
        (68ul, "provenanceGenerator", KnownValuesRegistry.ProvenanceGeneratorRaw, KnownValuesRegistry.ProvenanceGenerator),
        (70ul, "All", KnownValuesRegistry.PrivilegeAllRaw, KnownValuesRegistry.PrivilegeAll),
        (71ul, "Authorize", KnownValuesRegistry.PrivilegeAuthRaw, KnownValuesRegistry.PrivilegeAuth),
        (72ul, "Sign", KnownValuesRegistry.PrivilegeSignRaw, KnownValuesRegistry.PrivilegeSign),
        (73ul, "Encrypt", KnownValuesRegistry.PrivilegeEncryptRaw, KnownValuesRegistry.PrivilegeEncrypt),
        (74ul, "Elide", KnownValuesRegistry.PrivilegeElideRaw, KnownValuesRegistry.PrivilegeElide),
        (75ul, "Issue", KnownValuesRegistry.PrivilegeIssueRaw, KnownValuesRegistry.PrivilegeIssue),
        (76ul, "Access", KnownValuesRegistry.PrivilegeAccessRaw, KnownValuesRegistry.PrivilegeAccess),
        (80ul, "Delegate", KnownValuesRegistry.PrivilegeDelegateRaw, KnownValuesRegistry.PrivilegeDelegate),
        (81ul, "Verify", KnownValuesRegistry.PrivilegeVerifyRaw, KnownValuesRegistry.PrivilegeVerify),
        (82ul, "Update", KnownValuesRegistry.PrivilegeUpdateRaw, KnownValuesRegistry.PrivilegeUpdate),
        (83ul, "Transfer", KnownValuesRegistry.PrivilegeTransferRaw, KnownValuesRegistry.PrivilegeTransfer),
        (84ul, "Elect", KnownValuesRegistry.PrivilegeElectRaw, KnownValuesRegistry.PrivilegeElect),
        (85ul, "Burn", KnownValuesRegistry.PrivilegeBurnRaw, KnownValuesRegistry.PrivilegeBurn),
        (86ul, "Revoke", KnownValuesRegistry.PrivilegeRevokeRaw, KnownValuesRegistry.PrivilegeRevoke),
        (100ul, "body", KnownValuesRegistry.BodyRaw, KnownValuesRegistry.Body),
        (101ul, "result", KnownValuesRegistry.ResultRaw, KnownValuesRegistry.Result),
        (102ul, "error", KnownValuesRegistry.ErrorRaw, KnownValuesRegistry.Error),
        (103ul, "OK", KnownValuesRegistry.OkValueRaw, KnownValuesRegistry.OkValue),
        (104ul, "Processing", KnownValuesRegistry.ProcessingValueRaw, KnownValuesRegistry.ProcessingValue),
        (105ul, "sender", KnownValuesRegistry.SenderRaw, KnownValuesRegistry.Sender),
        (106ul, "senderContinuation", KnownValuesRegistry.SenderContinuationRaw, KnownValuesRegistry.SenderContinuation),
        (107ul, "recipientContinuation", KnownValuesRegistry.RecipientContinuationRaw, KnownValuesRegistry.RecipientContinuation),
        (108ul, "content", KnownValuesRegistry.ContentRaw, KnownValuesRegistry.Content),
        (200ul, "Seed", KnownValuesRegistry.SeedTypeRaw, KnownValuesRegistry.SeedType),
        (201ul, "PrivateKey", KnownValuesRegistry.PrivateKeyTypeRaw, KnownValuesRegistry.PrivateKeyType),
        (202ul, "PublicKey", KnownValuesRegistry.PublicKeyTypeRaw, KnownValuesRegistry.PublicKeyType),
        (203ul, "MasterKey", KnownValuesRegistry.MasterKeyTypeRaw, KnownValuesRegistry.MasterKeyType),
        (300ul, "asset", KnownValuesRegistry.AssetRaw, KnownValuesRegistry.Asset),
        (301ul, "Bitcoin", KnownValuesRegistry.BitcoinValueRaw, KnownValuesRegistry.BitcoinValue),
        (302ul, "Ethereum", KnownValuesRegistry.EthereumValueRaw, KnownValuesRegistry.EthereumValue),
        (303ul, "Tezos", KnownValuesRegistry.TezosValueRaw, KnownValuesRegistry.TezosValue),
        (400ul, "network", KnownValuesRegistry.NetworkRaw, KnownValuesRegistry.Network),
        (401ul, "MainNet", KnownValuesRegistry.MainNetValueRaw, KnownValuesRegistry.MainNetValue),
        (402ul, "TestNet", KnownValuesRegistry.TestNetValueRaw, KnownValuesRegistry.TestNetValue),
        (500ul, "BIP32Key", KnownValuesRegistry.BIP32KeyTypeRaw, KnownValuesRegistry.BIP32KeyType),
        (501ul, "chainCode", KnownValuesRegistry.ChainCodeRaw, KnownValuesRegistry.ChainCode),
        (502ul, "DerivationPath", KnownValuesRegistry.DerivationPathTypeRaw, KnownValuesRegistry.DerivationPathType),
        (503ul, "parentPath", KnownValuesRegistry.ParentPathRaw, KnownValuesRegistry.ParentPath),
        (504ul, "childrenPath", KnownValuesRegistry.ChildrenPathRaw, KnownValuesRegistry.ChildrenPath),
        (505ul, "parentFingerprint", KnownValuesRegistry.ParentFingerprintRaw, KnownValuesRegistry.ParentFingerprint),
        (506ul, "PSBT", KnownValuesRegistry.PSBTTypeRaw, KnownValuesRegistry.PSBTType),
        (507ul, "OutputDescriptor", KnownValuesRegistry.OutputDescriptorTypeRaw, KnownValuesRegistry.OutputDescriptorType),
        (508ul, "outputDescriptor", KnownValuesRegistry.OutputDescriptorRaw, KnownValuesRegistry.OutputDescriptor),
        (600ul, "Graph", KnownValuesRegistry.GraphRaw, KnownValuesRegistry.Graph),
        (601ul, "SourceTargetGraph", KnownValuesRegistry.SourceTargetGraphRaw, KnownValuesRegistry.SourceTargetGraph),
        (602ul, "ParentChildGraph", KnownValuesRegistry.ParentChildGraphRaw, KnownValuesRegistry.ParentChildGraph),
        (603ul, "Digraph", KnownValuesRegistry.DigraphRaw, KnownValuesRegistry.Digraph),
        (604ul, "AcyclicGraph", KnownValuesRegistry.AcyclicGraphRaw, KnownValuesRegistry.AcyclicGraph),
        (605ul, "Multigraph", KnownValuesRegistry.MultigraphRaw, KnownValuesRegistry.Multigraph),
        (606ul, "Pseudograph", KnownValuesRegistry.PseudographRaw, KnownValuesRegistry.Pseudograph),
        (607ul, "GraphFragment", KnownValuesRegistry.GraphFragmentRaw, KnownValuesRegistry.GraphFragment),
        (608ul, "DAG", KnownValuesRegistry.DAGRaw, KnownValuesRegistry.DAG),
        (609ul, "Tree", KnownValuesRegistry.TreeRaw, KnownValuesRegistry.Tree),
        (610ul, "Forest", KnownValuesRegistry.ForestRaw, KnownValuesRegistry.Forest),
        (611ul, "CompoundGraph", KnownValuesRegistry.CompoundGraphRaw, KnownValuesRegistry.CompoundGraph),
        (612ul, "Hypergraph", KnownValuesRegistry.HypergraphRaw, KnownValuesRegistry.Hypergraph),
        (613ul, "Dihypergraph", KnownValuesRegistry.DihypergraphRaw, KnownValuesRegistry.Dihypergraph),
        (700ul, "node", KnownValuesRegistry.NodeRaw, KnownValuesRegistry.Node),
        (701ul, "edge", KnownValuesRegistry.EdgeRaw, KnownValuesRegistry.Edge),
        (702ul, "source", KnownValuesRegistry.SourceRaw, KnownValuesRegistry.Source),
        (703ul, "target", KnownValuesRegistry.TargetRaw, KnownValuesRegistry.Target),
        (704ul, "parent", KnownValuesRegistry.ParentRaw, KnownValuesRegistry.Parent),
        (705ul, "child", KnownValuesRegistry.ChildRaw, KnownValuesRegistry.Child),
        (706ul, "Self", KnownValuesRegistry.SelfRaw, KnownValuesRegistry.Self),
    ];

    public KnownValuesRegistryTests()
    {
        KnownValuesTestHooks.ResetGlobalState();
        DirectoryLoader.SetDirectoryConfig(DirectoryConfig.WithPaths([]));
    }

    public void Dispose()
    {
        KnownValuesTestHooks.ResetGlobalState();
    }

    [Fact]
    public void RegistryConstantsMatchRustInventory()
    {
        Assert.Equal(104, ExpectedRegistry.Length);
        Assert.Equal(104, KnownValuesRegistry.AllKnownValues.Length);

        foreach (var (expectedValue, expectedName, rawConstant, knownValue) in ExpectedRegistry)
        {
            Assert.Equal(expectedValue, rawConstant);
            Assert.Equal(expectedValue, knownValue.Value);
            Assert.Equal(expectedName, knownValue.AssignedName);
            Assert.Equal(expectedName, knownValue.Name);
        }
    }

    [Fact]
    public void Test1()
    {
        Assert.Equal(1ul, KnownValuesRegistry.IsA.Value);
        Assert.Equal("isA", KnownValuesRegistry.IsA.Name);

        var knownValues = KnownValuesRegistry.KnownValues.Get();
        Assert.Equal(1ul, knownValues.KnownValueNamed("isA")!.Value);
    }

    [Fact]
    public void GlobalRegistryPreservesRustInitializerOmissions()
    {
        var knownValues = KnownValuesRegistry.KnownValues.Get();

        Assert.Null(knownValues.KnownValueNamed("value"));
        Assert.Null(knownValues.KnownValueNamed("Self"));
        Assert.Equal(1ul, knownValues.KnownValueNamed("isA")!.Value);
    }
}
