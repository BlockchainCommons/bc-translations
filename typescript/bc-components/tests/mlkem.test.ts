import { describe, expect, test } from 'vitest';

import {
    MLKEM,
    MLKEMCiphertext,
    MLKEMPrivateKey,
    MLKEMPublicKey,
} from '../src/index.js';

describe('MLKEM', () => {
    test('encapsulation/decapsulation by level', () => {
        for (const level of [MLKEM.mlkem512, MLKEM.mlkem768, MLKEM.mlkem1024]) {
            const [privateKey, publicKey] = level.keypair();
            const [shared, ciphertext] = publicKey.encapsulateNewSharedSecret();
            const recovered = privateKey.decapsulateSharedSecret(ciphertext);
            expect(shared.equals(recovered)).toBe(true);
        }
    });

    test('cbor roundtrip', () => {
        const [privateKey, publicKey] = MLKEM.mlkem768.keypair();
        const [, ciphertext] = publicKey.encapsulateNewSharedSecret();

        expect(MLKEMPrivateKey.fromCbor(privateKey.taggedCbor()).equals(privateKey)).toBe(true);
        expect(MLKEMPublicKey.fromCbor(publicKey.taggedCbor()).equals(publicKey)).toBe(true);
        expect(MLKEMCiphertext.fromCbor(ciphertext.taggedCbor()).equals(ciphertext)).toBe(true);
    });
});
