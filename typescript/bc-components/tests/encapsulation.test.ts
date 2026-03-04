import { describe, expect, test } from 'vitest';
import { bytesToHex } from '@bc/dcbor';

import {
    EncapsulationScheme,
    SealedMessage,
} from '../src/index.js';
import { utf8, expectBytes } from './test-helpers.js';

describe('Encapsulation and SealedMessage', () => {
    test('x25519 sealed message decrypts only for recipient', () => {
        const plaintext = utf8("Some mysteries aren't meant to be solved.");

        const [alicePrivate] = EncapsulationScheme.x25519.keypair();
        const [bobPrivate, bobPublic] = EncapsulationScheme.x25519.keypair();
        const [carolPrivate] = EncapsulationScheme.x25519.keypair();

        const sealed = SealedMessage.new(plaintext, bobPublic);

        expectBytes(sealed.decrypt(bobPrivate), bytesToHex(plaintext));
        expect(() => sealed.decrypt(alicePrivate)).toThrow();
        expect(() => sealed.decrypt(carolPrivate)).toThrow();
    });

    test('mlkem sealed message decrypts only for recipient', () => {
        const plaintext = utf8("Some mysteries aren't meant to be solved.");

        const [alicePrivate] = EncapsulationScheme.mlkem512.keypair();
        const [bobPrivate, bobPublic] = EncapsulationScheme.mlkem512.keypair();
        const [carolPrivate] = EncapsulationScheme.mlkem512.keypair();

        const sealed = SealedMessage.new(plaintext, bobPublic);

        expectBytes(sealed.decrypt(bobPrivate), bytesToHex(plaintext));
        expect(() => sealed.decrypt(alicePrivate)).toThrow();
        expect(() => sealed.decrypt(carolPrivate)).toThrow();
    });
});
