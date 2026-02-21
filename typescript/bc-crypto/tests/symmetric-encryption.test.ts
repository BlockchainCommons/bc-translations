import { describe, expect, test } from 'vitest';
import { secureRandomData } from '@bc/rand';

import {
    aeadChaCha20Poly1305DecryptWithAad,
    aeadChaCha20Poly1305EncryptWithAad,
} from '../src/index.js';
import { expectBytes, hexToBytes } from './test-helpers.js';

const PLAINTEXT = new TextEncoder().encode(
    "Ladies and Gentlemen of the class of '99: If I could offer you only one tip for the future, sunscreen would be it.",
);
const AAD = hexToBytes('50515253c0c1c2c3c4c5c6c7');
const KEY = hexToBytes(
    '808182838485868788898a8b8c8d8e8f909192939495969798999a9b9c9d9e9f',
);
const NONCE = hexToBytes('070000004041424344454647');
const CIPHERTEXT =
    'd31a8d34648e60db7b86afbc53ef7ec2a4aded51296e08fea9e2b5a736ee62d63dbea45e8ca9671282fafb69da92728b1a71de0a9e060b2905d6a5b67ecd3b3692ddbd7f2d778b8c9803aee328091b58fab324e4fad675945585808b4831d7bc3ff4def08e4b7a9de576d26586cec64b6116';
const AUTH = '1ae10b594f09e26a7e902ecbd0600691';

function encrypted(): [Uint8Array, Uint8Array] {
    return aeadChaCha20Poly1305EncryptWithAad(PLAINTEXT, KEY, NONCE, AAD);
}

describe('symmetricEncryption', () => {
    test('RFC test vector', () => {
        const [ciphertext, auth] = encrypted();
        expectBytes(ciphertext, CIPHERTEXT);
        expectBytes(auth, AUTH);

        const decrypted = aeadChaCha20Poly1305DecryptWithAad(
            ciphertext,
            KEY,
            NONCE,
            AAD,
            auth,
        );
        expect(decrypted).toEqual(PLAINTEXT);
    });

    test('random key and nonce', () => {
        const key = secureRandomData(32);
        const nonce = secureRandomData(12);
        const [ciphertext, auth] = aeadChaCha20Poly1305EncryptWithAad(
            PLAINTEXT,
            key,
            nonce,
            AAD,
        );
        const decrypted = aeadChaCha20Poly1305DecryptWithAad(
            ciphertext,
            key,
            nonce,
            AAD,
            auth,
        );
        expect(decrypted).toEqual(PLAINTEXT);
    });

    test('empty data', () => {
        const key = secureRandomData(32);
        const nonce = secureRandomData(12);
        const [ciphertext, auth] = aeadChaCha20Poly1305EncryptWithAad(
            new Uint8Array(0),
            key,
            nonce,
            new Uint8Array(0),
        );
        const decrypted = aeadChaCha20Poly1305DecryptWithAad(
            ciphertext,
            key,
            nonce,
            new Uint8Array(0),
            auth,
        );
        expectBytes(decrypted, '');
    });
});
