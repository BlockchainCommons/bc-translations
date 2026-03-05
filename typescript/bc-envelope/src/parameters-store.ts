import {
    type EnvelopeParameter,
    knownParameter,
    parameterName,
} from './parameter.js';

export class ParametersStore {
    readonly #dict = new Map<string, string>();

    constructor(parameters: Iterable<EnvelopeParameter> = []) {
        for (const value of parameters) {
            this.insert(value);
        }
    }

    #key(value: EnvelopeParameter): string {
        return value.kind === 'known' ? `k:${value.value}` : `n:${value.name}`;
    }

    insert(value: EnvelopeParameter): void {
        if (value.kind !== 'known') {
            throw new Error('Only known parameters can be inserted');
        }
        this.#dict.set(this.#key(value), parameterName(value));
    }

    assignedName(value: EnvelopeParameter): string | undefined {
        return this.#dict.get(this.#key(value));
    }

    name(value: EnvelopeParameter): string {
        return this.assignedName(value) ?? parameterName(value);
    }

    static nameForParameter(value: EnvelopeParameter, store?: ParametersStore): string {
        return store?.assignedName(value) ?? parameterName(value);
    }
}

export const BLANK = knownParameter(1n, '_');
export const LHS = knownParameter(2n, 'lhs');
export const RHS = knownParameter(3n, 'rhs');

export const GLOBAL_PARAMETERS = new ParametersStore([BLANK, LHS, RHS]);
