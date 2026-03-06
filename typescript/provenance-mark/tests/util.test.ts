import { describe, expect, test } from 'vitest';
import { CborDate } from '@bc/dcbor';

import {
  ProvenanceSeed,
  parseDate,
  parseSeed,
} from '../src/index.js';

describe('util', () => {
  test('parse_seed', () => {
    const seed = ProvenanceSeed.createWithPassphrase('Wolf');
    const parsed = parseSeed(seed.toJSON());

    expect(parsed.equals(seed)).toBe(true);
  });

  test('parse_date', () => {
    const expected = CborDate.fromString('2025-10-26');

    expect(parseDate('2025-10-26').equals(expected)).toBe(true);
    expect(() => parseDate('not-a-date')).toThrow();
  });
});
