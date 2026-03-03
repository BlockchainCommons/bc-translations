import type { Digest } from './digest.js';

export interface DigestProvider {
    digest(): Digest;
}
