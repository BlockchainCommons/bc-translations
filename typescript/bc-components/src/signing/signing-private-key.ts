import { type RandomNumberGenerator, SecureRandomNumberGenerator } from '@bc/rand';
import { type Cbor, cbor as toCbor, createTag, toByteString } from '@bc/dcbor';
import {
    TAG_MLDSA_PRIVATE_KEY,
    TAG_SIGNING_PRIVATE_KEY,
} from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import {
    decodeTaggedCbor,
    defaultTaggedCbor,
    defaultTaggedCborData,
    fromTaggedUrString,
    toUrString,
} from '../cbor-ur.js';
import { Digest } from '../digest.js';
import { BCComponentsError, type Result } from '../error.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { ECPrivateKey } from '../ec-key/ec-private-key.js';
import { Ed25519PrivateKey } from '../ed25519/ed25519-private-key.js';
import { MLDSAPrivateKey } from '../mldsa/mldsa-private-key.js';
import { type Signer, type Verifier } from './signer.js';
import { Signature } from './signature.js';
import { SigningPublicKey } from './signing-public-key.js';

const SIGNING_PRIVATE_KEY_TAG: Tag = createTag(
    TAG_SIGNING_PRIVATE_KEY,
    'signing-private-key',
);

export type SigningOptions =
    | {
        kind: 'schnorr';
        rng: RandomNumberGenerator;
    };

type SigningPrivateKeyVariant =
    | { type: 'schnorr'; key: ECPrivateKey }
    | { type: 'ecdsa'; key: ECPrivateKey }
    | { type: 'ed25519'; key: Ed25519PrivateKey }
    | { type: 'mldsa'; key: MLDSAPrivateKey };

export class SigningPrivateKey implements Signer, Verifier, ReferenceProvider {
    readonly #variant: SigningPrivateKeyVariant;

    private constructor(variant: SigningPrivateKeyVariant) {
        this.#variant = variant;
    }

    static newSchnorr(key: ECPrivateKey): SigningPrivateKey {
        return new SigningPrivateKey({ type: 'schnorr', key });
    }

    static newEcdsa(key: ECPrivateKey): SigningPrivateKey {
        return new SigningPrivateKey({ type: 'ecdsa', key });
    }

    static newEd25519(key: Ed25519PrivateKey): SigningPrivateKey {
        return new SigningPrivateKey({ type: 'ed25519', key });
    }

    static fromMlDsa(key: MLDSAPrivateKey): SigningPrivateKey {
        return new SigningPrivateKey({ type: 'mldsa', key });
    }

    toSchnorr(): ECPrivateKey | undefined {
        return this.#variant.type === 'schnorr' ? this.#variant.key : undefined;
    }

    isSchnorr(): boolean {
        return this.#variant.type === 'schnorr';
    }

    toEcdsa(): ECPrivateKey | undefined {
        return this.#variant.type === 'ecdsa' ? this.#variant.key : undefined;
    }

    isEcdsa(): boolean {
        return this.#variant.type === 'ecdsa';
    }

    toEd25519(): Ed25519PrivateKey | undefined {
        return this.#variant.type === 'ed25519' ? this.#variant.key : undefined;
    }

    toMlDsa(): MLDSAPrivateKey | undefined {
        return this.#variant.type === 'mldsa' ? this.#variant.key : undefined;
    }

    publicKey(): Result<SigningPublicKey> {
        switch (this.#variant.type) {
            case 'schnorr':
                return SigningPublicKey.fromSchnorr(
                    this.#variant.key.schnorrPublicKey(),
                );
            case 'ecdsa':
                return SigningPublicKey.fromEcdsa(this.#variant.key.publicKey());
            case 'ed25519':
                return SigningPublicKey.fromEd25519(this.#variant.key.publicKey());
            case 'mldsa':
                throw BCComponentsError.general(
                    'Deriving ML-DSA public key not supported',
                );
        }
    }

    #ecdsaSign(message: Uint8Array): Result<Signature> {
        if (this.#variant.type !== 'ecdsa') {
            throw BCComponentsError.crypto('Invalid key type for ECDSA signing');
        }
        return Signature.ecdsaFromData(this.#variant.key.ecdsaSign(message));
    }

    schnorrSign(message: Uint8Array, rng: RandomNumberGenerator): Result<Signature> {
        if (this.#variant.type !== 'schnorr') {
            throw BCComponentsError.crypto('Invalid key type for Schnorr signing');
        }
        return Signature.schnorrFromData(this.#variant.key.schnorrSignUsing(message, rng));
    }

    #ed25519Sign(message: Uint8Array): Result<Signature> {
        if (this.#variant.type !== 'ed25519') {
            throw BCComponentsError.crypto('Invalid key type for Ed25519 signing');
        }
        return Signature.ed25519FromData(this.#variant.key.sign(message));
    }

    #mldsaSign(message: Uint8Array): Result<Signature> {
        if (this.#variant.type !== 'mldsa') {
            throw BCComponentsError.postQuantum('Invalid key type for MLDSA signing');
        }
        return Signature.fromMlDsa(this.#variant.key.sign(message));
    }

    signWithOptions(message: Uint8Array, options?: SigningOptions): Result<Signature> {
        switch (this.#variant.type) {
            case 'schnorr': {
                const rng = options?.kind === 'schnorr'
                    ? options.rng
                    : new SecureRandomNumberGenerator();
                return this.schnorrSign(message, rng);
            }
            case 'ecdsa':
                return this.#ecdsaSign(message);
            case 'ed25519':
                return this.#ed25519Sign(message);
            case 'mldsa':
                return this.#mldsaSign(message);
        }
    }

    sign(message: Uint8Array): Result<Signature> {
        return this.signWithOptions(message);
    }

    verify(signature: Signature, message: Uint8Array): boolean {
        if (this.#variant.type !== 'schnorr') {
            return false;
        }
        const schnorr = signature.toSchnorr();
        if (schnorr === undefined) {
            return false;
        }
        return this.#variant.key.schnorrPublicKey().schnorrVerify(schnorr, message);
    }

    cborTags(): Tag[] {
        return [SIGNING_PRIVATE_KEY_TAG];
    }

    untaggedCbor(): Cbor {
        switch (this.#variant.type) {
            case 'schnorr':
                return toByteString(this.#variant.key.data());
            case 'ecdsa':
                return toCbor([1, toByteString(this.#variant.key.data())]);
            case 'ed25519':
                return toCbor([2, toByteString(this.#variant.key.data())]);
            case 'mldsa':
                return this.#variant.key.taggedCbor();
        }
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

    static fromURString(value: string): SigningPrivateKey {
        return fromTaggedUrString(SigningPrivateKey, value);
    }

    static cborTags(): Tag[] {
        return [SIGNING_PRIVATE_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): SigningPrivateKey {
        try {
            return SigningPrivateKey.newSchnorr(
                ECPrivateKey.fromDataRef(cbor.toByteString()),
            );
        } catch {
            // continue
        }

        try {
            const elements = cbor.toArray();
            const discriminator = Number(elements[0]!.toInteger());
            const data = elements[1]!.toByteString();
            if (discriminator === 1) {
                return SigningPrivateKey.newEcdsa(ECPrivateKey.fromDataRef(data));
            }
            if (discriminator === 2) {
                return SigningPrivateKey.newEd25519(
                    Ed25519PrivateKey.fromDataRef(data),
                );
            }
        } catch {
            // continue
        }

        try {
            const [tag] = cbor.toTagged();
            if (Number(tag.value) === TAG_MLDSA_PRIVATE_KEY) {
                return SigningPrivateKey.fromMlDsa(MLDSAPrivateKey.fromCbor(cbor));
            }
        } catch {
            // continue
        }

        throw BCComponentsError.invalidData(
            'SigningPrivateKey',
            'Invalid CBOR case for SigningPrivateKey',
        );
    }

    static fromCbor(cbor: Cbor): SigningPrivateKey {
        return decodeTaggedCbor(SigningPrivateKey, cbor);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        if (!(other instanceof SigningPrivateKey)) {
            return false;
        }
        if (this.#variant.type !== other.#variant.type) {
            return false;
        }
        switch (this.#variant.type) {
            case 'schnorr':
                return this.#variant.key.equals(other.#variant.key);
            case 'ecdsa':
                return this.#variant.key.equals(other.#variant.key);
            case 'ed25519':
                return this.#variant.key.equals(other.#variant.key);
            case 'mldsa':
                return this.#variant.key.equals(other.#variant.key);
        }
    }

    toString(): string {
        return `SigningPrivateKey(${this.reference().refHexShort()})`;
    }
}
