/**
 * Xoshiro256** PRNG with deterministic cross-platform byte generation.
 */

const MASK64 = 0xFFFFFFFFFFFFFFFFn;

function rotateLeft64(value: bigint, n: number): bigint {
  const v = value & MASK64;
  return ((v << BigInt(n)) | (v >> BigInt(64 - n))) & MASK64;
}

function readU64LE(buf: Uint8Array, offset: number): bigint {
  let value = 0n;
  for (let i = 0; i < 8; i++) {
    value |= BigInt(buf[offset + i]!) << BigInt(i * 8);
  }
  return value;
}

function writeU64LE(buf: Uint8Array, offset: number, value: bigint): void {
  for (let i = 0; i < 8; i++) {
    buf[offset + i] = Number((value >> BigInt(i * 8)) & 0xFFn);
  }
}

export class Xoshiro256StarStar {
  private s: BigUint64Array;

  private constructor(state: BigUint64Array) {
    this.s = state;
  }

  static fromState(state: readonly [bigint, bigint, bigint, bigint]): Xoshiro256StarStar {
    const s = new BigUint64Array(4);
    s[0] = state[0] & MASK64;
    s[1] = state[1] & MASK64;
    s[2] = state[2] & MASK64;
    s[3] = state[3] & MASK64;
    return new Xoshiro256StarStar(s);
  }

  toState(): [bigint, bigint, bigint, bigint] {
    return [this.s[0]!, this.s[1]!, this.s[2]!, this.s[3]!];
  }

  static fromData(data: Uint8Array): Xoshiro256StarStar {
    if (data.length !== 32) {
      throw new RangeError('Xoshiro256StarStar data must be exactly 32 bytes');
    }
    const s = new BigUint64Array(4);
    for (let i = 0; i < 4; i++) {
      s[i] = readU64LE(data, i * 8);
    }
    return new Xoshiro256StarStar(s);
  }

  toData(): Uint8Array {
    const data = new Uint8Array(32);
    for (let i = 0; i < 4; i++) {
      writeU64LE(data, i * 8, this.s[i]!);
    }
    return data;
  }

  nextU64(): bigint {
    const resultStarstar =
      ((rotateLeft64((this.s[1]! * 5n) & MASK64, 7)) * 9n) & MASK64;

    const t = (this.s[1]! << 17n) & MASK64;

    this.s[2] = (this.s[2]! ^ this.s[0]!) & MASK64;
    this.s[3] = (this.s[3]! ^ this.s[1]!) & MASK64;
    this.s[1] = (this.s[1]! ^ this.s[2]!) & MASK64;
    this.s[0] = (this.s[0]! ^ this.s[3]!) & MASK64;

    this.s[2] = (this.s[2]! ^ t) & MASK64;

    this.s[3] = rotateLeft64(this.s[3]!, 45);

    return resultStarstar;
  }

  nextByte(): number {
    return Number(this.nextU64() & 0xFFn);
  }

  nextBytes(len: number): Uint8Array {
    const bytes = new Uint8Array(len);
    for (let i = 0; i < len; i++) {
      bytes[i] = this.nextByte();
    }
    return bytes;
  }

  clone(): Xoshiro256StarStar {
    const s = new BigUint64Array(4);
    s.set(this.s);
    return new Xoshiro256StarStar(s);
  }
}
