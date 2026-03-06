/**
 * ProvenanceMarkGenerator -- produces sequential provenance marks.
 */

import { type Cbor, toByteString, CborDate, expectUnsigned } from '@bc/dcbor';
import { type RandomNumberGenerator } from '@bc/rand';
import { Envelope } from '@bc/envelope';

import { ProvenanceMarkError } from './error.js';
import {
  ProvenanceMarkResolution,
  resolutionFromCbor,
  resolutionToCbor,
  linkLength,
} from './resolution.js';
import { ProvenanceSeed } from './seed.js';
import { RngState } from './rng-state.js';
import { Xoshiro256StarStar } from './xoshiro256starstar.js';
import { ProvenanceMark } from './provenance-mark.js';
import { sha256 } from './crypto-utils.js';
import { bytesToHex, bytesEqual, toBase64, fromBase64 } from './utils.js';

// ---- ProvenanceMarkGenerator ----

export class ProvenanceMarkGenerator {
  #resolution: ProvenanceMarkResolution;
  #seed: ProvenanceSeed;
  #chainId: Uint8Array;
  #nextSeq: number;
  #rngState: RngState;

  private constructor(
    resolution: ProvenanceMarkResolution,
    seed: ProvenanceSeed,
    chainId: Uint8Array,
    nextSeq: number,
    rngState: RngState,
  ) {
    this.#resolution = resolution;
    this.#seed = seed;
    this.#chainId = chainId;
    this.#nextSeq = nextSeq;
    this.#rngState = rngState;
  }

  // ---- Accessors ----

  get resolution(): ProvenanceMarkResolution {
    return this.#resolution;
  }
  get seed(): ProvenanceSeed {
    return this.#seed;
  }
  get chainId(): Uint8Array {
    return new Uint8Array(this.#chainId);
  }
  get nextSeq(): number {
    return this.#nextSeq;
  }
  get rngState(): RngState {
    return this.#rngState;
  }

  // ---- Constructors ----

  static create(
    resolution: ProvenanceMarkResolution,
    seed: ProvenanceSeed,
    chainId: Uint8Array,
    nextSeq: number,
    rngState: RngState,
  ): ProvenanceMarkGenerator {
    const ll = linkLength(resolution);
    if (chainId.length !== ll) {
      throw new ProvenanceMarkError(
        'InvalidChainIdLength',
        `invalid chain ID length: expected ${ll}, got ${chainId.length}`,
      );
    }
    return new ProvenanceMarkGenerator(resolution, seed, chainId, nextSeq, rngState);
  }

  static createWithSeed(
    resolution: ProvenanceMarkResolution,
    seed: ProvenanceSeed,
  ): ProvenanceMarkGenerator {
    const digest1 = sha256(seed.toBytes());
    const ll = linkLength(resolution);
    const chainId = digest1.slice(0, ll);
    const digest2 = sha256(digest1);
    return ProvenanceMarkGenerator.create(
      resolution,
      seed,
      chainId,
      0,
      RngState.fromBytes(digest2),
    );
  }

  static createWithPassphrase(
    resolution: ProvenanceMarkResolution,
    passphrase: string,
  ): ProvenanceMarkGenerator {
    const seed = ProvenanceSeed.createWithPassphrase(passphrase);
    return ProvenanceMarkGenerator.createWithSeed(resolution, seed);
  }

  static createUsing(
    resolution: ProvenanceMarkResolution,
    rng: RandomNumberGenerator,
  ): ProvenanceMarkGenerator {
    const seed = ProvenanceSeed.createUsing(rng);
    return ProvenanceMarkGenerator.createWithSeed(resolution, seed);
  }

  static createRandom(
    resolution: ProvenanceMarkResolution,
  ): ProvenanceMarkGenerator {
    const seed = ProvenanceSeed.create();
    return ProvenanceMarkGenerator.createWithSeed(resolution, seed);
  }

  // ---- Generation ----

  next(date: CborDate, info?: Cbor): ProvenanceMark {
    const data = this.#rngState.toBytes();
    const rng = Xoshiro256StarStar.fromData(data);

    const seq = this.#nextSeq;
    this.#nextSeq += 1;

    let key: Uint8Array;
    const ll = linkLength(this.#resolution);

    if (seq === 0) {
      key = new Uint8Array(this.#chainId);
    } else {
      key = rng.nextBytes(ll);
      this.#rngState = RngState.fromBytes(rng.toData());
    }

    const nextRng = rng.clone();
    const nextKey = nextRng.nextBytes(ll);

    return ProvenanceMark.create(
      this.#resolution,
      key,
      nextKey,
      new Uint8Array(this.#chainId),
      seq,
      date,
      info,
    );
  }

  // ---- Display ----

  toString(): string {
    return (
      `ProvenanceMarkGenerator(chainID: ${bytesToHex(this.#chainId)}, ` +
      `res: ${this.#resolution}, ` +
      `seed: ${this.#seed.hex()}, ` +
      `nextSeq: ${this.#nextSeq}, ` +
      `rngState: ${this.#rngState.hex()})`
    );
  }

  // ---- Equality ----

  equals(other: ProvenanceMarkGenerator): boolean {
    return (
      this.#resolution === other.#resolution &&
      this.#seed.equals(other.#seed) &&
      bytesEqual(this.#chainId, other.#chainId) &&
      this.#nextSeq === other.#nextSeq &&
      this.#rngState.equals(other.#rngState)
    );
  }

  // ---- Envelope ----

  toEnvelope(): Envelope {
    return Envelope.from(toByteString(this.#chainId))
      .addType('provenance-generator')
      .addAssertion('res', resolutionToCbor(this.#resolution))
      .addAssertion('seed', this.#seed.toCbor())
      .addAssertion('next-seq', this.#nextSeq)
      .addAssertion('rng-state', this.#rngState.toCbor());
  }

  static fromEnvelope(envelope: Envelope): ProvenanceMarkGenerator {
    envelope.checkType('provenance-generator');
    const chainId = envelope.subject().tryByteString();
    const expectedKeyCount = 5;
    const assertionCount = envelope.assertions().length;
    if (assertionCount !== expectedKeyCount) {
      throw new ProvenanceMarkError(
        'ExtraKeys',
        `wrong number of keys: expected ${expectedKeyCount}, got ${assertionCount}`,
      );
    }
    const resolution = resolutionFromCbor(
      envelope.objectForPredicate('res').tryLeaf(),
    );
    const seed = ProvenanceSeed.fromCbor(
      envelope.objectForPredicate('seed').tryLeaf(),
    );
    const nextSeq = Number(
      expectUnsigned(envelope.objectForPredicate('next-seq').tryLeaf()),
    );
    const rngState = RngState.fromCbor(
      envelope.objectForPredicate('rng-state').tryLeaf(),
    );

    return ProvenanceMarkGenerator.create(
      resolution,
      seed,
      new Uint8Array(chainId),
      nextSeq,
      rngState,
    );
  }

  // ---- JSON ----

  toJSON(): Record<string, unknown> {
    return {
      res: this.#resolution as number,
      seed: this.#seed.toJSON(),
      chainID: toBase64(this.#chainId),
      nextSeq: this.#nextSeq,
      rngState: this.#rngState.toJSON(),
    };
  }

  static fromJSON(json: Record<string, unknown>): ProvenanceMarkGenerator {
    const resVal = json['res'] as number;
    const seedStr = json['seed'] as string;
    const chainIdB64 = json['chainID'] as string;
    const nextSeq = json['nextSeq'] as number;
    const rngStateStr = json['rngState'] as string;

    const resolution = resVal as ProvenanceMarkResolution;
    const seed = ProvenanceSeed.fromJSON(seedStr);
    const chainId = fromBase64(chainIdB64);
    const rngState = RngState.fromJSON(rngStateStr);

    return ProvenanceMarkGenerator.create(resolution, seed, chainId, nextSeq, rngState);
  }
}
