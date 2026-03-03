import { type Cbor } from '@bc/dcbor';
import { TAG_MLKEM_PUBLIC_KEY, TAG_X25519_PUBLIC_KEY } from '@bc/tags';

import { BCComponentsError } from '../error.js';
import type { Encrypter } from '../encrypter.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { MLKEMPublicKey } from '../mlkem/mlkem-public-key.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import { X25519PublicKey } from '../x25519/x25519-public-key.js';
import { PrivateKeyBase } from '../private-key-base.js';
import { EncapsulationCiphertext } from './encapsulation-ciphertext.js';
import { EncapsulationScheme } from './encapsulation-scheme.js';

export type EncapsulationPublicKeyVariant =
    | { type: 'x25519'; value: X25519PublicKey }
    | { type: 'mlkem'; value: MLKEMPublicKey };

export class EncapsulationPublicKey implements Encrypter, ReferenceProvider {
    readonly #variant: EncapsulationPublicKeyVariant;

    private constructor(variant: EncapsulationPublicKeyVariant) {
        this.#variant = variant;
    }

    static fromX25519(publicKey: X25519PublicKey): EncapsulationPublicKey {
        return new EncapsulationPublicKey({ type: 'x25519', value: publicKey });
    }

    static fromMlkem(publicKey: MLKEMPublicKey): EncapsulationPublicKey {
        return new EncapsulationPublicKey({ type: 'mlkem', value: publicKey });
    }

    encapsulationScheme(): EncapsulationScheme {
        if (this.#variant.type === 'x25519') {
            return EncapsulationScheme.x25519;
        }
        const level = this.#variant.value.level();
        switch (level.name) {
            case 'mlkem512':
                return EncapsulationScheme.mlkem512;
            case 'mlkem768':
                return EncapsulationScheme.mlkem768;
            case 'mlkem1024':
                return EncapsulationScheme.mlkem1024;
        }
    }

    encapsulateNewSharedSecret(): [SymmetricKey, EncapsulationCiphertext] {
        if (this.#variant.type === 'x25519') {
            const ephemeralSender = PrivateKeyBase.new();
            const ephemeralPrivate = ephemeralSender.x25519PrivateKey();
            const ephemeralPublic = ephemeralPrivate.publicKey();
            const shared = ephemeralPrivate.sharedKeyWith(this.#variant.value);
            return [shared, EncapsulationCiphertext.fromX25519(ephemeralPublic)];
        }
        const [shared, ciphertext] = this.#variant.value.encapsulateNewSharedSecret();
        return [shared, EncapsulationCiphertext.fromMlkem(ciphertext)];
    }

    encapsulationPublicKey(): EncapsulationPublicKey {
        return this;
    }

    toCbor(): Cbor {
        if (this.#variant.type === 'x25519') {
            return this.#variant.value.taggedCbor();
        }
        return this.#variant.value.taggedCbor();
    }

    static fromCbor(cbor: Cbor): EncapsulationPublicKey {
        const tagged = cbor.toTagged();
        const tag = Number(tagged[0].value);
        if (tag === TAG_X25519_PUBLIC_KEY) {
            return EncapsulationPublicKey.fromX25519(X25519PublicKey.fromCbor(cbor));
        }
        if (tag === TAG_MLKEM_PUBLIC_KEY) {
            return EncapsulationPublicKey.fromMlkem(MLKEMPublicKey.fromCbor(cbor));
        }
        throw BCComponentsError.invalidData('EncapsulationPublicKey', 'invalid tag');
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.toCbor().toData()));
    }

    equals(other: unknown): boolean {
        if (!(other instanceof EncapsulationPublicKey)) {
            return false;
        }
        if (this.#variant.type !== other.#variant.type) {
            return false;
        }
        if (this.#variant.type === 'x25519' && other.#variant.type === 'x25519') {
            return this.#variant.value.equals(other.#variant.value);
        }
        if (this.#variant.type === 'mlkem' && other.#variant.type === 'mlkem') {
            return this.#variant.value.equals(other.#variant.value);
        }
        return false;
    }

    toString(): string {
        return `EncapsulationPublicKey(${this.reference().refHexShort()}, ${this.encapsulationScheme().toString()})`;
    }
}
