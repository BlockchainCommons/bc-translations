import { describe, expect, test } from 'vitest';
import { createFakeRandomNumberGenerator } from '@bc/rand';

import {
    ecdsaCompressPublicKey,
    ecdsaDecompressPublicKey,
    ecdsaDerivePrivateKey,
    ecdsaNewPrivateKeyUsing,
    ecdsaPublicKeyFromPrivateKey,
    schnorrPublicKeyFromPrivateKey,
} from '../src/index.js';
import { expectBytes } from './test-helpers.js';

describe('ecdsaKeys', () => {
    test('testEcdsaKeys', () => {
        const rng = createFakeRandomNumberGenerator();
        const privateKey = ecdsaNewPrivateKeyUsing(rng);
        expectBytes(
            privateKey,
            '7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed',
        );

        const publicKey = ecdsaPublicKeyFromPrivateKey(privateKey);
        expectBytes(
            publicKey,
            '0271b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b',
        );

        const decompressed = ecdsaDecompressPublicKey(publicKey);
        expectBytes(
            decompressed,
            '0471b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b72325f1f3bb69a44d3f1cb6d1fd488220dd502f49c0b1a46cb91ce3718d8334a',
        );

        const compressed = ecdsaCompressPublicKey(decompressed);
        expect(compressed).toEqual(publicKey);

        const xOnly = schnorrPublicKeyFromPrivateKey(privateKey);
        expectBytes(
            xOnly,
            '71b92b6212a79b9215f1d24efb9e6294a1bedc95b6c8cf187cb94771ca02626b',
        );

        const derived = ecdsaDerivePrivateKey('password');
        expectBytes(
            derived,
            '05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b',
        );
    });
});
