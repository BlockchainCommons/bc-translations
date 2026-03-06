import { describe, test, expect } from 'vitest';
import { bytesToHex, CborDate } from '@bc/dcbor';
import {
  serialize2Bytes, deserialize2Bytes,
  serialize4Bytes, deserialize4Bytes,
  serialize6Bytes, deserialize6Bytes,
} from '../src/index.js';

describe('date serialization', () => {
  test('2-byte dates', () => {
    const baseDate = CborDate.fromYmd(2023, 6, 20);
    const serialized = serialize2Bytes(baseDate);
    expect(bytesToHex(serialized)).toBe('00d4');
    const deserialized = deserialize2Bytes(serialized);
    expect(baseDate.equals(deserialized)).toBe(true);

    // Minimum date
    const minSerialized = new Uint8Array([0x00, 0x21]);
    const minDate = CborDate.fromYmd(2023, 1, 1);
    const deserializedMin = deserialize2Bytes(minSerialized);
    expect(minDate.equals(deserializedMin)).toBe(true);

    // Maximum date
    const maxSerialized = new Uint8Array([0xff, 0x9f]);
    const deserializedMax = deserialize2Bytes(maxSerialized);
    const expectedMaxDate = CborDate.fromYmd(2150, 12, 31);
    expect(deserializedMax.equals(expectedMaxDate)).toBe(true);

    // Invalid date (2023-02-30)
    const invalidSerialized = new Uint8Array([0x00, 0x5e]);
    expect(() => deserialize2Bytes(invalidSerialized)).toThrow();
  });

  test('4-byte dates', () => {
    const baseDate = CborDate.fromYmdHms(2023, 6, 20, 12, 34, 56);
    const serialized = serialize4Bytes(baseDate);
    expect(bytesToHex(serialized)).toBe('2a41d470');
    const deserialized = deserialize4Bytes(serialized);
    expect(baseDate.equals(deserialized)).toBe(true);

    // Minimum date
    const minSerialized = new Uint8Array([0x00, 0x00, 0x00, 0x00]);
    const minDate = CborDate.fromYmd(2001, 1, 1);
    const deserializedMin = deserialize4Bytes(minSerialized);
    expect(minDate.equals(deserializedMin)).toBe(true);

    // Maximum date
    const maxSerialized = new Uint8Array([0xff, 0xff, 0xff, 0xff]);
    const deserializedMax = deserialize4Bytes(maxSerialized);
    const expectedMaxDate = CborDate.fromYmdHms(2137, 2, 7, 6, 28, 15);
    expect(deserializedMax.equals(expectedMaxDate)).toBe(true);
  });

  test('6-byte dates', () => {
    // Base date with milliseconds
    const dt = new Date(Date.UTC(2023, 5, 20, 12, 34, 56, 789));
    const baseDate = CborDate.fromDatetime(dt);
    const serialized = serialize6Bytes(baseDate);
    expect(bytesToHex(serialized)).toBe('00a51125d895');
    const deserialized = deserialize6Bytes(serialized);
    expect(baseDate.equals(deserialized)).toBe(true);

    // Minimum date
    const minSerialized = new Uint8Array([0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
    const minDate = CborDate.fromYmd(2001, 1, 1);
    const deserializedMin = deserialize6Bytes(minSerialized);
    expect(minDate.equals(deserializedMin)).toBe(true);

    // Maximum date
    const maxSerialized = new Uint8Array([0xe5, 0x94, 0x0a, 0x78, 0xa7, 0xff]);
    const deserializedMax = deserialize6Bytes(maxSerialized);
    const expectedDt = new Date(Date.UTC(9999, 11, 31, 23, 59, 59, 999));
    const expectedMaxDate = CborDate.fromDatetime(expectedDt);
    expect(deserializedMax.equals(expectedMaxDate)).toBe(true);

    // Invalid date (exceeds maximum)
    const invalidSerialized = new Uint8Array([0xe5, 0x94, 0x0a, 0x78, 0xa8, 0x00]);
    expect(() => deserialize6Bytes(invalidSerialized)).toThrow();
  });
});
