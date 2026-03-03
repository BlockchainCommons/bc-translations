import { hkdfHmacSha256 } from '@bc/crypto';
import type { RandomNumberGenerator } from '@bc/rand';

export class HKDFRng implements RandomNumberGenerator {
    #buffer = new Uint8Array(0);
    #position = 0;
    readonly #keyMaterial: Uint8Array;
    readonly #salt: string;
    readonly #pageLength: number;
    #pageIndex = 0;

    constructor(keyMaterial: Uint8Array, salt: string, pageLength = 32) {
        this.#keyMaterial = new Uint8Array(keyMaterial);
        this.#salt = salt;
        this.#pageLength = pageLength;
    }

    static newWithPageLength(
        keyMaterial: Uint8Array,
        salt: string,
        pageLength: number,
    ): HKDFRng {
        return new HKDFRng(keyMaterial, salt, pageLength);
    }

    static new(keyMaterial: Uint8Array, salt: string): HKDFRng {
        return new HKDFRng(keyMaterial, salt, 32);
    }

    #fillBuffer(): void {
        const saltString = `${this.#salt}-${this.#pageIndex}`;
        this.#buffer = new Uint8Array(
            hkdfHmacSha256(this.#keyMaterial, saltString, this.#pageLength),
        );
        this.#position = 0;
        this.#pageIndex += 1;
    }

    #nextBytes(length: number): Uint8Array {
        const result = new Uint8Array(length);
        let offset = 0;
        while (offset < length) {
            if (this.#position >= this.#buffer.length) {
                this.#fillBuffer();
            }
            const available = this.#buffer.length - this.#position;
            const take = Math.min(length - offset, available);
            result.set(this.#buffer.slice(this.#position, this.#position + take), offset);
            this.#position += take;
            offset += take;
        }
        return result;
    }

    nextU32(): number {
        const bytes = this.#nextBytes(4);
        return new DataView(bytes.buffer, bytes.byteOffset, 4).getUint32(0, true);
    }

    nextU64(): bigint {
        const bytes = this.#nextBytes(8);
        return new DataView(bytes.buffer, bytes.byteOffset, 8).getBigUint64(0, true);
    }

    randomData(size: number): Uint8Array {
        return this.#nextBytes(size);
    }

    fillRandomData(data: Uint8Array): void {
        data.set(this.#nextBytes(data.length));
    }
}
