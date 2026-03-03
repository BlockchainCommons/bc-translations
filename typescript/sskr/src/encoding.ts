import type { RandomNumberGenerator } from '@bc/rand';
import { SecureRandomNumberGenerator } from '@bc/rand';
import { ShamirError, splitSecret, recoverSecret } from '@bc/shamir';

import { SskrError } from './error.js';
import { Secret } from './secret.js';
import { SSKRShare } from './share.js';
import type { Spec } from './spec.js';
import { METADATA_SIZE_BYTES } from './constants.js';

/**
 * Generates SSKR shares for the given specification and secret.
 *
 * @param spec - The specification that defines group and member thresholds.
 * @param masterSecret - The secret to split into shares.
 * @returns A nested array of share byte arrays, grouped by SSKR group.
 * @throws {SskrError} If the specification or secret is invalid.
 */
export function sskrGenerate(
    spec: Spec,
    masterSecret: Secret,
): Uint8Array[][] {
    const rng = new SecureRandomNumberGenerator();
    return sskrGenerateUsing(spec, masterSecret, rng);
}

/**
 * Generates SSKR shares using a specific random number generator.
 *
 * @param spec - The specification that defines group and member thresholds.
 * @param masterSecret - The secret to split into shares.
 * @param randomGenerator - The random number generator to use.
 * @returns A nested array of share byte arrays, grouped by SSKR group.
 * @throws {SskrError} If the specification or secret is invalid.
 */
export function sskrGenerateUsing(
    spec: Spec,
    masterSecret: Secret,
    randomGenerator: RandomNumberGenerator,
): Uint8Array[][] {
    const groupsShares = generateShares(spec, masterSecret, randomGenerator);
    return groupsShares.map(group =>
        group.map(share => serializeShare(share)),
    );
}

/**
 * Combines SSKR shares to reconstruct the original secret.
 *
 * @param shares - The SSKR shares to combine. Each share is a byte array.
 * @returns The reconstructed secret.
 * @throws {SskrError} If the shares do not meet the necessary quorum.
 */
export function sskrCombine(shares: Uint8Array[]): Secret {
    const sskrShares: SSKRShare[] = [];
    for (const share of shares) {
        sskrShares.push(deserializeShare(share));
    }
    return combineShares(sskrShares);
}

function serializeShare(share: SSKRShare): Uint8Array {
    // pack the id, group and member data into 5 bytes:
    // 76543210        76543210        76543210
    //         76543210        76543210
    // ----------------====----====----====----
    // identifier: 16
    //                 group-threshold: 4
    //                     group-count: 4
    //                         group-index: 4
    //                             member-threshold: 4
    //                                 reserved (MUST be zero): 4
    //                                     member-index: 4

    const data = share.value.data;
    const result = new Uint8Array(data.length + METADATA_SIZE_BYTES);
    const id = share.identifier;
    const gt = (share.groupThreshold - 1) & 0xf;
    const gc = (share.groupCount - 1) & 0xf;
    const gi = share.groupIndex & 0xf;
    const mt = (share.memberThreshold - 1) & 0xf;
    const mi = share.memberIndex & 0xf;

    result[0] = (id >> 8) & 0xff;
    result[1] = id & 0xff;
    result[2] = (gt << 4) | gc;
    result[3] = (gi << 4) | mt;
    result[4] = mi;
    result.set(data, METADATA_SIZE_BYTES);

    return result;
}

function deserializeShare(source: Uint8Array): SSKRShare {
    if (source.length < METADATA_SIZE_BYTES) {
        throw SskrError.shareLengthInvalid();
    }

    const groupThreshold = ((source[2]! >> 4) + 1);
    const groupCount = ((source[2]! & 0xf) + 1);

    if (groupThreshold > groupCount) {
        throw SskrError.groupThresholdInvalid();
    }

    const identifier = (source[0]! << 8) | source[1]!;
    const groupIndex = (source[3]! >> 4);
    const memberThreshold = ((source[3]! & 0xf) + 1);
    const reserved = source[4]! >> 4;
    if (reserved !== 0) {
        throw SskrError.shareReservedBitsInvalid();
    }
    const memberIndex = (source[4]! & 0xf);
    const value = Secret.create(source.slice(METADATA_SIZE_BYTES));

    return new SSKRShare(
        identifier,
        groupIndex,
        groupThreshold,
        groupCount,
        memberIndex,
        memberThreshold,
        value,
    );
}

function generateShares(
    spec: Spec,
    masterSecret: Secret,
    randomGenerator: RandomNumberGenerator,
): SSKRShare[][] {
    // assign a random identifier
    const idBytes = new Uint8Array(2);
    randomGenerator.fillRandomData(idBytes);
    const identifier = (idBytes[0]! << 8) | idBytes[1]!;

    const groupsShares: SSKRShare[][] = [];

    let groupSecrets: Uint8Array[];
    try {
        groupSecrets = splitSecret(
            spec.groupThreshold,
            spec.groupCount,
            masterSecret.data,
            randomGenerator,
        );
    } catch (e) {
        if (e instanceof ShamirError) {
            throw SskrError.shamirError(e);
        }
        throw e;
    }

    for (let groupIndex = 0; groupIndex < spec.groups.length; groupIndex++) {
        const group = spec.groups[groupIndex]!;
        const groupSecret = groupSecrets[groupIndex]!;

        let memberSecretsRaw: Uint8Array[];
        try {
            memberSecretsRaw = splitSecret(
                group.memberThreshold,
                group.memberCount,
                groupSecret,
                randomGenerator,
            );
        } catch (e) {
            if (e instanceof ShamirError) {
                throw SskrError.shamirError(e);
            }
            throw e;
        }

        const memberSskrShares: SSKRShare[] = memberSecretsRaw.map(
            (memberSecretRaw, memberIndex) => {
                const memberSecret = Secret.create(memberSecretRaw);
                return new SSKRShare(
                    identifier,
                    groupIndex,
                    spec.groupThreshold,
                    spec.groupCount,
                    memberIndex,
                    group.memberThreshold,
                    memberSecret,
                );
            },
        );
        groupsShares.push(memberSskrShares);
    }

    return groupsShares;
}

interface CombineGroup {
    groupIndex: number;
    memberThreshold: number;
    memberIndexes: number[];
    memberShares: Secret[];
}

function combineShares(shares: SSKRShare[]): Secret {
    if (shares.length === 0) {
        throw SskrError.sharesEmpty();
    }

    let identifier = 0;
    let groupThreshold = 0;
    let groupCount = 0;
    let secretLen = 0;

    const groups: CombineGroup[] = [];

    for (let i = 0; i < shares.length; i++) {
        const share = shares[i]!;
        if (i === 0) {
            identifier = share.identifier;
            groupCount = share.groupCount;
            groupThreshold = share.groupThreshold;
            secretLen = share.value.length;
        } else {
            if (
                share.identifier !== identifier ||
                share.groupThreshold !== groupThreshold ||
                share.groupCount !== groupCount ||
                share.value.length !== secretLen
            ) {
                throw SskrError.shareSetInvalid();
            }
        }

        // sort shares into member groups
        let groupFound = false;
        for (const group of groups) {
            if (share.groupIndex === group.groupIndex) {
                groupFound = true;
                if (share.memberThreshold !== group.memberThreshold) {
                    throw SskrError.memberThresholdInvalid();
                }
                for (const existingIndex of group.memberIndexes) {
                    if (share.memberIndex === existingIndex) {
                        throw SskrError.duplicateMemberIndex();
                    }
                }
                if (group.memberIndexes.length < group.memberThreshold) {
                    group.memberIndexes.push(share.memberIndex);
                    group.memberShares.push(share.value);
                }
            }
        }

        if (!groupFound) {
            groups.push({
                groupIndex: share.groupIndex,
                memberThreshold: share.memberThreshold,
                memberIndexes: [share.memberIndex],
                memberShares: [share.value],
            });
        }
    }

    // Check that we have enough groups to recover the master secret
    if (groups.length < groupThreshold) {
        throw SskrError.notEnoughGroups();
    }

    // Recover each group secret, then combine to recover the master secret
    const masterIndexes: number[] = [];
    const masterShares: Uint8Array[] = [];

    for (const group of groups) {
        // Only attempt to recover the group secret if we have enough shares
        if (group.memberIndexes.length < group.memberThreshold) {
            continue;
        }
        // Recover the group secret
        try {
            const groupSecret = recoverSecret(group.memberIndexes, group.memberShares.map(s => s.data));
            masterIndexes.push(group.groupIndex);
            masterShares.push(groupSecret);
        } catch {
            // Skip groups that cannot be recovered
        }
        // Stop if we have enough groups to recover the master secret
        if (masterIndexes.length === groupThreshold) {
            break;
        }
    }

    // If we don't have enough groups to recover the master secret, return an error
    if (masterIndexes.length < groupThreshold) {
        throw SskrError.notEnoughGroups();
    }

    // Recover the master secret
    let masterSecretRaw: Uint8Array;
    try {
        masterSecretRaw = recoverSecret(masterIndexes, masterShares);
    } catch (e) {
        if (e instanceof ShamirError) {
            throw SskrError.shamirError(e);
        }
        throw e;
    }

    return Secret.create(masterSecretRaw);
}
