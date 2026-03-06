/**
 * Provenance seed -- a 32-byte seed for mark generation.
 */

import { type Cbor, toByteString, expectBytes } from '@bc/dcbor';
import { type RandomNumberGenerator, rngRandomData, SecureRandomNumberGenerator } from '@bc/rand';
import { extendKey } from './crypto-utils.js';
import { ProvenanceMarkError } from './error.js';
import { bytesToHex, bytesEqual, toBase64, fromBase64 } from './utils.js';

export const PROVENANCE_SEED_LENGTH = 32;

export class ProvenanceSeed {
  readonly #data: Uint8Array;

  private constructor(data: Uint8Array) {
    this.#data = data;
  }

  static create(): ProvenanceSeed {
    const rng = new SecureRandomNumberGenerator();
    return ProvenanceSeed.createUsing(rng);
  }

  static createUsing(rng: RandomNumberGenerator): ProvenanceSeed {
    const data = rngRandomData(rng, PROVENANCE_SEED_LENGTH);
    return ProvenanceSeed.fromBytes(data);
  }

  static createWithPassphrase(passphrase: string): ProvenanceSeed {
    const seedData = extendKey(new TextEncoder().encode(passphrase));
    return ProvenanceSeed.fromBytes(seedData);
  }

  toBytes(): Uint8Array {
    return new Uint8Array(this.#data);
  }

  static fromBytes(bytes: Uint8Array): ProvenanceSeed {
    if (bytes.length !== PROVENANCE_SEED_LENGTH) {
      throw new ProvenanceMarkError('InvalidSeedLength',
        `invalid seed length: expected 32 bytes, got ${bytes.length} bytes`);
    }
    return new ProvenanceSeed(new Uint8Array(bytes));
  }

  hex(): string {
    return bytesToHex(this.#data);
  }

  toCbor(): Cbor {
    return toByteString(this.#data);
  }

  static fromCbor(c: Cbor): ProvenanceSeed {
    const bytes = expectBytes(c);
    return ProvenanceSeed.fromBytes(new Uint8Array(bytes));
  }

  toJSON(): string {
    return toBase64(this.#data);
  }

  static fromJSON(s: string): ProvenanceSeed {
    const bytes = fromBase64(s);
    return ProvenanceSeed.fromBytes(bytes);
  }

  equals(other: ProvenanceSeed): boolean {
    return bytesEqual(this.#data, other.#data);
  }
}
