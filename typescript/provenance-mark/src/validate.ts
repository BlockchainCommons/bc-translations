/**
 * Validation support for provenance mark chains.
 */

import { CborDate } from '@bc/dcbor';
import { ProvenanceMark } from './provenance-mark.js';
import { ProvenanceMarkError } from './error.js';
import { bytesToHex, compareBytes } from './utils.js';

/**
 * Output formats for validation reports.
 */
export enum ValidationReportFormat {
  /** Human-readable text format. */
  Text = 'Text',
  /** Compact JSON format with no whitespace. */
  JsonCompact = 'JsonCompact',
  /** Pretty-printed JSON format with indentation. */
  JsonPretty = 'JsonPretty',
}

export type ValidationIssue =
  /** Hash mismatch between consecutive marks. */
  | { type: 'HashMismatch'; expected: Uint8Array; actual: Uint8Array }
  /** Key mismatch between consecutive marks. */
  | { type: 'KeyMismatch' }
  /** Sequence number gap. */
  | { type: 'SequenceGap'; expected: number; actual: number }
  /** Date ordering violation. */
  | { type: 'DateOrdering'; previous: CborDate; next: CborDate }
  /** Non-genesis mark at sequence 0. */
  | { type: 'NonGenesisAtZero' }
  /** Invalid genesis key. */
  | { type: 'InvalidGenesisKey' };

export function validationIssueToString(issue: ValidationIssue): string {
  switch (issue.type) {
    case 'HashMismatch':
      return `hash mismatch: expected ${bytesToHex(issue.expected)}, got ${bytesToHex(issue.actual)}`;
    case 'KeyMismatch':
      return 'key mismatch: current hash was not generated from next key';
    case 'SequenceGap':
      return `sequence number gap: expected ${issue.expected}, got ${issue.actual}`;
    case 'DateOrdering':
      return `date must be equal or later: previous is ${issue.previous.toString()}, next is ${issue.next.toString()}`;
    case 'NonGenesisAtZero':
      return 'non-genesis mark at sequence 0';
    case 'InvalidGenesisKey':
      return 'genesis mark must have key equal to chain_id';
  }
}

function validationIssueToJSON(issue: ValidationIssue): unknown {
  switch (issue.type) {
    case 'HashMismatch':
      return {
        type: 'HashMismatch',
        data: {
          expected: bytesToHex(issue.expected),
          actual: bytesToHex(issue.actual),
        },
      };
    case 'KeyMismatch':
      return { type: 'KeyMismatch' };
    case 'SequenceGap':
      return {
        type: 'SequenceGap',
        data: {
          expected: issue.expected,
          actual: issue.actual,
        },
      };
    case 'DateOrdering':
      return {
        type: 'DateOrdering',
        data: {
          previous: issue.previous.toString(),
          next: issue.next.toString(),
        },
      };
    case 'NonGenesisAtZero':
      return { type: 'NonGenesisAtZero' };
    case 'InvalidGenesisKey':
      return { type: 'InvalidGenesisKey' };
  }
}

/**
 * A provenance mark plus the issues flagged during validation.
 */
export class FlaggedMark {
  readonly #mark: ProvenanceMark;
  readonly #issues: ValidationIssue[];

  private constructor(mark: ProvenanceMark, issues: ValidationIssue[]) {
    this.#mark = mark;
    this.#issues = issues;
  }

  static create(mark: ProvenanceMark): FlaggedMark {
    return new FlaggedMark(mark, []);
  }

  static withIssue(mark: ProvenanceMark, issue: ValidationIssue): FlaggedMark {
    return new FlaggedMark(mark, [issue]);
  }

  get mark(): ProvenanceMark {
    return this.#mark;
  }

  get issues(): readonly ValidationIssue[] {
    return [...this.#issues];
  }

  toJSON(): unknown {
    return {
      mark: this.#mark.toUrString(),
      issues: this.#issues.map(validationIssueToJSON),
    };
  }
}

/**
 * Report for a contiguous sequence of marks within a chain.
 */
export class SequenceReport {
  readonly #startSeq: number;
  readonly #endSeq: number;
  readonly #marks: FlaggedMark[];

  constructor(startSeq: number, endSeq: number, marks: FlaggedMark[]) {
    this.#startSeq = startSeq;
    this.#endSeq = endSeq;
    this.#marks = marks;
  }

  get startSeq(): number {
    return this.#startSeq;
  }

  get endSeq(): number {
    return this.#endSeq;
  }

  get marks(): readonly FlaggedMark[] {
    return [...this.#marks];
  }

  toJSON(): unknown {
    return {
      start_seq: this.#startSeq,
      end_seq: this.#endSeq,
      marks: this.#marks.map((m) => m.toJSON()),
    };
  }
}

/**
 * Report for all marks that share the same chain ID.
 */
export class ChainReport {
  readonly #chainId: Uint8Array;
  readonly #hasGenesis: boolean;
  readonly #marks: ProvenanceMark[];
  readonly #sequences: SequenceReport[];

  constructor(
    chainId: Uint8Array,
    hasGenesis: boolean,
    marks: ProvenanceMark[],
    sequences: SequenceReport[],
  ) {
    this.#chainId = chainId;
    this.#hasGenesis = hasGenesis;
    this.#marks = marks;
    this.#sequences = sequences;
  }

  get chainId(): Uint8Array {
    return new Uint8Array(this.#chainId);
  }

  get hasGenesis(): boolean {
    return this.#hasGenesis;
  }

  get marks(): readonly ProvenanceMark[] {
    return [...this.#marks];
  }

  get sequences(): readonly SequenceReport[] {
    return [...this.#sequences];
  }

  /**
   * Return the chain ID as a hexadecimal string.
   */
  chainIdHex(): string {
    return bytesToHex(this.#chainId);
  }

  toJSON(): unknown {
    return {
      chain_id: bytesToHex(this.#chainId),
      has_genesis: this.#hasGenesis,
      marks: this.#marks.map((m) => m.toUrString()),
      sequences: this.#sequences.map((s) => s.toJSON()),
    };
  }
}

/**
 * Complete validation report for a set of provenance marks.
 */
export class ValidationReport {
  readonly #marks: ProvenanceMark[];
  readonly #chains: ChainReport[];

  private constructor(marks: ProvenanceMark[], chains: ChainReport[]) {
    this.#marks = marks;
    this.#chains = chains;
  }

  get marks(): readonly ProvenanceMark[] {
    return [...this.#marks];
  }

  get chains(): readonly ChainReport[] {
    return [...this.#chains];
  }

  // -----------------------------------------------------------------------
  // Validation
  // -----------------------------------------------------------------------

  /**
   * Validate a collection of provenance marks.
   */
  static validate(marks: ProvenanceMark[]): ValidationReport {
    // Deduplicate exact duplicates by message equality
    const seen = new Set<string>();
    const deduplicatedMarks: ProvenanceMark[] = [];
    for (const mark of marks) {
      const msg = bytesToHex(mark.message());
      if (!seen.has(msg)) {
        seen.add(msg);
        deduplicatedMarks.push(mark);
      }
    }

    // Bin marks by chain ID
    const chainBins = new Map<string, ProvenanceMark[]>();
    for (const mark of deduplicatedMarks) {
      const key = bytesToHex(mark.chainId);
      let bin = chainBins.get(key);
      if (bin === undefined) {
        bin = [];
        chainBins.set(key, bin);
      }
      bin.push(mark);
    }

    // Process each chain
    const chains: ChainReport[] = [];
    for (const [, chainMarks] of chainBins) {
      // Sort by sequence number
      chainMarks.sort((a, b) => a.seq - b.seq);

      // Check for genesis mark
      const first = chainMarks[0];
      const hasGenesis =
        first !== undefined && first.seq === 0 && first.isGenesis();

      // Build sequence bins
      const sequences = ValidationReport.buildSequenceBins(chainMarks);

      chains.push(
        new ChainReport(
          first!.chainId,
          hasGenesis,
          [...chainMarks],
          sequences,
        ),
      );
    }

    // Sort chains by chain ID bytes for consistent output
    chains.sort((a, b) => compareBytes(a.chainId, b.chainId));

    return new ValidationReport(deduplicatedMarks, chains);
  }

  private static buildSequenceBins(
    marks: ProvenanceMark[],
  ): SequenceReport[] {
    const sequences: SequenceReport[] = [];
    let currentSequence: FlaggedMark[] = [];

    for (let i = 0; i < marks.length; i++) {
      const mark = marks[i]!;

      if (i === 0) {
        // First mark starts a sequence
        currentSequence.push(FlaggedMark.create(mark));
      } else {
        const prev = marks[i - 1]!;

        // Check if this mark follows the previous one
        try {
          prev.assertPrecedes(mark);
          // Continues the current sequence
          currentSequence.push(FlaggedMark.create(mark));
        } catch (e: unknown) {
          // Breaks the sequence - save current and start new
          if (currentSequence.length > 0) {
            sequences.push(
              ValidationReport.createSequenceReport(currentSequence),
            );
          }

          // Extract ValidationIssue from the error
          let issue: ValidationIssue;
          if (
            e instanceof ProvenanceMarkError &&
            e.code === 'Validation' &&
            e.validationIssue !== undefined
          ) {
            issue = e.validationIssue as ValidationIssue;
          } else {
            issue = { type: 'KeyMismatch' };
          }

          // Start new sequence with this mark, flagged with the issue
          currentSequence = [FlaggedMark.withIssue(mark, issue)];
        }
      }
    }

    // Add the final sequence
    if (currentSequence.length > 0) {
      sequences.push(
        ValidationReport.createSequenceReport(currentSequence),
      );
    }

    return sequences;
  }

  private static createSequenceReport(
    marks: FlaggedMark[],
  ): SequenceReport {
    const startSeq = marks[0]?.mark.seq ?? 0;
    const endSeq = marks[marks.length - 1]?.mark.seq ?? 0;
    return new SequenceReport(startSeq, endSeq, marks);
  }

  // -----------------------------------------------------------------------
  // Formatting
  // -----------------------------------------------------------------------

  /**
   * Format the validation report as text or JSON.
   */
  format(fmt: ValidationReportFormat): string {
    switch (fmt) {
      case ValidationReportFormat.Text:
        return this.formatText();
      case ValidationReportFormat.JsonCompact:
        return JSON.stringify(this.toJSON());
      case ValidationReportFormat.JsonPretty:
        return JSON.stringify(this.toJSON(), null, 2);
    }
  }

  private formatText(): string {
    if (!this.isInteresting()) {
      return '';
    }

    const lines: string[] = [];

    // Report summary
    lines.push(`Total marks: ${this.#marks.length}`);
    lines.push(`Chains: ${this.#chains.length}`);
    lines.push('');

    // Report each chain
    for (let chainIdx = 0; chainIdx < this.#chains.length; chainIdx++) {
      const chain = this.#chains[chainIdx]!;

      // Show short chain ID (first 4 bytes = 8 hex chars)
      const chainIdHex = chain.chainIdHex();
      const shortChainId =
        chainIdHex.length > 8 ? chainIdHex.slice(0, 8) : chainIdHex;

      lines.push(`Chain ${chainIdx + 1}: ${shortChainId}`);

      if (!chain.hasGenesis) {
        lines.push('  Warning: No genesis mark found');
      }

      // Report each sequence
      for (const seq of chain.sequences) {
        // Report each mark in the sequence
        for (const flaggedMark of seq.marks) {
          const mark = flaggedMark.mark;
          const shortId = mark.identifier();
          const seqNum = mark.seq;

          // Build the mark line with annotations
          const annotations: string[] = [];

          // Check if it's genesis
          if (mark.isGenesis()) {
            annotations.push('genesis mark');
          }

          // Add issue annotations
          for (const issue of flaggedMark.issues) {
            let issueStr: string;
            switch (issue.type) {
              case 'SequenceGap':
                issueStr = `gap: ${issue.expected} missing`;
                break;
              case 'DateOrdering':
                issueStr = `date ${issue.previous.toString()} < ${issue.next.toString()}`;
                break;
              case 'HashMismatch':
                issueStr = 'hash mismatch';
                break;
              case 'KeyMismatch':
                issueStr = 'key mismatch';
                break;
              case 'NonGenesisAtZero':
                issueStr = 'non-genesis at seq 0';
                break;
              case 'InvalidGenesisKey':
                issueStr = 'invalid genesis key';
                break;
            }
            annotations.push(issueStr);
          }

          // Format the line
          if (annotations.length === 0) {
            lines.push(`  ${seqNum}: ${shortId}`);
          } else {
            lines.push(
              `  ${seqNum}: ${shortId} (${annotations.join(', ')})`,
            );
          }
        }
      }

      lines.push('');
    }

    return lines.join('\n').trimEnd();
  }

  /**
   * Check if the validation report contains interesting information.
   * Returns false for a single perfect chain with no issues, true otherwise.
   */
  private isInteresting(): boolean {
    // Not interesting if empty
    if (this.#chains.length === 0) {
      return false;
    }

    // Check if any chain is missing genesis
    for (const chain of this.#chains) {
      if (!chain.hasGenesis) {
        return true;
      }
    }

    // Not interesting if single chain with single perfect sequence
    if (this.#chains.length === 1) {
      const chain = this.#chains[0]!;
      if (chain.sequences.length === 1) {
        const seq = chain.sequences[0]!;
        // Check if the sequence has no issues
        if (seq.marks.every((m) => m.issues.length === 0)) {
          return false;
        }
      }
    }

    return true;
  }

  /**
   * Check if the validation report has any issues.
   * Returns true if there are validation issues, missing genesis,
   * multiple chains, or multiple sequences.
   */
  hasIssues(): boolean {
    // Missing genesis is considered an issue
    for (const chain of this.#chains) {
      if (!chain.hasGenesis) {
        return true;
      }
    }

    // Check for validation issues in marks
    for (const chain of this.#chains) {
      for (const seq of chain.sequences) {
        for (const mark of seq.marks) {
          if (mark.issues.length > 0) {
            return true;
          }
        }
      }
    }

    // Multiple chains or sequences are also considered issues
    if (this.#chains.length > 1) {
      return true;
    }

    if (
      this.#chains.length === 1 &&
      this.#chains[0]!.sequences.length > 1
    ) {
      return true;
    }

    return false;
  }

  toJSON(): unknown {
    return {
      marks: this.#marks.map((m) => m.toUrString()),
      chains: this.#chains.map((c) => c.toJSON()),
    };
  }
}
