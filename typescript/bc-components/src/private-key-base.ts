import {
    type RandomNumberGenerator,
    SecureRandomNumberGenerator,
    rngRandomData,
} from '@bc/rand';
import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_PRIVATE_KEY_BASE } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import {
    decodeTaggedCbor,
    defaultTaggedCbor,
    defaultTaggedCborData,
    fromTaggedUrString,
    toUrString,
} from './cbor-ur.js';
import type { Decrypter } from './encrypter.js';
import { type ReferenceProvider, Reference } from './reference.js';
import { Digest } from './digest.js';
import { BCComponentsError } from './error.js';
import type { PrivateKeyDataProvider } from './private-key-data-provider.js';
import { PrivateKeys, type PrivateKeysProvider } from './private-keys.js';
import { PublicKeys, type PublicKeysProvider } from './public-keys.js';
import { EncapsulationPrivateKey } from './encapsulation/encapsulation-private-key.js';
import { EncapsulationPublicKey } from './encapsulation/encapsulation-public-key.js';
import { ECPrivateKey } from './ec-key/ec-private-key.js';
import { Ed25519PrivateKey } from './ed25519/ed25519-private-key.js';
import { X25519PrivateKey } from './x25519/x25519-private-key.js';
import type { Signature } from './signing/signature.js';
import type { Signer, Verifier } from './signing/signer.js';
import { type SigningOptions, SigningPrivateKey } from './signing/signing-private-key.js';

const PRIVATE_KEY_BASE_TAG: Tag = createTag(TAG_PRIVATE_KEY_BASE, 'crypto-prvkey-base');

export class PrivateKeyBase
    implements
        PrivateKeyDataProvider,
        PrivateKeysProvider,
        PublicKeysProvider,
        Signer,
        Verifier,
        Decrypter,
        ReferenceProvider {
    readonly #data: Uint8Array;

    private constructor(data: Uint8Array) {
        this.#data = new Uint8Array(data);
    }

    static new(): PrivateKeyBase {
        const rng = new SecureRandomNumberGenerator();
        return PrivateKeyBase.newUsing(rng);
    }

    static fromData(data: Uint8Array): PrivateKeyBase {
        return new PrivateKeyBase(data);
    }

    static fromOptionalData(data?: Uint8Array): PrivateKeyBase {
        return data === undefined ? PrivateKeyBase.new() : PrivateKeyBase.fromData(data);
    }

    static newUsing(rng: RandomNumberGenerator): PrivateKeyBase {
        return PrivateKeyBase.fromData(rngRandomData(rng, 32));
    }

    static newWithProvider(provider: PrivateKeyDataProvider): PrivateKeyBase {
        return PrivateKeyBase.fromData(provider.privateKeyData());
    }

    ecdsaSigningPrivateKey(): SigningPrivateKey {
        return SigningPrivateKey.newEcdsa(
            ECPrivateKey.deriveFromKeyMaterial(this.#data),
        );
    }

    schnorrSigningPrivateKey(): SigningPrivateKey {
        return SigningPrivateKey.newSchnorr(
            ECPrivateKey.deriveFromKeyMaterial(this.#data),
        );
    }

    ed25519SigningPrivateKey(): SigningPrivateKey {
        return SigningPrivateKey.newEd25519(
            Ed25519PrivateKey.deriveFromKeyMaterial(this.#data),
        );
    }

    sshSigningPrivateKey(_algorithm: string, _comment: string): never {
        throw BCComponentsError.unsupported(
            'SSH key generation is not supported in this translation',
        );
    }

    x25519PrivateKey(): X25519PrivateKey {
        return X25519PrivateKey.deriveFromKeyMaterial(this.#data);
    }

    schnorrPrivateKeys(): PrivateKeys {
        return PrivateKeys.withKeys(
            this.schnorrSigningPrivateKey(),
            EncapsulationPrivateKey.fromX25519(this.x25519PrivateKey()),
        );
    }

    schnorrPublicKeys(): PublicKeys {
        return PublicKeys.new(
            this.schnorrSigningPrivateKey().publicKey(),
            EncapsulationPublicKey.fromX25519(this.x25519PrivateKey().publicKey()),
        );
    }

    ecdsaPrivateKeys(): PrivateKeys {
        return PrivateKeys.withKeys(
            this.ecdsaSigningPrivateKey(),
            EncapsulationPrivateKey.fromX25519(this.x25519PrivateKey()),
        );
    }

    ecdsaPublicKeys(): PublicKeys {
        return PublicKeys.new(
            this.ecdsaSigningPrivateKey().publicKey(),
            EncapsulationPublicKey.fromX25519(this.x25519PrivateKey().publicKey()),
        );
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    privateKeyData(): Uint8Array {
        return this.asBytes();
    }

    privateKeys(): PrivateKeys {
        return PrivateKeys.withKeys(
            this.schnorrSigningPrivateKey(),
            EncapsulationPrivateKey.fromX25519(this.x25519PrivateKey()),
        );
    }

    publicKeys(): PublicKeys {
        return this.schnorrPublicKeys();
    }

    signWithOptions(message: Uint8Array, options?: SigningOptions): Signature {
        return this.schnorrSigningPrivateKey().signWithOptions(message, options);
    }

    sign(message: Uint8Array): Signature {
        return this.schnorrSigningPrivateKey().sign(message);
    }

    verify(signature: Signature, message: Uint8Array): boolean {
        const schnorr = signature.toSchnorr();
        if (schnorr === undefined) {
            return false;
        }
        return this
            .schnorrSigningPrivateKey()
            .toSchnorr()!
            .publicKey()
            .verify(schnorr, message);
    }

    encapsulationPrivateKey(): EncapsulationPrivateKey {
        return EncapsulationPrivateKey.fromX25519(this.x25519PrivateKey());
    }

    decapsulateSharedSecret(ciphertext: Parameters<Decrypter['decapsulateSharedSecret']>[0]) {
        return this.encapsulationPrivateKey().decapsulateSharedSecret(ciphertext);
    }

    cborTags(): Tag[] {
        return [PRIVATE_KEY_BASE_TAG];
    }

    untaggedCbor(): Cbor {
        return toByteString(this.#data);
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    urString(): string {
        return toUrString(this);
    }

    static cborTags(): Tag[] {
        return [PRIVATE_KEY_BASE_TAG];
    }

    static fromUntaggedCbor(untaggedCbor: Cbor): PrivateKeyBase {
        return PrivateKeyBase.fromData(untaggedCbor.toByteString());
    }

    static fromCbor(cbor: Cbor): PrivateKeyBase {
        return decodeTaggedCbor(PrivateKeyBase, cbor);
    }

    static fromURString(value: string): PrivateKeyBase {
        return fromTaggedUrString(PrivateKeyBase, value);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        if (!(other instanceof PrivateKeyBase)) {
            return false;
        }
        const a = this.#data;
        const b = other.#data;
        if (a.length !== b.length) {
            return false;
        }
        for (let i = 0; i < a.length; i += 1) {
            if (a[i] !== b[i]) {
                return false;
            }
        }
        return true;
    }

    toString(): string {
        return `PrivateKeyBase(${this.reference().refHexShort()})`;
    }
}
