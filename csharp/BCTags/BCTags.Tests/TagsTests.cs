using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCTags.Tests;

public class TagsTests
{
    private static readonly (ulong Value, string Name)[] ExpectedTags =
    [
        (32ul, "url"),
        (37ul, "uuid"),
        (24ul, "encoded-cbor"),
        (200ul, "envelope"),
        (201ul, "leaf"),
        (262ul, "json"),
        (40000ul, "known-value"),
        (40001ul, "digest"),
        (40002ul, "encrypted"),
        (40003ul, "compressed"),
        (40004ul, "request"),
        (40005ul, "response"),
        (40006ul, "function"),
        (40007ul, "parameter"),
        (40008ul, "placeholder"),
        (40009ul, "replacement"),
        (40010ul, "agreement-private-key"),
        (40011ul, "agreement-public-key"),
        (40012ul, "arid"),
        (40013ul, "crypto-prvkeys"),
        (40014ul, "nonce"),
        (40015ul, "password"),
        (40016ul, "crypto-prvkey-base"),
        (40017ul, "crypto-pubkeys"),
        (40018ul, "salt"),
        (40019ul, "crypto-sealed"),
        (40020ul, "signature"),
        (40021ul, "signing-private-key"),
        (40022ul, "signing-public-key"),
        (40023ul, "crypto-key"),
        (40024ul, "xid"),
        (40025ul, "reference"),
        (40026ul, "event"),
        (40027ul, "encrypted-key"),
        (40100ul, "mlkem-private-key"),
        (40101ul, "mlkem-public-key"),
        (40102ul, "mlkem-ciphertext"),
        (40103ul, "mldsa-private-key"),
        (40104ul, "mldsa-public-key"),
        (40105ul, "mldsa-signature"),
        (40300ul, "seed"),
        (40303ul, "hdkey"),
        (40304ul, "keypath"),
        (40305ul, "coin-info"),
        (40306ul, "eckey"),
        (40307ul, "address"),
        (40308ul, "output-descriptor"),
        (40309ul, "sskr"),
        (40310ul, "psbt"),
        (40311ul, "account-descriptor"),
        (40800ul, "ssh-private"),
        (40801ul, "ssh-public"),
        (40802ul, "ssh-signature"),
        (40803ul, "ssh-certificate"),
        (1347571542ul, "provenance"),
        (300ul, "crypto-seed"),
        (306ul, "crypto-eckey"),
        (309ul, "crypto-sskr"),
        (303ul, "crypto-hdkey"),
        (304ul, "crypto-keypath"),
        (305ul, "crypto-coin-info"),
        (307ul, "crypto-output"),
        (310ul, "crypto-psbt"),
        (311ul, "crypto-account"),
        (400ul, "output-script-hash"),
        (401ul, "output-witness-script-hash"),
        (402ul, "output-public-key"),
        (403ul, "output-public-key-hash"),
        (404ul, "output-witness-public-key-hash"),
        (405ul, "output-combo"),
        (406ul, "output-multisig"),
        (407ul, "output-sorted-multisig"),
        (408ul, "output-raw-script"),
        (409ul, "output-taproot"),
        (410ul, "output-cosigner"),
    ];

    private static readonly (ulong Value, string Name)[] ActualTagsFromConstants =
    [
        (BcTags.TagUri, BcTags.TagNameUri),
        (BcTags.TagUuid, BcTags.TagNameUuid),
        (BcTags.TagEncodedCbor, BcTags.TagNameEncodedCbor),
        (BcTags.TagEnvelope, BcTags.TagNameEnvelope),
        (BcTags.TagLeaf, BcTags.TagNameLeaf),
        (BcTags.TagJson, BcTags.TagNameJson),
        (BcTags.TagKnownValue, BcTags.TagNameKnownValue),
        (BcTags.TagDigest, BcTags.TagNameDigest),
        (BcTags.TagEncrypted, BcTags.TagNameEncrypted),
        (BcTags.TagCompressed, BcTags.TagNameCompressed),
        (BcTags.TagRequest, BcTags.TagNameRequest),
        (BcTags.TagResponse, BcTags.TagNameResponse),
        (BcTags.TagFunction, BcTags.TagNameFunction),
        (BcTags.TagParameter, BcTags.TagNameParameter),
        (BcTags.TagPlaceholder, BcTags.TagNamePlaceholder),
        (BcTags.TagReplacement, BcTags.TagNameReplacement),
        (BcTags.TagX25519PrivateKey, BcTags.TagNameX25519PrivateKey),
        (BcTags.TagX25519PublicKey, BcTags.TagNameX25519PublicKey),
        (BcTags.TagArid, BcTags.TagNameArid),
        (BcTags.TagPrivateKeys, BcTags.TagNamePrivateKeys),
        (BcTags.TagNonce, BcTags.TagNameNonce),
        (BcTags.TagPassword, BcTags.TagNamePassword),
        (BcTags.TagPrivateKeyBase, BcTags.TagNamePrivateKeyBase),
        (BcTags.TagPublicKeys, BcTags.TagNamePublicKeys),
        (BcTags.TagSalt, BcTags.TagNameSalt),
        (BcTags.TagSealedMessage, BcTags.TagNameSealedMessage),
        (BcTags.TagSignature, BcTags.TagNameSignature),
        (BcTags.TagSigningPrivateKey, BcTags.TagNameSigningPrivateKey),
        (BcTags.TagSigningPublicKey, BcTags.TagNameSigningPublicKey),
        (BcTags.TagSymmetricKey, BcTags.TagNameSymmetricKey),
        (BcTags.TagXid, BcTags.TagNameXid),
        (BcTags.TagReference, BcTags.TagNameReference),
        (BcTags.TagEvent, BcTags.TagNameEvent),
        (BcTags.TagEncryptedKey, BcTags.TagNameEncryptedKey),
        (BcTags.TagMlkemPrivateKey, BcTags.TagNameMlkemPrivateKey),
        (BcTags.TagMlkemPublicKey, BcTags.TagNameMlkemPublicKey),
        (BcTags.TagMlkemCiphertext, BcTags.TagNameMlkemCiphertext),
        (BcTags.TagMldsaPrivateKey, BcTags.TagNameMldsaPrivateKey),
        (BcTags.TagMldsaPublicKey, BcTags.TagNameMldsaPublicKey),
        (BcTags.TagMldsaSignature, BcTags.TagNameMldsaSignature),
        (BcTags.TagSeed, BcTags.TagNameSeed),
        (BcTags.TagHdkey, BcTags.TagNameHdkey),
        (BcTags.TagDerivationPath, BcTags.TagNameDerivationPath),
        (BcTags.TagUseInfo, BcTags.TagNameUseInfo),
        (BcTags.TagEcKey, BcTags.TagNameEcKey),
        (BcTags.TagAddress, BcTags.TagNameAddress),
        (BcTags.TagOutputDescriptor, BcTags.TagNameOutputDescriptor),
        (BcTags.TagSskrShare, BcTags.TagNameSskrShare),
        (BcTags.TagPsbt, BcTags.TagNamePsbt),
        (BcTags.TagAccountDescriptor, BcTags.TagNameAccountDescriptor),
        (BcTags.TagSshTextPrivateKey, BcTags.TagNameSshTextPrivateKey),
        (BcTags.TagSshTextPublicKey, BcTags.TagNameSshTextPublicKey),
        (BcTags.TagSshTextSignature, BcTags.TagNameSshTextSignature),
        (BcTags.TagSshTextCertificate, BcTags.TagNameSshTextCertificate),
        (BcTags.TagProvenanceMark, BcTags.TagNameProvenanceMark),
        (BcTags.TagSeedV1, BcTags.TagNameSeedV1),
        (BcTags.TagEcKeyV1, BcTags.TagNameEcKeyV1),
        (BcTags.TagSskrShareV1, BcTags.TagNameSskrShareV1),
        (BcTags.TagHdkeyV1, BcTags.TagNameHdkeyV1),
        (BcTags.TagDerivationPathV1, BcTags.TagNameDerivationPathV1),
        (BcTags.TagUseInfoV1, BcTags.TagNameUseInfoV1),
        (BcTags.TagOutputDescriptorV1, BcTags.TagNameOutputDescriptorV1),
        (BcTags.TagPsbtV1, BcTags.TagNamePsbtV1),
        (BcTags.TagAccountV1, BcTags.TagNameAccountV1),
        (BcTags.TagOutputScriptHash, BcTags.TagNameOutputScriptHash),
        (BcTags.TagOutputWitnessScriptHash, BcTags.TagNameOutputWitnessScriptHash),
        (BcTags.TagOutputPublicKey, BcTags.TagNameOutputPublicKey),
        (BcTags.TagOutputPublicKeyHash, BcTags.TagNameOutputPublicKeyHash),
        (BcTags.TagOutputWitnessPublicKeyHash, BcTags.TagNameOutputWitnessPublicKeyHash),
        (BcTags.TagOutputCombo, BcTags.TagNameOutputCombo),
        (BcTags.TagOutputMultisig, BcTags.TagNameOutputMultisig),
        (BcTags.TagOutputSortedMultisig, BcTags.TagNameOutputSortedMultisig),
        (BcTags.TagOutputRawScript, BcTags.TagNameOutputRawScript),
        (BcTags.TagOutputTaproot, BcTags.TagNameOutputTaproot),
        (BcTags.TagOutputCosigner, BcTags.TagNameOutputCosigner),
    ];

    [Fact]
    public void TagConstantsMatchRustRegistry()
    {
        Assert.Equal(ExpectedTags.Length, ActualTagsFromConstants.Length);

        for (var index = 0; index < ExpectedTags.Length; index++)
        {
            Assert.Equal(ExpectedTags[index].Value, ActualTagsFromConstants[index].Value);
            Assert.Equal(ExpectedTags[index].Name, ActualTagsFromConstants[index].Name);
        }

        Assert.Equal(ExpectedTags.Length, ExpectedTags.Select(tag => tag.Value).Distinct().Count());
        Assert.Equal(ExpectedTags.Length, ExpectedTags.Select(tag => tag.Name).Distinct().Count());
    }

    [Fact]
    public void RegisterTagsInRegistersDcborAndBcTags()
    {
        var store = new TagsStore();

        BcTags.RegisterTagsIn(store);

        var dateTag = store.TagForValue(CborTags.TagDate);
        Assert.NotNull(dateTag);
        Assert.Equal(CborTags.TagNameDate, dateTag!.Name);

        foreach (var (value, name) in ExpectedTags)
        {
            var byValue = store.TagForValue(value);
            Assert.NotNull(byValue);
            Assert.Equal(name, byValue!.Name);

            var byName = store.TagForName(name);
            Assert.NotNull(byName);
            Assert.Equal(value, byName!.Value);
        }
    }

    [Fact]
    public void RegisterTagsRegistersInGlobalStore()
    {
        BcTags.RegisterTags();

        var envelopeTag = GlobalTags.WithTags(store => store.TagForValue(BcTags.TagEnvelope));
        Assert.NotNull(envelopeTag);
        Assert.Equal(BcTags.TagNameEnvelope, envelopeTag!.Name);

        var dateTag = GlobalTags.WithTags(store => store.TagForValue(CborTags.TagDate));
        Assert.NotNull(dateTag);
        Assert.Equal(CborTags.TagNameDate, dateTag!.Name);
    }
}
