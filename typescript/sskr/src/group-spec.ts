import { MAX_SHARE_COUNT } from './constants.js';
import { SskrError } from './error.js';

/**
 * A specification for a group of shares within an SSKR split.
 */
export class GroupSpec {
    readonly #memberThreshold: number;
    readonly #memberCount: number;

    private constructor(memberThreshold: number, memberCount: number) {
        this.#memberThreshold = memberThreshold;
        this.#memberCount = memberCount;
    }

    /**
     * Creates a new `GroupSpec` with the given member threshold and count.
     *
     * @param memberThreshold - The minimum number of member shares required
     *   to reconstruct the secret within the group.
     * @param memberCount - The total number of member shares in the group.
     * @returns A new `GroupSpec` instance.
     * @throws {SskrError} If the member count is zero, greater than
     *   {@link MAX_SHARE_COUNT}, or if the member threshold is greater than
     *   the member count.
     */
    static create(memberThreshold: number, memberCount: number): GroupSpec {
        if (memberCount === 0) {
            throw SskrError.memberCountInvalid();
        }
        if (memberCount > MAX_SHARE_COUNT) {
            throw SskrError.memberCountInvalid();
        }
        if (memberThreshold > memberCount) {
            throw SskrError.memberThresholdInvalid();
        }
        return new GroupSpec(memberThreshold, memberCount);
    }

    /** Creates a default `GroupSpec` of 1-of-1. */
    static default(): GroupSpec {
        return GroupSpec.create(1, 1);
    }

    /**
     * Parses a group specification from a string like `"2-of-3"`.
     *
     * @param s - The string to parse.
     * @returns A new `GroupSpec` instance.
     * @throws {SskrError} If the string format is invalid.
     */
    static parse(s: string): GroupSpec {
        const parts = s.split('-');
        if (parts.length !== 3) {
            throw SskrError.groupSpecInvalid();
        }
        const memberThreshold = parseInt(parts[0]!, 10);
        if (isNaN(memberThreshold)) {
            throw SskrError.groupSpecInvalid();
        }
        if (parts[1] !== 'of') {
            throw SskrError.groupSpecInvalid();
        }
        const memberCount = parseInt(parts[2]!, 10);
        if (isNaN(memberCount)) {
            throw SskrError.groupSpecInvalid();
        }
        return GroupSpec.create(memberThreshold, memberCount);
    }

    /** The member share threshold for this group. */
    get memberThreshold(): number {
        return this.#memberThreshold;
    }

    /** The number of member shares in this group. */
    get memberCount(): number {
        return this.#memberCount;
    }

    /** Returns the string representation, e.g. `"2-of-3"`. */
    toString(): string {
        return `${this.#memberThreshold}-of-${this.#memberCount}`;
    }
}
