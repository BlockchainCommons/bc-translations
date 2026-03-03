export type BCComponentsErrorCode =
    | 'general'
    | 'invalid-size'
    | 'invalid-data'
    | 'data-too-short'
    | 'crypto'
    | 'ssh'
    | 'compression'
    | 'post-quantum'
    | 'level-mismatch'
    | 'unsupported';

/** bc-components package error. */
export class BCComponentsError extends Error {
    readonly code: BCComponentsErrorCode;

    constructor(code: BCComponentsErrorCode, message: string) {
        super(message);
        this.name = 'BCComponentsError';
        this.code = code;
    }

    equals(other: unknown): boolean {
        return other instanceof BCComponentsError && other.code === this.code;
    }

    static general(message: string): BCComponentsError {
        return new BCComponentsError('general', message);
    }

    static invalidSize(
        what: string,
        expected: number,
        actual: number,
    ): BCComponentsError {
        return new BCComponentsError(
            'invalid-size',
            `${what}: expected ${expected} bytes, got ${actual}`,
        );
    }

    static invalidData(what: string, message: string): BCComponentsError {
        return new BCComponentsError('invalid-data', `${what}: ${message}`);
    }

    static dataTooShort(
        what: string,
        expectedAtLeast: number,
        actual: number,
    ): BCComponentsError {
        return new BCComponentsError(
            'data-too-short',
            `${what}: expected at least ${expectedAtLeast} bytes, got ${actual}`,
        );
    }

    static crypto(message: string): BCComponentsError {
        return new BCComponentsError('crypto', message);
    }

    static ssh(message: string): BCComponentsError {
        return new BCComponentsError('ssh', message);
    }

    static compression(message: string): BCComponentsError {
        return new BCComponentsError('compression', message);
    }

    static postQuantum(message: string): BCComponentsError {
        return new BCComponentsError('post-quantum', message);
    }

    static levelMismatch(
        expected: string,
        actual: string,
    ): BCComponentsError {
        return new BCComponentsError(
            'level-mismatch',
            `Level mismatch: expected ${expected}, got ${actual}`,
        );
    }

    static unsupported(message: string): BCComponentsError {
        return new BCComponentsError('unsupported', message);
    }
}

export type Result<T> = T;
