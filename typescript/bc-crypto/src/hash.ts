import {
    createHash,
    createHmac,
    hkdfSync,
    pbkdf2Sync,
} from 'node:crypto';

import { type BytesLike, toBytes } from './bytes.js';

export const CRC32_SIZE = 4;
export const SHA256_SIZE = 32;
export const SHA512_SIZE = 64;

const CRC32_TABLE = new Uint32Array(256);
for (let i = 0; i < 256; i++) {
    let c = i;
    for (let j = 0; j < 8; j++) {
        c = (c & 1) !== 0 ? (0xedb88320 ^ (c >>> 1)) : (c >>> 1);
    }
    CRC32_TABLE[i] = c >>> 0;
}

/** Computes the CRC-32 checksum of the given data. */
export function crc32(data: BytesLike): number {
    const bytes = toBytes(data);
    let c = 0xffffffff;
    for (const byte of bytes) {
        c = CRC32_TABLE[(c ^ byte) & 0xff] ^ (c >>> 8);
    }
    return (c ^ 0xffffffff) >>> 0;
}

/** Computes the CRC-32 checksum bytes in big-endian or little-endian. */
export function crc32DataOpt(
    data: BytesLike,
    littleEndian: boolean,
): Uint8Array {
    const checksum = crc32(data);
    const out = new Uint8Array(CRC32_SIZE);
    const view = new DataView(out.buffer, out.byteOffset, out.byteLength);
    view.setUint32(0, checksum, littleEndian);
    return out;
}

/** Computes the CRC-32 checksum bytes in big-endian format. */
export function crc32Data(data: BytesLike): Uint8Array {
    return crc32DataOpt(data, false);
}

/** Computes the SHA-256 digest of the input buffer. */
export function sha256(data: BytesLike): Uint8Array {
    return new Uint8Array(createHash('sha256').update(toBytes(data)).digest());
}

/** Computes the double SHA-256 digest of the input buffer. */
export function doubleSha256(message: BytesLike): Uint8Array {
    return sha256(sha256(message));
}

/** Computes the SHA-512 digest of the input buffer. */
export function sha512(data: BytesLike): Uint8Array {
    return new Uint8Array(createHash('sha512').update(toBytes(data)).digest());
}

/** Computes the HMAC-SHA-256 for the given key and message. */
export function hmacSha256(key: BytesLike, message: BytesLike): Uint8Array {
    return new Uint8Array(
        createHmac('sha256', toBytes(key)).update(toBytes(message)).digest(),
    );
}

/** Computes the HMAC-SHA-512 for the given key and message. */
export function hmacSha512(key: BytesLike, message: BytesLike): Uint8Array {
    return new Uint8Array(
        createHmac('sha512', toBytes(key)).update(toBytes(message)).digest(),
    );
}

/** Computes the PBKDF2-HMAC-SHA-256 for the given password. */
export function pbkdf2HmacSha256(
    pass: BytesLike,
    salt: BytesLike,
    iterations: number,
    keyLen: number,
): Uint8Array {
    return new Uint8Array(
        pbkdf2Sync(toBytes(pass), toBytes(salt), iterations, keyLen, 'sha256'),
    );
}

/** Computes the PBKDF2-HMAC-SHA-512 for the given password. */
export function pbkdf2HmacSha512(
    pass: BytesLike,
    salt: BytesLike,
    iterations: number,
    keyLen: number,
): Uint8Array {
    return new Uint8Array(
        pbkdf2Sync(toBytes(pass), toBytes(salt), iterations, keyLen, 'sha512'),
    );
}

/** Computes the HKDF-HMAC-SHA-256 for the given key material. */
export function hkdfHmacSha256(
    keyMaterial: BytesLike,
    salt: BytesLike,
    keyLen: number,
): Uint8Array {
    return new Uint8Array(
        hkdfSync(
            'sha256',
            toBytes(keyMaterial),
            toBytes(salt),
            new Uint8Array(0),
            keyLen,
        ),
    );
}

/** Computes the HKDF-HMAC-SHA-512 for the given key material. */
export function hkdfHmacSha512(
    keyMaterial: BytesLike,
    salt: BytesLike,
    keyLen: number,
): Uint8Array {
    return new Uint8Array(
        hkdfSync(
            'sha512',
            toBytes(keyMaterial),
            toBytes(salt),
            new Uint8Array(0),
            keyLen,
        ),
    );
}
