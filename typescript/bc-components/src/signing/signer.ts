import type { Result } from '../error.js';
import type { Signature } from './signature.js';
import type { SigningOptions } from './signing-private-key.js';

export interface Signer {
    signWithOptions(
        message: Uint8Array,
        options?: SigningOptions,
    ): Result<Signature>;

    sign(message: Uint8Array): Result<Signature>;
}

export interface Verifier {
    verify(signature: Signature, message: Uint8Array): boolean;
}
