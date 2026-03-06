import { MultipartEncoder, UR } from "@bc/ur";

import { FragmentState } from "./fragment-state.js";

/** Callback invoked when the display state changes (new part emitted). */
export type URDisplayStateCallback = (state: URDisplayState) => void;

/**
 * Tracks state of ongoing display of (possibly multi-part) UR.
 *
 * Drives animated multi-part UR QR code display by cycling through
 * fountain-coded parts on a timer.
 */
export class URDisplayState {
  readonly ur: UR;
  readonly maxFragmentLen: number;

  /** Frames per second for animated display. Default 8. */
  framesPerSecond: number = 8;

  /** The current QR part as uppercase UTF-8 bytes (suitable for QR rendering). */
  #part: Uint8Array = new Uint8Array(0);

  /** Fragment state indicators for the progress bar. */
  #fragmentStates: FragmentState[] = [FragmentState.Off];

  #encoder: MultipartEncoder;
  #partsCount: number;
  #currentSequence: number = 0;
  #timer: ReturnType<typeof setInterval> | null = null;
  #onUpdate: URDisplayStateCallback | null;

  constructor(
    ur: UR,
    maxFragmentLen: number,
    onUpdate: URDisplayStateCallback | null = null,
  ) {
    this.ur = ur;
    this.maxFragmentLen = maxFragmentLen;
    this.#onUpdate = onUpdate;

    this.#encoder = new MultipartEncoder(ur, maxFragmentLen);
    this.#partsCount = this.#encoder.partCount;
    this.#emitNextPart();
  }

  /** The current part as uppercase UTF-8 bytes. */
  get part(): Uint8Array {
    return this.#part;
  }

  /** Fragment state indicators for each fragment. */
  get fragmentStates(): readonly FragmentState[] {
    return this.#fragmentStates;
  }

  /** Whether the UR fits in a single part (no animation needed). */
  get isSinglePart(): boolean {
    return this.#partsCount === 1;
  }

  /** Current sequence number (1-based). */
  get seqNum(): number {
    return this.#currentSequence;
  }

  /** Total number of distinct fragments (sequence length). */
  get seqLen(): number {
    return this.#partsCount;
  }

  /** Set or replace the update callback. */
  set onUpdate(callback: URDisplayStateCallback | null) {
    this.#onUpdate = callback;
  }

  /** Reset the encoder and emit the first part again. */
  restart(): void {
    this.stop();
    this.#encoder = new MultipartEncoder(this.ur, this.maxFragmentLen);
    this.#partsCount = this.#encoder.partCount;
    this.#currentSequence = 0;
    this.#emitNextPart();
  }

  /** Start the animation timer. Does nothing for single-part URs. */
  run(): void {
    if (this.isSinglePart) return;
    this.stop();
    this.#timer = setInterval(() => {
      this.#emitNextPart();
    }, 1000 / this.framesPerSecond);
  }

  /** Stop the animation timer. */
  stop(): void {
    if (this.#timer !== null) {
      clearInterval(this.#timer);
      this.#timer = null;
    }
  }

  /** Manually advance to the next part (useful for custom timing). */
  emitNextPart(): void {
    this.#emitNextPart();
  }

  #emitNextPart(): void {
    const partString = this.#encoder.nextPart();
    this.#currentSequence = this.#encoder.currentIndex;
    this.#part = new TextEncoder().encode(partString.toUpperCase());

    const indexes = this.#encoder.lastFragmentIndexes;
    this.#fragmentStates = Array.from({ length: this.#partsCount }, (_, i) =>
      indexes.includes(i) ? FragmentState.Highlighted : FragmentState.Off,
    );

    this.#onUpdate?.(this);
  }
}
