import { describe, expect, test } from 'vitest';
import { createFakeRandomNumberGenerator } from '@bc/rand';

import {
    ecdsaNewPrivateKeyUsing,
    ecdsaPublicKeyFromPrivateKey,
    ecdsaSign,
    ecdsaVerify,
} from '../src/index.js';
import { expectBytes } from './test-helpers.js';

const MESSAGE =
    "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.";

describe('ecdsaSigning', () => {
    test('ECDSA sign and verify', () => {
        const rng = createFakeRandomNumberGenerator();
        const privateKey = ecdsaNewPrivateKeyUsing(rng);
        const publicKey = ecdsaPublicKeyFromPrivateKey(privateKey);
        const signature = ecdsaSign(privateKey, MESSAGE);

        expectBytes(
            signature,
            'e75702ed8f645ce7fe510507b2403029e461ef4570d12aa440e4f81385546a13740b7d16878ff0b46b1cbe08bc218ccb0b00937b61c4707de2ca6148508e51fb',
        );
        expect(ecdsaVerify(publicKey, signature, MESSAGE)).toBe(true);
    });
});
