/**
 * ProvenanceMark -- an individual mark in a provenance chain.
 */

import {
  type Cbor,
  cbor,
  cborData,
  toByteString,
  decodeCbor,
  CborDate,
  type Tag,
  type CborTaggedEncodable,
  type CborTaggedDecodable,
  createTaggedCbor,
  validateTag,
  extractTaggedContent,
  expectArray,
  expectBytes,
  tagsForValues,
  diagnosticOpt,
} from '@bc/dcbor';
import {
  UR,
  Style,
  encode as encodeBytewords,
  decode as decodeBytewords,
  identifier as bwIdentifier,
  encodeToWords as bwEncodeToWords,
  encodeToBytemojis as bwEncodeToBytemojis,
  ur,
  urString,
  fromUr,
  fromUrString,
} from '@bc/ur';
import { TAG_PROVENANCE_MARK } from '@bc/tags';
import {
  Envelope,
  registerTags as registerEnvelopeTags,
  registerTagsIn as registerEnvelopeTagsIn,
  type FormatContext,
  getGlobalFormatContext,
} from '@bc/envelope';

import { ProvenanceMarkError } from './error.js';
import {
  ProvenanceMarkResolution,
  resolutionFromCbor,
  resolutionToCbor,
  linkLength,
  fixedLength,
  hashRangeStart,
  hashRangeEnd,
  chainIdRangeEnd,
  seqBytesRangeStart,
  seqBytesRangeEnd,
  dateBytesRangeStart,
  dateBytesRangeEnd,
  infoRangeStart,
  serializeDate,
  deserializeDate,
  serializeSeq,
  deserializeSeq,
} from './resolution.js';
import { sha256, sha256Prefix, SHA256_SIZE, obfuscate } from './crypto-utils.js';
import { bytesToHex, bytesEqual, concatBytes, toBase64, fromBase64 } from './utils.js';
import { ValidationReport } from './validate.js';

/** The 🅟 (U+1F15F) prefix used to visually tag provenance-mark identifiers. */
export const PROVENANCE_MARK_PREFIX = '\u{1F15F}';

// ---- ProvenanceMark ----

export class ProvenanceMark
  implements CborTaggedEncodable, CborTaggedDecodable<ProvenanceMark>
{
  readonly #resolution: ProvenanceMarkResolution;
  readonly #key: Uint8Array;
  readonly #hash: Uint8Array;
  readonly #chainId: Uint8Array;
  readonly #seqBytes: Uint8Array;
  readonly #dateBytes: Uint8Array;
  readonly #infoBytes: Uint8Array;
  readonly #seq: number;
  readonly #date: CborDate;

  private constructor(
    resolution: ProvenanceMarkResolution,
    key: Uint8Array,
    hash: Uint8Array,
    chainId: Uint8Array,
    seqBytes: Uint8Array,
    dateBytes: Uint8Array,
    infoBytes: Uint8Array,
    seq: number,
    date: CborDate,
  ) {
    this.#resolution = resolution;
    this.#key = key;
    this.#hash = hash;
    this.#chainId = chainId;
    this.#seqBytes = seqBytes;
    this.#dateBytes = dateBytes;
    this.#infoBytes = infoBytes;
    this.#seq = seq;
    this.#date = date;
  }

  // ---- Accessors ----

  get resolution(): ProvenanceMarkResolution {
    return this.#resolution;
  }
  get key(): Uint8Array {
    return new Uint8Array(this.#key);
  }
  get hash(): Uint8Array {
    return new Uint8Array(this.#hash);
  }
  get chainId(): Uint8Array {
    return new Uint8Array(this.#chainId);
  }
  get seqBytes(): Uint8Array {
    return new Uint8Array(this.#seqBytes);
  }
  get dateBytes(): Uint8Array {
    return new Uint8Array(this.#dateBytes);
  }
  get seq(): number {
    return this.#seq;
  }
  get date(): CborDate {
    return this.#date;
  }

  // ---- Construction ----

  static create(
    resolution: ProvenanceMarkResolution,
    key: Uint8Array,
    nextKey: Uint8Array,
    chainId: Uint8Array,
    seq: number,
    date: CborDate,
    info?: Cbor,
  ): ProvenanceMark {
    const ll = linkLength(resolution);
    if (key.length !== ll) {
      throw new ProvenanceMarkError(
        'InvalidKeyLength',
        `invalid key length: expected ${ll}, got ${key.length}`,
      );
    }
    if (nextKey.length !== ll) {
      throw new ProvenanceMarkError(
        'InvalidNextKeyLength',
        `invalid next key length: expected ${ll}, got ${nextKey.length}`,
      );
    }
    if (chainId.length !== ll) {
      throw new ProvenanceMarkError(
        'InvalidChainIdLength',
        `invalid chain ID length: expected ${ll}, got ${chainId.length}`,
      );
    }

    const dateB = serializeDate(resolution, date);
    const seqB = serializeSeq(resolution, seq);

    // Round-trip the date through serialization so it matches Rust behavior
    const roundTrippedDate = deserializeDate(resolution, dateB);

    const infoBytes = info ? cborData(info) : new Uint8Array(0);

    const hash = ProvenanceMark.makeHash(
      resolution,
      key,
      nextKey,
      chainId,
      seqB,
      dateB,
      infoBytes,
    );

    return new ProvenanceMark(
      resolution,
      key,
      hash,
      chainId,
      seqB,
      dateB,
      infoBytes,
      seq,
      roundTrippedDate,
    );
  }

  static fromMessage(
    resolution: ProvenanceMarkResolution,
    message: Uint8Array,
  ): ProvenanceMark {
    const fl = fixedLength(resolution);
    if (message.length < fl) {
      throw new ProvenanceMarkError(
        'InvalidMessageLength',
        `invalid message length: expected at least ${fl}, got ${message.length}`,
      );
    }

    const ll = linkLength(resolution);
    const key = message.slice(0, ll);
    const payload = obfuscate(key, message.slice(ll));

    const hash = payload.slice(hashRangeStart(resolution), hashRangeEnd(resolution));
    const cid = payload.slice(0, chainIdRangeEnd(resolution));
    const sBytes = payload.slice(seqBytesRangeStart(resolution), seqBytesRangeEnd(resolution));
    const seq = deserializeSeq(resolution, sBytes);
    const dBytes = payload.slice(dateBytesRangeStart(resolution), dateBytesRangeEnd(resolution));
    const date = deserializeDate(resolution, dBytes);

    const infoBytes = payload.slice(infoRangeStart(resolution));
    if (infoBytes.length > 0) {
      try {
        decodeCbor(infoBytes);
      } catch {
        throw new ProvenanceMarkError(
          'InvalidInfoCbor',
          'invalid CBOR data in info field',
        );
      }
    }

    return new ProvenanceMark(
      resolution,
      key,
      hash,
      cid,
      sBytes,
      dBytes,
      infoBytes,
      seq,
      date,
    );
  }

  private static makeHash(
    resolution: ProvenanceMarkResolution,
    key: Uint8Array,
    nextKey: Uint8Array,
    chainId: Uint8Array,
    seqBytes: Uint8Array,
    dateBytes: Uint8Array,
    infoBytes: Uint8Array,
  ): Uint8Array {
    const buf = concatBytes(key, nextKey, chainId, seqBytes, dateBytes, infoBytes);
    return sha256Prefix(buf, linkLength(resolution));
  }

  // ---- Message ----

  message(): Uint8Array {
    const payload = concatBytes(
      this.#chainId,
      this.#hash,
      this.#seqBytes,
      this.#dateBytes,
      this.#infoBytes,
    );
    return concatBytes(this.#key, obfuscate(this.#key, payload));
  }

  // ---- Info ----

  info(): Cbor | undefined {
    if (this.#infoBytes.length === 0) return undefined;
    return decodeCbor(this.#infoBytes);
  }

  // ---- Identifiers ----

  /**
   * A 32-byte identifier hash. First `hash.length` bytes are the stored hash;
   * remaining bytes come from `fingerprint()` (SHA-256 of CBOR).
   */
  identifierHash(): Uint8Array {
    const result = new Uint8Array(32);
    const n = this.#hash.length;
    result.set(this.#hash);
    if (n < 32) {
      const fp = this.fingerprint();
      result.set(fp.slice(0, 32 - n), n);
    }
    return result;
  }

  /**
   * The first `byteCount` bytes of the identifier hash as a hex string.
   * @throws if `byteCount` is not in 4..32.
   */
  identifierN(byteCount: number): string {
    if (byteCount < 4 || byteCount > 32) {
      throw new Error(`byteCount must be 4..32, got ${byteCount}`);
    }
    return bytesToHex(this.identifierHash().slice(0, byteCount));
  }

  /**
   * Return the first four bytes of the mark hash as a hex string.
   */
  identifier(): string {
    return this.identifierN(4);
  }

  /**
   * The first `wordCount` bytes of the identifier hash as upper-case Bytewords.
   * @throws if `wordCount` is not in 4..32.
   */
  bytewordsIdentifierN(wordCount: number, prefix: boolean): string {
    if (wordCount < 4 || wordCount > 32) {
      throw new Error(`wordCount must be 4..32, got ${wordCount}`);
    }
    const s = bwEncodeToWords(this.identifierHash().slice(0, wordCount)).toUpperCase();
    return prefix ? `${PROVENANCE_MARK_PREFIX} ${s}` : s;
  }

  /**
   * Return the first four bytes of the mark hash as upper-case Bytewords.
   */
  bytewordsIdentifier(prefix: boolean): string {
    return this.bytewordsIdentifierN(4, prefix);
  }

  /**
   * The first `wordCount` bytes of the identifier hash as Bytemoji.
   * @throws if `wordCount` is not in 4..32.
   */
  bytemojiIdentifierN(wordCount: number, prefix: boolean): string {
    if (wordCount < 4 || wordCount > 32) {
      throw new Error(`wordCount must be 4..32, got ${wordCount}`);
    }
    const s = bwEncodeToBytemojis(this.identifierHash().slice(0, wordCount)).toUpperCase();
    return prefix ? `${PROVENANCE_MARK_PREFIX} ${s}` : s;
  }

  /**
   * Return the first four bytes of the mark hash as Bytemoji.
   */
  bytemojiIdentifier(prefix: boolean): string {
    return this.bytemojiIdentifierN(4, prefix);
  }

  /**
   * Return an 8-letter identifier derived from the Bytewords identifier.
   */
  bytewordsMinimalIdentifier(prefix: boolean): string {
    const first4 = this.#hash.slice(0, 4);
    const full = bwIdentifier(first4);

    const words = full.split(/\s+/);
    let out = '';
    if (words.length === 4) {
      for (const w of words) {
        if (w.length === 0) continue;
        out += w[0]!.toUpperCase();
        out += w[w.length - 1]!.toUpperCase();
      }
    }

    // Conservative fallback
    if (out.length !== 8) {
      out = '';
      const compact = full
        .split('')
        .filter((c) => /[a-zA-Z]/.test(c))
        .map((c) => c.toUpperCase())
        .join('');
      for (let i = 0; i + 3 < compact.length; i += 4) {
        out += compact[i]!;
        out += compact[i + 3]!;
      }
    }

    return prefix ? `${PROVENANCE_MARK_PREFIX} ${out}` : out;
  }

  // ---- Disambiguation ----

  /**
   * Returns disambiguated upper-case Bytewords identifiers for a set of marks.
   * Non-colliding marks get 4-word identifiers; only colliders are extended.
   */
  static disambiguatedBytewordsIdentifiers(
    marks: ProvenanceMark[],
    prefix: boolean,
  ): string[] {
    const hashes = marks.map((m) => m.identifierHash());
    const lengths = ProvenanceMark.minimalNoncollidingPrefixLengths(hashes);
    return hashes.map((hash, i) => {
      const s = bwEncodeToWords(hash.slice(0, lengths[i]!)).toUpperCase();
      return prefix ? `${PROVENANCE_MARK_PREFIX} ${s}` : s;
    });
  }

  /**
   * Returns disambiguated Bytemoji identifiers for a set of marks.
   * Non-colliding marks get 4-emoji identifiers; only colliders are extended.
   */
  static disambiguatedBytemojiIdentifiers(
    marks: ProvenanceMark[],
    prefix: boolean,
  ): string[] {
    const hashes = marks.map((m) => m.identifierHash());
    const lengths = ProvenanceMark.minimalNoncollidingPrefixLengths(hashes);
    return hashes.map((hash, i) => {
      const s = bwEncodeToBytemojis(hash.slice(0, lengths[i]!)).toUpperCase();
      return prefix ? `${PROVENANCE_MARK_PREFIX} ${s}` : s;
    });
  }

  private static minimalNoncollidingPrefixLengths(
    hashes: Uint8Array[],
  ): number[] {
    const lengths = new Array<number>(hashes.length).fill(4);

    // Group by 4-byte prefix
    const groups = new Map<string, number[]>();
    for (let i = 0; i < hashes.length; i++) {
      const key = bytesToHex(hashes[i]!.slice(0, 4));
      if (!groups.has(key)) groups.set(key, []);
      groups.get(key)!.push(i);
    }

    for (const [, indices] of groups) {
      if (indices.length <= 1) continue;
      ProvenanceMark.resolveCollisionGroup(hashes, indices, lengths);
    }

    return lengths;
  }

  private static resolveCollisionGroup(
    hashes: Uint8Array[],
    initialIndices: number[],
    lengths: number[],
  ): void {
    let unresolved = [...initialIndices];

    for (let prefixLen = 5; prefixLen <= 32; prefixLen++) {
      const subGroups = new Map<string, number[]>();
      for (const i of unresolved) {
        const key = bytesToHex(hashes[i]!.slice(0, prefixLen));
        if (!subGroups.has(key)) subGroups.set(key, []);
        subGroups.get(key)!.push(i);
      }

      const nextUnresolved: number[] = [];
      for (const [, subIndices] of subGroups) {
        if (subIndices.length === 1) {
          lengths[subIndices[0]!] = prefixLen;
        } else {
          nextUnresolved.push(...subIndices);
        }
      }

      if (nextUnresolved.length === 0) return;
      unresolved = nextUnresolved;
    }

    for (const i of unresolved) {
      lengths[i] = 32;
    }
  }

  // ---- Sequence validation ----

  precedes(next: ProvenanceMark): boolean {
    try {
      this.assertPrecedes(next);
      return true;
    } catch {
      return false;
    }
  }

  assertPrecedes(next: ProvenanceMark): void {
    // `next` cannot be a genesis
    if (next.#seq === 0) {
      throw new ProvenanceMarkError(
        'Validation',
        'non-genesis mark at sequence 0',
        { type: 'NonGenesisAtZero' } as const,
      );
    }
    if (bytesEqual(next.#key, next.#chainId)) {
      throw new ProvenanceMarkError(
        'Validation',
        'genesis mark must have key equal to chain_id',
        { type: 'InvalidGenesisKey' } as const,
      );
    }
    // `next` must have the next highest sequence number
    if (this.#seq !== next.#seq - 1) {
      throw new ProvenanceMarkError(
        'Validation',
        `sequence number gap: expected ${this.#seq + 1}, got ${next.#seq}`,
        { type: 'SequenceGap', expected: this.#seq + 1, actual: next.#seq } as const,
      );
    }
    // `next` must have an equal or later date
    if (this.#date.compare(next.#date) > 0) {
      throw new ProvenanceMarkError(
        'Validation',
        `date must be equal or later: previous is ${this.#date.toString()}, next is ${next.#date.toString()}`,
        { type: 'DateOrdering', previous: this.#date, next: next.#date } as const,
      );
    }
    // `next` must reveal the key that was used to generate this mark's hash
    const expectedHash = ProvenanceMark.makeHash(
      this.#resolution,
      this.#key,
      next.#key,
      this.#chainId,
      this.#seqBytes,
      this.#dateBytes,
      this.#infoBytes,
    );
    if (!bytesEqual(this.#hash, expectedHash)) {
      throw new ProvenanceMarkError(
        'Validation',
        `hash mismatch: expected ${bytesToHex(expectedHash)}, got ${bytesToHex(this.#hash)}`,
        { type: 'HashMismatch', expected: expectedHash, actual: new Uint8Array(this.#hash) } as const,
      );
    }
  }

  static isSequenceValid(marks: ProvenanceMark[]): boolean {
    if (marks.length < 2) return false;
    if (marks[0]!.seq === 0 && !marks[0]!.isGenesis()) return false;
    for (let i = 0; i < marks.length - 1; i++) {
      if (!marks[i]!.precedes(marks[i + 1]!)) return false;
    }
    return true;
  }

  isGenesis(): boolean {
    return this.#seq === 0 && bytesEqual(this.#key, this.#chainId);
  }

  // ---- Bytewords ----

  toBytewordsWithStyle(style: Style): string {
    return encodeBytewords(this.message(), style);
  }

  toBytewords(): string {
    return this.toBytewordsWithStyle(Style.STANDARD);
  }

  static fromBytewords(
    resolution: ProvenanceMarkResolution,
    bytewords: string,
  ): ProvenanceMark {
    const message = decodeBytewords(bytewords, Style.STANDARD);
    return ProvenanceMark.fromMessage(resolution, message);
  }

  // ---- URL encoding ----

  toUrlEncoding(): string {
    return encodeBytewords(this.taggedCborData(), Style.MINIMAL);
  }

  static fromUrlEncoding(urlEncoding: string): ProvenanceMark {
    const cborBytes = decodeBytewords(urlEncoding, Style.MINIMAL);
    const c = decodeCbor(cborBytes);
    return ProvenanceMark.fromTaggedCbor(c);
  }

  toUrl(base: string): URL {
    const url = new URL(base);
    url.searchParams.set('provenance', this.toUrlEncoding());
    return url;
  }

  static fromUrl(url: URL): ProvenanceMark {
    const value = url.searchParams.get('provenance');
    if (value == null) {
      throw new ProvenanceMarkError(
        'MissingUrlParameter',
        'missing required URL parameter: provenance',
      );
    }
    return ProvenanceMark.fromUrlEncoding(value);
  }

  // ---- Fingerprint ----

  fingerprint(): Uint8Array {
    return sha256(this.taggedCborData());
  }

  // ---- Display ----

  toString(): string {
    return `ProvenanceMark(${this.identifier()})`;
  }

  toDebugString(): string {
    const components: string[] = [
      `key: ${bytesToHex(this.#key)}`,
      `hash: ${bytesToHex(this.#hash)}`,
      `chainID: ${bytesToHex(this.#chainId)}`,
      `seq: ${this.#seq}`,
      `date: ${this.#date.toString()}`,
    ];

    const infoVal = this.info();
    if (infoVal !== undefined) {
      components.push(`info: ${diagnosticOpt(infoVal)}`);
    }

    return `ProvenanceMark(${components.join(', ')})`;
  }

  // ---- Equality ----

  equals(other: ProvenanceMark): boolean {
    return (
      this.#resolution === other.#resolution &&
      bytesEqual(this.message(), other.message())
    );
  }

  // ---- CBOR Tagged Encodable ----

  cborTags(): Tag[] {
    return tagsForValues([TAG_PROVENANCE_MARK]);
  }

  untaggedCbor(): Cbor {
    return cbor([resolutionToCbor(this.#resolution), toByteString(this.message())]);
  }

  taggedCbor(): Cbor {
    return createTaggedCbor(this);
  }

  taggedCborData(): Uint8Array {
    return cborData(this.taggedCbor());
  }

  // ---- CBOR Tagged Decodable (static) ----

  static cborTags(): Tag[] {
    return tagsForValues([TAG_PROVENANCE_MARK]);
  }

  static fromUntaggedCbor(c: Cbor): ProvenanceMark {
    const arr = expectArray(c);
    if (arr.length !== 2) {
      throw new ProvenanceMarkError(
        'Cbor',
        'Invalid provenance mark length',
      );
    }
    const resolution = resolutionFromCbor(arr[0]!);
    const message = expectBytes(arr[1]!);
    return ProvenanceMark.fromMessage(resolution, message);
  }

  static fromTaggedCbor(c: Cbor): ProvenanceMark {
    validateTag(c, ProvenanceMark.cborTags());
    return ProvenanceMark.fromUntaggedCbor(extractTaggedContent(c));
  }

  static fromTaggedCborData(data: Uint8Array): ProvenanceMark {
    return ProvenanceMark.fromTaggedCbor(decodeCbor(data));
  }

  // Instance methods required by CborTaggedDecodable interface
  fromUntaggedCbor(c: Cbor): ProvenanceMark {
    return ProvenanceMark.fromUntaggedCbor(c);
  }

  fromTaggedCbor(c: Cbor): ProvenanceMark {
    return ProvenanceMark.fromTaggedCbor(c);
  }

  // ---- UR ----

  toUr(): UR {
    return ur(this);
  }

  toUrString(): string {
    return urString(this);
  }

  static fromUr(value: UR): ProvenanceMark {
    return fromUr(ProvenanceMark.prototype, value);
  }

  static fromUrString(value: string): ProvenanceMark {
    return fromUrString(ProvenanceMark.prototype, value);
  }

  // ---- Envelope ----

  toEnvelope(): Envelope {
    return Envelope.from(this.taggedCbor());
  }

  static fromEnvelope(envelope: Envelope): ProvenanceMark {
    const leaf = envelope.subject().tryLeaf();
    return ProvenanceMark.fromTaggedCbor(leaf);
  }

  /**
   * Validate a collection of provenance marks and build a validation report.
   */
  static validate(marks: ProvenanceMark[]): ValidationReport {
    return ValidationReport.validate(marks);
  }

  /**
   * Register provenance-mark formatting tags in the global envelope context.
   */
  static registerTags(): void {
    registerEnvelopeTags();
    const ctx = getGlobalFormatContext();
    ctx.tags().setSummarizer(TAG_PROVENANCE_MARK, (untaggedCbor: Cbor, _flat: boolean) => {
      try {
        const mark = ProvenanceMark.fromUntaggedCbor(untaggedCbor);
        return { ok: true, value: mark.toString() } as const;
      } catch (e) {
        return { ok: false, error: { type: 'Custom', message: String(e) } } as never;
      }
    });
  }

  /**
   * Register provenance-mark formatting tags in a specific envelope context.
   */
  static registerTagsIn(context: FormatContext): void {
    registerEnvelopeTagsIn(context);
    context.tags().setSummarizer(TAG_PROVENANCE_MARK, (untaggedCbor: Cbor, _flat: boolean) => {
      try {
        const mark = ProvenanceMark.fromUntaggedCbor(untaggedCbor);
        return { ok: true, value: mark.toString() } as const;
      } catch (e) {
        return { ok: false, error: { type: 'Custom', message: String(e) } } as never;
      }
    });
  }

  // ---- JSON ----

  toJSON(): Record<string, unknown> {
    const result: Record<string, unknown> = {
      seq: this.#seq,
      date: this.#date.toString(),
      res: this.#resolution as number,
      chain_id: toBase64(this.#chainId),
      key: toBase64(this.#key),
      hash: toBase64(this.#hash),
    };

    if (this.#infoBytes.length > 0) {
      result['info_bytes'] = toBase64(this.#infoBytes);
    }

    return result;
  }

  static fromJSON(json: Record<string, unknown>): ProvenanceMark {
    const seq = json['seq'] as number;
    const dateStr = json['date'] as string;
    const resVal = json['res'] as number;
    const chainIdB64 = json['chain_id'] as string;
    const keyB64 = json['key'] as string;
    const hashB64 = json['hash'] as string;
    const infoBytesB64 = json['info_bytes'] as string | undefined;

    const resolution = resVal as ProvenanceMarkResolution;
    const key = fromBase64(keyB64);
    const hash = fromBase64(hashB64);
    const chainId = fromBase64(chainIdB64);
    const infoBytes =
      infoBytesB64 != null
        ? fromBase64(infoBytesB64)
        : new Uint8Array(0);

    const date = CborDate.fromString(dateStr);
    const seqBytes = serializeSeq(resolution, seq);
    const dateBytes = serializeDate(resolution, date);

    return new ProvenanceMark(
      resolution,
      key,
      hash,
      chainId,
      seqBytes,
      dateBytes,
      infoBytes,
      seq,
      date,
    );
  }
}
