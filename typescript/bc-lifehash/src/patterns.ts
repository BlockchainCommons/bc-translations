import { BitEnumerator } from './bit-enumerator.js';
import { Version } from './version.js';

export enum Pattern {
    Snowflake = 'snowflake',
    Pinwheel = 'pinwheel',
    Fiducial = 'fiducial',
}

export function selectPattern(
    entropy: BitEnumerator,
    version: Version,
): Pattern {
    if (
        version === Version.Fiducial ||
        version === Version.GrayscaleFiducial
    ) {
        return Pattern.Fiducial;
    }
    return entropy.next() ? Pattern.Snowflake : Pattern.Pinwheel;
}
