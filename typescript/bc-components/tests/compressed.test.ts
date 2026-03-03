import { describe, expect, test } from 'vitest';

import {
    Compressed,
    Digest,
} from '../src/index.js';
import { utf8 } from './test-helpers.js';

describe('Compressed', () => {
    test('compress/decompress roundtrip', () => {
        const source = utf8(
            'Lorem ipsum dolor sit amet consectetur adipiscing elit mi nibh ornare proin blandit diam ridiculus, faucibus mus dui eu vehicula nam donec dictumst sed vivamus bibendum aliquet efficitur.',
        );
        const compressed = Compressed.fromDecompressedData(source);

        expect(compressed.decompress()).toEqual(source);
        expect(compressed.compressedSize()).toBeLessThanOrEqual(source.length);
    });

    test('optional digest is preserved', () => {
        const source = utf8('hello world');
        const digest = Digest.fromImage(source);
        const compressed = Compressed.fromDecompressedData(source, digest);

        expect(compressed.hasDigest()).toBe(true);
        expect(compressed.digest().equals(digest)).toBe(true);
    });
});
