import type { CborTaggedDecodable } from "@bc/dcbor";

import { UR } from "./ur.js";

/**
 * Marker interface for values that can be decoded from UR payloads.
 */
export interface URDecodable<T> extends CborTaggedDecodable<T> {}

/**
 * Decodes a value from a UR after validating the expected tag name.
 */
export const fromUr = <T>(codec: URDecodable<T>, value: UR): T => {
  const tag = codec.cborTags()[0];
  const tagName = tag?.name;
  if (tagName === undefined) {
    throw new Error("First CBOR tag must have a name.");
  }

  value.checkType(tagName);
  return codec.fromUntaggedCbor(value.cbor());
};

/**
 * Parses a UR string and decodes a value from it.
 */
export const fromUrString = <T>(codec: URDecodable<T>, urStringValue: string): T => {
  return fromUr(codec, UR.fromUrString(urStringValue));
};
