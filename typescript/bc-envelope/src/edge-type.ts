/** Traversal edge labels used during envelope walking. */
export enum EdgeType {
    None = 'none',
    Subject = 'subject',
    Assertion = 'assertion',
    Predicate = 'predicate',
    Object = 'object',
    Content = 'content',
}

export function edgeTypeLabel(edgeType: EdgeType): string | undefined {
    switch (edgeType) {
        case EdgeType.Subject:
            return 'subj';
        case EdgeType.Content:
            return 'cont';
        case EdgeType.Predicate:
            return 'pred';
        case EdgeType.Object:
            return 'obj';
        default:
            return undefined;
    }
}
