import type { EnvelopeFunction } from './function.js';
import { namedFunction, functionFromTaggedCbor, functionTaggedCbor, functionsEqual } from './function.js';
import type { EnvelopeParameter } from './parameter.js';
import { parameterTaggedCbor } from './parameter.js';
import { asEnvelope } from './envelope-encodable.js';
import { EnvelopeError } from './error.js';
import { Envelope } from './envelope.js';

export class Expression {
    readonly #function: EnvelopeFunction;
    readonly #envelope: Envelope;

    constructor(functionValue: EnvelopeFunction | string, envelope?: Envelope) {
        if (typeof functionValue === 'string') {
            this.#function = namedFunction(functionValue);
        } else {
            this.#function = functionValue;
        }

        this.#envelope = envelope ?? Envelope.from(functionTaggedCbor(this.#function));
    }

    function(): EnvelopeFunction {
        return this.#function;
    }

    expressionEnvelope(): Envelope {
        return this.#envelope;
    }

    withParameter(parameter: EnvelopeParameter, value: unknown): Expression {
        const assertion = Envelope.newAssertion(parameterTaggedCbor(parameter), asEnvelope(value));
        return new Expression(this.#function, this.#envelope.addAssertionEnvelope(assertion));
    }

    withOptionalParameter(parameter: EnvelopeParameter, value?: unknown): Expression {
        if (value !== undefined) {
            return this.withParameter(parameter, value);
        }
        return this;
    }

    objectForParameter(parameter: EnvelopeParameter): Envelope {
        return this.#envelope.objectForPredicate(parameterTaggedCbor(parameter));
    }

    objectsForParameter(parameter: EnvelopeParameter): Envelope[] {
        return this.#envelope.objectsForPredicate(parameterTaggedCbor(parameter));
    }

    extractObjectForParameter<T>(parameter: EnvelopeParameter, decoder?: (envelope: Envelope) => T): T {
        return this.#envelope.extractObjectForPredicate(parameterTaggedCbor(parameter), decoder);
    }

    extractOptionalObjectForParameter<T>(parameter: EnvelopeParameter, decoder?: (envelope: Envelope) => T): T | undefined {
        return this.#envelope.extractOptionalObjectForPredicate(parameterTaggedCbor(parameter), decoder);
    }

    extractObjectsForParameter<T>(parameter: EnvelopeParameter, decoder?: (envelope: Envelope) => T): T[] {
        return this.#envelope.extractObjectsForPredicate(parameterTaggedCbor(parameter), decoder);
    }

    toEnvelope(): Envelope {
        return this.#envelope;
    }

    equals(other: unknown): boolean {
        return other instanceof Expression && this.#envelope.isEquivalentTo(other.#envelope);
    }

    toString(): string {
        return this.#envelope.formatFlat();
    }

    static fromEnvelope(envelope: Envelope, expectedFunction?: EnvelopeFunction): Expression {
        const functionValue = functionFromTaggedCbor(envelope.subject().tryLeaf());
        if (expectedFunction != null) {
            if (!functionsEqual(expectedFunction, functionValue)) {
                throw EnvelopeError.invalidFormat();
            }
        }
        return new Expression(functionValue, envelope);
    }
}
