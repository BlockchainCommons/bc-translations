/**
 * Secure Components for TypeScript.
 *
 * Translation of Rust `bc-components`.
 */

export { BCComponentsError as Error, BCComponentsError, type Result } from './error.js';

export { Digest } from './digest.js';

export { ARID } from './id/arid.js';
export { URI } from './id/uri.js';
export { UUID } from './id/uuid.js';
export { XID, type XIDProvider } from './id/xid.js';

export type { DigestProvider } from './digest-provider.js';

export { Compressed } from './compressed.js';

export { Nonce } from './nonce.js';

export {
    AuthenticationTag,
} from './symmetric/authentication-tag.js';
export {
    EncryptedMessage,
} from './symmetric/encrypted-message.js';
export {
    SymmetricKey,
} from './symmetric/symmetric-key.js';

export { SALT_LEN } from './encrypted-key/hkdf-params.js';
export { HashType } from './encrypted-key/hash-type.js';
export {
    KeyDerivationMethod,
} from './encrypted-key/key-derivation-method.js';
export { HKDFParams } from './encrypted-key/hkdf-params.js';
export { PBKDF2Params } from './encrypted-key/pbkdf2-params.js';
export { ScryptParams } from './encrypted-key/scrypt-params.js';
export { Argon2idParams } from './encrypted-key/argon2id-params.js';
export { KeyDerivationParams } from './encrypted-key/key-derivation-params.js';
export { EncryptedKey } from './encrypted-key/encrypted-key.js';

export { Salt } from './salt.js';

export { JSON } from './json.js';

export { X25519PrivateKey } from './x25519/x25519-private-key.js';
export { X25519PublicKey } from './x25519/x25519-public-key.js';

export { Ed25519PrivateKey } from './ed25519/ed25519-private-key.js';
export { Ed25519PublicKey } from './ed25519/ed25519-public-key.js';

export { Seed } from './seed.js';

export { Signature } from './signing/signature.js';
export { SignatureScheme } from './signing/signature-scheme.js';
export type { Signer, Verifier } from './signing/signer.js';
export {
    type SigningOptions,
    SigningPrivateKey,
} from './signing/signing-private-key.js';
export { SigningPublicKey } from './signing/signing-public-key.js';

export type { Encrypter, Decrypter } from './encrypter.js';

export {
    ECDSA_PRIVATE_KEY_SIZE,
    ECDSA_PUBLIC_KEY_SIZE,
    ECDSA_UNCOMPRESSED_PUBLIC_KEY_SIZE,
    SCHNORR_PUBLIC_KEY_SIZE,
} from '@bc/crypto';
export type { ECKey, ECKeyBase } from './ec-key/ec-key-base.js';
export type { ECPublicKeyBase } from './ec-key/ec-public-key-base.js';
export { ECPrivateKey } from './ec-key/ec-private-key.js';
export { ECPublicKey } from './ec-key/ec-public-key.js';
export { ECUncompressedPublicKey } from './ec-key/ec-uncompressed-public-key.js';
export { SchnorrPublicKey } from './ec-key/schnorr-public-key.js';

export { Reference, type ReferenceProvider } from './reference.js';

export { registerTags, registerTagsIn } from './tags-registry.js';
export * as tags from '@bc/tags';

export type { PrivateKeyDataProvider } from './private-key-data-provider.js';

export { PrivateKeyBase } from './private-key-base.js';

export { PrivateKeys, type PrivateKeysProvider } from './private-keys.js';

export { PublicKeys, type PublicKeysProvider } from './public-keys.js';

export { MLDSA } from './mldsa/mldsa-level.js';
export { MLDSAPrivateKey } from './mldsa/mldsa-private-key.js';
export { MLDSAPublicKey } from './mldsa/mldsa-public-key.js';
export { MLDSASignature } from './mldsa/mldsa-signature.js';

export { MLKEM } from './mlkem/mlkem-level.js';
export { MLKEMPrivateKey } from './mlkem/mlkem-private-key.js';
export { MLKEMPublicKey } from './mlkem/mlkem-public-key.js';
export { MLKEMCiphertext } from './mlkem/mlkem-ciphertext.js';

export { EncapsulationScheme } from './encapsulation/encapsulation-scheme.js';
export { EncapsulationPrivateKey } from './encapsulation/encapsulation-private-key.js';
export { EncapsulationPublicKey } from './encapsulation/encapsulation-public-key.js';
export { EncapsulationCiphertext } from './encapsulation/encapsulation-ciphertext.js';
export { SealedMessage } from './encapsulation/sealed-message.js';

export {
    SSKRError,
    SSKRGroupSpec,
    SSKRSecret,
    SSKRSpec,
    SSKRShare,
    sskrCombine,
    sskrGenerate,
    sskrGenerateUsing,
} from './sskr-mod.js';

export { HKDFRng } from './hkdf-rng.js';

export {
    keypair,
    keypairUsing,
    keypairOpt,
    keypairOptUsing,
} from './keypair.js';
