import * as crypto from 'node:crypto';

import { type BytesLike, toBytes } from './bytes.js';
import { BCryptoError } from './error.js';

const ARGON2_ID_TIME = 2;
const ARGON2_ID_MEMORY = 19456;
const ARGON2_ID_THREADS = 1;

type Argon2SyncFn = (
    algorithm: 'argon2id' | 'argon2i' | 'argon2d',
    options: {
        message: Uint8Array;
        nonce: Uint8Array;
        parallelism: number;
        memory: number;
        passes: number;
        tagLength: number;
    },
) => ArrayBuffer | Uint8Array;

/** Derives a key using Argon2id with recommended parameters. */
export function argon2id(
    pass: BytesLike,
    salt: BytesLike,
    outputLen: number,
): Uint8Array {
    if (outputLen < 0) {
        throw new RangeError('Invalid Argon2 output length');
    }

    try {
        const argon2Sync = (crypto as unknown as { argon2Sync?: Argon2SyncFn })
            .argon2Sync;
        if (!argon2Sync) {
            throw new BCryptoError('argon2 unavailable: requires Node.js 22+');
        }
        return new Uint8Array(
            argon2Sync('argon2id', {
                message: toBytes(pass),
                nonce: toBytes(salt),
                parallelism: ARGON2_ID_THREADS,
                memory: ARGON2_ID_MEMORY,
                passes: ARGON2_ID_TIME,
                tagLength: outputLen,
            }),
        );
    } catch (e) {
        if (e instanceof BCryptoError || e instanceof RangeError) {
            throw e;
        }
        throw new BCryptoError('argon2 failed');
    }
}
