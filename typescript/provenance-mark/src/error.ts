/**
 * Error types for the provenance-mark package.
 */

export type ProvenanceMarkErrorCode =
  /** Invalid seed length. */
  | 'InvalidSeedLength'
  /** Duplicate key. */
  | 'DuplicateKey'
  /** Missing key. */
  | 'MissingKey'
  /** Invalid key. */
  | 'InvalidKey'
  /** Extra keys. */
  | 'ExtraKeys'
  /** Invalid key length for the given resolution. */
  | 'InvalidKeyLength'
  /** Invalid next key length for the given resolution. */
  | 'InvalidNextKeyLength'
  /** Invalid chain ID length for the given resolution. */
  | 'InvalidChainIdLength'
  /** Invalid message length for the given resolution. */
  | 'InvalidMessageLength'
  /** Invalid CBOR data in the info field. */
  | 'InvalidInfoCbor'
  /** Date out of range for serialization. */
  | 'DateOutOfRange'
  /** Invalid date components. */
  | 'InvalidDate'
  /** Missing required URL parameter. */
  | 'MissingUrlParameter'
  /** Year out of range for 2-byte serialization. */
  | 'YearOutOfRange'
  /** Invalid month or day. */
  | 'InvalidMonthOrDay'
  /** Resolution serialization error. */
  | 'ResolutionError'
  /** Bytewords encoding or decoding error. */
  | 'Bytewords'
  /** CBOR encoding or decoding error. */
  | 'Cbor'
  /** URL parsing error. */
  | 'Url'
  /** Base64 decoding error. */
  | 'Base64'
  /** JSON serialization error. */
  | 'Json'
  /** Envelope conversion error. */
  | 'Envelope'
  /** Validation error. */
  | 'Validation';

/**
 * Provenance mark error with a typed error code.
 */
export class ProvenanceMarkError extends Error {
  readonly code: ProvenanceMarkErrorCode;
  readonly validationIssue?: unknown;

  constructor(code: ProvenanceMarkErrorCode, message: string, validationIssue?: unknown) {
    super(message);
    this.name = 'ProvenanceMarkError';
    this.code = code;
    this.validationIssue = validationIssue;
  }
}
