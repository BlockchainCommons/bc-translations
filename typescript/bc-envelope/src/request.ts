import { ARID } from '@bc/components';
import {
    BODY,
    DATE,
    NOTE,
} from '@bc/known-values';
import { TAG_REQUEST } from '@bc/tags';
import { CborDate, toTaggedValue } from '@bc/dcbor';

import type { EnvelopeFunction } from './function.js';
import type { EnvelopeParameter } from './parameter.js';
import { Expression } from './expression.js';
import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

export class Request {
    readonly #body: Expression;
    readonly #id: ARID;
    readonly #note: string;
    readonly #date?: CborDate;

    constructor(body: Expression | EnvelopeFunction | string, id: ARID, note = '', date?: CborDate) {
        this.#body = body instanceof Expression ? body : new Expression(body);
        this.#id = id;
        this.#note = note;
        this.#date = date;
    }

    body(): Expression {
        return this.#body;
    }

    id(): ARID {
        return this.#id;
    }

    note(): string {
        return this.#note;
    }

    date(): CborDate | undefined {
        return this.#date;
    }

    withNote(note: string): Request {
        return new Request(this.#body, this.#id, note, this.#date);
    }

    withDate(date: CborDate): Request {
        return new Request(this.#body, this.#id, this.#note, date);
    }

    withParameter(parameter: EnvelopeParameter, value: unknown): Request {
        return new Request(this.#body.withParameter(parameter, value), this.#id, this.#note, this.#date);
    }

    withOptionalParameter(parameter: EnvelopeParameter, value?: unknown): Request {
        if (value === undefined) return this;
        return this.withParameter(parameter, value);
    }

    objectForParameter(parameter: EnvelopeParameter): Envelope {
        return this.#body.objectForParameter(parameter);
    }

    objectsForParameter(parameter: EnvelopeParameter): Envelope[] {
        return this.#body.objectsForParameter(parameter);
    }

    extractObjectForParameter<T>(parameter: EnvelopeParameter, decoder?: (envelope: Envelope) => T): T {
        return this.#body.extractObjectForParameter(parameter, decoder);
    }

    extractOptionalObjectForParameter<T>(parameter: EnvelopeParameter, decoder?: (envelope: Envelope) => T): T | undefined {
        return this.#body.extractOptionalObjectForParameter(parameter, decoder);
    }

    function(): EnvelopeFunction {
        return this.#body.function();
    }

    expressionEnvelope(): Envelope {
        return this.#body.expressionEnvelope();
    }

    summary(): string {
        return `id: ${this.#id.shortDescription()}, body: ${this.#body.expressionEnvelope().formatFlat()}`;
    }

    toEnvelope(): Envelope {
        let envelope = Envelope.from(toTaggedValue(TAG_REQUEST, this.#id.taggedCbor()))
            .addAssertion(BODY, this.#body.toEnvelope());

        if (this.#note.length > 0) {
            envelope = envelope.addAssertion(NOTE, this.#note);
        }
        if (this.#date != null) {
            envelope = envelope.addAssertion(DATE, this.#date);
        }
        return envelope;
    }

    equals(other: unknown): boolean {
        return other instanceof Request
            && this.#id.equals(other.#id)
            && this.#body.equals(other.#body)
            && this.#note === other.#note
            && ((this.#date == null && other.#date == null) || (this.#date != null && other.#date != null && this.#date.equals(other.#date)));
    }

    toString(): string {
        return `Request(${this.summary()})`;
    }

    static fromEnvelope(envelope: Envelope, expectedFunction?: EnvelopeFunction): Request {
        const bodyEnvelope = envelope.objectForPredicate(BODY);
        const expression = Expression.fromEnvelope(bodyEnvelope, expectedFunction);

        const [tag, idCbor] = envelope.subject().tryLeaf().toTagged();
        if (Number(tag.value) !== TAG_REQUEST) {
            throw EnvelopeError.invalidFormat();
        }
        const id = ARID.fromCbor(idCbor);

        const note = envelope.extractOptionalObjectForPredicate<string>(NOTE);
        const date = envelope.extractOptionalObjectForPredicate<CborDate>(DATE);

        return new Request(expression, id, note ?? '', date);
    }
}
