import { cbor as toCbor, decodeCbor, type Cbor, type CborInput } from "@bc/dcbor";

import { Style, decode as decodeBytewords, encode as encodeBytewords } from "./bytewords.js";
import { URError } from "./error.js";
import { URType } from "./ur-type.js";
import { isUrType, messageFromUnknown } from "./utils.js";

/**
 * A Uniform Resource (UR) is a URI-encoded CBOR object.
 */
export class UR {
  readonly #urType: URType;
  readonly #cbor: Cbor;

  constructor(urType: string | URType, cborValue: CborInput | Cbor) {
    this.#urType = URType.from(urType);
    this.#cbor = toCbor(cborValue);
  }

  /**
   * Parses a single-part UR string into a `UR` object.
   *
   * Accepts both lowercase and uppercase UR strings (e.g. QR-optimized).
   */
  static fromUrString(urString: string): UR {
    const normalized = urString.toLowerCase();

    const stripped = normalized.startsWith("ur:")
      ? normalized.slice(3)
      : undefined;
    if (stripped === undefined) {
      throw URError.invalidScheme();
    }

    const slashIndex = stripped.indexOf("/");
    if (slashIndex === -1) {
      throw URError.typeUnspecified();
    }

    const typeComponent = stripped.slice(0, slashIndex);
    if (!isUrType(typeComponent)) {
      throw URError.invalidType();
    }

    const body = stripped.slice(slashIndex + 1);

    // Reject multipart strings (they contain additional slashes for sequence info)
    if (body.includes("/")) {
      throw URError.notSinglePart();
    }

    try {
      const data = decodeBytewords(body, Style.MINIMAL);
      const cborValue = decodeCbor(data);
      return new UR(typeComponent, cborValue);
    } catch (error) {
      if (error instanceof URError) {
        throw error;
      }
      throw URError.cbor(messageFromUnknown(error));
    }
  }

  /**
   * Returns the canonical lowercase UR string representation.
   */
  toString(): string {
    const data = this.#cbor.toData();
    const body = encodeBytewords(data, Style.MINIMAL);
    return `ur:${this.#urType.toString()}/${body}`;
  }

  /**
   * Returns the uppercase UR string, optimized for QR codes.
   */
  qrString(): string {
    return this.toString().toUpperCase();
  }

  /**
   * Returns UTF-8 bytes for the uppercase QR string.
   */
  qrData(): Uint8Array {
    return new TextEncoder().encode(this.qrString());
  }

  /**
   * Validates the UR type against an expected type value.
   *
   * @throws URError if the types do not match.
   */
  checkType(otherType: string | URType): void {
    const expected = URType.from(otherType);
    if (!this.#urType.equals(expected)) {
      throw URError.unexpectedType(expected.toString(), this.#urType.toString());
    }
  }

  /** The UR type. */
  get urType(): URType {
    return this.#urType;
  }

  /** The UR type as a string. */
  get type(): string {
    return this.#urType.toString();
  }

  /** The CBOR payload. */
  cbor(): Cbor {
    return this.#cbor;
  }

  /** Returns true if both URs have the same type and identical CBOR encoding. */
  equals(other: UR): boolean {
    if (!this.#urType.equals(other.#urType)) {
      return false;
    }
    const a = this.#cbor.toData();
    const b = other.#cbor.toData();
    if (a.length !== b.length) {
      return false;
    }
    for (let i = 0; i < a.length; i++) {
      if (a[i] !== b[i]) {
        return false;
      }
    }
    return true;
  }
}
