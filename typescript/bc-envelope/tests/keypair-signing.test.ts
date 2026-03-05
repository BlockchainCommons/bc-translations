import { beforeAll, describe, expect, test } from 'vitest';
import { keypair } from '@bc/components';

import { Envelope, registerTags } from '../src/index.js';

describe('keypair signing tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('sign and verify with generated keypair', () => {
        const [privateKeys, publicKeys] = keypair();
        const envelope = Envelope.from('keypair test').addSignature(privateKeys);

        expect(envelope.hasSignatureFrom(publicKeys)).toBe(true);
        expect(envelope.verifySignatureFrom(publicKeys).isIdenticalTo(envelope)).toBe(true);
    });
});
