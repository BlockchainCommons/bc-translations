import { MAX_SHARE_COUNT } from './constants.js';
import { SskrError } from './error.js';
import type { GroupSpec } from './group-spec.js';

/**
 * A specification for an SSKR split.
 */
export class Spec {
    readonly #groupThreshold: number;
    readonly #groups: GroupSpec[];

    private constructor(groupThreshold: number, groups: GroupSpec[]) {
        this.#groupThreshold = groupThreshold;
        this.#groups = groups;
    }

    /**
     * Creates a new `Spec` with the given group threshold and groups.
     *
     * @param groupThreshold - The minimum number of groups required to
     *   reconstruct the secret.
     * @param groups - The list of group specifications.
     * @returns A new `Spec` instance.
     * @throws {SskrError} If the group threshold is zero, greater than the
     *   number of groups, or if the number of groups exceeds
     *   {@link MAX_SHARE_COUNT}.
     */
    static create(groupThreshold: number, groups: GroupSpec[]): Spec {
        if (groupThreshold === 0) {
            throw SskrError.groupThresholdInvalid();
        }
        if (groupThreshold > groups.length) {
            throw SskrError.groupThresholdInvalid();
        }
        if (groups.length > MAX_SHARE_COUNT) {
            throw SskrError.groupCountInvalid();
        }
        return new Spec(groupThreshold, [...groups]);
    }

    /** The group threshold. */
    get groupThreshold(): number {
        return this.#groupThreshold;
    }

    /** The group specifications. */
    get groups(): readonly GroupSpec[] {
        return this.#groups;
    }

    /** The number of groups. */
    get groupCount(): number {
        return this.#groups.length;
    }

    /** The total number of shares across all groups. */
    get shareCount(): number {
        return this.#groups.reduce((sum, g) => sum + g.memberCount, 0);
    }
}
