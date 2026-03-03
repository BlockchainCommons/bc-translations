import { describe, expect, test } from 'vitest';

import { HKDFRng } from '../src/index.js';
import { expectBytes } from './test-helpers.js';

describe('HKDFRng', () => {
    test('generates expected deterministic sequence', () => {
        const rng = HKDFRng.new(new TextEncoder().encode('key_material'), 'salt');

        expectBytes(rng.randomData(16), '1032ac8ffea232a27c79fe381d7eb7e4');
        expectBytes(rng.randomData(16), 'aeaaf727d35b6f338218391f9f8fa1f3');
        expectBytes(rng.randomData(16), '4348a59427711deb1e7d8a6959c6adb4');
        expectBytes(rng.randomData(16), '5d937a42cb5fb090fe1a1ec88f56e32b');
    });

    test('nextU32 and nextU64 vectors', () => {
        const rng1 = HKDFRng.new(new TextEncoder().encode('key_material'), 'salt');
        expect(rng1.nextU32()).toBe(2410426896);

        const rng2 = HKDFRng.new(new TextEncoder().encode('key_material'), 'salt');
        expect(rng2.nextU64()).toBe(11687583197195678224n);
    });
});
