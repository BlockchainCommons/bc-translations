import { beforeAll, describe, expect, test } from 'vitest';
import { Ed25519PrivateKey, SigningPrivateKey } from '@bc/components';

import { Envelope, registerTags } from '../src/index.js';
import { aliceSeed } from './test-data.js';

describe('ed25519 tests', () => {
    beforeAll(() => {
        registerTags();
    });

    test('sign/verify with ed25519 key', () => {
        const ed25519 = Ed25519PrivateKey.deriveFromKeyMaterial(aliceSeed());
        const signer = SigningPrivateKey.newEd25519(ed25519);
        const verifier = signer.publicKey();

        const envelope = Envelope.from('ed25519').addSignature(signer);
        expect(envelope.hasSignatureFrom(verifier)).toBe(true);
    });
});
