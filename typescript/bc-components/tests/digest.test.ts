import { describe, expect, test } from 'vitest';

import { Digest, registerTags } from '../src/index.js';
import { utf8 } from './test-helpers.js';

describe('Digest', () => {
    test('hash and validation', () => {
        const data = utf8('hello world');
        const digest = Digest.fromImage(data);
        expect(digest.validate(data)).toBe(true);
        expect(digest.validate(utf8('hello world!'))).toBe(false);
    });

    test('cbor and ur roundtrip', () => {
        registerTags();
        const digest = Digest.fromImage(utf8('vector'));
        expect(Digest.fromCbor(digest.taggedCbor()).equals(digest)).toBe(true);

        const ur = digest.urString();
        expect(Digest.fromURString(ur).equals(digest)).toBe(true);
    });
});
