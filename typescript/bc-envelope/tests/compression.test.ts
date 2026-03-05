import { beforeAll, describe, expect, test } from 'vitest';

import { Envelope, registerTags } from '../src/index.js';

describe('compression tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('compress/decompress roundtrip', () => {
        const envelope = Envelope.from('compress me').addAssertion('n', 1);
        const compressed = envelope.compress();
        const decompressed = compressed.decompress();

        expect(compressed.isCompressed()).toBe(true);
        expect(decompressed.isIdenticalTo(envelope)).toBe(true);
    });

    test('compress/decompress subject roundtrip', () => {
        const envelope = Envelope.from('compress subject').addAssertion('k', 'v');
        const compressed = envelope.compressSubject();
        const decompressed = compressed.decompressSubject();

        expect(compressed.subject().isCompressed()).toBe(true);
        expect(decompressed.isIdenticalTo(envelope)).toBe(true);
    });
});
