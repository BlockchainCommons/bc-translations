import { decodeCbor } from "@bc/dcbor";

import { Style, decode as decodeBytewords } from "./bytewords.js";
import { URError } from "./error.js";
import { FountainDecoder, FountainPart } from "./internal/fountain.js";
import { UR } from "./ur.js";
import { URType } from "./ur-type.js";
import { isUrType, messageFromUnknown } from "./utils.js";

/**
 * Multipart UR decoder backed by fountain decoding.
 *
 * Receives multipart UR strings one at a time via {@link receive} and
 * reconstructs the original UR payload once enough parts have arrived.
 */
export class MultipartDecoder {
  #urType: URType | undefined;
  readonly #decoder: FountainDecoder;

  constructor() {
    this.#decoder = new FountainDecoder();
  }

  /**
   * Receives a multipart UR part.
   *
   * @throws URError if the part type is inconsistent or the payload is invalid.
   */
  receive(value: string): void {
    const decodedType = MultipartDecoder.#decodeType(value);
    if (this.#urType !== undefined) {
      if (!this.#urType.equals(decodedType)) {
        throw URError.unexpectedType(this.#urType.toString(), decodedType.toString());
      }
    } else {
      this.#urType = decodedType;
    }

    try {
      const parsed = parseMultipartUR(value);
      const part = FountainPart.fromCbor(parsed);
      this.#decoder.receive(part);
    } catch (error) {
      if (error instanceof URError) {
        throw error;
      }
      throw URError.ur(messageFromUnknown(error));
    }
  }

  /**
   * Returns true when a full message has been reconstructed.
   */
  get isComplete(): boolean {
    return this.#decoder.complete();
  }

  /**
   * Returns the decoded UR when complete, otherwise `undefined`.
   */
  get message(): UR | undefined {
    const data = this.#decoder.message();
    if (data === undefined) {
      return undefined;
    }

    const urType = this.#urType;
    if (urType === undefined) {
      throw URError.invalidType();
    }

    try {
      const cbor = decodeCbor(data);
      return new UR(urType.toString(), cbor);
    } catch (error) {
      throw URError.cbor(messageFromUnknown(error));
    }
  }

  static #decodeType(urString: string): URType {
    if (!urString.startsWith("ur:")) {
      throw URError.invalidScheme();
    }

    const withoutScheme = urString.slice(3);
    const firstComponent = withoutScheme.split("/")[0];
    if (firstComponent === undefined) {
      throw URError.invalidType();
    }

    return new URType(firstComponent);
  }
}

// --- Internal multipart UR parser ---

/**
 * Parses a multipart UR string and returns the bytewords-decoded payload.
 *
 * Expected format: `ur:<type>/<seqNum>-<seqCount>/<bytewords>`
 */
const parseMultipartUR = (value: string): Uint8Array => {
  const stripped = value.startsWith("ur:") ? value.slice(3) : undefined;
  if (stripped === undefined) {
    throw URError.invalidScheme();
  }

  const typeEnd = stripped.indexOf("/");
  if (typeEnd === -1) {
    throw URError.typeUnspecified();
  }

  const urType = stripped.slice(0, typeEnd);
  if (!isUrType(urType)) {
    throw URError.invalidType();
  }

  const rest = stripped.slice(typeEnd + 1);
  const payloadStart = rest.lastIndexOf("/");
  if (payloadStart === -1) {
    throw URError.ur("expected multipart UR with sequence info");
  }

  const payload = rest.slice(payloadStart + 1);
  return decodeBytewords(payload, Style.MINIMAL);
};
