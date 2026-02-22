import BCShamir
import Foundation

/// Errors that can occur when using the SSKR library.
public enum SSKRError: Error, Equatable, Sendable, LocalizedError {
    /// When combining shares, the provided shares contained a duplicate member index.
    case duplicateMemberIndex

    /// Invalid group specification.
    case groupSpecInvalid

    /// When creating a split spec, the group count is invalid.
    case groupCountInvalid

    /// SSKR group threshold is invalid.
    case groupThresholdInvalid

    /// SSKR member count is invalid.
    case memberCountInvalid

    /// SSKR member threshold is invalid.
    case memberThresholdInvalid

    /// SSKR shares did not contain enough groups.
    case notEnoughGroups

    /// SSKR secret is not of even length.
    case secretLengthNotEven

    /// SSKR secret is too long.
    case secretTooLong

    /// SSKR secret is too short.
    case secretTooShort

    /// SSKR shares did not contain enough serialized bytes.
    case shareLengthInvalid

    /// SSKR shares contained invalid reserved bits.
    case shareReservedBitsInvalid

    /// SSKR shares were empty.
    case sharesEmpty

    /// SSKR shares were invalid.
    case shareSetInvalid

    /// An error from the underlying Shamir library.
    case shamirError(ShamirError)

    public var errorDescription: String? {
        switch self {
        case .duplicateMemberIndex:
            "When combining shares, the provided shares contained a duplicate member index"
        case .groupSpecInvalid:
            "Invalid group specification"
        case .groupCountInvalid:
            "When creating a split spec, the group count is invalid"
        case .groupThresholdInvalid:
            "SSKR group threshold is invalid"
        case .memberCountInvalid:
            "SSKR member count is invalid"
        case .memberThresholdInvalid:
            "SSKR member threshold is invalid"
        case .notEnoughGroups:
            "SSKR shares did not contain enough groups"
        case .secretLengthNotEven:
            "SSKR secret is not of even length"
        case .secretTooLong:
            "SSKR secret is too long"
        case .secretTooShort:
            "SSKR secret is too short"
        case .shareLengthInvalid:
            "SSKR shares did not contain enough serialized bytes"
        case .shareReservedBitsInvalid:
            "SSKR shares contained invalid reserved bits"
        case .sharesEmpty:
            "SSKR shares were empty"
        case .shareSetInvalid:
            "SSKR shares were invalid"
        case .shamirError(let error):
            "SSKR Shamir error: \(error.localizedDescription)"
        }
    }
}
