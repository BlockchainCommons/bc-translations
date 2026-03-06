/**
 * Parsing helpers that mirror the Rust crate's public `util` entrypoints.
 */

import { CborDate } from '@bc/dcbor';

import { ProvenanceMarkError } from './error.js';
import { ProvenanceSeed } from './seed.js';

/**
 * Parse a base64-encoded seed string into a provenance seed.
 */
export function parseSeed(value: string): ProvenanceSeed {
  return ProvenanceSeed.fromJSON(value);
}

/**
 * Parse an ISO 8601 date string into a CBOR date.
 */
export function parseDate(value: string): CborDate {
  try {
    return CborDate.fromString(value);
  } catch (error: unknown) {
    const message =
      error instanceof Error ? error.message : String(error);
    throw new ProvenanceMarkError('InvalidDate', message);
  }
}
