import { Style, encode as encodeBytewords } from "./bytewords.js";
import { URError } from "./error.js";
import { FountainEncoder } from "./internal/fountain.js";
import { UR } from "./ur.js";
import { messageFromUnknown } from "./utils.js";

/**
 * Multipart UR encoder backed by fountain coding.
 *
 * Splits a UR payload into fragments that can be transmitted as a sequence
 * of UR strings. Mixed (XOR) fragments enable the decoder to reconstruct
 * the original message even if some parts are missed.
 */
export class MultipartEncoder {
  readonly #encoder: FountainEncoder;
  readonly #urType: string;

  constructor(ur: UR, maxFragmentLen: number) {
    this.#urType = ur.type;

    try {
      this.#encoder = new FountainEncoder(ur.cbor().toData(), maxFragmentLen);
    } catch (error) {
      throw URError.ur(messageFromUnknown(error));
    }
  }

  /**
   * Returns the next multipart UR part string.
   */
  nextPart(): string {
    try {
      const part = this.#encoder.nextPart();
      const body = encodeBytewords(part.cbor(), Style.MINIMAL);
      return `ur:${this.#urType}/${part.sequenceId()}/${body}`;
    } catch (error) {
      throw URError.ur(messageFromUnknown(error));
    }
  }

  /**
   * Returns the current emitted part index (starts at 0).
   */
  get currentIndex(): number {
    return this.#encoder.currentSequence();
  }

  /**
   * Returns the total fragment count for the source message.
   */
  get partCount(): number {
    return this.#encoder.fragmentCount();
  }
}
