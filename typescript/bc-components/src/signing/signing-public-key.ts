import { type Cbor, cbor as toCbor, createTag, toByteString } from '@bc/dcbor';
import {
    TAG_MLDSA_PUBLIC_KEY,
    TAG_SIGNING_PUBLIC_KEY,
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
import { type ReferenceProvider, Reference } from '../reference.js';
import { BCComponentsError } from '../error.js';
import { ECPublicKey } from '../ec-key/ec-public-key.js';
import { SchnorrPublicKey } from '../ec-key/schnorr-public-key.js';
import { Ed25519PublicKey } from '../ed25519/ed25519-public-key.js';
import { MLDSAPublicKey } from '../mldsa/mldsa-public-key.js';
import type { Signature } from './signature.js';
import type { Verifier } from './signer.js';

const SIGNING_PUBLIC_KEY_TAG: Tag = createTag(
    TAG_SIGNING_PUBLIC_KEY,
    'signing-public-key',
);

type SigningPublicKeyVariant =
    | { type: 'schnorr'; key: SchnorrPublicKey }
    | { type: 'ecdsa'; key: ECPublicKey }
    | { type: 'ed25519'; key: Ed25519PublicKey }
    | { type: 'mldsa'; key: MLDSAPublicKey };

export class SigningPublicKey implements Verifier, ReferenceProvider {
    readonly #variant: SigningPublicKeyVariant;

    private constructor(variant: SigningPublicKeyVariant) {
        this.#variant = variant;
    }

    static fromSchnorr(key: SchnorrPublicKey): SigningPublicKey {
        return new SigningPublicKey({ type: 'schnorr', key });
    }

    static fromEcdsa(key: ECPublicKey): SigningPublicKey {
        return new SigningPublicKey({ type: 'ecdsa', key });
    }

    static fromEd25519(key: Ed25519PublicKey): SigningPublicKey {
        return new SigningPublicKey({ type: 'ed25519', key });
    }

    static fromMlDsa(key: MLDSAPublicKey): SigningPublicKey {
        return new SigningPublicKey({ type: 'mldsa', key });
    }

    toSchnorr(): SchnorrPublicKey | undefined {
        return this.#variant.type === 'schnorr' ? this.#variant.key : undefined;
    }

    toEcdsa(): ECPublicKey | undefined {
        return this.#variant.type === 'ecdsa' ? this.#variant.key : undefined;
    }

    toEd25519(): Ed25519PublicKey | undefined {
        return this.#variant.type === 'ed25519' ? this.#variant.key : undefined;
    }

    toMlDsa(): MLDSAPublicKey | undefined {
        return this.#variant.type === 'mldsa' ? this.#variant.key : undefined;
    }

    verify(signature: Signature, message: Uint8Array): boolean {
        switch (this.#variant.type) {
            case 'schnorr': {
                const schnorr = signature.toSchnorr();
                return schnorr !== undefined
                    ? this.#variant.key.schnorrVerify(schnorr, message)
                    : false;
            }
            case 'ecdsa': {
                const ecdsa = signature.toEcdsa();
                return ecdsa !== undefined
                    ? this.#variant.key.verify(ecdsa, message)
                    : false;
            }
            case 'ed25519': {
                const ed = signature.toEd25519();
                return ed !== undefined
                    ? this.#variant.key.verify(ed, message)
                    : false;
            }
            case 'mldsa': {
                const mldsa = signature.toMlDsa();
                if (mldsa === undefined) {
                    return false;
                }
                try {
                    return this.#variant.key.verify(mldsa, message);
                } catch {
                    return false;
                }
            }
        }
    }

    cborTags(): Tag[] {
        return [SIGNING_PUBLIC_KEY_TAG];
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

    static fromURString(value: string): SigningPublicKey {
        return fromTaggedUrString(SigningPublicKey, value);
    }

    static cborTags(): Tag[] {
        return [SIGNING_PUBLIC_KEY_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): SigningPublicKey {
        try {
            return SigningPublicKey.fromSchnorr(
                SchnorrPublicKey.fromDataRef(cbor.toByteString()),
            );
        } catch {
            // continue
        }

        try {
            const elements = cbor.toArray();
            if (elements.length === 2) {
                const discriminator = Number(elements[0]!.toInteger());
                const data = elements[1]!.toByteString();
                if (discriminator === 1) {
                    return SigningPublicKey.fromEcdsa(ECPublicKey.fromDataRef(data));
                }
                if (discriminator === 2) {
                    return SigningPublicKey.fromEd25519(
                        Ed25519PublicKey.fromDataRef(data),
                    );
                }
            }
        } catch {
            // continue
        }

        try {
            const [tag] = cbor.toTagged();
            if (Number(tag.value) === TAG_MLDSA_PUBLIC_KEY) {
                return SigningPublicKey.fromMlDsa(MLDSAPublicKey.fromCbor(cbor));
            }
        } catch {
            // continue
        }

        throw BCComponentsError.invalidData(
            'SigningPublicKey',
            'invalid signing public key',
        );
    }

    static fromCbor(cbor: Cbor): SigningPublicKey {
        return decodeTaggedCbor(SigningPublicKey, cbor);
    }

    reference(): Reference {
        return Reference.fromDigest(Digest.fromImage(this.taggedCborData()));
    }

    equals(other: unknown): boolean {
        if (!(other instanceof SigningPublicKey)) {
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
        return `SigningPublicKey(${this.reference().refHexShort()})`;
    }
}
