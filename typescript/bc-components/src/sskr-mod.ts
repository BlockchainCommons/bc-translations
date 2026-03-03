import type { RandomNumberGenerator } from '@bc/rand';
import { SecureRandomNumberGenerator } from '@bc/rand';
import { type Cbor, createTag, toByteString } from '@bc/dcbor';
import {
    TAG_SSKR_SHARE,
    TAG_SSKR_SHARE_V1,
} from '@bc/tags';
import type { Tag } from '@bc/dcbor';
import {
    type GroupSpec as SSKRGroupSpec,
    type Secret as SSKRSecret,
    type Spec as SSKRSpec,
    SskrError as SSKRError,
    sskrGenerate as rawSskrGenerate,
    sskrGenerateUsing as rawSskrGenerateUsing,
    sskrCombine as rawSskrCombine,
} from '@bc/sskr';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from './cbor-ur.js';

const SSKR_SHARE_TAGS: Tag[] = [
    createTag(TAG_SSKR_SHARE, 'sskr'),
    createTag(TAG_SSKR_SHARE_V1, 'crypto-sskr'),
];

export class SSKRShare {
    readonly #data: Uint8Array;

    private constructor(data: Uint8Array) {
        this.#data = new Uint8Array(data);
    }

    static fromData(data: Uint8Array): SSKRShare {
        return new SSKRShare(data);
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    static fromHex(hex: string): SSKRShare {
        return new SSKRShare(Uint8Array.from(Buffer.from(hex, 'hex')));
    }

    hex(): string {
        return Buffer.from(this.#data).toString('hex');
    }

    identifier(): number {
        return ((this.#data[0] ?? 0) << 8) | (this.#data[1] ?? 0);
    }

    identifierHex(): string {
        return Buffer.from(this.#data.slice(0, 2)).toString('hex');
    }

    groupThreshold(): number {
        return ((this.#data[2] ?? 0) >> 4) + 1;
    }

    groupCount(): number {
        return ((this.#data[2] ?? 0) & 0x0f) + 1;
    }

    groupIndex(): number {
        return (this.#data[3] ?? 0) >> 4;
    }

    memberThreshold(): number {
        return ((this.#data[3] ?? 0) & 0x0f) + 1;
    }

    memberIndex(): number {
        return (this.#data[4] ?? 0) & 0x0f;
    }

    cborTags(): Tag[] {
        return SSKR_SHARE_TAGS;
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

    static cborTags(): Tag[] {
        return SSKR_SHARE_TAGS;
    }

    static fromUntaggedCbor(cbor: Cbor): SSKRShare {
        return SSKRShare.fromData(cbor.toByteString());
    }

    static fromCbor(cbor: Cbor): SSKRShare {
        return decodeTaggedCbor(SSKRShare, cbor);
    }

    equals(other: unknown): boolean {
        if (!(other instanceof SSKRShare)) {
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
}

export function sskrGenerate(
    spec: SSKRSpec,
    masterSecret: SSKRSecret,
): Array<Array<SSKRShare>> {
    return sskrGenerateUsing(spec, masterSecret, new SecureRandomNumberGenerator());
}

export function sskrGenerateUsing(
    spec: SSKRSpec,
    masterSecret: SSKRSecret,
    rng: RandomNumberGenerator,
): Array<Array<SSKRShare>> {
    return rawSskrGenerateUsing(spec, masterSecret, rng)
        .map((group) => group.map((share) => SSKRShare.fromData(share)));
}

export function sskrCombine(shares: readonly SSKRShare[]): SSKRSecret {
    return rawSskrCombine(shares.map((share) => share.asBytes()));
}

export {
    SSKRError,
    SSKRGroupSpec,
    SSKRSecret,
    SSKRSpec,
};
