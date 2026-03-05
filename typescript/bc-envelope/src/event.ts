import { ARID } from '@bc/components';
import {
    CONTENT,
    DATE,
    NOTE,
} from '@bc/known-values';
import { TAG_EVENT } from '@bc/tags';
import { CborDate, toTaggedValue } from '@bc/dcbor';

import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

export class Event<T> {
    readonly #content: T;
    readonly #id: ARID;
    readonly #note: string;
    readonly #date?: CborDate;
    readonly #contentEncoder: (content: T) => Envelope;

    private constructor(
        content: T,
        id: ARID,
        note: string,
        date: CborDate | undefined,
        contentEncoder: (content: T) => Envelope,
    ) {
        this.#content = content;
        this.#id = id;
        this.#note = note;
        this.#date = date;
        this.#contentEncoder = contentEncoder;
    }

    static create<T>(content: T, id: ARID, contentEncoder: (content: T) => Envelope): Event<T> {
        return new Event(content, id, '', undefined, contentEncoder);
    }

    static ofString(content: string, id: ARID): Event<string> {
        return Event.create(content, id, (item) => Envelope.from(item));
    }

    content(): T {
        return this.#content;
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

    withNote(note: string): Event<T> {
        return new Event(this.#content, this.#id, note, this.#date, this.#contentEncoder);
    }

    withDate(date: CborDate): Event<T> {
        return new Event(this.#content, this.#id, this.#note, date, this.#contentEncoder);
    }

    summary(): string {
        return `id: ${this.#id.shortDescription()}, content: ${this.#contentEncoder(this.#content).formatFlat()}`;
    }

    toEnvelope(): Envelope {
        let envelope = Envelope.from(toTaggedValue(TAG_EVENT, this.#id.taggedCbor()))
            .addAssertion(CONTENT, this.#contentEncoder(this.#content));

        if (this.#note.length > 0) {
            envelope = envelope.addAssertion(NOTE, this.#note);
        }
        if (this.#date != null) {
            envelope = envelope.addAssertion(DATE, this.#date);
        }
        return envelope;
    }

    equals(other: unknown): boolean {
        return other instanceof Event
            && this.#id.equals(other.#id)
            && this.#content === other.#content
            && this.#note === other.#note
            && ((this.#date == null && other.#date == null) || (this.#date != null && other.#date != null && this.#date.equals(other.#date)));
    }

    toString(): string {
        return `Event(${this.summary()})`;
    }

    static fromEnvelope<T>(
        envelope: Envelope,
        contentDecoder: (contentEnvelope: Envelope) => T,
        contentEncoder: (content: T) => Envelope,
    ): Event<T> {
        const [tag, idCbor] = envelope.subject().tryLeaf().toTagged();
        if (Number(tag.value) !== TAG_EVENT) {
            throw EnvelopeError.invalidFormat();
        }
        const id = ARID.fromCbor(idCbor);

        const contentEnvelope = envelope.objectForPredicate(CONTENT);
        const content = contentDecoder(contentEnvelope);

        const note = envelope.extractOptionalObjectForPredicate<string>(NOTE) ?? '';
        const date = envelope.extractOptionalObjectForPredicate<CborDate>(DATE);

        return new Event(content, id, note, date, contentEncoder);
    }

    static stringFromEnvelope(envelope: Envelope): Event<string> {
        return Event.fromEnvelope(
            envelope,
            (contentEnvelope) => contentEnvelope.extractSubject<string>(),
            (content) => Envelope.from(content),
        );
    }
}
