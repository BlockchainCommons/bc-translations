/**
 * RNG state -- a 32-byte wrapper for the Xoshiro256** state.
 */

import { type Cbor, toByteString, expectBytes } from '@bc/dcbor';
import { ProvenanceMarkError } from './error.js';
import { bytesToHex, bytesEqual, toBase64, fromBase64 } from './utils.js';

export const RNG_STATE_LENGTH = 32;

export class RngState {
  readonly #data: Uint8Array;

  private constructor(data: Uint8Array) {
    this.#data = data;
  }

  toBytes(): Uint8Array {
    return new Uint8Array(this.#data);
  }

  static fromBytes(bytes: Uint8Array): RngState {
    if (bytes.length !== RNG_STATE_LENGTH) {
      throw new ProvenanceMarkError(
        'InvalidSeedLength',
        `invalid RNG state length: expected ${RNG_STATE_LENGTH} bytes, got ${bytes.length} bytes`,
      );
    }
    return new RngState(new Uint8Array(bytes));
  }

  hex(): string {
    return bytesToHex(this.#data);
  }

  toCbor(): Cbor {
    return toByteString(this.#data);
  }

  static fromCbor(c: Cbor): RngState {
    const bytes = expectBytes(c);
    return RngState.fromBytes(new Uint8Array(bytes));
  }

  toJSON(): string {
    return toBase64(this.#data);
  }

  static fromJSON(s: string): RngState {
    const bytes = fromBase64(s);
    return RngState.fromBytes(bytes);
  }

  equals(other: RngState): boolean {
    return bytesEqual(this.#data, other.#data);
  }
}
