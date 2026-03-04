import { describe, expect, test } from 'vitest';

import { JSON as BCJSON } from '../src/index.js';

describe('JSON', () => {
    test('string and hex conversion', () => {
        const json = BCJSON.fromString('{"k":"v"}');
        expect(json.stringValue).toBe('{"k":"v"}');
        expect(BCJSON.fromHex(json.hex()).equals(json)).toBe(true);
    });

    test('cbor roundtrip', () => {
        const json = BCJSON.fromString('{"a":1}');
        expect(BCJSON.fromCbor(json.taggedCbor()).equals(json)).toBe(true);
    });
});
