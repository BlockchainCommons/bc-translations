import { MIN_SECRET_LEN, MAX_SECRET_LEN } from './constants.js';
import { SskrError } from './error.js';

/**
 * A secret to be split into shares.
 *
 * Wraps a `Uint8Array` with length validation ensuring the data is between
 * {@link MIN_SECRET_LEN} and {@link MAX_SECRET_LEN} bytes and has even length.
 */
export class Secret {
    readonly #data: Uint8Array;

    private constructor(data: Uint8Array) {
        this.#data = data;
    }

    /**
     * Creates a new `Secret` from the given byte data.
     *
     * @param data - The secret data to be split into shares.
     * @returns A new `Secret` instance.
     * @throws {SskrError} If the length is less than {@link MIN_SECRET_LEN},
     *   greater than {@link MAX_SECRET_LEN}, or not even.
     */
    static create(data: Uint8Array): Secret {
        const len = data.length;
        if (len < MIN_SECRET_LEN) {
            throw SskrError.secretTooShort();
        }
        if (len > MAX_SECRET_LEN) {
            throw SskrError.secretTooLong();
        }
        if ((len & 1) !== 0) {
            throw SskrError.secretLengthNotEven();
        }
        return new Secret(Uint8Array.from(data));
    }

    /** The length of the secret in bytes. */
    get length(): number {
        return this.#data.length;
    }

    /** Whether the secret is empty. */
    get isEmpty(): boolean {
        return this.length === 0;
    }

    /** The raw secret data. */
    get data(): Uint8Array {
        return this.#data;
    }

    /** Returns true if this secret is equal to another. */
    equals(other: Secret): boolean {
        if (this.length !== other.length) return false;
        for (let i = 0; i < this.length; i++) {
            if (this.#data[i] !== other.#data[i]) return false;
        }
        return true;
    }
}
