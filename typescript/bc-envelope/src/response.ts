import { ARID } from '@bc/components';
import {
    ERROR,
    KnownValue,
    OK_VALUE,
    RESULT,
    UNKNOWN_VALUE,
} from '@bc/known-values';
import { TAG_RESPONSE } from '@bc/tags';
import { toTaggedValue } from '@bc/dcbor';

import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

export class Response {
    readonly #success: boolean;
    readonly #id?: ARID;
    readonly #resultOrError: Envelope;

    private constructor(success: boolean, id: ARID | undefined, resultOrError: Envelope) {
        this.#success = success;
        this.#id = id;
        this.#resultOrError = resultOrError;
    }

    static newSuccess(id: ARID): Response {
        return new Response(true, id, Envelope.ok());
    }

    static newFailure(id: ARID): Response {
        return new Response(false, id, Envelope.unknown());
    }

    static newEarlyFailure(): Response {
        return new Response(false, undefined, Envelope.unknown());
    }

    isSuccess(): boolean {
        return this.#success;
    }

    isFailure(): boolean {
        return !this.#success;
    }

    id(): ARID | undefined {
        return this.#id;
    }

    expectId(): ARID {
        if (this.#id == null) {
            throw EnvelopeError.unexpectedResponseId();
        }
        return this.#id;
    }

    result(): Envelope {
        if (!this.#success) {
            throw EnvelopeError.invalidFormat();
        }
        return this.#resultOrError;
    }

    error(): Envelope {
        if (this.#success) {
            throw EnvelopeError.invalidFormat();
        }
        return this.#resultOrError;
    }

    extractResult<T>(decoder?: (envelope: Envelope) => T): T {
        return this.result().extractSubject(decoder);
    }

    extractError<T>(decoder?: (envelope: Envelope) => T): T {
        return this.error().extractSubject(decoder);
    }

    withResult(result: unknown): Response {
        if (!this.#success) {
            throw EnvelopeError.invalidFormat();
        }
        return new Response(true, this.#id, Envelope.from(result));
    }

    withOptionalResult(result?: unknown): Response {
        return this.withResult(result ?? Envelope.null_());
    }

    withError(error: unknown): Response {
        if (this.#success) {
            throw EnvelopeError.invalidFormat();
        }
        return new Response(false, this.#id, Envelope.from(error));
    }

    withOptionalError(error?: unknown): Response {
        if (error !== undefined) {
            return this.withError(error);
        }
        return this;
    }

    summary(): string {
        if (this.#success) {
            return `id: ${this.#id!.shortDescription()}, result: ${this.#resultOrError.formatFlat()}`;
        }
        const idText = this.#id?.shortDescription() ?? "'Unknown'";
        return `id: ${idText} error: ${this.#resultOrError.formatFlat()}`;
    }

    toEnvelope(): Envelope {
        if (this.#success) {
            return Envelope.from(toTaggedValue(TAG_RESPONSE, this.#id!.taggedCbor()))
                .addAssertion(RESULT, this.#resultOrError);
        }

        const subject = this.#id != null
            ? Envelope.from(toTaggedValue(TAG_RESPONSE, this.#id.taggedCbor()))
            : Envelope.from(toTaggedValue(TAG_RESPONSE, UNKNOWN_VALUE.taggedCbor()));

        return subject.addAssertion(ERROR, this.#resultOrError);
    }

    equals(other: unknown): boolean {
        return other instanceof Response
            && this.#success === other.#success
            && ((this.#id == null && other.#id == null) || (this.#id != null && other.#id != null && this.#id.equals(other.#id)))
            && this.#resultOrError.isEquivalentTo(other.#resultOrError);
    }

    toString(): string {
        return `Response(${this.summary()})`;
    }

    static fromEnvelope(envelope: Envelope): Response {
        const hasResult = envelope.optionalAssertionWithPredicate(RESULT) != null;
        const hasError = envelope.optionalAssertionWithPredicate(ERROR) != null;

        if (hasResult === hasError) {
            throw EnvelopeError.invalidFormat();
        }

        const [tag, idCbor] = envelope.subject().tryLeaf().toTagged();
        if (Number(tag.value) !== TAG_RESPONSE) {
            throw EnvelopeError.invalidFormat();
        }

        if (hasResult) {
            const id = ARID.fromCbor(idCbor);
            const result = envelope.objectForPredicate(RESULT);
            return new Response(true, id, result);
        }

        let id: ARID | undefined;
        try {
            const kv = KnownValue.fromCbor(idCbor);
            if (!kv.equals(UNKNOWN_VALUE)) {
                throw EnvelopeError.invalidFormat();
            }
            id = undefined;
        } catch {
            id = ARID.fromCbor(idCbor);
        }

        const error = envelope.objectForPredicate(ERROR);
        return new Response(false, id, error);
    }
}
