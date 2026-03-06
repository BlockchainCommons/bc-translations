/**
 * Provenance mark resolution levels.
 */

import { type Cbor, cbor, expectUnsigned } from '@bc/dcbor';
import { CborDate } from '@bc/dcbor';
import { ProvenanceMarkError } from './error.js';
import {
  serialize2Bytes, deserialize2Bytes,
  serialize4Bytes, deserialize4Bytes,
  serialize6Bytes, deserialize6Bytes,
} from './date-serialization.js';

export enum ProvenanceMarkResolution {
  Low = 0,
  Medium = 1,
  Quartile = 2,
  High = 3,
}

export function resolutionFromU8(value: number): ProvenanceMarkResolution {
  switch (value) {
    case 0: return ProvenanceMarkResolution.Low;
    case 1: return ProvenanceMarkResolution.Medium;
    case 2: return ProvenanceMarkResolution.Quartile;
    case 3: return ProvenanceMarkResolution.High;
    default:
      throw new ProvenanceMarkError('ResolutionError',
        `invalid provenance mark resolution value: ${value}`);
  }
}

export function resolutionFromCbor(c: Cbor): ProvenanceMarkResolution {
  const value = expectUnsigned(c);
  return resolutionFromU8(Number(value));
}

export function resolutionToCbor(res: ProvenanceMarkResolution): Cbor {
  return cbor(res as number);
}

export function linkLength(res: ProvenanceMarkResolution): number {
  switch (res) {
    case ProvenanceMarkResolution.Low: return 4;
    case ProvenanceMarkResolution.Medium: return 8;
    case ProvenanceMarkResolution.Quartile: return 16;
    case ProvenanceMarkResolution.High: return 32;
  }
}

export function seqBytesLength(res: ProvenanceMarkResolution): number {
  switch (res) {
    case ProvenanceMarkResolution.Low: return 2;
    default: return 4;
  }
}

export function dateBytesLength(res: ProvenanceMarkResolution): number {
  switch (res) {
    case ProvenanceMarkResolution.Low: return 2;
    case ProvenanceMarkResolution.Medium: return 4;
    default: return 6;
  }
}

export function fixedLength(res: ProvenanceMarkResolution): number {
  return linkLength(res) * 3 + seqBytesLength(res) + dateBytesLength(res);
}

export function keyRangeEnd(res: ProvenanceMarkResolution): number {
  return linkLength(res);
}

export function chainIdRangeEnd(res: ProvenanceMarkResolution): number {
  return linkLength(res);
}

export function hashRangeStart(res: ProvenanceMarkResolution): number {
  return chainIdRangeEnd(res);
}

export function hashRangeEnd(res: ProvenanceMarkResolution): number {
  return hashRangeStart(res) + linkLength(res);
}

export function seqBytesRangeStart(res: ProvenanceMarkResolution): number {
  return hashRangeEnd(res);
}

export function seqBytesRangeEnd(res: ProvenanceMarkResolution): number {
  return seqBytesRangeStart(res) + seqBytesLength(res);
}

export function dateBytesRangeStart(res: ProvenanceMarkResolution): number {
  return seqBytesRangeEnd(res);
}

export function dateBytesRangeEnd(res: ProvenanceMarkResolution): number {
  return dateBytesRangeStart(res) + dateBytesLength(res);
}

export function infoRangeStart(res: ProvenanceMarkResolution): number {
  return dateBytesRangeEnd(res);
}

/**
 * Serialize a date using the byte-width required by the selected resolution.
 */
export function serializeDate(res: ProvenanceMarkResolution, date: CborDate): Uint8Array {
  switch (res) {
    case ProvenanceMarkResolution.Low:
      return serialize2Bytes(date);
    case ProvenanceMarkResolution.Medium:
      return serialize4Bytes(date);
    case ProvenanceMarkResolution.Quartile:
    case ProvenanceMarkResolution.High:
      return serialize6Bytes(date);
  }
}

/**
 * Deserialize bytes into a date using the selected resolution's precision.
 */
export function deserializeDate(res: ProvenanceMarkResolution, data: Uint8Array): CborDate {
  switch (res) {
    case ProvenanceMarkResolution.Low:
      if (data.length !== 2) break;
      return deserialize2Bytes(data);
    case ProvenanceMarkResolution.Medium:
      if (data.length !== 4) break;
      return deserialize4Bytes(data);
    case ProvenanceMarkResolution.Quartile:
    case ProvenanceMarkResolution.High:
      if (data.length !== 6) break;
      return deserialize6Bytes(data);
  }
  throw new ProvenanceMarkError('ResolutionError',
    `invalid date length: expected 2, 4, or 6 bytes, got ${data.length}`);
}

/**
 * Serialize a sequence number using the byte-width required by the resolution.
 */
export function serializeSeq(res: ProvenanceMarkResolution, seq: number): Uint8Array {
  const len = seqBytesLength(res);
  if (len === 2) {
    if (seq > 0xFFFF) {
      throw new ProvenanceMarkError('ResolutionError',
        `sequence number ${seq} out of range for 2-byte format (max 65535)`);
    }
    return new Uint8Array([(seq >> 8) & 0xff, seq & 0xff]);
  }
  return new Uint8Array([
    (seq >>> 24) & 0xff,
    (seq >>> 16) & 0xff,
    (seq >>> 8) & 0xff,
    seq & 0xff,
  ]);
}

/**
 * Deserialize bytes into a sequence number using the selected resolution.
 */
export function deserializeSeq(res: ProvenanceMarkResolution, data: Uint8Array): number {
  const len = seqBytesLength(res);
  if (len === 2 && data.length === 2) {
    return (data[0]! << 8) | data[1]!;
  }
  if (len === 4 && data.length === 4) {
    return ((data[0]! << 24) | (data[1]! << 16) | (data[2]! << 8) | data[3]!) >>> 0;
  }
  throw new ProvenanceMarkError('ResolutionError',
    `invalid sequence number length: expected 2 or 4 bytes, got ${data.length}`);
}

export function resolutionToString(res: ProvenanceMarkResolution): string {
  switch (res) {
    case ProvenanceMarkResolution.Low: return 'low';
    case ProvenanceMarkResolution.Medium: return 'medium';
    case ProvenanceMarkResolution.Quartile: return 'quartile';
    case ProvenanceMarkResolution.High: return 'high';
  }
}
