import { describe, expect, test } from 'vitest';

import { PrivateKeys, keypair, registerTags } from '../src/index.js';

describe('PrivateKeys', () => {
    test('cbor and ur roundtrip', () => {
        registerTags();
        const [privateKeys] = keypair();

        expect(PrivateKeys.fromCbor(privateKeys.taggedCbor()).equals(privateKeys)).toBe(true);

        const ur = privateKeys.urString();
        expect(PrivateKeys.fromURString(ur).equals(privateKeys)).toBe(true);
    });
});
