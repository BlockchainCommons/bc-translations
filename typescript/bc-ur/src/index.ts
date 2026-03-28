/**
 * `@bc/ur` provides Uniform Resources (UR) support for TypeScript.
 *
 * It wraps deterministic CBOR values from `@bc/dcbor` in UR string form,
 * plus bytewords utilities and multipart fountain encoding/decoding helpers.
 *
 * @packageDocumentation
 */

export { URError, type URErrorKind } from "./error.js";

export { URType } from "./ur-type.js";
export { UR } from "./ur.js";

export {
  Style,
  type Style as BytewordsStyle,
  encode,
  decode,
  encodeToWords,
  encodeToBytemojis,
  identifier,
  bytemojiIdentifier,
  isValidWord,
  isValidBytemoji,
  canonicalizeByteword,
  BYTEWORDS,
  BYTEMOJIS,
} from "./bytewords.js";

export { MultipartEncoder } from "./multipart-encoder.js";
export { MultipartDecoder } from "./multipart-decoder.js";

export type { UREncodable } from "./ur-encodable.js";
export { ur, urString } from "./ur-encodable.js";

export type { URDecodable } from "./ur-decodable.js";
export { fromUr, fromUrString } from "./ur-decodable.js";

export type { URCodable } from "./ur-codable.js";

export * from "./prelude.js";
