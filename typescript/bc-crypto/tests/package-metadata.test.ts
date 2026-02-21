import { readFileSync } from 'node:fs';
import { describe, expect, test } from 'vitest';

import {
    aeadChaCha20Poly1305Encrypt,
    ecdsaSign,
    sha256,
} from '../src/index.js';

describe('packageMetadata', () => {
    test('package name and version', () => {
        const packageJson = JSON.parse(
            readFileSync(new URL('../package.json', import.meta.url), 'utf8'),
        ) as { name: string; version: string };

        expect(packageJson.name).toBe('@bc/crypto');
        expect(packageJson.version).toBe('0.14.0');
    });

    test('expected exports present', () => {
        expect(typeof sha256).toBe('function');
        expect(typeof aeadChaCha20Poly1305Encrypt).toBe('function');
        expect(typeof ecdsaSign).toBe('function');
    });
});
