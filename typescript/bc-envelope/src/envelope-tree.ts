import { type Digest } from '@bc/components';

import type { Envelope } from './envelope.js';
import { EdgeType, edgeTypeLabel } from './edge-type.js';
import type { FormatContext, FormatContextOpt } from './format-context.js';
import { FormatContextOpts, withFormatContext } from './format-context.js';

export enum DigestDisplayFormat {
    Short = 'short',
    Full = 'full',
    UR = 'ur',
}

export interface TreeFormatOpts {
    hideNodes?: boolean;
    highlightingTarget?: Set<Digest>;
    context?: FormatContextOpt;
    digestDisplay?: DigestDisplayFormat;
}

interface TreeElement {
    level: number;
    envelope: Envelope;
    incomingEdge: EdgeType;
    showId: boolean;
    isHighlighted: boolean;
}

function digestIn(target: Set<Digest>, digest: Digest): boolean {
    for (const item of target) {
        if (item.equals(digest)) {
            return true;
        }
    }
    return false;
}

function shortId(envelope: Envelope, format: DigestDisplayFormat): string {
    const digest = envelope.digest();
    if (format === DigestDisplayFormat.Short) {
        return digest.shortDescription;
    }
    if (format === DigestDisplayFormat.Full) {
        return digest.hex();
    }
    return digest.urString();
}

function toFormattedString(element: TreeElement, context: FormatContext, digestDisplay: DigestDisplayFormat): string {
    const parts: string[] = [];
    if (element.isHighlighted) {
        parts.push('*');
    }
    if (element.showId) {
        parts.push(shortId(element.envelope, digestDisplay));
    }
    const label = edgeTypeLabel(element.incomingEdge);
    if (label != null) {
        parts.push(label);
    }
    parts.push(element.envelope.summary(40, context));
    return `${' '.repeat(element.level * 4)}${parts.join(' ')}`;
}

export function treeFormat(envelope: Envelope): string {
    return treeFormatOpt(envelope, {});
}

export function treeFormatOpt(envelope: Envelope, opts: TreeFormatOpts): string {
    const hideNodes = opts.hideNodes ?? false;
    const highlightingTarget = opts.highlightingTarget ?? new Set<Digest>();
    const context = opts.context ?? FormatContextOpts.global();
    const digestDisplay = opts.digestDisplay ?? DigestDisplayFormat.Short;

    const elements: TreeElement[] = [];
    envelope.walk(hideNodes, undefined, (item, level, incomingEdge, state) => {
        elements.push({
            level,
            envelope: item,
            incomingEdge,
            showId: !hideNodes,
            isHighlighted: digestIn(highlightingTarget, item.digest()),
        });
        return [state, false];
    });

    const formatElements = (formatContext: FormatContext): string => {
        return elements.map((item) => toFormattedString(item, formatContext, digestDisplay)).join('\n');
    };

    if (context.type === 'none') {
        return formatElements(withFormatContext((global) => global));
    }
    if (context.type === 'global') {
        return withFormatContext((global) => formatElements(global));
    }
    return formatElements(context.context);
}
