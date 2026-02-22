import type { CborTaggedEncodable } from "@bc/dcbor";

import { UR } from "./ur.js";

/**
 * Marker interface for values that can be encoded as UR.
 */
export interface UREncodable extends CborTaggedEncodable {}

/**
 * Creates a UR from a tagged encodable value using its primary tag name.
 */
export const ur = (value: CborTaggedEncodable): UR => {
  const tag = value.cborTags()[0];
  if (tag?.name === undefined) {
    const tagValue = tag?.value?.toString() ?? "undefined";
    throw new Error(
      `CBOR tag ${tagValue} must have a name. Did you call \`registerTags()\`?`,
    );
  }

  return new UR(tag.name, value.untaggedCbor());
};

/**
 * Creates a canonical UR string from a tagged encodable value.
 */
export const urString = (value: CborTaggedEncodable): string => {
  return ur(value).toString();
};
