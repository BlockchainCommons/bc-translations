import type { UREncodable } from "./ur-encodable.js";
import type { URDecodable } from "./ur-decodable.js";

/**
 * Marker type for values that support both UR encoding and decoding.
 */
export type URCodable<T> = UREncodable & URDecodable<T>;
