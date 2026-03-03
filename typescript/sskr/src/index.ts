/**
 * Sharded Secret Key Reconstruction (SSKR) for TypeScript.
 *
 * SSKR is a protocol for splitting a secret into a set of shares across one
 * or more groups, such that the secret can be reconstructed from any
 * combination of shares totaling or exceeding a threshold number of shares
 * within each group and across all groups.
 *
 * @packageDocumentation
 */

export {
    MIN_SECRET_LEN,
    MAX_SECRET_LEN,
    MAX_SHARE_COUNT,
    MAX_GROUPS_COUNT,
    METADATA_SIZE_BYTES,
    MIN_SERIALIZE_SIZE_BYTES,
} from './constants.js';

export { SskrError } from './error.js';
export { Secret } from './secret.js';
export { GroupSpec } from './group-spec.js';
export { Spec } from './spec.js';
export { sskrGenerate, sskrGenerateUsing, sskrCombine } from './encoding.js';
