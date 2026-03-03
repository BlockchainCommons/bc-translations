import { deflateRawSync, inflateRawSync } from 'node:zlib';

import { crc32 } from '@bc/crypto';
import { type Cbor, cbor as toCbor, createTag, toByteString } from '@bc/dcbor';
import { TAG_COMPRESSED } from '@bc/tags';
import type { Tag } from '@bc/dcbor';

import { decodeTaggedCbor, defaultTaggedCbor, defaultTaggedCborData } from './cbor-ur.js';
import { Digest } from './digest.js';
import type { DigestProvider } from './digest-provider.js';
import { BCComponentsError } from './error.js';
import { nanToString } from './utils.js';

const COMPRESSED_TAG: Tag = createTag(TAG_COMPRESSED, 'compressed');

export class Compressed implements DigestProvider {
    readonly #checksum: number;
    readonly #decompressedSize: number;
    readonly #compressedData: Uint8Array;
    readonly #digest?: Digest;

    constructor(
        checksum: number,
        decompressedSize: number,
        compressedData: Uint8Array,
        digest?: Digest,
    ) {
        if (compressedData.length > decompressedSize) {
            throw BCComponentsError.compression(
                'compressed data is larger than decompressed size',
            );
        }
        this.#checksum = checksum >>> 0;
        this.#decompressedSize = decompressedSize;
        this.#compressedData = new Uint8Array(compressedData);
        this.#digest = digest;
    }

    static fromDecompressedData(
        decompressedData: Uint8Array,
        digest?: Digest,
    ): Compressed {
        const compressed = deflateRawSync(decompressedData, { level: 6 });
        const checksum = crc32(decompressedData);
        const decompressedSize = decompressedData.length;
        if (compressed.length !== 0 && compressed.length < decompressedSize) {
            return new Compressed(
                checksum,
                decompressedSize,
                new Uint8Array(compressed),
                digest,
            );
        }
        return new Compressed(
            checksum,
            decompressedSize,
            new Uint8Array(decompressedData),
            digest,
        );
    }

    decompress(): Uint8Array {
        if (this.#compressedData.length >= this.#decompressedSize) {
            return new Uint8Array(this.#compressedData);
        }
        try {
            const decompressed = new Uint8Array(inflateRawSync(this.#compressedData));
            if ((crc32(decompressed) >>> 0) !== this.#checksum) {
                throw BCComponentsError.compression(
                    'compressed data checksum mismatch',
                );
            }
            return decompressed;
        } catch (error) {
            if (error instanceof BCComponentsError) {
                throw error;
            }
            throw BCComponentsError.compression('corrupt compressed data');
        }
    }

    compressedSize(): number {
        return this.#compressedData.length;
    }

    compressionRatio(): number {
        return this.compressedSize() / this.#decompressedSize;
    }

    digestOpt(): Digest | undefined {
        return this.#digest;
    }

    hasDigest(): boolean {
        return this.#digest !== undefined;
    }

    digest(): Digest {
        if (this.#digest === undefined) {
            throw BCComponentsError.invalidData('Compressed', 'missing digest');
        }
        return this.#digest;
    }

    cborTags(): Tag[] {
        return [COMPRESSED_TAG];
    }

    untaggedCbor(): Cbor {
        const elements: Cbor[] = [
            toCbor(this.#checksum),
            toCbor(this.#decompressedSize),
            toByteString(this.#compressedData),
        ];
        if (this.#digest !== undefined) {
            elements.push(this.#digest.taggedCbor());
        }
        return toCbor(elements);
    }

    taggedCbor(): Cbor {
        return defaultTaggedCbor(this);
    }

    taggedCborData(): Uint8Array {
        return defaultTaggedCborData(this);
    }

    static cborTags(): Tag[] {
        return [COMPRESSED_TAG];
    }

    static fromUntaggedCbor(cbor: Cbor): Compressed {
        const elements = cbor.toArray();
        if (elements.length < 3 || elements.length > 4) {
            throw BCComponentsError.invalidData(
                'Compressed',
                'invalid number of elements in compressed',
            );
        }
        const checksum = Number(elements[0]!.toInteger());
        const decompressedSize = Number(elements[1]!.toInteger());
        const compressedData = elements[2]!.toByteString();
        const digest = elements.length === 4
            ? Digest.fromCbor(elements[3]!)
            : undefined;
        return new Compressed(checksum, decompressedSize, compressedData, digest);
    }

    static fromCbor(cbor: Cbor): Compressed {
        return decodeTaggedCbor(Compressed, cbor);
    }

    toString(): string {
        const digestText = this.#digest?.shortDescription ?? 'None';
        return `Compressed(checksum: ${this.#checksum
            .toString(16)
            .padStart(8, '0')}, size: ${this.compressedSize()}/${this.#decompressedSize}, ratio: ${nanToString(
            this.compressionRatio(),
        )}, digest: ${digestText})`;
    }
}
