import { describe, expect, test } from 'vitest';
import { createFakeRandomNumberGenerator } from '@bc/rand';
import { diagnosticOpt } from '@bc/dcbor';

import {
    ECPrivateKey,
    Signature,
    SigningOptions,
    SigningPrivateKey,
    registerTags,
} from '../src/index.js';
import { hexToBytes, utf8 } from './test-helpers.js';

const MESSAGE = utf8('Wolf McNally');

const ECDSA_SIGNING_PRIVATE_KEY = SigningPrivateKey.newEcdsa(
    ECPrivateKey.fromData(
        hexToBytes('322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36'),
    ),
);
const SCHNORR_SIGNING_PRIVATE_KEY = SigningPrivateKey.newSchnorr(
    ECPrivateKey.fromData(
        hexToBytes('322b5c1dd5a17c3481c2297990c85c232ed3c17b52ce9905c6ec5193ad132c36'),
    ),
);

describe('signing', () => {
    test('schnorr sign/verify and deterministic diagnostic', () => {
        registerTags();
        const publicKey = SCHNORR_SIGNING_PRIVATE_KEY.publicKey();

        const options: SigningOptions = {
            kind: 'schnorr',
            rng: createFakeRandomNumberGenerator(),
        };
        const signature = SCHNORR_SIGNING_PRIVATE_KEY.signWithOptions(
            MESSAGE,
            options,
        );

        expect(publicKey.verify(signature, MESSAGE)).toBe(true);
        expect(publicKey.verify(signature, utf8('Wolf Mcnally'))).toBe(false);

        const expected = `40020(h'9d113392074dd52dfb7f309afb3698a1993cd14d32bc27c00070407092c9ec8c096643b5b1b535bb5277c44f256441ac660cd600739aa910b150d4f94757cf95')`;
        expect(diagnosticOpt(signature.taggedCbor())).toBe(expected);

        const decoded = Signature.fromCbor(signature.taggedCbor());
        expect(decoded.equals(signature)).toBe(true);
    });

    test('ecdsa sign/verify and deterministic diagnostic', () => {
        registerTags();
        const publicKey = ECDSA_SIGNING_PRIVATE_KEY.publicKey();
        const signature = ECDSA_SIGNING_PRIVATE_KEY.sign(MESSAGE);

        expect(publicKey.verify(signature, MESSAGE)).toBe(true);
        expect(publicKey.verify(signature, utf8('Wolf Mcnally'))).toBe(false);

        const expected = `
40020([
    1,
    h'1458d0f3d97e25109b38fd965782b43213134d02b01388a14e74ebf21e5dea4866f25a23866de9ecf0f9b72404d8192ed71fba4dc355cd89b47213e855cf6d23'
])
`.trim();
        expect(diagnosticOpt(signature.taggedCbor())).toBe(expected);

        const decoded = Signature.fromCbor(signature.taggedCbor());
        expect(decoded.equals(signature)).toBe(true);
    });
});
