import { type Cbor } from '@bc/dcbor';
import { TAG_MLKEM_PRIVATE_KEY, TAG_X25519_PRIVATE_KEY } from '@bc/tags';

import { BCComponentsError } from '../error.js';
import type { Decrypter } from '../encrypter.js';
import { Digest } from '../digest.js';
import { type ReferenceProvider, Reference } from '../reference.js';
import { MLKEMPrivateKey } from '../mlkem/mlkem-private-key.js';
import { SymmetricKey } from '../symmetric/symmetric-key.js';
import { X25519PrivateKey } from '../x25519/x25519-private-key.js';
import { EncapsulationCiphertext } from './encapsulation-ciphertext.js';
import { EncapsulationPublicKey } from './encapsulation-public-key.js';
import { EncapsulationScheme } from './encapsulation-scheme.js';

export type EncapsulationPrivateKeyVariant =
    | { type: 'x25519'; value: X25519PrivateKey }
    | { type: 'mlkem'; value: MLKEMPrivateKey };

export class EncapsulationPrivateKey implements Decrypter, ReferenceProvider {
    readonly #variant: EncapsulationPrivateKeyVariant;

    private constructor(variant: EncapsulationPrivateKeyVariant) {
        this.#variant = variant;
    }

    static fromX25519(privateKey: X25519PrivateKey): EncapsulationPrivateKey {
        return new EncapsulationPrivateKey({ type: 'x25519', value: privateKey });
    }

    static fromMlkem(privateKey: MLKEMPrivateKey): EncapsulationPrivateKey {
        return new EncapsulationPrivateKey({ type: 'mlkem', value: privateKey });
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

    decapsulateSharedSecret(ciphertext: EncapsulationCiphertext): SymmetricKey {
        if (this.#variant.type === 'x25519' && ciphertext.isX25519()) {
            return this.#variant.value.sharedKeyWith(ciphertext.x25519PublicKey());
        }
        if (this.#variant.type === 'mlkem' && ciphertext.isMlkem()) {
            return this.#variant.value.decapsulateSharedSecret(ciphertext.mlkemCiphertext());
        }
        throw BCComponentsError.crypto(
            `Mismatched key encapsulation types. private key: ${this.encapsulationScheme()}, ciphertext: ${ciphertext.encapsulationScheme()}`,
        );
    }

    publicKey(): EncapsulationPublicKey {
        if (this.#variant.type === 'x25519') {
            return EncapsulationPublicKey.fromX25519(this.#variant.value.publicKey());
        }
        throw BCComponentsError.crypto('Deriving ML-KEM public key not supported');
    }

    encapsulationPrivateKey(): EncapsulationPrivateKey {
        return this;
    }

    toCbor(): Cbor {
        if (this.#variant.type === 'x25519') {
            return this.#variant.value.taggedCbor();
        }
        return this.#variant.value.taggedCbor();
    }

    static fromCbor(cbor: Cbor): EncapsulationPrivateKey {
        const tagged = cbor.toTagged();
        const tag = Number(tagged[0].value);
        if (tag === TAG_X25519_PRIVATE_KEY) {
            return EncapsulationPrivateKey.fromX25519(X25519PrivateKey.fromCbor(cbor));
        }
        if (tag === TAG_MLKEM_PRIVATE_KEY) {
            return EncapsulationPrivateKey.fromMlkem(MLKEMPrivateKey.fromCbor(cbor));
        }
        throw BCComponentsError.invalidData('EncapsulationPrivateKey', 'invalid tag');
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.toCbor().toData()));
    }

    equals(other: unknown): boolean {
        if (!(other instanceof EncapsulationPrivateKey)) {
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
        return `EncapsulationPrivateKey(${this.reference().refHexShort()}, ${this.encapsulationScheme().toString()})`;
    }
}
