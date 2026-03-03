import { describe, expect, test } from 'vitest';

import {
    MLDSA,
    MLDSAPrivateKey,
    MLDSAPublicKey,
    MLDSASignature,
} from '../src/index.js';
import { utf8 } from './test-helpers.js';

describe('MLDSA', () => {
    test('keypair sign verify by level', () => {
        const message = utf8('post-quantum message');
        for (const level of [MLDSA.mldsa44, MLDSA.mldsa65, MLDSA.mldsa87]) {
            const [privateKey, publicKey] = level.keypair();
            const signature = privateKey.sign(message);
            expect(publicKey.verify(signature, message)).toBe(true);
            expect(publicKey.verify(signature, utf8('tampered'))).toBe(false);
        }
    });

    test('cbor roundtrip', () => {
        const [privateKey, publicKey] = MLDSA.mldsa65.keypair();
        const signature = privateKey.sign(utf8('cbor'));

        expect(MLDSAPrivateKey.fromCbor(privateKey.taggedCbor()).equals(privateKey)).toBe(true);
        expect(MLDSAPublicKey.fromCbor(publicKey.taggedCbor()).equals(publicKey)).toBe(true);
        expect(MLDSASignature.fromCbor(signature.taggedCbor()).equals(signature)).toBe(true);
    });
});
