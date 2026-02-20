import { describe, expect, test } from 'vitest';
import { createFakeRandomNumberGenerator } from '@bc/rand';

import {
    deriveAgreementPrivateKey,
    deriveSigningPrivateKey,
    x25519NewPrivateKeyUsing,
    x25519PublicKeyFromPrivateKey,
    x25519SharedKey,
} from '../src/index.js';
import { expectBytes } from './test-helpers.js';

describe('publicKeyEncryption', () => {
    test('testX25519Keys', () => {
        const rng = createFakeRandomNumberGenerator();
        const privateKey = x25519NewPrivateKeyUsing(rng);
        expectBytes(
            privateKey,
            '7eb559bbbf6cce2632cf9f194aeb50943de7e1cbad54dcfab27a42759f5e2fed',
        );

        const publicKey = x25519PublicKeyFromPrivateKey(privateKey);
        expectBytes(
            publicKey,
            'f1bd7a7e118ea461eba95126a3efef543ebb78439d1574bedcbe7d89174cf025',
        );

        const derivedAgreement = deriveAgreementPrivateKey('password');
        expectBytes(
            derivedAgreement,
            '7b19769132648ff43ae60cbaa696d5be3f6d53e6645db72e2d37516f0729619f',
        );

        const derivedSigning = deriveSigningPrivateKey('password');
        expectBytes(
            derivedSigning,
            '05cc550daa75058e613e606d9898fedf029e395911c43273a208b7e0e88e271b',
        );
    });

    test('testKeyAgreement', () => {
        const rng = createFakeRandomNumberGenerator();
        const alicePrivateKey = x25519NewPrivateKeyUsing(rng);
        const alicePublicKey = x25519PublicKeyFromPrivateKey(alicePrivateKey);
        const bobPrivateKey = x25519NewPrivateKeyUsing(rng);
        const bobPublicKey = x25519PublicKeyFromPrivateKey(bobPrivateKey);

        const aliceShared = x25519SharedKey(alicePrivateKey, bobPublicKey);
        const bobShared = x25519SharedKey(bobPrivateKey, alicePublicKey);

        expect(aliceShared).toEqual(bobShared);
        expectBytes(
            aliceShared,
            '1e9040d1ff45df4bfca7ef2b4dd2b11101b40d91bf5bf83f8c83d53f0fbb6c23',
        );
    });
});
