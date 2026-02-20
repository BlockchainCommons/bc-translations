/** Package-specific base error. */
export class BCryptoError extends globalThis.Error {
    constructor(message: string) {
        super(message);
        this.name = 'BCryptoError';
    }
}

/** AEAD authentication/decryption failure. */
export class AeadError extends BCryptoError {
    constructor() {
        super('AEAD error');
        this.name = 'AeadError';
    }
}

export { BCryptoError as Error };

/** Rust-style Result alias; errors are represented as thrown exceptions in TS. */
export type Result<T> = T;
