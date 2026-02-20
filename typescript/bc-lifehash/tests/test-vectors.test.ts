import { readFileSync } from 'node:fs';
import { resolve, dirname } from 'node:path';
import { fileURLToPath } from 'node:url';
import { describe, expect, test } from 'vitest';
import { Version, makeFromUtf8, makeFromData } from '../src/index.js';

const __dirname = dirname(fileURLToPath(import.meta.url));

interface TestVector {
    input: string;
    input_type: string;
    version: string;
    module_size: number;
    has_alpha: boolean;
    width: number;
    height: number;
    colors: number[];
}

function parseVersion(s: string): Version {
    switch (s) {
        case 'version1': return Version.Version1;
        case 'version2': return Version.Version2;
        case 'detailed': return Version.Detailed;
        case 'fiducial': return Version.Fiducial;
        case 'grayscale_fiducial': return Version.GrayscaleFiducial;
        default: throw new Error(`Unknown version: ${s}`);
    }
}

function hexToBytes(hex: string): Uint8Array {
    const bytes = new Uint8Array(hex.length / 2);
    for (let i = 0; i < bytes.length; i++) {
        bytes[i] = parseInt(hex.slice(i * 2, i * 2 + 2), 16);
    }
    return bytes;
}

describe('test vectors', () => {
    const jsonStr = readFileSync(
        resolve(__dirname, 'test-vectors.json'),
        'utf-8',
    );
    const vectors: TestVector[] = JSON.parse(jsonStr);

    test('expected 35 vectors', () => {
        expect(vectors.length).toBe(35);
    });

    vectors.forEach((tv, i) => {
        const label = tv.input_type === 'hex'
            ? `hex(${tv.input.slice(0, 16)}${tv.input.length > 16 ? '...' : ''})`
            : JSON.stringify(tv.input);

        test(`vector ${i}: ${tv.version} ${label}`, () => {
            const version = parseVersion(tv.version);

            let image;
            if (tv.input_type === 'hex') {
                const data = tv.input === ''
                    ? new Uint8Array(0)
                    : hexToBytes(tv.input);
                image = makeFromData(data, version, tv.module_size, tv.has_alpha);
            } else {
                image = makeFromUtf8(tv.input, version, tv.module_size, tv.has_alpha);
            }

            expect(image.width).toBe(tv.width);
            expect(image.height).toBe(tv.height);
            expect(image.colors.length).toBe(tv.colors.length);

            const expected = new Uint8Array(tv.colors);
            if (!arraysEqual(image.colors, expected)) {
                for (let j = 0; j < image.colors.length; j++) {
                    if (image.colors[j] !== expected[j]) {
                        const components = tv.has_alpha ? 4 : 3;
                        const pixel = Math.trunc(j / components);
                        const component = j % components;
                        const compName = ['R', 'G', 'B', 'A'][component];
                        throw new Error(
                            `Vector ${i}: pixel data mismatch for input=${JSON.stringify(tv.input)} version=${tv.version}\n` +
                            `First diff at byte ${j} (pixel ${pixel}, ${compName}): got ${image.colors[j]}, expected ${expected[j]}`,
                        );
                    }
                }
            }
        });
    });
});

function arraysEqual(a: Uint8Array, b: Uint8Array): boolean {
    if (a.length !== b.length) return false;
    for (let i = 0; i < a.length; i++) {
        if (a[i] !== b[i]) return false;
    }
    return true;
}
