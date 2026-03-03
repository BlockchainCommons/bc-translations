import { expect, test } from 'vitest';
import { IS_A, KNOWN_VALUES } from '../src/index.js';

test('basic constant lookup', () => {
    expect(IS_A.value).toBe(1n);
    expect(IS_A.name).toBe('isA');

    const isA = KNOWN_VALUES.knownValueNamed('isA');
    expect(isA).toBeDefined();
    expect(isA!.value).toBe(1n);
});
