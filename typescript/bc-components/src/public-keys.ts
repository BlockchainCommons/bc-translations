import { type Cbor, cbor as toCbor, createTag } from '@bc/dcbor';
import { TAG_PUBLIC_KEYS } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import {
    decodeTaggedCbor,
    defaultTaggedCbor,
    defaultTaggedCborData,
    fromTaggedUrString,
    toUrString,
} from './cbor-ur.js';
import type { Encrypter } from './encrypter.js';
import { type ReferenceProvider, Reference } from './reference.js';
import { Digest } from './digest.js';
import { BCComponentsError } from './error.js';
import { EncapsulationPublicKey } from './encapsulation/encapsulation-public-key.js';
import type { Signature } from './signing/signature.js';
import type { Verifier } from './signing/signer.js';
import { SigningPublicKey } from './signing/signing-public-key.js';

const PUBLIC_KEYS_TAG: Tag = createTag(TAG_PUBLIC_KEYS, 'crypto-pubkeys');

export class PublicKeys implements Verifier, Encrypter, ReferenceProvider {
    readonly #signingPublicKey: SigningPublicKey;
    readonly #encapsulationPublicKey: EncapsulationPublicKey;

    private constructor(
        signingPublicKey: SigningPublicKey,
        encapsulationPublicKey: EncapsulationPublicKey,
    ) {
        this.#signingPublicKey = signingPublicKey;
        this.#encapsulationPublicKey = encapsulationPublicKey;
    }

    static new(
        signingPublicKey: SigningPublicKey,
        encapsulationPublicKey: EncapsulationPublicKey,
    ): PublicKeys {
        return new PublicKeys(signingPublicKey, encapsulationPublicKey);
    }

    signingPublicKey(): SigningPublicKey {
        return this.#signingPublicKey;
    }

    verify(signature: Signature, message: Uint8Array): boolean {
        return this.#signingPublicKey.verify(signature, message);
    }

    encapsulationPublicKey(): EncapsulationPublicKey {
        return this.#encapsulationPublicKey;
    }

    encapsulateNewSharedSecret() {
        return this.#encapsulationPublicKey.encapsulateNewSharedSecret();
    }

    cborTags(): Tag[] {
        return [PUBLIC_KEYS_TAG];
    }

    untaggedCbor(): Cbor {
        return toCbor([
            this.#signingPublicKey.taggedCbor(),
            this.#encapsulationPublicKey.toCbor(),
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
        return [PUBLIC_KEYS_TAG];
    }

    static fromUntaggedCbor(untaggedCbor: Cbor): PublicKeys {
        const elements = untaggedCbor.toArray();
        if (elements.length !== 2) {
            throw BCComponentsError.invalidData(
                'PublicKeys',
                'PublicKeys must have two elements',
            );
        }

        const signingPublicKey = SigningPublicKey.fromCbor(elements[0]!);
        const encapsulationPublicKey = EncapsulationPublicKey.fromCbor(elements[1]!);
        return PublicKeys.new(signingPublicKey, encapsulationPublicKey);
    }

    static fromCbor(cbor: Cbor): PublicKeys {
        return decodeTaggedCbor(PublicKeys, cbor);
    }

    static fromURString(value: string): PublicKeys {
        return fromTaggedUrString(PublicKeys, value);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        return (
            other instanceof PublicKeys
            && this.#signingPublicKey.equals(other.#signingPublicKey)
            && this.#encapsulationPublicKey.equals(other.#encapsulationPublicKey)
        );
    }

    toString(): string {
        return `PublicKeys(${this.reference().refHexShort()}, ${this.#signingPublicKey.toString()}, ${this.#encapsulationPublicKey.toString()})`;
    }
}

export interface PublicKeysProvider {
    publicKeys(): PublicKeys;
}
