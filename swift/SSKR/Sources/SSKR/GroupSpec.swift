/// A specification for a group of shares within an SSKR split.
public struct GroupSpec: Equatable, Sendable {
    /// The minimum number of member shares required to reconstruct the
    /// secret within this group.
    public let memberThreshold: Int

    /// The total number of member shares in this group.
    public let memberCount: Int

    /// Creates a new `GroupSpec` with the given member threshold and count.
    ///
    /// - Parameters:
    ///   - memberThreshold: The minimum number of member shares required.
    ///   - memberCount: The total number of member shares.
    /// - Throws: ``SSKRError`` if the member count is zero, exceeds
    ///   ``maxShareCount``, or the threshold exceeds the count.
    public init(memberThreshold: Int, memberCount: Int) throws(SSKRError) {
        if memberCount == 0 {
            throw .memberCountInvalid
        }
        if memberCount > maxShareCount {
            throw .memberCountInvalid
        }
        if memberThreshold > memberCount {
            throw .memberThresholdInvalid
        }
        self.memberThreshold = memberThreshold
        self.memberCount = memberCount
    }

    /// Parses a group specification from a string in "M-of-N" format.
    ///
    /// - Parameter s: The string to parse (e.g., "2-of-3").
    /// - Returns: A new `GroupSpec`.
    /// - Throws: ``SSKRError/groupSpecInvalid`` if the format is invalid.
    public static func parse(_ s: String) throws(SSKRError) -> GroupSpec {
        let parts = s.split(separator: "-")
        guard parts.count == 3 else {
            throw .groupSpecInvalid
        }
        guard let memberThreshold = Int(parts[0]) else {
            throw .groupSpecInvalid
        }
        guard parts[1] == "of" else {
            throw .groupSpecInvalid
        }
        guard let memberCount = Int(parts[2]) else {
            throw .groupSpecInvalid
        }
        return try GroupSpec(memberThreshold: memberThreshold, memberCount: memberCount)
    }
}

extension GroupSpec {
    /// Creates a default `GroupSpec` with threshold 1 and count 1.
    public init() {
        self.memberThreshold = 1
        self.memberCount = 1
    }
}

extension GroupSpec: CustomStringConvertible {
    public var description: String {
        "\(memberThreshold)-of-\(memberCount)"
    }
}
