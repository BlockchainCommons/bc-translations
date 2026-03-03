import { describe, expect, test } from 'vitest';

import {
    keypair,
    PrivateKeys,
    PublicKeys,
    registerTags,
} from '../src/index.js';
import { utf8 } from './test-helpers.js';

describe('Key containers', () => {
    test('private/public key container cbor and ur roundtrip', () => {
        registerTags();
        const [privateKeys, publicKeys] = keypair();

        const privateCbor = privateKeys.taggedCbor();
        const privateRoundtrip = PrivateKeys.fromCbor(privateCbor);
        expect(privateRoundtrip.equals(privateKeys)).toBe(true);

        const publicCbor = publicKeys.taggedCbor();
        const publicRoundtrip = PublicKeys.fromCbor(publicCbor);
        expect(publicRoundtrip.equals(publicKeys)).toBe(true);

        const privateUr = privateKeys.urString();
        expect(PrivateKeys.fromURString(privateUr).equals(privateKeys)).toBe(true);

        const publicUr = publicKeys.urString();
        expect(PublicKeys.fromURString(publicUr).equals(publicKeys)).toBe(true);
    });

    test('sign and verify through containers', () => {
        const [privateKeys, publicKeys] = keypair();
        const message = utf8('container signing test');

        const signature = privateKeys.sign(message);
        expect(publicKeys.verify(signature, message)).toBe(true);
        expect(publicKeys.verify(signature, utf8('tampered'))).toBe(false);
    });
});
