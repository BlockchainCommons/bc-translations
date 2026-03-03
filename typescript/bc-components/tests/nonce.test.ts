import { describe, expect, test } from 'vitest';

import { Nonce, registerTags } from '../src/index.js';

describe('Nonce', () => {
    test('size and hex conversion', () => {
        const nonce = Nonce.new();
        expect(nonce.data.length).toBe(12);

        const hex = nonce.hex();
        expect(Nonce.fromHex(hex).equals(nonce)).toBe(true);
    });

    test('cbor and ur roundtrip', () => {
        registerTags();
        const nonce = Nonce.fromData(new Uint8Array(12).fill(7));
        expect(Nonce.fromCbor(nonce.taggedCbor()).equals(nonce)).toBe(true);

        const ur = nonce.urString();
        expect(Nonce.fromURString(ur).equals(nonce)).toBe(true);
    });
});
