import { URError } from "./error.js";
import { isUrType } from "./utils.js";

/**
 * Uniform Resource type wrapper with UR character validation.
 *
 * Valid UR type strings contain only lowercase letters, digits, and hyphens.
 */
export class URType {
  readonly #value: string;

  constructor(urType: string) {
    if (!isUrType(urType)) {
      throw URError.invalidType();
    }
    this.#value = urType;
  }

  /**
   * Converts a raw type string or existing `URType` into a validated `URType`.
   *
   * If the value is already a `URType`, it is returned as-is.
   */
  static from(value: string | URType): URType {
    if (value instanceof URType) {
      return value;
    }
    return new URType(value);
  }

  /** Returns true if this type matches `other`. */
  equals(other: URType): boolean {
    return this.#value === other.#value;
  }

  toString(): string {
    return this.#value;
  }
}
