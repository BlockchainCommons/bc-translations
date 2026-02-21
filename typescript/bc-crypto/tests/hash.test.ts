import { describe, expect, test } from 'vitest';

import {
    crc32,
    crc32Bytes,
    crc32Data,
    hkdfHmacSha256,
    hmacSha256,
    hmacSha512,
    pbkdf2HmacSha256,
    sha256,
    sha512,
} from '../src/index.js';
import { expectBytes, hexToBytes } from './test-helpers.js';

describe('hash', () => {
    test('crc32 checksum', () => {
        const input = new TextEncoder().encode('Hello, world!');
        expect(crc32(input)).toBe(0xebe6c6e6);
        expectBytes(crc32Data(input), 'ebe6c6e6');
        expectBytes(crc32Bytes(input, true), 'e6c6e6eb');
    });

    test('sha256 digest', () => {
        const input = 'abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq';
        expectBytes(
            sha256(input),
            '248d6a61d20638b8e5c026930c3e6039a33ce45964ff2167f6ecedd419db06c1',
        );
    });

    test('sha512 digest', () => {
        const input = 'abcdbcdecdefdefgefghfghighijhijkijkljklmklmnlmnomnopnopq';
        expectBytes(
            sha512(input),
            '204a8fc6dda82f0a0ced7beb8e08a41657c16ef468b228a8279be331a703c33596fd15c13b1b07f9aa1d3bea57789ca031ad85c7a71dd70354ec631238ca3445',
        );
    });

    test('hmac-sha256 and hmac-sha512', () => {
        const key = hexToBytes('0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b');
        const message = 'Hi There';

        expectBytes(
            hmacSha256(key, message),
            'b0344c61d8db38535ca8afceaf0bf12b881dc200c9833da726e9376c2e32cff7',
        );
        expectBytes(
            hmacSha512(key, message),
            '87aa7cdea5ef619d4ff0b4241a1d6cb02379f4e2ce4ec2787ad0b30545e17cdedaa833b7d6b8a702038b274eaea3f4e4be9d914eeb61f1702e696c203a126854',
        );
    });

    test('pbkdf2-hmac-sha256', () => {
        expectBytes(
            pbkdf2HmacSha256('password', 'salt', 1, 32),
            '120fb6cffcf8b32c43e7225256c4f837a86548c92ccc35480805987cb70be17b',
        );
    });

    test('hkdf-hmac-sha256', () => {
        const keyMaterial = new TextEncoder().encode('hello');
        const salt = hexToBytes('8e94ef805b93e683ff18');
        expectBytes(
            hkdfHmacSha256(keyMaterial, salt, 32),
            '13485067e21af17c0900f70d885f02593c0e61e46f86450e4a0201a54c14db76',
        );
    });
});
