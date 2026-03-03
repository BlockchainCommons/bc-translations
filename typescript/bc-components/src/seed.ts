import { type RandomNumberGenerator, SecureRandomNumberGenerator, rngRandomData } from '@bc/rand';
import {
    type Cbor,
    CborDate,
    CborMap,
    cbor as toCbor,
    createTag,
} from '@bc/dcbor';
import { TAG_SEED, TAG_SEED_V1 } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from './cbor-ur.js';
import { BCComponentsError } from './error.js';
import type { PrivateKeyDataProvider } from './private-key-data-provider.js';

const SEED_TAGS: Tag[] = [
    createTag(TAG_SEED, 'seed'),
    createTag(TAG_SEED_V1, 'crypto-seed'),
];

export class Seed implements PrivateKeyDataProvider {
    static readonly MIN_SEED_LENGTH = 16;

    readonly #data: Uint8Array;
    #name: string;
    #note: string;
    #creationDate?: CborDate;

    private constructor(
        data: Uint8Array,
        name = '',
        note = '',
        creationDate?: CborDate,
    ) {
        this.#data = data;
        this.#name = name;
        this.#note = note;
        this.#creationDate = creationDate;
    }

    static new(): Seed {
        return Seed.newWithLen(Seed.MIN_SEED_LENGTH);
    }

    static newWithLen(count: number): Seed {
        const rng = new SecureRandomNumberGenerator();
        return Seed.newWithLenUsing(count, rng);
    }

    static newWithLenUsing(count: number, rng: RandomNumberGenerator): Seed {
        const data = rngRandomData(rng, count);
        return Seed.newOpt(data);
    }

    static newOpt(
        data: Uint8Array,
        name?: string,
        note?: string,
        creationDate?: CborDate,
    ): Seed {
        if (data.length < Seed.MIN_SEED_LENGTH) {
            throw BCComponentsError.dataTooShort(
                'seed',
                Seed.MIN_SEED_LENGTH,
                data.length,
            );
        }
        return new Seed(
            new Uint8Array(data),
            name ?? '',
            note ?? '',
            creationDate,
        );
    }

    asBytes(): Uint8Array {
        return new Uint8Array(this.#data);
    }

    name(): string {
        return this.#name;
    }

    setName(name: string): void {
        this.#name = name;
    }

    note(): string {
        return this.#note;
    }

    setNote(note: string): void {
        this.#note = note;
    }

    creationDate(): CborDate | undefined {
        return this.#creationDate;
    }

    setCreationDate(creationDate?: CborDate): void {
        this.#creationDate = creationDate;
    }

    privateKeyData(): Uint8Array {
        return this.asBytes();
    }

    cborTags(): Tag[] {
        return SEED_TAGS;
    }

    untaggedCbor(): Cbor {
        const map = new CborMap();
        map.set(1, this.#data);
        if (this.#creationDate !== undefined) {
            map.set(2, this.#creationDate.taggedCbor());
        }
        if (this.#name.length > 0) {
            map.set(3, this.#name);
        }
        if (this.#note.length > 0) {
            map.set(4, this.#note);
        }
        return toCbor(map);
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    static cborTags(): Tag[] {
        return SEED_TAGS;
    }

    static fromUntaggedCbor(cbor: Cbor): Seed {
        const map = cbor.toMap();
        const data = map.extract<1, Uint8Array>(1);
        const creationDateCbor = map.get<2, Cbor>(2);
        const name = map.get<3, string>(3);
        const note = map.get<4, string>(4);
        return Seed.newOpt(
            data,
            name,
            note,
            creationDateCbor === undefined
                ? undefined
                : CborDate.fromTaggedCbor(creationDateCbor),
        );
    }

    static fromCbor(cbor: Cbor): Seed {
        return decodeTaggedCbor(Seed, cbor);
    }
}
