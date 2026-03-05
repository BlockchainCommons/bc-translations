import { expect } from 'vitest';

function countLeadingWhitespace(value: string): number {
    let index = 0;
    while (index < value.length) {
        const ch = value[index];
        if (ch !== ' ' && ch !== '\t') {
            break;
        }
        index += 1;
    }
    return index;
}

export function normalizeText(value: string): string {
    const normalized = value.replace(/\r\n/g, '\n');
    const lines = normalized.split('\n');

    while (lines.length > 0 && lines[0]!.trim() === '') {
        lines.shift();
    }
    while (lines.length > 0 && lines[lines.length - 1]!.trim() === '') {
        lines.pop();
    }

    if (lines.length === 0) {
        return '';
    }

    // expected-text-output-rubric: treat first-line indentation as code-indent.
    const codeIndent = countLeadingWhitespace(lines[0]!);
    const dedented = lines.map((line) => {
        const leading = countLeadingWhitespace(line);
        if (leading >= codeIndent) {
            return line.slice(codeIndent);
        }
        return line;
    });

    return dedented.join('\n');
}

export function expectActualExpected(actual: string, expected: string): void {
    const normalizedActual = normalizeText(actual);
    const normalizedExpected = normalizeText(expected);
    if (normalizedActual !== normalizedExpected) {
        // expected-text-output-rubric: print both values on mismatch.
        // eslint-disable-next-line no-console
        console.log(`Actual:\n${normalizedActual}\nExpected:\n${normalizedExpected}`);
    }
    expect(normalizedActual).toBe(normalizedExpected);
}

export function utf8(value: string): Uint8Array {
    return new TextEncoder().encode(value);
}
