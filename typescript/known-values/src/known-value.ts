/**
 * A value in a namespace of unsigned integers that represents a stand-alone
 * ontological concept.
 *
 * Known Values provide a compact, deterministic way to represent commonly used
 * ontological concepts such as relationships between entities, classes of
 * entities, properties, or enumerated values.
 *
 * A Known Value is represented as a 64-bit unsigned integer with an optional
 * human-readable name.
 */

import {
    type Cbor,
    type Tag,
    cbor,
    cborData,
    createTaggedCbor,
    decodeCbor,
    expectUnsigned,
    extractTaggedContent,
    tagsForValues,
    validateTag,
} from '@bc/dcbor';
import { Digest, type DigestProvider } from '@bc/components';
import { TAG_KNOWN_VALUE } from '@bc/tags';

const KNOWN_VALUE_TAG: Tag = tagsForValues([TAG_KNOWN_VALUE])[0]!;

/**
 * A Known Value represented as a 64-bit unsigned integer with an optional
 * human-readable name.
 *
 * Equality is based solely on the numeric value, ignoring the assigned name.
 */
export class KnownValue implements DigestProvider {
    readonly #value: bigint;
    readonly #assignedName: string | undefined;

    constructor(value: bigint, assignedName?: string) {
        this.#value = value;
        this.#assignedName = assignedName;
    }

    /** Creates a KnownValue with the given value and associated name. */
    static withName(value: bigint | number, assignedName: string): KnownValue {
        return new KnownValue(BigInt(value), assignedName);
    }

    /** Returns the numeric value of the KnownValue. */
    get value(): bigint {
        return this.#value;
    }

    /** Returns the assigned name of the KnownValue, if one exists. */
    get assignedName(): string | undefined {
        return this.#assignedName;
    }

    /**
     * Returns a human-readable name for the KnownValue.
     *
     * If the KnownValue has an assigned name, that name is returned.
     * Otherwise, the string representation of the numeric value is returned.
     */
    get name(): string {
        return this.#assignedName ?? this.#value.toString();
    }

    /** Returns a string representation of this KnownValue. */
    toString(): string {
        return this.name;
    }

    /**
     * Checks equality with another KnownValue.
     *
     * Equality is based solely on the numeric value, ignoring the assigned name.
     */
    equals(other: unknown): boolean {
        return other instanceof KnownValue && this.#value === other.#value;
    }

    // --- DigestProvider ---

    digest(): Digest {
        return Digest.fromImage(this.taggedCborData());
    }

    // --- CBOR Tagged ---

    cborTags(): Tag[] {
        return [KNOWN_VALUE_TAG];
    }

    untaggedCbor(): Cbor {
        return cbor(this.#value);
    }

    taggedCbor(): Cbor {
        return createTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return cborData(this.taggedCbor());
    }

    /** Returns the canonical CBOR encoding, which is always tagged for KnownValue. */
    cbor(): Cbor {
        return this.taggedCbor();
    }

    static cborTags(): Tag[] {
        return [KNOWN_VALUE_TAG];
    }

    static fromUntaggedCbor(untaggedCbor: Cbor): KnownValue {
        const value = expectUnsigned(untaggedCbor);
        if (value === undefined) {
            throw new Error('KnownValue: expected unsigned integer in CBOR');
        }
        return new KnownValue(BigInt(value));
    }

    static fromCbor(taggedCbor: Cbor): KnownValue {
        validateTag(taggedCbor, KnownValue.cborTags());
        return KnownValue.fromUntaggedCbor(extractTaggedContent(taggedCbor));
    }

    static fromCborData(data: Uint8Array): KnownValue {
        return KnownValue.fromCbor(decodeCbor(data));
    }

    // --- Conversions ---

    static fromNumber(value: number): KnownValue {
        return new KnownValue(BigInt(value));
    }
}
