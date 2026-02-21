/**
 * Shamir's Secret Sharing (SSS) for TypeScript.
 *
 * Splits a secret into shares such that a threshold number of shares are
 * needed to reconstruct the secret. Uses bitsliced GF(2^8) arithmetic
 * for constant-time operations.
 *
 * @packageDocumentation
 */

/** The minimum length of a secret in bytes. */
export const MIN_SECRET_LEN = 16;

/** The maximum length of a secret in bytes. */
export const MAX_SECRET_LEN = 32;

/** The maximum number of shares that can be generated from a secret. */
export const MAX_SHARE_COUNT = 16;

export { ShamirError } from './error.js';
export { splitSecret, recoverSecret } from './shamir.js';
