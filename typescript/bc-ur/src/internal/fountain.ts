import { createHash } from "node:crypto";

import { cborData, decodeCbor, type Cbor } from "@bc/dcbor";

const MASK_64 = 0xffff_ffff_ffff_ffffn;

const toUint64 = (value: bigint): bigint => {
  return value & MASK_64;
};

const rotl = (value: bigint, shift: number): bigint => {
  return toUint64(
    (value << BigInt(shift)) | (value >> BigInt(64 - shift)),
  );
};

class WeightedSampler {
  readonly #aliases: number[];
  readonly #probs: number[];

  constructor(weights: number[]) {
    if (weights.some((weight) => weight < 0)) {
      throw new Error("negative probability encountered");
    }

    const sum = weights.reduce((accumulator, weight) => accumulator + weight, 0);
    if (sum <= 0) {
      throw new Error("probabilities don't sum to a positive value");
    }

    const count = weights.length;
    const normalized = weights.map((weight) => (weight * count) / sum);

    const s: number[] = [];
    const l: number[] = [];
    for (let j = 1; j <= count; j++) {
      const index = count - j;
      if ((normalized[index] ?? 0) < 1.0) {
        s.push(index);
      } else {
        l.push(index);
      }
    }

    const probs = new Array<number>(count).fill(0);
    const aliases = new Array<number>(count).fill(0);

    while (s.length > 0 && l.length > 0) {
      const a = s.pop();
      const g = l.pop();
      if (a === undefined || g === undefined) {
        throw new Error("expected item");
      }

      probs[a] = normalized[a] ?? 0;
      aliases[a] = g;
      normalized[g] = (normalized[g] ?? 0) + (normalized[a] ?? 0) - 1.0;

      if ((normalized[g] ?? 0) < 1.0) {
        s.push(g);
      } else {
        l.push(g);
      }
    }

    while (l.length > 0) {
      const g = l.pop();
      if (g === undefined) {
        throw new Error("expected item");
      }
      probs[g] = 1.0;
    }

    while (s.length > 0) {
      const a = s.pop();
      if (a === undefined) {
        throw new Error("expected item");
      }
      probs[a] = 1.0;
    }

    this.#aliases = aliases;
    this.#probs = probs;
  }

  next(xoshiro: Xoshiro256): number {
    const r1 = xoshiro.nextDouble();
    const r2 = xoshiro.nextDouble();

    const n = this.#probs.length;
    const index = Math.floor(n * r1);
    if (r2 < (this.#probs[index] ?? 0)) {
      return index;
    }
    return this.#aliases[index] ?? 0;
  }
}

class Xoshiro256 {
  readonly #state: [bigint, bigint, bigint, bigint];

  constructor(seed: Uint8Array) {
    const digest = createHash("sha256").update(seed).digest();
    this.#state = [
      digest.readBigUInt64BE(0),
      digest.readBigUInt64BE(8),
      digest.readBigUInt64BE(16),
      digest.readBigUInt64BE(24),
    ];
  }

  next(): bigint {
    const result = toUint64(rotl(toUint64(this.#state[1] * 5n), 7) * 9n);

    const t = toUint64(this.#state[1] << 17n);

    this.#state[2] = toUint64(this.#state[2] ^ this.#state[0]);
    this.#state[3] = toUint64(this.#state[3] ^ this.#state[1]);
    this.#state[1] = toUint64(this.#state[1] ^ this.#state[2]);
    this.#state[0] = toUint64(this.#state[0] ^ this.#state[3]);

    this.#state[2] = toUint64(this.#state[2] ^ t);
    this.#state[3] = toUint64(rotl(this.#state[3], 45));

    return result;
  }

  nextDouble(): number {
    return Number(this.next()) / (Number(MASK_64) + 1.0);
  }

  nextInt(low: number, high: number): number {
    return Math.floor(this.nextDouble() * (high - low + 1)) + low;
  }

  shuffled<T>(items: T[]): T[] {
    const remaining = [...items];
    const shuffled: T[] = [];

    while (remaining.length > 0) {
      const index = this.nextInt(0, remaining.length - 1);
      const item = remaining.splice(index, 1)[0];
      if (item === undefined) {
        throw new Error("expected item");
      }
      shuffled.push(item);
    }

    return shuffled;
  }

  chooseDegree(length: number): number {
    const weights = new Array<number>(length)
      .fill(0)
      .map((_, index) => 1.0 / (index + 1));

    const sampler = new WeightedSampler(weights);
    return sampler.next(this) + 1;
  }
}

const CRC32_TABLE = (() => {
  const table = new Uint32Array(256);
  for (let index = 0; index < 256; index++) {
    let value = index;
    for (let bit = 0; bit < 8; bit++) {
      if ((value & 1) === 1) {
        value = (value >>> 1) ^ 0xedb8_8320;
      } else {
        value >>>= 1;
      }
    }
    table[index] = value >>> 0;
  }
  return table;
})();

const crc32 = (data: Uint8Array): number => {
  let crc = 0xffff_ffff;
  for (const byte of data) {
    crc = (CRC32_TABLE[(crc ^ byte) & 0xff] ?? 0) ^ (crc >>> 8);
  }
  return (crc ^ 0xffff_ffff) >>> 0;
};

const divCeil = (a: number, b: number): number => {
  const d = Math.floor(a / b);
  const r = a % b;
  return r > 0 ? d + 1 : d;
};

const fragmentLength = (dataLength: number, maxFragmentLength: number): number => {
  const fragmentCount = divCeil(dataLength, maxFragmentLength);
  return divCeil(dataLength, fragmentCount);
};

const partition = (data: Uint8Array, size: number): Uint8Array[] => {
  const paddingLength = (size - (data.length % size)) % size;
  const padded = new Uint8Array(data.length + paddingLength);
  padded.set(data);

  const fragments: Uint8Array[] = [];
  for (let index = 0; index < padded.length; index += size) {
    fragments.push(padded.slice(index, index + size));
  }
  return fragments;
};

const chooseFragments = (sequence: number, fragmentCount: number, checksum: number): number[] => {
  if (sequence <= fragmentCount) {
    return [sequence - 1];
  }

  const seed = new Uint8Array(8);
  const view = new DataView(seed.buffer);
  view.setUint32(0, sequence >>> 0, false);
  view.setUint32(4, checksum >>> 0, false);

  const xoshiro = new Xoshiro256(seed);
  const degree = xoshiro.chooseDegree(fragmentCount);
  const indexes = [...new Array<number>(fragmentCount)].map((_, index) => index);
  const shuffled = xoshiro.shuffled(indexes);
  shuffled.length = degree;
  return shuffled;
};

const xorInto = (left: Uint8Array, right: Uint8Array): void => {
  for (let index = 0; index < left.length; index++) {
    left[index] = (left[index] ?? 0) ^ (right[index] ?? 0);
  }
};

const integerFromCbor = (value: Cbor): number => {
  const integer = value.toInteger();
  const number = typeof integer === "bigint" ? Number(integer) : integer;
  if (!Number.isInteger(number) || number < 0) {
    throw new Error("invalid integer in fountain part");
  }
  return number;
};

export class FountainPart {
  readonly sequence: number;
  readonly sequenceCount: number;
  readonly messageLength: number;
  readonly checksum: number;
  readonly data: Uint8Array;

  constructor(
    sequence: number,
    sequenceCount: number,
    messageLength: number,
    checksum: number,
    data: Uint8Array,
  ) {
    this.sequence = sequence;
    this.sequenceCount = sequenceCount;
    this.messageLength = messageLength;
    this.checksum = checksum >>> 0;
    this.data = data.slice();
  }

  static fromCbor(payload: Uint8Array): FountainPart {
    const decoded = decodeCbor(payload);
    const values = decoded.toArray();
    if (values.length !== 5) {
      throw new Error("invalid CBOR array length");
    }

    const sequence = integerFromCbor(values[0]!);
    const sequenceCount = integerFromCbor(values[1]!);
    const messageLength = integerFromCbor(values[2]!);
    const checksum = integerFromCbor(values[3]!);
    const data = values[4]!.toByteString();

    return new FountainPart(sequence, sequenceCount, messageLength, checksum, data);
  }

  indexes(): number[] {
    return chooseFragments(this.sequence, this.sequenceCount, this.checksum);
  }

  isSimple(): boolean {
    return this.indexes().length === 1;
  }

  cbor(): Uint8Array {
    return cborData([
      this.sequence,
      this.sequenceCount,
      this.messageLength,
      this.checksum,
      this.data,
    ]);
  }

  sequenceId(): string {
    return `${this.sequence}-${this.sequenceCount}`;
  }

  withData(data: Uint8Array): FountainPart {
    return new FountainPart(
      this.sequence,
      this.sequenceCount,
      this.messageLength,
      this.checksum,
      data,
    );
  }
}

export class FountainEncoder {
  readonly #parts: Uint8Array[];
  readonly #messageLength: number;
  readonly #checksum: number;
  #currentSequence: number;

  constructor(message: Uint8Array, maxFragmentLength: number) {
    if (message.length === 0) {
      throw new Error("expected non-empty message");
    }
    if (maxFragmentLength === 0) {
      throw new Error("expected positive maximum fragment length");
    }

    const size = fragmentLength(message.length, maxFragmentLength);
    this.#parts = partition(message, size);
    this.#messageLength = message.length;
    this.#checksum = crc32(message);
    this.#currentSequence = 0;
  }

  nextPart(): FountainPart {
    this.#currentSequence += 1;
    const indexes = chooseFragments(
      this.#currentSequence,
      this.#parts.length,
      this.#checksum,
    );

    const mixed = new Uint8Array(this.#parts[0]?.length ?? 0);
    for (const index of indexes) {
      const part = this.#parts[index];
      if (part === undefined) {
        throw new Error("expected item");
      }
      xorInto(mixed, part);
    }

    return new FountainPart(
      this.#currentSequence,
      this.#parts.length,
      this.#messageLength,
      this.#checksum,
      mixed,
    );
  }

  currentSequence(): number {
    return this.#currentSequence;
  }

  fragmentCount(): number {
    return this.#parts.length;
  }

  complete(): boolean {
    return this.#currentSequence >= this.#parts.length;
  }
}

type BufferedPart = {
  indexes: number[];
  part: FountainPart;
};

export class FountainDecoder {
  readonly #decoded = new Map<number, FountainPart>();
  readonly #received = new Set<string>();
  readonly #buffer = new Map<string, BufferedPart>();
  readonly #queue: Array<[number, FountainPart]> = [];

  #sequenceCount = 0;
  #messageLength = 0;
  #checksum = 0;
  #fragmentLength = 0;

  #key(indexes: number[]): string {
    return indexes.join(",");
  }

  receive(part: FountainPart): boolean {
    if (this.complete()) {
      return false;
    }

    if (
      part.sequenceCount === 0 ||
      part.data.length === 0 ||
      part.messageLength === 0
    ) {
      throw new Error("expected non-empty part");
    }

    if (this.#received.size === 0) {
      this.#sequenceCount = part.sequenceCount;
      this.#messageLength = part.messageLength;
      this.#checksum = part.checksum;
      this.#fragmentLength = part.data.length;
    } else if (!this.validate(part)) {
      throw new Error("part is inconsistent with previous ones");
    }

    const indexes = part.indexes();
    const key = this.#key(indexes);
    if (this.#received.has(key)) {
      return false;
    }
    this.#received.add(key);

    if (part.isSimple()) {
      this.#processSimple(part);
    } else {
      this.#processComplex(part);
    }

    return true;
  }

  #processSimple(part: FountainPart): void {
    const index = part.indexes()[0];
    if (index === undefined) {
      throw new Error("expected item");
    }

    this.#decoded.set(index, part);
    this.#queue.push([index, part]);
    this.#processQueue();
  }

  #processQueue(): void {
    while (this.#queue.length > 0) {
      const item = this.#queue.pop();
      if (item === undefined) {
        throw new Error("expected item");
      }

      const [index, simple] = item;
      const toProcess = [...this.#buffer.entries()]
        .filter(([, value]) => value.indexes.includes(index))
        .map(([key]) => key);

      for (const key of toProcess) {
        const value = this.#buffer.get(key);
        if (value === undefined) {
          throw new Error("expected item");
        }
        this.#buffer.delete(key);

        const newIndexes = [...value.indexes];
        const remove = newIndexes.indexOf(index);
        if (remove === -1) {
          throw new Error("expected item");
        }
        newIndexes.splice(remove, 1);

        const mixed = value.part.data.slice();
        xorInto(mixed, simple.data);
        const reduced = value.part.withData(mixed);

        if (newIndexes.length === 1) {
          const newIndex = newIndexes[0];
          if (newIndex === undefined) {
            throw new Error("expected item");
          }
          this.#decoded.set(newIndex, reduced);
          this.#queue.push([newIndex, reduced]);
        } else {
          this.#buffer.set(this.#key(newIndexes), {
            indexes: newIndexes,
            part: reduced,
          });
        }
      }
    }
  }

  #processComplex(part: FountainPart): void {
    const indexes = part.indexes();
    const toRemove = indexes.filter((index) => this.#decoded.has(index));

    if (indexes.length === toRemove.length) {
      return;
    }

    const mixed = part.data.slice();
    for (const remove of toRemove) {
      const removeIndex = indexes.indexOf(remove);
      if (removeIndex === -1) {
        throw new Error("expected item");
      }
      indexes.splice(removeIndex, 1);

      const decoded = this.#decoded.get(remove);
      if (decoded === undefined) {
        throw new Error("expected item");
      }
      xorInto(mixed, decoded.data);
    }

    const reduced = part.withData(mixed);
    if (indexes.length === 1) {
      const index = indexes[0];
      if (index === undefined) {
        throw new Error("expected item");
      }
      this.#decoded.set(index, reduced);
      this.#queue.push([index, reduced]);
    } else {
      this.#buffer.set(this.#key(indexes), { indexes, part: reduced });
    }
  }

  validate(part: FountainPart): boolean {
    if (this.#received.size === 0) {
      return false;
    }

    if (part.sequenceCount !== this.#sequenceCount) {
      return false;
    }
    if (part.messageLength !== this.#messageLength) {
      return false;
    }
    if (part.checksum !== this.#checksum) {
      return false;
    }
    if (part.data.length !== this.#fragmentLength) {
      return false;
    }

    return true;
  }

  complete(): boolean {
    return this.#messageLength !== 0 && this.#decoded.size === this.#sequenceCount;
  }

  message(): Uint8Array | undefined {
    if (!this.complete()) {
      return undefined;
    }

    const combined = new Uint8Array(this.#sequenceCount * this.#fragmentLength);
    for (let index = 0; index < this.#sequenceCount; index++) {
      const part = this.#decoded.get(index);
      if (part === undefined) {
        throw new Error("expected item");
      }
      combined.set(part.data, index * this.#fragmentLength);
    }

    for (let index = this.#messageLength; index < combined.length; index++) {
      if (combined[index] !== 0) {
        throw new Error("invalid padding");
      }
    }

    return combined.slice(0, this.#messageLength);
  }
}
