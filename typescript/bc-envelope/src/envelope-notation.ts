import {
    diagnosticOpt,
    type Cbor,
} from '@bc/dcbor';
import {
    IS_A,
    type KnownValue,
} from '@bc/known-values';

import { Envelope } from './envelope.js';
import { flankedBy } from './string-utils.js';
import type { FormatContextOpt } from './format-context.js';
import { withFormatContext } from './format-context.js';

export interface EnvelopeFormatOpts {
    flat?: boolean;
    context: FormatContextOpt;
}

type EnvelopeFormatItem =
    | { kind: 'begin'; delimiter: string }
    | { kind: 'end'; delimiter: string }
    | { kind: 'item'; text: string }
    | { kind: 'separator' }
    | { kind: 'list'; items: EnvelopeFormatItem[] };

function flatten(items: EnvelopeFormatItem[]): EnvelopeFormatItem[] {
    const result: EnvelopeFormatItem[] = [];
    for (const item of items) {
        if (item.kind === 'list') {
            result.push(...flatten(item.items));
        } else {
            result.push(item);
        }
    }
    return result;
}

function nicen(items: EnvelopeFormatItem[]): EnvelopeFormatItem[] {
    const input = [...items];
    const result: EnvelopeFormatItem[] = [];
    while (input.length > 0) {
        const current = input.shift()!;
        if (input.length === 0) {
            result.push(current);
            break;
        }
        const next = input[0]!;
        if (current.kind === 'end' && next.kind === 'begin') {
            result.push({ kind: 'end', delimiter: `${current.delimiter} ${next.delimiter}` });
            result.push({ kind: 'begin', delimiter: '' });
            input.shift();
        } else {
            result.push(current);
        }
    }
    return result;
}

function addSpaceAtEndIfNeeded(value: string): string {
    if (value.length === 0) {
        return ' ';
    }
    if (value.endsWith(' ')) {
        return value;
    }
    return `${value} `;
}

function formatFlat(items: EnvelopeFormatItem[]): string {
    let text = '';
    for (const item of flatten(items)) {
        switch (item.kind) {
            case 'begin':
                if (!text.endsWith(' ')) {
                    text += ' ';
                }
                text += `${item.delimiter} `;
                break;
            case 'end':
                if (!text.endsWith(' ')) {
                    text += ' ';
                }
                text += `${item.delimiter} `;
                break;
            case 'item':
                text += item.text;
                break;
            case 'separator':
                text = `${text.trimEnd()}, `;
                break;
            case 'list':
                text += formatFlat(item.items);
                break;
        }
    }
    return text;
}

function formatHierarchical(items: EnvelopeFormatItem[]): string {
    const lines: string[] = [];
    let level = 0;
    let currentLine = '';
    for (const item of nicen(flatten(items))) {
        switch (item.kind) {
            case 'begin': {
                if (item.delimiter.length > 0) {
                    const content = currentLine.length === 0
                        ? item.delimiter
                        : `${addSpaceAtEndIfNeeded(currentLine)}${item.delimiter}`;
                    lines.push(`${' '.repeat(level * 4)}${content}\n`);
                }
                level += 1;
                currentLine = '';
                break;
            }
            case 'end':
                if (currentLine.length > 0) {
                    lines.push(`${' '.repeat(level * 4)}${currentLine}\n`);
                    currentLine = '';
                }
                level -= 1;
                lines.push(`${' '.repeat(level * 4)}${item.delimiter}\n`);
                break;
            case 'item':
                currentLine += item.text;
                break;
            case 'separator':
                if (currentLine.length > 0) {
                    lines.push(`${' '.repeat(level * 4)}${currentLine}\n`);
                    currentLine = '';
                }
                break;
            case 'list':
                lines.push(formatHierarchical(item.items));
                break;
        }
    }
    if (currentLine.length > 0) {
        lines.push(currentLine);
    }
    return lines.join('');
}

function cborFormatItem(cbor: Cbor, opts: Required<EnvelopeFormatOpts>): EnvelopeFormatItem {
    try {
        if (cbor.isTagged()) {
            const [tag, item] = cbor.toTagged();
            if (Number(tag.value) === 200) {
                return envelopeFormatItem(Envelope.fromUntaggedCbor(item), opts);
            }
        }
    } catch {
        // fall through
    }

    try {
        const text = opts.context.type === 'global'
            ? withFormatContext((ctx) => cborEnvelopeSummary(cbor, Number.MAX_SAFE_INTEGER, { type: 'custom', context: ctx }))
            : cborEnvelopeSummary(cbor, Number.MAX_SAFE_INTEGER, opts.context);
        return { kind: 'item', text };
    } catch {
        return { kind: 'item', text: '<error>' };
    }
}

function cborEnvelopeSummary(cbor: Cbor, maxLength: number, context: FormatContextOpt): string {
    void maxLength;

    if (context.type === 'none') {
        return cbor.toString();
    }

    if (context.type === 'global') {
        return withFormatContext((ctx) => diagnosticOpt(cbor, {
            summarize: true,
            flat: true,
            tags: ctx.tags(),
        }));
    }

    return diagnosticOpt(cbor, {
        summarize: true,
        flat: true,
        tags: context.context.tags(),
    });
}

function knownValueFormatItem(value: KnownValue, opts: Required<EnvelopeFormatOpts>): EnvelopeFormatItem {
    let text = flankedBy(value.name, "'", "'");
    if (opts.context.type === 'global') {
        text = withFormatContext((context) => {
            const assigned = context.knownValues().assignedName(value);
            return flankedBy(assigned ?? value.name, "'", "'");
        });
    } else if (opts.context.type === 'custom') {
        const assigned = opts.context.context.knownValues().assignedName(value);
        text = flankedBy(assigned ?? value.name, "'", "'");
    }
    return { kind: 'item', text };
}

function assertionFormatItem(assertion: import('./assertion.js').Assertion, opts: Required<EnvelopeFormatOpts>): EnvelopeFormatItem {
    return {
        kind: 'list',
        items: [
            envelopeFormatItem(assertion.predicate(), opts),
            { kind: 'item', text: ': ' },
            envelopeFormatItem(assertion.objectEnvelope(), opts),
        ],
    };
}

function envelopeFormatItem(envelope: Envelope, opts: Required<EnvelopeFormatOpts>): EnvelopeFormatItem {
    const c = envelope.case();
    switch (c.kind) {
        case 'leaf':
            return cborFormatItem(c.cbor, opts);
        case 'wrapped':
            return {
                kind: 'list',
                items: [
                    { kind: 'begin', delimiter: '{' },
                    envelopeFormatItem(c.envelope, opts),
                    { kind: 'end', delimiter: '}' },
                ],
            };
        case 'assertion':
            return assertionFormatItem(c.assertion, opts);
        case 'known-value':
            return knownValueFormatItem(c.value, opts);
        case 'encrypted':
            return { kind: 'item', text: 'ENCRYPTED' };
        case 'compressed':
            return { kind: 'item', text: 'COMPRESSED' };
        case 'elided':
            return { kind: 'item', text: 'ELIDED' };
        case 'node': {
            const typeAssertionItems: EnvelopeFormatItem[] = [];
            const assertionItems: EnvelopeFormatItem[] = [];
            let elidedCount = 0;
            let encryptedCount = 0;
            let compressedCount = 0;

            for (const assertion of c.assertions) {
                const caseKind = assertion.case().kind;
                if (caseKind === 'elided') {
                    elidedCount += 1;
                    continue;
                }
                if (caseKind === 'encrypted') {
                    encryptedCount += 1;
                    continue;
                }
                if (caseKind === 'compressed') {
                    compressedCount += 1;
                    continue;
                }

                const item = envelopeFormatItem(assertion, opts);
                const predicate = assertion.asPredicate();
                const knownValue = predicate?.subject().asKnownValue();
                if (knownValue != null && knownValue.equals(IS_A)) {
                    typeAssertionItems.push(item);
                } else {
                    assertionItems.push(item);
                }
            }

            const toComparable = (item: EnvelopeFormatItem): string => formatFlat([item]);
            typeAssertionItems.sort((a, b) => toComparable(a).localeCompare(toComparable(b)));
            assertionItems.sort((a, b) => toComparable(a).localeCompare(toComparable(b)));
            const ordered = [...typeAssertionItems, ...assertionItems];

            if (compressedCount > 0) {
                ordered.push({
                    kind: 'item',
                    text: compressedCount > 1 ? `COMPRESSED (${compressedCount})` : 'COMPRESSED',
                });
            }
            if (elidedCount > 0) {
                ordered.push({
                    kind: 'item',
                    text: elidedCount > 1 ? `ELIDED (${elidedCount})` : 'ELIDED',
                });
            }
            if (encryptedCount > 0) {
                ordered.push({
                    kind: 'item',
                    text: encryptedCount > 1 ? `ENCRYPTED (${encryptedCount})` : 'ENCRYPTED',
                });
            }

            const joined: EnvelopeFormatItem[] = [];
            for (let i = 0; i < ordered.length; i += 1) {
                joined.push(ordered[i]!);
                if (i < ordered.length - 1) {
                    joined.push({ kind: 'separator' });
                }
            }

            const needsBraces = c.subject.isSubjectAssertion();
            const items: EnvelopeFormatItem[] = [];
            if (needsBraces) {
                items.push({ kind: 'begin', delimiter: '{' });
            }
            items.push(envelopeFormatItem(c.subject, opts));
            if (needsBraces) {
                items.push({ kind: 'end', delimiter: '}' });
            }
            items.push({ kind: 'begin', delimiter: '[' });
            items.push(...joined);
            items.push({ kind: 'end', delimiter: ']' });
            return { kind: 'list', items };
        }
    }
}

export function formatEnvelope(envelope: Envelope, opts: { flat?: boolean; context: FormatContextOpt }): string {
    const options: Required<EnvelopeFormatOpts> = {
        flat: opts.flat ?? false,
        context: opts.context,
    };
    const item = envelopeFormatItem(envelope, options);
    return (options.flat ? formatFlat([item]) : formatHierarchical([item])).trim();
}
