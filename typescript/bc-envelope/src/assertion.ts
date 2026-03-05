import { Digest, type DigestProvider } from '@bc/components';
import { CborMap, cbor as toCbor, type Cbor } from '@bc/dcbor';

import { EnvelopeError } from './error.js';
import type { EnvelopeEncodable } from './envelope-encodable.js';
import { asEnvelope } from './envelope-encodable.js';
import { Envelope } from './envelope.js';

/** Predicate-object relationship representing an assertion about a subject. */
export class Assertion implements DigestProvider {
    readonly #predicate: Envelope;
    readonly #objectEnvelope: Envelope;
    readonly #digest: Digest;

    private constructor(predicate: Envelope, objectEnvelope: Envelope, digest: Digest) {
        this.#predicate = predicate;
        this.#objectEnvelope = objectEnvelope;
        this.#digest = digest;
    }

    static create(predicate: EnvelopeEncodable | unknown, objectValue: EnvelopeEncodable | unknown): Assertion {
        const predicateEnvelope = asEnvelope(predicate);
        const objectEnvelope = asEnvelope(objectValue);
        const digest = Digest.fromDigests([predicateEnvelope.digest(), objectEnvelope.digest()]);
        return new Assertion(predicateEnvelope, objectEnvelope, digest);
    }

    predicate(): Envelope {
        return this.#predicate;
    }

    objectEnvelope(): Envelope {
        return this.#objectEnvelope;
    }

    digest(): Digest {
        return this.#digest;
    }

    toCbor(): Cbor {
        const map = new CborMap();
        map.insert(this.#predicate.untaggedCbor(), this.#objectEnvelope.untaggedCbor());
        return toCbor(map);
    }

    toEnvelope(): Envelope {
        return Envelope.newWithAssertion(this);
    }

    equals(other: unknown): boolean {
        return other instanceof Assertion && this.#digest.equals(other.#digest);
    }

    toString(): string {
        return `Assertion(${this.#predicate.format()}: ${this.#objectEnvelope.format()})`;
    }

    static fromCborMap(map: CborMap): Assertion {
        const entries = map.entriesArray;
        if (entries.length !== 1) {
            throw EnvelopeError.invalidAssertion();
        }
        const entry = entries[0]!;
        const predicate = Envelope.fromUntaggedCbor(entry.key);
        const objectEnvelope = Envelope.fromUntaggedCbor(entry.value);
        return Assertion.create(predicate, objectEnvelope);
    }

    static fromCbor(cbor: Cbor): Assertion {
        if (!cbor.isMap()) {
            throw EnvelopeError.invalidAssertion();
        }
        return Assertion.fromCborMap(cbor.toMap());
    }
}
