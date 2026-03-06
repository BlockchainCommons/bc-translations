/**
 * @bc/provenance-mark — Provenance marks for TypeScript.
 *
 * A cryptographically-secured system for establishing and verifying
 * the authenticity of works.
 *
 *
 * @module @bc/provenance-mark
 */

export { ProvenanceMarkError, type ProvenanceMarkErrorCode } from './error.js';

export {
  SHA256_SIZE,
  sha256,
  sha256Prefix,
  extendKey,
  hkdfHmacSha256,
  obfuscate,
} from './crypto-utils.js';

export {
  serialize2Bytes,
  deserialize2Bytes,
  serialize4Bytes,
  deserialize4Bytes,
  serialize6Bytes,
  deserialize6Bytes,
  rangeOfDaysInMonth,
} from './date-serialization.js';

export {
  ProvenanceMarkResolution,
  resolutionFromU8,
  resolutionFromCbor,
  resolutionToCbor,
  linkLength,
  seqBytesLength,
  dateBytesLength,
  fixedLength,
  keyRangeEnd,
  chainIdRangeEnd,
  hashRangeStart,
  hashRangeEnd,
  seqBytesRangeStart,
  seqBytesRangeEnd,
  dateBytesRangeStart,
  dateBytesRangeEnd,
  infoRangeStart,
  serializeDate,
  deserializeDate,
  serializeSeq,
  deserializeSeq,
  resolutionToString,
} from './resolution.js';

export { PROVENANCE_SEED_LENGTH, ProvenanceSeed } from './seed.js';
export { RNG_STATE_LENGTH, RngState } from './rng-state.js';
export { Xoshiro256StarStar } from './xoshiro256starstar.js';
export { parseSeed, parseDate } from './util.js';
export { ProvenanceMark } from './provenance-mark.js';
export { ProvenanceMarkInfo } from './provenance-mark-info.js';
export { ProvenanceMarkGenerator } from './provenance-mark-generator.js';

export {
  ValidationReportFormat,
  type ValidationIssue,
  validationIssueToString,
  FlaggedMark,
  SequenceReport,
  ChainReport,
  ValidationReport,
} from './validate.js';
