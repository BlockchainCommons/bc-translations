import type { ECUncompressedPublicKey } from './ec-uncompressed-public-key.js';

export interface ECPublicKeyBase {
    uncompressedPublicKey(): ECUncompressedPublicKey;
}
