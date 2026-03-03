import { MIN_SECRET_LEN as SHAMIR_MIN, MAX_SECRET_LEN as SHAMIR_MAX, MAX_SHARE_COUNT as SHAMIR_MAX_SHARES } from '@bc/shamir';

/** The minimum length of a secret in bytes. */
export const MIN_SECRET_LEN: number = SHAMIR_MIN;

/** The maximum length of a secret in bytes. */
export const MAX_SECRET_LEN: number = SHAMIR_MAX;

/** The maximum number of shares that can be generated from a secret. */
export const MAX_SHARE_COUNT: number = SHAMIR_MAX_SHARES;

/** The maximum number of groups in a split. */
export const MAX_GROUPS_COUNT: number = MAX_SHARE_COUNT;

/** The number of bytes used to encode the metadata for a share. */
export const METADATA_SIZE_BYTES = 5;

/** The minimum number of bytes required to encode a share. */
export const MIN_SERIALIZE_SIZE_BYTES: number = METADATA_SIZE_BYTES + MIN_SECRET_LEN;
