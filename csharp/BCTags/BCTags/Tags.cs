using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCTags;

/// <summary>
/// Well-known Blockchain Commons CBOR tag constants and registration helpers.
/// Each tag is a (value, name) pair that can be registered in a <see cref="TagsStore"/>
/// for human-readable CBOR diagnostic output.
/// </summary>
public static class BcTags
{
    /// <summary>Tag value for URI (RFC 3986).</summary>
    public const ulong TagUri = 32ul;
    /// <summary>Tag name for <see cref="TagUri"/>.</summary>
    public const string TagNameUri = "url";

    /// <summary>Tag value for UUID (RFC 4122).</summary>
    public const ulong TagUuid = 37ul;
    /// <summary>Tag name for <see cref="TagUuid"/>.</summary>
    public const string TagNameUuid = "uuid";

    /// <summary>Tag value for encoded CBOR data item.</summary>
    public const ulong TagEncodedCbor = 24ul;
    /// <summary>Tag name for <see cref="TagEncodedCbor"/>.</summary>
    public const string TagNameEncodedCbor = "encoded-cbor";

    /// <summary>Tag value for Gordian Envelope.</summary>
    public const ulong TagEnvelope = 200ul;
    /// <summary>Tag name for <see cref="TagEnvelope"/>.</summary>
    public const string TagNameEnvelope = "envelope";

    /// <summary>Tag value for Gordian Envelope leaf node.</summary>
    public const ulong TagLeaf = 201ul;
    /// <summary>Tag name for <see cref="TagLeaf"/>.</summary>
    public const string TagNameLeaf = "leaf";

    /// <summary>Tag value for embedded JSON text.</summary>
    public const ulong TagJson = 262ul;
    /// <summary>Tag name for <see cref="TagJson"/>.</summary>
    public const string TagNameJson = "json";

    /// <summary>Tag value for Gordian Known Value.</summary>
    public const ulong TagKnownValue = 40000ul;
    /// <summary>Tag name for <see cref="TagKnownValue"/>.</summary>
    public const string TagNameKnownValue = "known-value";

    /// <summary>Tag value for cryptographic digest.</summary>
    public const ulong TagDigest = 40001ul;
    /// <summary>Tag name for <see cref="TagDigest"/>.</summary>
    public const string TagNameDigest = "digest";

    /// <summary>Tag value for IETF-ChaCha20-Poly1305 encrypted message.</summary>
    public const ulong TagEncrypted = 40002ul;
    /// <summary>Tag name for <see cref="TagEncrypted"/>.</summary>
    public const string TagNameEncrypted = "encrypted";

    /// <summary>Tag value for compressed data.</summary>
    public const ulong TagCompressed = 40003ul;
    /// <summary>Tag name for <see cref="TagCompressed"/>.</summary>
    public const string TagNameCompressed = "compressed";

    /// <summary>Tag value for Gordian request.</summary>
    public const ulong TagRequest = 40004ul;
    /// <summary>Tag name for <see cref="TagRequest"/>.</summary>
    public const string TagNameRequest = "request";

    /// <summary>Tag value for Gordian response.</summary>
    public const ulong TagResponse = 40005ul;
    /// <summary>Tag name for <see cref="TagResponse"/>.</summary>
    public const string TagNameResponse = "response";

    /// <summary>Tag value for Gordian Envelope function call.</summary>
    public const ulong TagFunction = 40006ul;
    /// <summary>Tag name for <see cref="TagFunction"/>.</summary>
    public const string TagNameFunction = "function";

    /// <summary>Tag value for Gordian Envelope function parameter.</summary>
    public const ulong TagParameter = 40007ul;
    /// <summary>Tag name for <see cref="TagParameter"/>.</summary>
    public const string TagNameParameter = "parameter";

    /// <summary>Tag value for Gordian Envelope placeholder.</summary>
    public const ulong TagPlaceholder = 40008ul;
    /// <summary>Tag name for <see cref="TagPlaceholder"/>.</summary>
    public const string TagNamePlaceholder = "placeholder";

    /// <summary>Tag value for Gordian Envelope replacement.</summary>
    public const ulong TagReplacement = 40009ul;
    /// <summary>Tag name for <see cref="TagReplacement"/>.</summary>
    public const string TagNameReplacement = "replacement";

    /// <summary>Tag value for X25519 agreement private key.</summary>
    public const ulong TagX25519PrivateKey = 40010ul;
    /// <summary>Tag name for <see cref="TagX25519PrivateKey"/>.</summary>
    public const string TagNameX25519PrivateKey = "agreement-private-key";

    /// <summary>Tag value for X25519 agreement public key.</summary>
    public const ulong TagX25519PublicKey = 40011ul;
    /// <summary>Tag name for <see cref="TagX25519PublicKey"/>.</summary>
    public const string TagNameX25519PublicKey = "agreement-public-key";

    /// <summary>Tag value for Apparently Random Identifier (ARID).</summary>
    public const ulong TagArid = 40012ul;
    /// <summary>Tag name for <see cref="TagArid"/>.</summary>
    public const string TagNameArid = "arid";

    /// <summary>Tag value for private key set.</summary>
    public const ulong TagPrivateKeys = 40013ul;
    /// <summary>Tag name for <see cref="TagPrivateKeys"/>.</summary>
    public const string TagNamePrivateKeys = "crypto-prvkeys";

    /// <summary>Tag value for cryptographic nonce.</summary>
    public const ulong TagNonce = 40014ul;
    /// <summary>Tag name for <see cref="TagNonce"/>.</summary>
    public const string TagNameNonce = "nonce";

    /// <summary>Tag value for password.</summary>
    public const ulong TagPassword = 40015ul;
    /// <summary>Tag name for <see cref="TagPassword"/>.</summary>
    public const string TagNamePassword = "password";

    /// <summary>Tag value for private key base.</summary>
    public const ulong TagPrivateKeyBase = 40016ul;
    /// <summary>Tag name for <see cref="TagPrivateKeyBase"/>.</summary>
    public const string TagNamePrivateKeyBase = "crypto-prvkey-base";

    /// <summary>Tag value for public key set.</summary>
    public const ulong TagPublicKeys = 40017ul;
    /// <summary>Tag name for <see cref="TagPublicKeys"/>.</summary>
    public const string TagNamePublicKeys = "crypto-pubkeys";

    /// <summary>Tag value for cryptographic salt.</summary>
    public const ulong TagSalt = 40018ul;
    /// <summary>Tag name for <see cref="TagSalt"/>.</summary>
    public const string TagNameSalt = "salt";

    /// <summary>Tag value for sealed message (public-key encrypted).</summary>
    public const ulong TagSealedMessage = 40019ul;
    /// <summary>Tag name for <see cref="TagSealedMessage"/>.</summary>
    public const string TagNameSealedMessage = "crypto-sealed";

    /// <summary>Tag value for cryptographic signature.</summary>
    public const ulong TagSignature = 40020ul;
    /// <summary>Tag name for <see cref="TagSignature"/>.</summary>
    public const string TagNameSignature = "signature";

    /// <summary>Tag value for signing private key.</summary>
    public const ulong TagSigningPrivateKey = 40021ul;
    /// <summary>Tag name for <see cref="TagSigningPrivateKey"/>.</summary>
    public const string TagNameSigningPrivateKey = "signing-private-key";

    /// <summary>Tag value for signing public key.</summary>
    public const ulong TagSigningPublicKey = 40022ul;
    /// <summary>Tag name for <see cref="TagSigningPublicKey"/>.</summary>
    public const string TagNameSigningPublicKey = "signing-public-key";

    /// <summary>Tag value for symmetric encryption key.</summary>
    public const ulong TagSymmetricKey = 40023ul;
    /// <summary>Tag name for <see cref="TagSymmetricKey"/>.</summary>
    public const string TagNameSymmetricKey = "crypto-key";

    /// <summary>Tag value for XID (extended identifier).</summary>
    public const ulong TagXid = 40024ul;
    /// <summary>Tag name for <see cref="TagXid"/>.</summary>
    public const string TagNameXid = "xid";

    /// <summary>Tag value for Gordian reference.</summary>
    public const ulong TagReference = 40025ul;
    /// <summary>Tag name for <see cref="TagReference"/>.</summary>
    public const string TagNameReference = "reference";

    /// <summary>Tag value for Gordian event.</summary>
    public const ulong TagEvent = 40026ul;
    /// <summary>Tag name for <see cref="TagEvent"/>.</summary>
    public const string TagNameEvent = "event";

    /// <summary>Tag value for encrypted key.</summary>
    public const ulong TagEncryptedKey = 40027ul;
    /// <summary>Tag name for <see cref="TagEncryptedKey"/>.</summary>
    public const string TagNameEncryptedKey = "encrypted-key";

    /// <summary>Tag value for ML-KEM private key (post-quantum).</summary>
    public const ulong TagMlkemPrivateKey = 40100ul;
    /// <summary>Tag name for <see cref="TagMlkemPrivateKey"/>.</summary>
    public const string TagNameMlkemPrivateKey = "mlkem-private-key";

    /// <summary>Tag value for ML-KEM public key (post-quantum).</summary>
    public const ulong TagMlkemPublicKey = 40101ul;
    /// <summary>Tag name for <see cref="TagMlkemPublicKey"/>.</summary>
    public const string TagNameMlkemPublicKey = "mlkem-public-key";

    /// <summary>Tag value for ML-KEM ciphertext (post-quantum).</summary>
    public const ulong TagMlkemCiphertext = 40102ul;
    /// <summary>Tag name for <see cref="TagMlkemCiphertext"/>.</summary>
    public const string TagNameMlkemCiphertext = "mlkem-ciphertext";

    /// <summary>Tag value for ML-DSA private key (post-quantum).</summary>
    public const ulong TagMldsaPrivateKey = 40103ul;
    /// <summary>Tag name for <see cref="TagMldsaPrivateKey"/>.</summary>
    public const string TagNameMldsaPrivateKey = "mldsa-private-key";

    /// <summary>Tag value for ML-DSA public key (post-quantum).</summary>
    public const ulong TagMldsaPublicKey = 40104ul;
    /// <summary>Tag name for <see cref="TagMldsaPublicKey"/>.</summary>
    public const string TagNameMldsaPublicKey = "mldsa-public-key";

    /// <summary>Tag value for ML-DSA signature (post-quantum).</summary>
    public const ulong TagMldsaSignature = 40105ul;
    /// <summary>Tag name for <see cref="TagMldsaSignature"/>.</summary>
    public const string TagNameMldsaSignature = "mldsa-signature";

    /// <summary>Tag value for cryptographic seed.</summary>
    public const ulong TagSeed = 40300ul;
    /// <summary>Tag name for <see cref="TagSeed"/>.</summary>
    public const string TagNameSeed = "seed";

    /// <summary>Tag value for HD (hierarchical deterministic) key.</summary>
    public const ulong TagHdkey = 40303ul;
    /// <summary>Tag name for <see cref="TagHdkey"/>.</summary>
    public const string TagNameHdkey = "hdkey";

    /// <summary>Tag value for key derivation path.</summary>
    public const ulong TagDerivationPath = 40304ul;
    /// <summary>Tag name for <see cref="TagDerivationPath"/>.</summary>
    public const string TagNameDerivationPath = "keypath";

    /// <summary>Tag value for cryptocurrency use info.</summary>
    public const ulong TagUseInfo = 40305ul;
    /// <summary>Tag name for <see cref="TagUseInfo"/>.</summary>
    public const string TagNameUseInfo = "coin-info";

    /// <summary>Tag value for elliptic-curve key.</summary>
    public const ulong TagEcKey = 40306ul;
    /// <summary>Tag name for <see cref="TagEcKey"/>.</summary>
    public const string TagNameEcKey = "eckey";

    /// <summary>Tag value for cryptocurrency address.</summary>
    public const ulong TagAddress = 40307ul;
    /// <summary>Tag name for <see cref="TagAddress"/>.</summary>
    public const string TagNameAddress = "address";

    /// <summary>Tag value for output descriptor.</summary>
    public const ulong TagOutputDescriptor = 40308ul;
    /// <summary>Tag name for <see cref="TagOutputDescriptor"/>.</summary>
    public const string TagNameOutputDescriptor = "output-descriptor";

    /// <summary>Tag value for SSKR (Sharded Secret Key Reconstruction) share.</summary>
    public const ulong TagSskrShare = 40309ul;
    /// <summary>Tag name for <see cref="TagSskrShare"/>.</summary>
    public const string TagNameSskrShare = "sskr";

    /// <summary>Tag value for Partially Signed Bitcoin Transaction (PSBT).</summary>
    public const ulong TagPsbt = 40310ul;
    /// <summary>Tag name for <see cref="TagPsbt"/>.</summary>
    public const string TagNamePsbt = "psbt";

    /// <summary>Tag value for account descriptor.</summary>
    public const ulong TagAccountDescriptor = 40311ul;
    /// <summary>Tag name for <see cref="TagAccountDescriptor"/>.</summary>
    public const string TagNameAccountDescriptor = "account-descriptor";

    /// <summary>Tag value for SSH text-format private key.</summary>
    public const ulong TagSshTextPrivateKey = 40800ul;
    /// <summary>Tag name for <see cref="TagSshTextPrivateKey"/>.</summary>
    public const string TagNameSshTextPrivateKey = "ssh-private";

    /// <summary>Tag value for SSH text-format public key.</summary>
    public const ulong TagSshTextPublicKey = 40801ul;
    /// <summary>Tag name for <see cref="TagSshTextPublicKey"/>.</summary>
    public const string TagNameSshTextPublicKey = "ssh-public";

    /// <summary>Tag value for SSH text-format signature.</summary>
    public const ulong TagSshTextSignature = 40802ul;
    /// <summary>Tag name for <see cref="TagSshTextSignature"/>.</summary>
    public const string TagNameSshTextSignature = "ssh-signature";

    /// <summary>Tag value for SSH text-format certificate.</summary>
    public const ulong TagSshTextCertificate = 40803ul;
    /// <summary>Tag name for <see cref="TagSshTextCertificate"/>.</summary>
    public const string TagNameSshTextCertificate = "ssh-certificate";

    /// <summary>Tag value for provenance mark.</summary>
    public const ulong TagProvenanceMark = 1347571542ul;
    /// <summary>Tag name for <see cref="TagProvenanceMark"/>.</summary>
    public const string TagNameProvenanceMark = "provenance";

    /// <summary>Tag value for output script hash descriptor.</summary>
    public const ulong TagOutputScriptHash = 400ul;
    /// <summary>Tag name for <see cref="TagOutputScriptHash"/>.</summary>
    public const string TagNameOutputScriptHash = "output-script-hash";

    /// <summary>Tag value for output witness script hash descriptor.</summary>
    public const ulong TagOutputWitnessScriptHash = 401ul;
    /// <summary>Tag name for <see cref="TagOutputWitnessScriptHash"/>.</summary>
    public const string TagNameOutputWitnessScriptHash = "output-witness-script-hash";

    /// <summary>Tag value for output public key descriptor.</summary>
    public const ulong TagOutputPublicKey = 402ul;
    /// <summary>Tag name for <see cref="TagOutputPublicKey"/>.</summary>
    public const string TagNameOutputPublicKey = "output-public-key";

    /// <summary>Tag value for output public key hash descriptor.</summary>
    public const ulong TagOutputPublicKeyHash = 403ul;
    /// <summary>Tag name for <see cref="TagOutputPublicKeyHash"/>.</summary>
    public const string TagNameOutputPublicKeyHash = "output-public-key-hash";

    /// <summary>Tag value for output witness public key hash descriptor.</summary>
    public const ulong TagOutputWitnessPublicKeyHash = 404ul;
    /// <summary>Tag name for <see cref="TagOutputWitnessPublicKeyHash"/>.</summary>
    public const string TagNameOutputWitnessPublicKeyHash = "output-witness-public-key-hash";

    /// <summary>Tag value for output combo descriptor.</summary>
    public const ulong TagOutputCombo = 405ul;
    /// <summary>Tag name for <see cref="TagOutputCombo"/>.</summary>
    public const string TagNameOutputCombo = "output-combo";

    /// <summary>Tag value for output multisig descriptor.</summary>
    public const ulong TagOutputMultisig = 406ul;
    /// <summary>Tag name for <see cref="TagOutputMultisig"/>.</summary>
    public const string TagNameOutputMultisig = "output-multisig";

    /// <summary>Tag value for output sorted multisig descriptor.</summary>
    public const ulong TagOutputSortedMultisig = 407ul;
    /// <summary>Tag name for <see cref="TagOutputSortedMultisig"/>.</summary>
    public const string TagNameOutputSortedMultisig = "output-sorted-multisig";

    /// <summary>Tag value for output raw script descriptor.</summary>
    public const ulong TagOutputRawScript = 408ul;
    /// <summary>Tag name for <see cref="TagOutputRawScript"/>.</summary>
    public const string TagNameOutputRawScript = "output-raw-script";

    /// <summary>Tag value for output Taproot descriptor.</summary>
    public const ulong TagOutputTaproot = 409ul;
    /// <summary>Tag name for <see cref="TagOutputTaproot"/>.</summary>
    public const string TagNameOutputTaproot = "output-taproot";

    /// <summary>Tag value for output cosigner descriptor.</summary>
    public const ulong TagOutputCosigner = 410ul;
    /// <summary>Tag name for <see cref="TagOutputCosigner"/>.</summary>
    public const string TagNameOutputCosigner = "output-cosigner";

    private static readonly Tag[] TagsInRegistrationOrder =
    [
        new(TagUri, TagNameUri),
        new(TagUuid, TagNameUuid),
        new(TagEncodedCbor, TagNameEncodedCbor),
        new(TagEnvelope, TagNameEnvelope),
        new(TagLeaf, TagNameLeaf),
        new(TagJson, TagNameJson),
        new(TagKnownValue, TagNameKnownValue),
        new(TagDigest, TagNameDigest),
        new(TagEncrypted, TagNameEncrypted),
        new(TagCompressed, TagNameCompressed),
        new(TagRequest, TagNameRequest),
        new(TagResponse, TagNameResponse),
        new(TagFunction, TagNameFunction),
        new(TagParameter, TagNameParameter),
        new(TagPlaceholder, TagNamePlaceholder),
        new(TagReplacement, TagNameReplacement),
        new(TagX25519PrivateKey, TagNameX25519PrivateKey),
        new(TagX25519PublicKey, TagNameX25519PublicKey),
        new(TagArid, TagNameArid),
        new(TagPrivateKeys, TagNamePrivateKeys),
        new(TagNonce, TagNameNonce),
        new(TagPassword, TagNamePassword),
        new(TagPrivateKeyBase, TagNamePrivateKeyBase),
        new(TagPublicKeys, TagNamePublicKeys),
        new(TagSalt, TagNameSalt),
        new(TagSealedMessage, TagNameSealedMessage),
        new(TagSignature, TagNameSignature),
        new(TagSigningPrivateKey, TagNameSigningPrivateKey),
        new(TagSigningPublicKey, TagNameSigningPublicKey),
        new(TagSymmetricKey, TagNameSymmetricKey),
        new(TagXid, TagNameXid),
        new(TagReference, TagNameReference),
        new(TagEvent, TagNameEvent),
        new(TagEncryptedKey, TagNameEncryptedKey),
        new(TagMlkemPrivateKey, TagNameMlkemPrivateKey),
        new(TagMlkemPublicKey, TagNameMlkemPublicKey),
        new(TagMlkemCiphertext, TagNameMlkemCiphertext),
        new(TagMldsaPrivateKey, TagNameMldsaPrivateKey),
        new(TagMldsaPublicKey, TagNameMldsaPublicKey),
        new(TagMldsaSignature, TagNameMldsaSignature),
        new(TagSeed, TagNameSeed),
        new(TagHdkey, TagNameHdkey),
        new(TagDerivationPath, TagNameDerivationPath),
        new(TagUseInfo, TagNameUseInfo),
        new(TagEcKey, TagNameEcKey),
        new(TagAddress, TagNameAddress),
        new(TagOutputDescriptor, TagNameOutputDescriptor),
        new(TagSskrShare, TagNameSskrShare),
        new(TagPsbt, TagNamePsbt),
        new(TagAccountDescriptor, TagNameAccountDescriptor),
        new(TagSshTextPrivateKey, TagNameSshTextPrivateKey),
        new(TagSshTextPublicKey, TagNameSshTextPublicKey),
        new(TagSshTextSignature, TagNameSshTextSignature),
        new(TagSshTextCertificate, TagNameSshTextCertificate),
        new(TagProvenanceMark, TagNameProvenanceMark),
        new(TagOutputScriptHash, TagNameOutputScriptHash),
        new(TagOutputWitnessScriptHash, TagNameOutputWitnessScriptHash),
        new(TagOutputPublicKey, TagNameOutputPublicKey),
        new(TagOutputPublicKeyHash, TagNameOutputPublicKeyHash),
        new(TagOutputWitnessPublicKeyHash, TagNameOutputWitnessPublicKeyHash),
        new(TagOutputCombo, TagNameOutputCombo),
        new(TagOutputMultisig, TagNameOutputMultisig),
        new(TagOutputSortedMultisig, TagNameOutputSortedMultisig),
        new(TagOutputRawScript, TagNameOutputRawScript),
        new(TagOutputTaproot, TagNameOutputTaproot),
        new(TagOutputCosigner, TagNameOutputCosigner),
    ];

    /// <summary>
    /// Registers the built-in dcbor tags followed by all Blockchain Commons tags
    /// in the provided <paramref name="tagsStore"/>.
    /// </summary>
    /// <param name="tagsStore">The target tag store to populate.</param>
    public static void RegisterTagsIn(TagsStore tagsStore)
    {
        ArgumentNullException.ThrowIfNull(tagsStore);
        GlobalTags.RegisterTagsIn(tagsStore);
        tagsStore.InsertAll(TagsInRegistrationOrder);
    }

    /// <summary>
    /// Registers the built-in dcbor tags and all Blockchain Commons tags
    /// in the global tag registry.
    /// </summary>
    public static void RegisterTags()
    {
        GlobalTags.WithTagsMut(RegisterTagsIn);
    }
}
