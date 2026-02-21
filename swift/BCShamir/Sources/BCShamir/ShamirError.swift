import Foundation

/// Errors that can occur during Shamir secret sharing operations.
public enum ShamirError: Error, Equatable, Sendable {
    case secretTooLong
    case tooManyShares
    case interpolationFailure
    case checksumFailure
    case secretTooShort
    case secretLengthNotEven
    case invalidThreshold
    case sharesUnequalLength
}

extension ShamirError: LocalizedError {
    public var errorDescription: String? {
        switch self {
        case .secretTooLong:
            "The secret is too long"
        case .tooManyShares:
            "Too many shares requested"
        case .interpolationFailure:
            "Interpolation failed"
        case .checksumFailure:
            "Checksum verification failed"
        case .secretTooShort:
            "The secret is too short"
        case .secretLengthNotEven:
            "The secret length must be even"
        case .invalidThreshold:
            "Invalid threshold"
        case .sharesUnequalLength:
            "Shares have unequal length"
        }
    }
}
