import { type Cbor, cbor as toCbor, createTag } from '@bc/dcbor';
import { TAG_PRIVATE_KEYS } from '@bc/tags';
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
import { BCComponentsError, type Result } from './error.js';
import { EncapsulationPrivateKey } from './encapsulation/encapsulation-private-key.js';
import { PublicKeys } from './public-keys.js';
import { type Signature } from './signing/signature.js';
import type { Signer, Verifier } from './signing/signer.js';
import { type SigningOptions, SigningPrivateKey } from './signing/signing-private-key.js';

const PRIVATE_KEYS_TAG: Tag = createTag(TAG_PRIVATE_KEYS, 'crypto-prvkeys');

export class PrivateKeys implements Signer, Decrypter, ReferenceProvider {
    readonly #signingPrivateKey: SigningPrivateKey;
    readonly #encapsulationPrivateKey: EncapsulationPrivateKey;

    private constructor(
        signingPrivateKey: SigningPrivateKey,
        encapsulationPrivateKey: EncapsulationPrivateKey,
    ) {
        this.#signingPrivateKey = signingPrivateKey;
        this.#encapsulationPrivateKey = encapsulationPrivateKey;
    }

    static withKeys(
        signingPrivateKey: SigningPrivateKey,
        encapsulationPrivateKey: EncapsulationPrivateKey,
    ): PrivateKeys {
        return new PrivateKeys(signingPrivateKey, encapsulationPrivateKey);
    }

    signingPrivateKey(): SigningPrivateKey {
        return this.#signingPrivateKey;
    }

    publicKeys(): Result<PublicKeys> {
        return PublicKeys.new(
            this.#signingPrivateKey.publicKey(),
            this.#encapsulationPrivateKey.publicKey(),
        );
    }

    signWithOptions(message: Uint8Array, options?: SigningOptions): Result<Signature> {
        return this.#signingPrivateKey.signWithOptions(message, options);
    }

    sign(message: Uint8Array): Result<Signature> {
        return this.#signingPrivateKey.sign(message);
    }

    encapsulationPrivateKey(): EncapsulationPrivateKey {
        return this.#encapsulationPrivateKey;
    }

    decapsulateSharedSecret(ciphertext: Parameters<Decrypter['decapsulateSharedSecret']>[0]) {
        return this.#encapsulationPrivateKey.decapsulateSharedSecret(ciphertext);
    }

    cborTags(): Tag[] {
        return [PRIVATE_KEYS_TAG];
    }

    untaggedCbor(): Cbor {
        return toCbor([
            this.#signingPrivateKey.taggedCbor(),
            this.#encapsulationPrivateKey.toCbor(),
        ]);
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
        return [PRIVATE_KEYS_TAG];
    }

    static fromUntaggedCbor(untaggedCbor: Cbor): PrivateKeys {
        const elements = untaggedCbor.toArray();
        if (elements.length !== 2) {
            throw BCComponentsError.invalidData(
                'PrivateKeys',
                'PrivateKeys must have two elements',
            );
        }

        const signingPrivateKey = SigningPrivateKey.fromCbor(elements[0]!);
        const encapsulationPrivateKey = EncapsulationPrivateKey.fromCbor(elements[1]!);
        return PrivateKeys.withKeys(signingPrivateKey, encapsulationPrivateKey);
    }

    static fromCbor(cbor: Cbor): PrivateKeys {
        return decodeTaggedCbor(PrivateKeys, cbor);
    }

    static fromURString(value: string): PrivateKeys {
        return fromTaggedUrString(PrivateKeys, value);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return (
            other instanceof PrivateKeys
            && this.#signingPrivateKey.equals(other.#signingPrivateKey)
            && this.#encapsulationPrivateKey.equals(other.#encapsulationPrivateKey)
        );
    }

    toString(): string {
        return `PrivateKeys(${this.reference().refHexShort()}, ${this.#signingPrivateKey.toString()}, ${this.#encapsulationPrivateKey.toString()})`;
    }
}

export interface PrivateKeysProvider {
    privateKeys(): PrivateKeys;
}
