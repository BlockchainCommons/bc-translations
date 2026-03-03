import {
    ECDSA_SIGNATURE_SIZE,
    ED25519_SIGNATURE_SIZE,
    SCHNORR_SIGNATURE_SIZE,
} from '@bc/crypto';
import { type Cbor, cbor as toCbor, createTag, toByteString } from '@bc/dcbor';
import {
    TAG_MLDSA_SIGNATURE,
    TAG_SIGNATURE,
} from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from '../cbor-ur.js';
import { BCComponentsError, type Result } from '../error.js';
import { bytesEqual, hexEncode, requireLength } from '../utils.js';
import { MLDSASignature } from '../mldsa/mldsa-signature.js';
import { SignatureScheme } from './signature-scheme.js';

const SIGNATURE_TAG: Tag = createTag(TAG_SIGNATURE, 'signature');

type SignatureVariant =
    | { type: 'schnorr'; data: Uint8Array }
    | { type: 'ecdsa'; data: Uint8Array }
    | { type: 'ed25519'; data: Uint8Array }
    | { type: 'mldsa'; data: MLDSASignature };

export class Signature {
    readonly #variant: SignatureVariant;

    private constructor(variant: SignatureVariant) {
        this.#variant = variant;
    }

    static schnorrFromData(data: Uint8Array): Signature {
        return new Signature({
            type: 'schnorr',
            data: requireLength(data, SCHNORR_SIGNATURE_SIZE, 'Schnorr signature'),
        });
    }

    static schnorrFromDataRef(data: Uint8Array): Result<Signature> {
        return Signature.schnorrFromData(new Uint8Array(data));
    }

    static ecdsaFromData(data: Uint8Array): Signature {
        return new Signature({
            type: 'ecdsa',
            data: requireLength(data, ECDSA_SIGNATURE_SIZE, 'ECDSA signature'),
        });
    }

    static ecdsaFromDataRef(data: Uint8Array): Result<Signature> {
        return Signature.ecdsaFromData(new Uint8Array(data));
    }

    static ed25519FromData(data: Uint8Array): Signature {
        return new Signature({
            type: 'ed25519',
            data: requireLength(data, ED25519_SIGNATURE_SIZE, 'Ed25519 signature'),
        });
    }

    static ed25519FromDataRef(data: Uint8Array): Result<Signature> {
        return Signature.ed25519FromData(new Uint8Array(data));
    }

    static fromMlDsa(signature: MLDSASignature): Signature {
        return new Signature({ type: 'mldsa', data: signature });
    }

    toSchnorr(): Uint8Array | undefined {
        if (this.#variant.type !== 'schnorr') {
            return undefined;
        }
        return new Uint8Array(this.#variant.data);
    }

    toEcdsa(): Uint8Array | undefined {
        if (this.#variant.type !== 'ecdsa') {
            return undefined;
        }
        return new Uint8Array(this.#variant.data);
    }

    toEd25519(): Uint8Array | undefined {
        if (this.#variant.type !== 'ed25519') {
            return undefined;
        }
        return new Uint8Array(this.#variant.data);
    }

    toMlDsa(): MLDSASignature | undefined {
        if (this.#variant.type !== 'mldsa') {
            return undefined;
        }
        return this.#variant.data;
    }

    scheme(): Result<SignatureScheme> {
        switch (this.#variant.type) {
            case 'schnorr':
                return SignatureScheme.schnorr;
            case 'ecdsa':
                return SignatureScheme.ecdsa;
            case 'ed25519':
                return SignatureScheme.ed25519;
            case 'mldsa': {
                const level = this.#variant.data.level();
                switch (level.name) {
                    case 'mldsa44':
                        return SignatureScheme.mldsa44;
                    case 'mldsa65':
                        return SignatureScheme.mldsa65;
                    case 'mldsa87':
                        return SignatureScheme.mldsa87;
                }
            }
        }
    }

    equals(other: unknown): boolean {
        if (!(other instanceof Signature)) {
            return false;
        }
        if (this.#variant.type !== other.#variant.type) {
            return false;
        }
        switch (this.#variant.type) {
            case 'schnorr':
            case 'ecdsa':
            case 'ed25519':
                return other.#variant.type === this.#variant.type
                    ? bytesEqual(this.#variant.data, other.#variant.data)
                    : false;
            case 'mldsa':
                return other.#variant.type === 'mldsa'
                    ? this.#variant.data.equals(other.#variant.data)
                    : false;
        }
    }

    cborTags(): Tag[] {
        return [SIGNATURE_TAG];
    }

    untaggedCbor(): Cbor {
        switch (this.#variant.type) {
            case 'schnorr':
                return toByteString(this.#variant.data);
            case 'ecdsa':
                return toCbor([1, toByteString(this.#variant.data)]);
            case 'ed25519':
                return toCbor([2, toByteString(this.#variant.data)]);
            case 'mldsa':
                return this.#variant.data.taggedCbor();
        }
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    static cborTags(): Tag[] {
        return [SIGNATURE_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): Signature {
        try {
            return Signature.schnorrFromDataRef(cbor.toByteString());
        } catch {
            // continue
        }

        try {
            const elements = cbor.toArray();
            if (elements.length === 2) {
                const first = elements[0]!;
                const second = elements[1]!;

                try {
                    return Signature.schnorrFromDataRef(first.toByteString());
                } catch {
                    // continue
                }

                const discriminator = Number(first.toInteger());
                if (discriminator === 1) {
                    return Signature.ecdsaFromDataRef(second.toByteString());
                }
                if (discriminator === 2) {
                    return Signature.ed25519FromDataRef(second.toByteString());
                }
            }
        } catch {
            // continue
        }

        try {
            const [tag] = cbor.toTagged();
            if (Number(tag.value) === TAG_MLDSA_SIGNATURE) {
                return Signature.fromMlDsa(MLDSASignature.fromCbor(cbor));
            }
        } catch {
            // continue
        }

        throw BCComponentsError.invalidData('Signature', 'Invalid signature format');
    }

    static fromCbor(cbor: Cbor): Signature {
        return decodeTaggedCbor(Signature, cbor);
    }

    toString(): string {
        switch (this.#variant.type) {
            case 'schnorr':
                return `Schnorr(${hexEncode(this.#variant.data)})`;
            case 'ecdsa':
                return `ECDSA(${hexEncode(this.#variant.data)})`;
            case 'ed25519':
                return `Ed25519(${hexEncode(this.#variant.data)})`;
            case 'mldsa':
                return `MLDSA(${this.#variant.data.level().toString()})`;
        }
    }
}
