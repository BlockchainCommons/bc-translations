import type { ShamirError } from '@bc/shamir';

/**
 * Error type for SSKR operations.
 */
export class SskrError extends Error {
    constructor(message: string) {
        super(message);
        this.name = 'SskrError';
    }

    static duplicateMemberIndex(): SskrError {
        return new SskrError(
            'when combining shares, the provided shares contained a duplicate member index',
        );
    }

    static groupSpecInvalid(): SskrError {
        return new SskrError('invalid group specification');
    }

    static groupCountInvalid(): SskrError {
        return new SskrError('when creating a split spec, the group count is invalid');
    }

    static groupThresholdInvalid(): SskrError {
        return new SskrError('SSKR group threshold is invalid');
    }

    static memberCountInvalid(): SskrError {
        return new SskrError('SSKR member count is invalid');
    }

    static memberThresholdInvalid(): SskrError {
        return new SskrError('SSKR member threshold is invalid');
    }

    static notEnoughGroups(): SskrError {
        return new SskrError('SSKR shares did not contain enough groups');
    }

    static secretLengthNotEven(): SskrError {
        return new SskrError('SSKR secret is not of even length');
    }

    static secretTooLong(): SskrError {
        return new SskrError('SSKR secret is too long');
    }

    static secretTooShort(): SskrError {
        return new SskrError('SSKR secret is too short');
    }

    static shareLengthInvalid(): SskrError {
        return new SskrError('SSKR shares did not contain enough serialized bytes');
    }

    static shareReservedBitsInvalid(): SskrError {
        return new SskrError('SSKR shares contained invalid reserved bits');
    }

    static sharesEmpty(): SskrError {
        return new SskrError('SSKR shares were empty');
    }

    static shareSetInvalid(): SskrError {
        return new SskrError('SSKR shares were invalid');
    }

    static shamirError(cause: ShamirError): SskrError {
        const err = new SskrError(`SSKR Shamir error: ${cause.message}`);
        err.cause = cause;
        return err;
    }
}
