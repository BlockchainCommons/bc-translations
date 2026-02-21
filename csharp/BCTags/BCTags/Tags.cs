using BlockchainCommons.DCbor;

namespace BlockchainCommons.BCTags;

/// <summary>
/// Blockchain Commons CBOR tag constants and registration helpers.
/// </summary>
public static class BcTags
{
    /// <summary>
    /// Numeric CBOR tag value for uri (url).
    /// </summary>
    public const ulong TagUri = 32ul;

    /// <summary>
    /// Canonical CBOR tag name for uri.
    /// </summary>
    public const string TagNameUri = "url";

    /// <summary>
    /// Numeric CBOR tag value for uuid (uuid).
    /// </summary>
    public const ulong TagUuid = 37ul;

    /// <summary>
    /// Canonical CBOR tag name for uuid.
    /// </summary>
    public const string TagNameUuid = "uuid";

    /// <summary>
    /// Numeric CBOR tag value for encoded cbor (encoded-cbor).
    /// </summary>
    public const ulong TagEncodedCbor = 24ul;

    /// <summary>
    /// Canonical CBOR tag name for encoded cbor.
    /// </summary>
    public const string TagNameEncodedCbor = "encoded-cbor";

    /// <summary>
    /// Numeric CBOR tag value for envelope (envelope).
    /// </summary>
    public const ulong TagEnvelope = 200ul;

    /// <summary>
    /// Canonical CBOR tag name for envelope.
    /// </summary>
    public const string TagNameEnvelope = "envelope";

    /// <summary>
    /// Numeric CBOR tag value for leaf (leaf).
    /// </summary>
    public const ulong TagLeaf = 201ul;

    /// <summary>
    /// Canonical CBOR tag name for leaf.
    /// </summary>
    public const string TagNameLeaf = "leaf";

    /// <summary>
    /// Numeric CBOR tag value for json (json).
    /// </summary>
    public const ulong TagJson = 262ul;

    /// <summary>
    /// Canonical CBOR tag name for json.
    /// </summary>
    public const string TagNameJson = "json";

    /// <summary>
    /// Numeric CBOR tag value for known value (known-value).
    /// </summary>
    public const ulong TagKnownValue = 40000ul;

    /// <summary>
    /// Canonical CBOR tag name for known value.
    /// </summary>
    public const string TagNameKnownValue = "known-value";

    /// <summary>
    /// Numeric CBOR tag value for digest (digest).
    /// </summary>
    public const ulong TagDigest = 40001ul;

    /// <summary>
    /// Canonical CBOR tag name for digest.
    /// </summary>
    public const string TagNameDigest = "digest";

    /// <summary>
    /// Numeric CBOR tag value for encrypted (encrypted).
    /// </summary>
    public const ulong TagEncrypted = 40002ul;

    /// <summary>
    /// Canonical CBOR tag name for encrypted.
    /// </summary>
    public const string TagNameEncrypted = "encrypted";

    /// <summary>
    /// Numeric CBOR tag value for compressed (compressed).
    /// </summary>
    public const ulong TagCompressed = 40003ul;

    /// <summary>
    /// Canonical CBOR tag name for compressed.
    /// </summary>
    public const string TagNameCompressed = "compressed";

    /// <summary>
    /// Numeric CBOR tag value for request (request).
    /// </summary>
    public const ulong TagRequest = 40004ul;

    /// <summary>
    /// Canonical CBOR tag name for request.
    /// </summary>
    public const string TagNameRequest = "request";

    /// <summary>
    /// Numeric CBOR tag value for response (response).
    /// </summary>
    public const ulong TagResponse = 40005ul;

    /// <summary>
    /// Canonical CBOR tag name for response.
    /// </summary>
    public const string TagNameResponse = "response";

    /// <summary>
    /// Numeric CBOR tag value for function (function).
    /// </summary>
    public const ulong TagFunction = 40006ul;

    /// <summary>
    /// Canonical CBOR tag name for function.
    /// </summary>
    public const string TagNameFunction = "function";

    /// <summary>
    /// Numeric CBOR tag value for parameter (parameter).
    /// </summary>
    public const ulong TagParameter = 40007ul;

    /// <summary>
    /// Canonical CBOR tag name for parameter.
    /// </summary>
    public const string TagNameParameter = "parameter";

    /// <summary>
    /// Numeric CBOR tag value for placeholder (placeholder).
    /// </summary>
    public const ulong TagPlaceholder = 40008ul;

    /// <summary>
    /// Canonical CBOR tag name for placeholder.
    /// </summary>
    public const string TagNamePlaceholder = "placeholder";

    /// <summary>
    /// Numeric CBOR tag value for replacement (replacement).
    /// </summary>
    public const ulong TagReplacement = 40009ul;

    /// <summary>
    /// Canonical CBOR tag name for replacement.
    /// </summary>
    public const string TagNameReplacement = "replacement";

    /// <summary>
    /// Numeric CBOR tag value for x25519 private key (agreement-private-key).
    /// </summary>
    public const ulong TagX25519PrivateKey = 40010ul;

    /// <summary>
    /// Canonical CBOR tag name for x25519 private key.
    /// </summary>
    public const string TagNameX25519PrivateKey = "agreement-private-key";

    /// <summary>
    /// Numeric CBOR tag value for x25519 public key (agreement-public-key).
    /// </summary>
    public const ulong TagX25519PublicKey = 40011ul;

    /// <summary>
    /// Canonical CBOR tag name for x25519 public key.
    /// </summary>
    public const string TagNameX25519PublicKey = "agreement-public-key";

    /// <summary>
    /// Numeric CBOR tag value for arid (arid).
    /// </summary>
    public const ulong TagArid = 40012ul;

    /// <summary>
    /// Canonical CBOR tag name for arid.
    /// </summary>
    public const string TagNameArid = "arid";

    /// <summary>
    /// Numeric CBOR tag value for private keys (crypto-prvkeys).
    /// </summary>
    public const ulong TagPrivateKeys = 40013ul;

    /// <summary>
    /// Canonical CBOR tag name for private keys.
    /// </summary>
    public const string TagNamePrivateKeys = "crypto-prvkeys";

    /// <summary>
    /// Numeric CBOR tag value for nonce (nonce).
    /// </summary>
    public const ulong TagNonce = 40014ul;

    /// <summary>
    /// Canonical CBOR tag name for nonce.
    /// </summary>
    public const string TagNameNonce = "nonce";

    /// <summary>
    /// Numeric CBOR tag value for password (password).
    /// </summary>
    public const ulong TagPassword = 40015ul;

    /// <summary>
    /// Canonical CBOR tag name for password.
    /// </summary>
    public const string TagNamePassword = "password";

    /// <summary>
    /// Numeric CBOR tag value for private key base (crypto-prvkey-base).
    /// </summary>
    public const ulong TagPrivateKeyBase = 40016ul;

    /// <summary>
    /// Canonical CBOR tag name for private key base.
    /// </summary>
    public const string TagNamePrivateKeyBase = "crypto-prvkey-base";

    /// <summary>
    /// Numeric CBOR tag value for public keys (crypto-pubkeys).
    /// </summary>
    public const ulong TagPublicKeys = 40017ul;

    /// <summary>
    /// Canonical CBOR tag name for public keys.
    /// </summary>
    public const string TagNamePublicKeys = "crypto-pubkeys";

    /// <summary>
    /// Numeric CBOR tag value for salt (salt).
    /// </summary>
    public const ulong TagSalt = 40018ul;

    /// <summary>
    /// Canonical CBOR tag name for salt.
    /// </summary>
    public const string TagNameSalt = "salt";

    /// <summary>
    /// Numeric CBOR tag value for sealed message (crypto-sealed).
    /// </summary>
    public const ulong TagSealedMessage = 40019ul;

    /// <summary>
    /// Canonical CBOR tag name for sealed message.
    /// </summary>
    public const string TagNameSealedMessage = "crypto-sealed";

    /// <summary>
    /// Numeric CBOR tag value for signature (signature).
    /// </summary>
    public const ulong TagSignature = 40020ul;

    /// <summary>
    /// Canonical CBOR tag name for signature.
    /// </summary>
    public const string TagNameSignature = "signature";

    /// <summary>
    /// Numeric CBOR tag value for signing private key (signing-private-key).
    /// </summary>
    public const ulong TagSigningPrivateKey = 40021ul;

    /// <summary>
    /// Canonical CBOR tag name for signing private key.
    /// </summary>
    public const string TagNameSigningPrivateKey = "signing-private-key";

    /// <summary>
    /// Numeric CBOR tag value for signing public key (signing-public-key).
    /// </summary>
    public const ulong TagSigningPublicKey = 40022ul;

    /// <summary>
    /// Canonical CBOR tag name for signing public key.
    /// </summary>
    public const string TagNameSigningPublicKey = "signing-public-key";

    /// <summary>
    /// Numeric CBOR tag value for symmetric key (crypto-key).
    /// </summary>
    public const ulong TagSymmetricKey = 40023ul;

    /// <summary>
    /// Canonical CBOR tag name for symmetric key.
    /// </summary>
    public const string TagNameSymmetricKey = "crypto-key";

    /// <summary>
    /// Numeric CBOR tag value for xid (xid).
    /// </summary>
    public const ulong TagXid = 40024ul;

    /// <summary>
    /// Canonical CBOR tag name for xid.
    /// </summary>
    public const string TagNameXid = "xid";

    /// <summary>
    /// Numeric CBOR tag value for reference (reference).
    /// </summary>
    public const ulong TagReference = 40025ul;

    /// <summary>
    /// Canonical CBOR tag name for reference.
    /// </summary>
    public const string TagNameReference = "reference";

    /// <summary>
    /// Numeric CBOR tag value for event (event).
    /// </summary>
    public const ulong TagEvent = 40026ul;

    /// <summary>
    /// Canonical CBOR tag name for event.
    /// </summary>
    public const string TagNameEvent = "event";

    /// <summary>
    /// Numeric CBOR tag value for encrypted key (encrypted-key).
    /// </summary>
    public const ulong TagEncryptedKey = 40027ul;

    /// <summary>
    /// Canonical CBOR tag name for encrypted key.
    /// </summary>
    public const string TagNameEncryptedKey = "encrypted-key";

    /// <summary>
    /// Numeric CBOR tag value for mlkem private key (mlkem-private-key).
    /// </summary>
    public const ulong TagMlkemPrivateKey = 40100ul;

    /// <summary>
    /// Canonical CBOR tag name for mlkem private key.
    /// </summary>
    public const string TagNameMlkemPrivateKey = "mlkem-private-key";

    /// <summary>
    /// Numeric CBOR tag value for mlkem public key (mlkem-public-key).
    /// </summary>
    public const ulong TagMlkemPublicKey = 40101ul;

    /// <summary>
    /// Canonical CBOR tag name for mlkem public key.
    /// </summary>
    public const string TagNameMlkemPublicKey = "mlkem-public-key";

    /// <summary>
    /// Numeric CBOR tag value for mlkem ciphertext (mlkem-ciphertext).
    /// </summary>
    public const ulong TagMlkemCiphertext = 40102ul;

    /// <summary>
    /// Canonical CBOR tag name for mlkem ciphertext.
    /// </summary>
    public const string TagNameMlkemCiphertext = "mlkem-ciphertext";

    /// <summary>
    /// Numeric CBOR tag value for mldsa private key (mldsa-private-key).
    /// </summary>
    public const ulong TagMldsaPrivateKey = 40103ul;

    /// <summary>
    /// Canonical CBOR tag name for mldsa private key.
    /// </summary>
    public const string TagNameMldsaPrivateKey = "mldsa-private-key";

    /// <summary>
    /// Numeric CBOR tag value for mldsa public key (mldsa-public-key).
    /// </summary>
    public const ulong TagMldsaPublicKey = 40104ul;

    /// <summary>
    /// Canonical CBOR tag name for mldsa public key.
    /// </summary>
    public const string TagNameMldsaPublicKey = "mldsa-public-key";

    /// <summary>
    /// Numeric CBOR tag value for mldsa signature (mldsa-signature).
    /// </summary>
    public const ulong TagMldsaSignature = 40105ul;

    /// <summary>
    /// Canonical CBOR tag name for mldsa signature.
    /// </summary>
    public const string TagNameMldsaSignature = "mldsa-signature";

    /// <summary>
    /// Numeric CBOR tag value for seed (seed).
    /// </summary>
    public const ulong TagSeed = 40300ul;

    /// <summary>
    /// Canonical CBOR tag name for seed.
    /// </summary>
    public const string TagNameSeed = "seed";

    /// <summary>
    /// Numeric CBOR tag value for hdkey (hdkey).
    /// </summary>
    public const ulong TagHdkey = 40303ul;

    /// <summary>
    /// Canonical CBOR tag name for hdkey.
    /// </summary>
    public const string TagNameHdkey = "hdkey";

    /// <summary>
    /// Numeric CBOR tag value for derivation path (keypath).
    /// </summary>
    public const ulong TagDerivationPath = 40304ul;

    /// <summary>
    /// Canonical CBOR tag name for derivation path.
    /// </summary>
    public const string TagNameDerivationPath = "keypath";

    /// <summary>
    /// Numeric CBOR tag value for use info (coin-info).
    /// </summary>
    public const ulong TagUseInfo = 40305ul;

    /// <summary>
    /// Canonical CBOR tag name for use info.
    /// </summary>
    public const string TagNameUseInfo = "coin-info";

    /// <summary>
    /// Numeric CBOR tag value for ec key (eckey).
    /// </summary>
    public const ulong TagEcKey = 40306ul;

    /// <summary>
    /// Canonical CBOR tag name for ec key.
    /// </summary>
    public const string TagNameEcKey = "eckey";

    /// <summary>
    /// Numeric CBOR tag value for address (address).
    /// </summary>
    public const ulong TagAddress = 40307ul;

    /// <summary>
    /// Canonical CBOR tag name for address.
    /// </summary>
    public const string TagNameAddress = "address";

    /// <summary>
    /// Numeric CBOR tag value for output descriptor (output-descriptor).
    /// </summary>
    public const ulong TagOutputDescriptor = 40308ul;

    /// <summary>
    /// Canonical CBOR tag name for output descriptor.
    /// </summary>
    public const string TagNameOutputDescriptor = "output-descriptor";

    /// <summary>
    /// Numeric CBOR tag value for sskr share (sskr).
    /// </summary>
    public const ulong TagSskrShare = 40309ul;

    /// <summary>
    /// Canonical CBOR tag name for sskr share.
    /// </summary>
    public const string TagNameSskrShare = "sskr";

    /// <summary>
    /// Numeric CBOR tag value for psbt (psbt).
    /// </summary>
    public const ulong TagPsbt = 40310ul;

    /// <summary>
    /// Canonical CBOR tag name for psbt.
    /// </summary>
    public const string TagNamePsbt = "psbt";

    /// <summary>
    /// Numeric CBOR tag value for account descriptor (account-descriptor).
    /// </summary>
    public const ulong TagAccountDescriptor = 40311ul;

    /// <summary>
    /// Canonical CBOR tag name for account descriptor.
    /// </summary>
    public const string TagNameAccountDescriptor = "account-descriptor";

    /// <summary>
    /// Numeric CBOR tag value for ssh text private key (ssh-private).
    /// </summary>
    public const ulong TagSshTextPrivateKey = 40800ul;

    /// <summary>
    /// Canonical CBOR tag name for ssh text private key.
    /// </summary>
    public const string TagNameSshTextPrivateKey = "ssh-private";

    /// <summary>
    /// Numeric CBOR tag value for ssh text public key (ssh-public).
    /// </summary>
    public const ulong TagSshTextPublicKey = 40801ul;

    /// <summary>
    /// Canonical CBOR tag name for ssh text public key.
    /// </summary>
    public const string TagNameSshTextPublicKey = "ssh-public";

    /// <summary>
    /// Numeric CBOR tag value for ssh text signature (ssh-signature).
    /// </summary>
    public const ulong TagSshTextSignature = 40802ul;

    /// <summary>
    /// Canonical CBOR tag name for ssh text signature.
    /// </summary>
    public const string TagNameSshTextSignature = "ssh-signature";

    /// <summary>
    /// Numeric CBOR tag value for ssh text certificate (ssh-certificate).
    /// </summary>
    public const ulong TagSshTextCertificate = 40803ul;

    /// <summary>
    /// Canonical CBOR tag name for ssh text certificate.
    /// </summary>
    public const string TagNameSshTextCertificate = "ssh-certificate";

    /// <summary>
    /// Numeric CBOR tag value for provenance mark (provenance).
    /// </summary>
    public const ulong TagProvenanceMark = 1347571542ul;

    /// <summary>
    /// Canonical CBOR tag name for provenance mark.
    /// </summary>
    public const string TagNameProvenanceMark = "provenance";

    /// <summary>
    /// Numeric CBOR tag value for seed v1 (crypto-seed).
    /// </summary>
    public const ulong TagSeedV1 = 300ul;

    /// <summary>
    /// Canonical CBOR tag name for seed v1.
    /// </summary>
    public const string TagNameSeedV1 = "crypto-seed";

    /// <summary>
    /// Numeric CBOR tag value for ec key v1 (crypto-eckey).
    /// </summary>
    public const ulong TagEcKeyV1 = 306ul;

    /// <summary>
    /// Canonical CBOR tag name for ec key v1.
    /// </summary>
    public const string TagNameEcKeyV1 = "crypto-eckey";

    /// <summary>
    /// Numeric CBOR tag value for sskr share v1 (crypto-sskr).
    /// </summary>
    public const ulong TagSskrShareV1 = 309ul;

    /// <summary>
    /// Canonical CBOR tag name for sskr share v1.
    /// </summary>
    public const string TagNameSskrShareV1 = "crypto-sskr";

    /// <summary>
    /// Numeric CBOR tag value for hdkey v1 (crypto-hdkey).
    /// </summary>
    public const ulong TagHdkeyV1 = 303ul;

    /// <summary>
    /// Canonical CBOR tag name for hdkey v1.
    /// </summary>
    public const string TagNameHdkeyV1 = "crypto-hdkey";

    /// <summary>
    /// Numeric CBOR tag value for derivation path v1 (crypto-keypath).
    /// </summary>
    public const ulong TagDerivationPathV1 = 304ul;

    /// <summary>
    /// Canonical CBOR tag name for derivation path v1.
    /// </summary>
    public const string TagNameDerivationPathV1 = "crypto-keypath";

    /// <summary>
    /// Numeric CBOR tag value for use info v1 (crypto-coin-info).
    /// </summary>
    public const ulong TagUseInfoV1 = 305ul;

    /// <summary>
    /// Canonical CBOR tag name for use info v1.
    /// </summary>
    public const string TagNameUseInfoV1 = "crypto-coin-info";

    /// <summary>
    /// Numeric CBOR tag value for output descriptor v1 (crypto-output).
    /// </summary>
    public const ulong TagOutputDescriptorV1 = 307ul;

    /// <summary>
    /// Canonical CBOR tag name for output descriptor v1.
    /// </summary>
    public const string TagNameOutputDescriptorV1 = "crypto-output";

    /// <summary>
    /// Numeric CBOR tag value for psbt v1 (crypto-psbt).
    /// </summary>
    public const ulong TagPsbtV1 = 310ul;

    /// <summary>
    /// Canonical CBOR tag name for psbt v1.
    /// </summary>
    public const string TagNamePsbtV1 = "crypto-psbt";

    /// <summary>
    /// Numeric CBOR tag value for account v1 (crypto-account).
    /// </summary>
    public const ulong TagAccountV1 = 311ul;

    /// <summary>
    /// Canonical CBOR tag name for account v1.
    /// </summary>
    public const string TagNameAccountV1 = "crypto-account";

    /// <summary>
    /// Numeric CBOR tag value for output script hash (output-script-hash).
    /// </summary>
    public const ulong TagOutputScriptHash = 400ul;

    /// <summary>
    /// Canonical CBOR tag name for output script hash.
    /// </summary>
    public const string TagNameOutputScriptHash = "output-script-hash";

    /// <summary>
    /// Numeric CBOR tag value for output witness script hash (output-witness-script-hash).
    /// </summary>
    public const ulong TagOutputWitnessScriptHash = 401ul;

    /// <summary>
    /// Canonical CBOR tag name for output witness script hash.
    /// </summary>
    public const string TagNameOutputWitnessScriptHash = "output-witness-script-hash";

    /// <summary>
    /// Numeric CBOR tag value for output public key (output-public-key).
    /// </summary>
    public const ulong TagOutputPublicKey = 402ul;

    /// <summary>
    /// Canonical CBOR tag name for output public key.
    /// </summary>
    public const string TagNameOutputPublicKey = "output-public-key";

    /// <summary>
    /// Numeric CBOR tag value for output public key hash (output-public-key-hash).
    /// </summary>
    public const ulong TagOutputPublicKeyHash = 403ul;

    /// <summary>
    /// Canonical CBOR tag name for output public key hash.
    /// </summary>
    public const string TagNameOutputPublicKeyHash = "output-public-key-hash";

    /// <summary>
    /// Numeric CBOR tag value for output witness public key hash (output-witness-public-key-hash).
    /// </summary>
    public const ulong TagOutputWitnessPublicKeyHash = 404ul;

    /// <summary>
    /// Canonical CBOR tag name for output witness public key hash.
    /// </summary>
    public const string TagNameOutputWitnessPublicKeyHash = "output-witness-public-key-hash";

    /// <summary>
    /// Numeric CBOR tag value for output combo (output-combo).
    /// </summary>
    public const ulong TagOutputCombo = 405ul;

    /// <summary>
    /// Canonical CBOR tag name for output combo.
    /// </summary>
    public const string TagNameOutputCombo = "output-combo";

    /// <summary>
    /// Numeric CBOR tag value for output multisig (output-multisig).
    /// </summary>
    public const ulong TagOutputMultisig = 406ul;

    /// <summary>
    /// Canonical CBOR tag name for output multisig.
    /// </summary>
    public const string TagNameOutputMultisig = "output-multisig";

    /// <summary>
    /// Numeric CBOR tag value for output sorted multisig (output-sorted-multisig).
    /// </summary>
    public const ulong TagOutputSortedMultisig = 407ul;

    /// <summary>
    /// Canonical CBOR tag name for output sorted multisig.
    /// </summary>
    public const string TagNameOutputSortedMultisig = "output-sorted-multisig";

    /// <summary>
    /// Numeric CBOR tag value for output raw script (output-raw-script).
    /// </summary>
    public const ulong TagOutputRawScript = 408ul;

    /// <summary>
    /// Canonical CBOR tag name for output raw script.
    /// </summary>
    public const string TagNameOutputRawScript = "output-raw-script";

    /// <summary>
    /// Numeric CBOR tag value for output taproot (output-taproot).
    /// </summary>
    public const ulong TagOutputTaproot = 409ul;

    /// <summary>
    /// Canonical CBOR tag name for output taproot.
    /// </summary>
    public const string TagNameOutputTaproot = "output-taproot";

    /// <summary>
    /// Numeric CBOR tag value for output cosigner (output-cosigner).
    /// </summary>
    public const ulong TagOutputCosigner = 410ul;

    /// <summary>
    /// Canonical CBOR tag name for output cosigner.
    /// </summary>
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
        new(TagSeedV1, TagNameSeedV1),
        new(TagEcKeyV1, TagNameEcKeyV1),
        new(TagSskrShareV1, TagNameSskrShareV1),
        new(TagHdkeyV1, TagNameHdkeyV1),
        new(TagDerivationPathV1, TagNameDerivationPathV1),
        new(TagUseInfoV1, TagNameUseInfoV1),
        new(TagOutputDescriptorV1, TagNameOutputDescriptorV1),
        new(TagPsbtV1, TagNamePsbtV1),
        new(TagAccountV1, TagNameAccountV1),
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
    /// Registers dcbor base tags, then all Blockchain Commons tags in the provided store.
    /// </summary>
    /// <param name="tagsStore">The target tag store.</param>
    public static void RegisterTagsIn(TagsStore tagsStore)
    {
        ArgumentNullException.ThrowIfNull(tagsStore);
        GlobalTags.RegisterTagsIn(tagsStore);
        tagsStore.InsertAll(TagsInRegistrationOrder);
    }

    /// <summary>
    /// Registers dcbor base tags and all Blockchain Commons tags in the global tag registry.
    /// </summary>
    public static void RegisterTags()
    {
        GlobalTags.WithTagsMut(RegisterTagsIn);
    }
}
