/**
 * Date serialization for 2-byte, 4-byte, and 6-byte formats.
 */

import { CborDate } from '@bc/dcbor';
import { ProvenanceMarkError } from './error.js';

export function serialize2Bytes(date: CborDate): Uint8Array {
  const dt = date.datetime();
  const year = dt.getUTCFullYear();
  const month = dt.getUTCMonth() + 1;
  const day = dt.getUTCDate();

  const yy = year - 2023;
  if (yy < 0 || yy >= 128) {
    throw new ProvenanceMarkError('YearOutOfRange',
      `year out of range for 2-byte serialization: must be between 2023-2150, got ${year}`);
  }
  if (month < 1 || month > 12 || day < 1 || day > 31) {
    throw new ProvenanceMarkError('InvalidMonthOrDay',
      `invalid month (${month}) or day (${day}) for year ${year}`);
  }

  const value = (yy << 9) | (month << 5) | day;
  return new Uint8Array([(value >> 8) & 0xff, value & 0xff]);
}

export function deserialize2Bytes(bytes: Uint8Array): CborDate {
  const value = (bytes[0]! << 8) | bytes[1]!;
  const day = value & 0b11111;
  const month = (value >> 5) & 0b1111;
  const yy = (value >> 9) & 0b1111111;
  const year = yy + 2023;

  if (month < 1 || month > 12) {
    throw new ProvenanceMarkError('InvalidMonthOrDay',
      `invalid month (${month}) or day (${day}) for year ${year}`);
  }

  const daysInMonth = rangeOfDaysInMonth(year, month);
  if (day < 1 || day > daysInMonth) {
    throw new ProvenanceMarkError('InvalidMonthOrDay',
      `invalid month (${month}) or day (${day}) for year ${year}`);
  }

  return CborDate.fromYmd(year, month, day);
}

// Reference date: 2001-01-01T00:00:00Z
const REFERENCE_DATE_MS = Date.UTC(2001, 0, 1, 0, 0, 0, 0);

export function serialize4Bytes(date: CborDate): Uint8Array {
  const dt = date.datetime();
  const diffMs = dt.getTime() - REFERENCE_DATE_MS;
  const seconds = Math.floor(diffMs / 1000);
  if (seconds < 0 || seconds > 0xFFFFFFFF) {
    throw new ProvenanceMarkError('DateOutOfRange',
      'seconds value too large for u32');
  }
  const buf = new Uint8Array(4);
  buf[0] = (seconds >>> 24) & 0xff;
  buf[1] = (seconds >>> 16) & 0xff;
  buf[2] = (seconds >>> 8) & 0xff;
  buf[3] = seconds & 0xff;
  return buf;
}

export function deserialize4Bytes(bytes: Uint8Array): CborDate {
  const n = ((bytes[0]! << 24) | (bytes[1]! << 16) | (bytes[2]! << 8) | bytes[3]!) >>> 0;
  const ms = REFERENCE_DATE_MS + n * 1000;
  return CborDate.fromDatetime(new Date(ms));
}

const MAX_6_BYTE = 0xe5940a78a7ff;

export function serialize6Bytes(date: CborDate): Uint8Array {
  const dt = date.datetime();
  const diffMs = dt.getTime() - REFERENCE_DATE_MS;
  if (diffMs < 0) {
    throw new ProvenanceMarkError('DateOutOfRange',
      'milliseconds value too large for u64');
  }
  if (diffMs > MAX_6_BYTE) {
    throw new ProvenanceMarkError('DateOutOfRange',
      'date exceeds maximum representable value');
  }

  const buf = new Uint8Array(6);
  // Write 6 bytes big-endian from a number (safe up to 2^48)
  let val = diffMs;
  for (let i = 5; i >= 0; i--) {
    buf[i] = val & 0xff;
    val = Math.floor(val / 256);
  }
  return buf;
}

export function deserialize6Bytes(bytes: Uint8Array): CborDate {
  let n = 0;
  for (let i = 0; i < 6; i++) {
    n = n * 256 + bytes[i]!;
  }
  if (n > MAX_6_BYTE) {
    throw new ProvenanceMarkError('DateOutOfRange',
      'date exceeds maximum representable value');
  }
  const ms = REFERENCE_DATE_MS + n;
  return CborDate.fromDatetime(new Date(ms));
}

export function rangeOfDaysInMonth(year: number, month: number): number {
  // Day 0 of the next month gives last day of current month
  if (month === 12) {
    return new Date(Date.UTC(year + 1, 0, 0)).getUTCDate();
  }
  return new Date(Date.UTC(year, month, 0)).getUTCDate();
}
