import {
    type EnvelopeFunction,
    functionName,
    knownFunction,
} from './function.js';

export class FunctionsStore {
    readonly #dict = new Map<string, string>();

    constructor(functions: Iterable<EnvelopeFunction> = []) {
        for (const value of functions) {
            this.insert(value);
        }
    }

    #key(value: EnvelopeFunction): string {
        return value.kind === 'known' ? `k:${value.value}` : `n:${value.name}`;
    }

    insert(value: EnvelopeFunction): void {
        if (value.kind !== 'known') {
            throw new Error('Only known functions can be inserted');
        }
        this.#dict.set(this.#key(value), functionName(value));
    }

    assignedName(value: EnvelopeFunction): string | undefined {
        return this.#dict.get(this.#key(value));
    }

    name(value: EnvelopeFunction): string {
        return this.assignedName(value) ?? functionName(value);
    }

    static nameForFunction(value: EnvelopeFunction, store?: FunctionsStore): string {
        return store?.assignedName(value) ?? functionName(value);
    }
}

export const ADD = knownFunction(1n, 'add');
export const SUB = knownFunction(2n, 'sub');
export const MUL = knownFunction(3n, 'mul');
export const DIV = knownFunction(4n, 'div');
export const NEG = knownFunction(5n, 'neg');
export const LT = knownFunction(6n, 'lt');
export const LE = knownFunction(7n, 'le');
export const GT = knownFunction(8n, 'gt');
export const GE = knownFunction(9n, 'ge');
export const EQ = knownFunction(10n, 'eq');
export const NE = knownFunction(11n, 'ne');
export const AND = knownFunction(12n, 'and');
export const OR = knownFunction(13n, 'or');
export const XOR = knownFunction(14n, 'xor');
export const NOT = knownFunction(15n, 'not');

export const GLOBAL_FUNCTIONS = new FunctionsStore([ADD, SUB, MUL, DIV]);
