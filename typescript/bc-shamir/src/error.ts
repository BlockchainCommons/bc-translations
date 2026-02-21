/**
 * Error type for Shamir's Secret Sharing operations.
 */
export class ShamirError extends Error {
    constructor(message: string) {
        super(message);
        this.name = 'ShamirError';
    }

    static secretTooLong(): ShamirError {
        return new ShamirError('secret is too long');
    }

    static tooManyShares(): ShamirError {
        return new ShamirError('too many shares');
    }

    static interpolationFailure(): ShamirError {
        return new ShamirError('interpolation failed');
    }

    static checksumFailure(): ShamirError {
        return new ShamirError('checksum failure');
    }

    static secretTooShort(): ShamirError {
        return new ShamirError('secret is too short');
    }

    static secretNotEvenLength(): ShamirError {
        return new ShamirError('secret is not of even length');
    }

    static invalidThreshold(): ShamirError {
        return new ShamirError('invalid threshold');
    }

    static sharesUnequalLength(): ShamirError {
        return new ShamirError('shares have unequal length');
    }
}
