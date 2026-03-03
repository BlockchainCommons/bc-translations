import { type Cbor } from '@bc/dcbor';
import { TAG_MLKEM_CIPHERTEXT, TAG_X25519_PUBLIC_KEY } from '@bc/tags';

import { BCComponentsError } from '../error.js';
import { MLKEMCiphertext } from '../mlkem/mlkem-ciphertext.js';
import { X25519PublicKey } from '../x25519/x25519-public-key.js';
import { EncapsulationScheme } from './encapsulation-scheme.js';

export type EncapsulationCiphertextVariant =
    | { type: 'x25519'; value: X25519PublicKey }
    | { type: 'mlkem'; value: MLKEMCiphertext };

export class EncapsulationCiphertext {
    readonly #variant: EncapsulationCiphertextVariant;

    private constructor(variant: EncapsulationCiphertextVariant) {
        this.#variant = variant;
    }

    static fromX25519(publicKey: X25519PublicKey): EncapsulationCiphertext {
        return new EncapsulationCiphertext({ type: 'x25519', value: publicKey });
    }

    static fromMlkem(ciphertext: MLKEMCiphertext): EncapsulationCiphertext {
        return new EncapsulationCiphertext({ type: 'mlkem', value: ciphertext });
    }

    x25519PublicKey(): X25519PublicKey {
        if (this.#variant.type !== 'x25519') {
            throw BCComponentsError.crypto('Invalid key encapsulation type');
        }
        return this.#variant.value;
    }

    mlkemCiphertext(): MLKEMCiphertext {
        if (this.#variant.type !== 'mlkem') {
            throw BCComponentsError.crypto('Invalid key encapsulation type');
        }
        return this.#variant.value;
    }

    isX25519(): boolean {
        return this.#variant.type === 'x25519';
    }

    isMlkem(): boolean {
        return this.#variant.type === 'mlkem';
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

    toCbor(): Cbor {
        if (this.#variant.type === 'x25519') {
            return this.#variant.value.taggedCbor();
        }
        return this.#variant.value.taggedCbor();
    }

    static fromCbor(cbor: Cbor): EncapsulationCiphertext {
        const tagged = cbor.toTagged();
        const tag = Number(tagged[0].value);
        if (tag === TAG_X25519_PUBLIC_KEY) {
            return EncapsulationCiphertext.fromX25519(X25519PublicKey.fromCbor(cbor));
        }
        if (tag === TAG_MLKEM_CIPHERTEXT) {
            return EncapsulationCiphertext.fromMlkem(MLKEMCiphertext.fromCbor(cbor));
        }
        throw BCComponentsError.invalidData('EncapsulationCiphertext', 'invalid tag');
    }

    equals(other: unknown): boolean {
        if (!(other instanceof EncapsulationCiphertext)) {
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
}
