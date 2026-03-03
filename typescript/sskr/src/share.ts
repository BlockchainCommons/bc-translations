import type { Secret } from './secret.js';

/** Internal representation of a deserialized SSKR share. */
export class SSKRShare {
    readonly #identifier: number;
    readonly #groupIndex: number;
    readonly #groupThreshold: number;
    readonly #groupCount: number;
    readonly #memberIndex: number;
    readonly #memberThreshold: number;
    readonly #value: Secret;

    constructor(
        identifier: number,
        groupIndex: number,
        groupThreshold: number,
        groupCount: number,
        memberIndex: number,
        memberThreshold: number,
        value: Secret,
    ) {
        this.#identifier = identifier;
        this.#groupIndex = groupIndex;
        this.#groupThreshold = groupThreshold;
        this.#groupCount = groupCount;
        this.#memberIndex = memberIndex;
        this.#memberThreshold = memberThreshold;
        this.#value = value;
    }

    get identifier(): number { return this.#identifier; }
    get groupIndex(): number { return this.#groupIndex; }
    get groupThreshold(): number { return this.#groupThreshold; }
    get groupCount(): number { return this.#groupCount; }
    get memberIndex(): number { return this.#memberIndex; }
    get memberThreshold(): number { return this.#memberThreshold; }
    get value(): Secret { return this.#value; }
}
