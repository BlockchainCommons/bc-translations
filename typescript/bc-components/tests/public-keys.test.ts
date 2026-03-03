import { describe, expect, test } from 'vitest';

import { PublicKeys, keypair, registerTags } from '../src/index.js';

describe('PublicKeys', () => {
    test('cbor and ur roundtrip', () => {
        registerTags();
        const [, publicKeys] = keypair();

        expect(PublicKeys.fromCbor(publicKeys.taggedCbor()).equals(publicKeys)).toBe(true);

        const ur = publicKeys.urString();
        expect(PublicKeys.fromURString(ur).equals(publicKeys)).toBe(true);
    });
});
