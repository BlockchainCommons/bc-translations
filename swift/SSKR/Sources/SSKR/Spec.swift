/// A specification for an SSKR split.
public struct Spec: Equatable, Sendable {
    /// The minimum number of groups required to reconstruct the secret.
    public let groupThreshold: Int

    /// The group specifications.
    public let groups: [GroupSpec]

    /// Creates a new `Spec` with the given group threshold and groups.
    ///
    /// - Parameters:
    ///   - groupThreshold: The minimum number of groups required.
    ///   - groups: The list of group specifications.
    /// - Throws: ``SSKRError`` if the group threshold is zero, exceeds the
    ///   number of groups, or the number of groups exceeds ``maxShareCount``.
    public init(groupThreshold: Int, groups: [GroupSpec]) throws(SSKRError) {
        if groupThreshold == 0 {
            throw .groupThresholdInvalid
        }
        if groupThreshold > groups.count {
            throw .groupThresholdInvalid
        }
        if groups.count > maxShareCount {
            throw .groupCountInvalid
        }
        self.groupThreshold = groupThreshold
        self.groups = groups
    }

    /// The number of groups.
    public var groupCount: Int { groups.count }

    /// The total number of shares across all groups.
    public var shareCount: Int {
        groups.reduce(0) { $0 + $1.memberCount }
    }
}
