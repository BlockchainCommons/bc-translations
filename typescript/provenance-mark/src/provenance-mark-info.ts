/**
 * ProvenanceMarkInfo -- summary information about a provenance mark.
 */

import { UR } from '@bc/ur';

import { ProvenanceMark } from './provenance-mark.js';

export class ProvenanceMarkInfo {
  readonly #ur: UR;
  readonly #bytewords: string;
  readonly #bytemoji: string;
  readonly #comment: string;
  readonly #mark: ProvenanceMark;

  private constructor(
    mark: ProvenanceMark,
    urValue: UR,
    bytewords: string,
    bytemoji: string,
    comment: string,
  ) {
    this.#mark = mark;
    this.#ur = urValue;
    this.#bytewords = bytewords;
    this.#bytemoji = bytemoji;
    this.#comment = comment;
  }

  static create(mark: ProvenanceMark, comment: string): ProvenanceMarkInfo {
    const urValue = mark.toUr();
    const bytewords = mark.idBytewords(4, true);
    const bytemoji = mark.idBytemoji(4, true);
    return new ProvenanceMarkInfo(mark, urValue, bytewords, bytemoji, comment);
  }

  // ---- Accessors ----

  get mark(): ProvenanceMark {
    return this.#mark;
  }

  get ur(): UR {
    return this.#ur;
  }

  get bytewords(): string {
    return this.#bytewords;
  }

  get bytemoji(): string {
    return this.#bytemoji;
  }

  get comment(): string {
    return this.#comment;
  }

  // ---- Markdown ----

  markdownSummary(): string {
    const lines: string[] = [];

    lines.push('---');

    lines.push('');
    lines.push(`${this.#mark.date.toString()}`);

    lines.push('');
    lines.push(`#### ${this.#ur.toString()}`);

    lines.push('');
    lines.push(`#### \`${this.#bytewords}\``);

    lines.push('');
    lines.push(this.#bytemoji);

    lines.push('');
    if (this.#comment.length > 0) {
      lines.push(this.#comment);
      lines.push('');
    }

    return lines.join('\n');
  }

  // ---- JSON ----

  toJSON(): Record<string, unknown> {
    const result: Record<string, unknown> = {
      ur: this.#ur.toString(),
      bytewords: this.#bytewords,
      bytemoji: this.#bytemoji,
      mark: this.#mark.toJSON(),
    };
    if (this.#comment.length > 0) {
      result['comment'] = this.#comment;
    }
    return result;
  }

  static fromJSON(json: Record<string, unknown>): ProvenanceMarkInfo {
    const urStr = json['ur'] as string;
    const bytewords = json['bytewords'] as string;
    const bytemoji = json['bytemoji'] as string;
    const comment = (json['comment'] as string) ?? '';

    // Reconstruct the mark from the UR to ensure date_bytes and seq_bytes
    // match what was originally generated
    const urValue = UR.fromUrString(urStr);
    const mark = ProvenanceMark.fromUr(urValue);

    return new ProvenanceMarkInfo(mark, urValue, bytewords, bytemoji, comment);
  }
}
