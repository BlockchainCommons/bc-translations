import type { Digest } from '@bc/components';

import type { Envelope } from './envelope.js';
import { EdgeType, edgeTypeLabel } from './edge-type.js';
import type { FormatContextOpt } from './format-context.js';
import { FormatContextOpts, withFormatContext } from './format-context.js';

export enum MermaidOrientation {
    LeftToRight = 'LR',
    TopToBottom = 'TB',
    RightToLeft = 'RL',
    BottomToTop = 'BT',
}

export enum MermaidTheme {
    Default = 'default',
    Neutral = 'neutral',
    Dark = 'dark',
    Forest = 'forest',
    Base = 'base',
}

export interface MermaidFormatOpts {
    hideNodes?: boolean;
    monochrome?: boolean;
    theme?: MermaidTheme;
    orientation?: MermaidOrientation;
    highlightingTarget?: Set<Digest>;
    context?: FormatContextOpt;
}

interface MermaidElement {
    id: number;
    level: number;
    envelope: Envelope;
    incomingEdge: EdgeType;
    showId: boolean;
    isHighlighted: boolean;
    parent?: MermaidElement;
}

function digestIn(target: Set<Digest>, digest: Digest): boolean {
    for (const item of target) {
        if (item.equals(digest)) {
            return true;
        }
    }
    return false;
}

function envelopeFrame(envelope: Envelope): [string, string] {
    switch (envelope.case().kind) {
        case 'node':
            return ['((', '))'];
        case 'leaf':
            return ['[', ']'];
        case 'wrapped':
            return ['[/', '\\]'];
        case 'assertion':
            return ['([', '])'];
        case 'elided':
            return ['{{', '}}'];
        case 'known-value':
            return ['[/', '/]'];
        case 'encrypted':
            return ['>', ']'];
        case 'compressed':
            return ['[[', ']]'];
    }
}

function nodeColor(envelope: Envelope): string {
    switch (envelope.case().kind) {
        case 'node':
            return 'red';
        case 'leaf':
            return 'teal';
        case 'wrapped':
            return 'blue';
        case 'assertion':
            return 'green';
        case 'elided':
            return 'gray';
        case 'known-value':
            return 'goldenrod';
        case 'encrypted':
            return 'coral';
        case 'compressed':
            return 'purple';
    }
}

function linkStrokeColor(edgeType: EdgeType): string | undefined {
    if (edgeType === EdgeType.Subject) return 'red';
    if (edgeType === EdgeType.Content) return 'blue';
    if (edgeType === EdgeType.Predicate) return 'cyan';
    if (edgeType === EdgeType.Object) return 'magenta';
    return undefined;
}

function formatNode(element: MermaidElement, elementIds: Set<number>): string {
    if (elementIds.has(element.id)) {
        elementIds.delete(element.id);
        const summary = withFormatContext((context) => element.envelope.summary(20, context).replaceAll('"', '&quot;'));
        const lines = [summary];
        if (element.showId) {
            lines.push(element.envelope.digest().shortDescription);
        }
        const [left, right] = envelopeFrame(element.envelope);
        return `${element.id}${left}"${lines.join('<br>')}"${right}`;
    }
    return `${element.id}`;
}

function formatEdge(element: MermaidElement, elementIds: Set<number>): string {
    const parent = element.parent!;
    const label = edgeTypeLabel(element.incomingEdge);
    const arrow = label ? `-- ${label} -->` : '-->';
    return `${formatNode(parent, elementIds)} ${arrow} ${formatNode(element, elementIds)}`;
}

export function mermaidFormat(envelope: Envelope): string {
    return mermaidFormatOpt(envelope, {});
}

export function mermaidFormatOpt(envelope: Envelope, opts: MermaidFormatOpts): string {
    const hideNodes = opts.hideNodes ?? false;
    const monochrome = opts.monochrome ?? false;
    const theme = opts.theme ?? MermaidTheme.Default;
    const orientation = opts.orientation ?? MermaidOrientation.LeftToRight;
    const highlightingTarget = opts.highlightingTarget ?? new Set<Digest>();

    const elements: MermaidElement[] = [];
    let nextId = 0;

    envelope.walk(hideNodes, undefined as MermaidElement | undefined, (item, level, incomingEdge, parent) => {
        const element: MermaidElement = {
            id: nextId,
            level,
            envelope: item,
            incomingEdge,
            showId: !hideNodes,
            isHighlighted: digestIn(highlightingTarget, item.digest()),
            parent,
        };
        nextId += 1;
        elements.push(element);
        return [element, false];
    });

    const elementIds = new Set<number>(elements.map((item) => item.id));
    const lines: string[] = [];
    lines.push(`%%{ init: { 'theme': '${theme}', 'flowchart': { 'curve': 'basis' } } }%%`);
    lines.push(`graph ${orientation}`);

    const nodeStyles: string[] = [];
    const linkStyles: string[] = [];
    let linkIndex = 0;

    for (const element of elements) {
        const indent = '    '.repeat(element.level);
        const content = element.parent == null ? formatNode(element, elementIds) : formatEdge(element, elementIds);

        const currentNodeStyles: string[] = [];
        if (!monochrome) {
            currentNodeStyles.push(`stroke:${nodeColor(element.envelope)}`);
        }
        currentNodeStyles.push(`stroke-width:${element.isHighlighted ? 6 : 4}px`);
        nodeStyles.push(`style ${element.id} ${currentNodeStyles.join(',')}`);

        if (element.parent != null) {
            const currentLinkStyles: string[] = [];
            if (!monochrome) {
                const stroke = linkStrokeColor(element.incomingEdge);
                if (stroke != null) {
                    currentLinkStyles.push(`stroke:${stroke}`);
                }
            }
            currentLinkStyles.push(`stroke-width:${element.isHighlighted && element.parent.isHighlighted ? 4 : 2}px`);
            linkStyles.push(`linkStyle ${linkIndex} ${currentLinkStyles.join(',')}`);
            linkIndex += 1;
        }

        lines.push(`${indent}${content}`);
    }

    lines.push(...nodeStyles);
    lines.push(...linkStyles);
    return lines.join('\n');
}
