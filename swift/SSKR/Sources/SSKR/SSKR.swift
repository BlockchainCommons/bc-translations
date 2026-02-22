/// Sharded Secret Key Reconstruction (SSKR) for Swift.
///
/// SSKR is a protocol for splitting a secret into shares across one or more
/// groups, such that the secret can be reconstructed from any combination of
/// shares meeting threshold requirements within and across groups.

import BCRand
import BCShamir

// MARK: - Constants

/// The minimum length of a secret.
public let minSecretLen = BCShamir.minSecretLen

/// The maximum length of a secret.
public let maxSecretLen = BCShamir.maxSecretLen

/// The maximum number of shares that can be generated from a secret.
public let maxShareCount = BCShamir.maxShareCount

/// The maximum number of groups in a split.
public let maxGroupsCount = maxShareCount

/// The number of bytes used to encode the metadata for a share.
public let metadataSizeBytes = 5

/// The minimum number of bytes required to encode a share.
public let minSerializeSizeBytes = metadataSizeBytes + minSecretLen

// MARK: - Public API

/// Generates SSKR shares for the given specification and secret.
///
/// Uses the system's secure random number generator.
///
/// - Parameters:
///   - spec: The split specification defining group and member thresholds.
///   - secret: The secret to split into shares.
/// - Returns: A nested array of share byte arrays, grouped by group.
/// - Throws: ``SSKRError`` if the specification or secret is invalid.
public func sskrGenerate(
    spec: Spec,
    secret: Secret
) throws(SSKRError) -> [[[UInt8]]] {
    var rng = SecureRandomNumberGenerator()
    return try sskrGenerateUsing(spec: spec, secret: secret, randomGenerator: &rng)
}

/// Generates SSKR shares using a caller-provided random number generator.
///
/// - Parameters:
///   - spec: The split specification defining group and member thresholds.
///   - secret: The secret to split into shares.
///   - randomGenerator: The random number generator to use.
/// - Returns: A nested array of share byte arrays, grouped by group.
/// - Throws: ``SSKRError`` if the specification or secret is invalid.
public func sskrGenerateUsing(
    spec: Spec,
    secret: Secret,
    randomGenerator: inout some BCRandomNumberGenerator
) throws(SSKRError) -> [[[UInt8]]] {
    let groupsShares = try generateShares(
        spec: spec,
        masterSecret: secret,
        randomGenerator: &randomGenerator
    )
    return groupsShares.map { group in
        group.map { serializeShare($0) }
    }
}

/// Combines SSKR shares to reconstruct the original secret.
///
/// - Parameter shares: A flat array of serialized share byte arrays from
///   one or more groups. The shares must meet the group and member
///   thresholds to successfully reconstruct the secret.
/// - Returns: The reconstructed ``Secret``.
/// - Throws: ``SSKRError`` if the shares are invalid or insufficient.
public func sskrCombine(shares: [[UInt8]]) throws(SSKRError) -> Secret {
    var sskrShares: [SSKRShare] = []
    sskrShares.reserveCapacity(shares.count)

    for share in shares {
        let sskrShare = try deserializeShare(share)
        sskrShares.append(sskrShare)
    }

    return try combineShares(sskrShares)
}

// MARK: - Serialization

func serializeShare(_ share: SSKRShare) -> [UInt8] {
    // Pack the id, group and member data into 5 bytes:
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

    var result = [UInt8]()
    result.reserveCapacity(share.value.count + metadataSizeBytes)

    let id = share.identifier
    let gt = (share.groupThreshold - 1) & 0xF
    let gc = (share.groupCount - 1) & 0xF
    let gi = share.groupIndex & 0xF
    let mt = (share.memberThreshold - 1) & 0xF
    let mi = share.memberIndex & 0xF

    let id1 = UInt8(id >> 8)
    let id2 = UInt8(id & 0xFF)

    result.append(id1)
    result.append(id2)
    result.append(UInt8((gt << 4) | gc))
    result.append(UInt8((gi << 4) | mt))
    result.append(UInt8(mi))
    result.append(contentsOf: share.value.data)

    return result
}

func deserializeShare(_ source: [UInt8]) throws(SSKRError) -> SSKRShare {
    if source.count < metadataSizeBytes {
        throw .shareLengthInvalid
    }

    let groupThreshold = Int(source[2] >> 4) + 1
    let groupCount = Int(source[2] & 0x0F) + 1

    if groupThreshold > groupCount {
        throw .groupThresholdInvalid
    }

    let identifier = (UInt16(source[0]) << 8) | UInt16(source[1])
    let groupIndex = Int(source[3] >> 4)
    let memberThreshold = Int(source[3] & 0x0F) + 1
    let reserved = source[4] >> 4
    if reserved != 0 {
        throw .shareReservedBitsInvalid
    }
    let memberIndex = Int(source[4] & 0x0F)
    let value = try Secret(Array(source[metadataSizeBytes...]))

    return SSKRShare(
        identifier: identifier,
        groupIndex: groupIndex,
        groupThreshold: groupThreshold,
        groupCount: groupCount,
        memberIndex: memberIndex,
        memberThreshold: memberThreshold,
        value: value
    )
}

// MARK: - Share Generation

private func generateShares(
    spec: Spec,
    masterSecret: Secret,
    randomGenerator: inout some BCRandomNumberGenerator
) throws(SSKRError) -> [[SSKRShare]] {
    // Assign a random identifier
    let identifierData = randomGenerator.randomData(count: 2)
    let identifierBytes = [UInt8](identifierData)
    let identifier = (UInt16(identifierBytes[0]) << 8) | UInt16(identifierBytes[1])

    var groupsShares: [[SSKRShare]] = []
    groupsShares.reserveCapacity(spec.groupCount)

    let groupSecrets: [[UInt8]]
    do {
        groupSecrets = try splitSecret(
            threshold: spec.groupThreshold,
            shareCount: spec.groupCount,
            secret: masterSecret.data,
            randomGenerator: &randomGenerator
        )
    } catch {
        throw .shamirError(error)
    }

    for (groupIndex, group) in spec.groups.enumerated() {
        let groupSecret = groupSecrets[groupIndex]
        let memberSecrets: [[UInt8]]
        do {
            memberSecrets = try splitSecret(
                threshold: group.memberThreshold,
                shareCount: group.memberCount,
                secret: groupSecret,
                randomGenerator: &randomGenerator
            )
        } catch {
            throw .shamirError(error)
        }

        var memberSSKRShares: [SSKRShare] = []
        memberSSKRShares.reserveCapacity(memberSecrets.count)
        for (memberIndex, memberSecret) in memberSecrets.enumerated() {
            let value = try Secret(memberSecret)
            memberSSKRShares.append(SSKRShare(
                identifier: identifier,
                groupIndex: groupIndex,
                groupThreshold: spec.groupThreshold,
                groupCount: spec.groupCount,
                memberIndex: memberIndex,
                memberThreshold: group.memberThreshold,
                value: value
            ))
        }
        groupsShares.append(memberSSKRShares)
    }

    return groupsShares
}

// MARK: - Share Combination

private struct CombineGroup {
    let groupIndex: Int
    let memberThreshold: Int
    var memberIndices: [Int]
    var memberShares: [Secret]

    init(groupIndex: Int, memberThreshold: Int) {
        self.groupIndex = groupIndex
        self.memberThreshold = memberThreshold
        self.memberIndices = []
        self.memberShares = []
    }
}

private func combineShares(_ shares: [SSKRShare]) throws(SSKRError) -> Secret {
    if shares.isEmpty {
        throw .sharesEmpty
    }

    var identifier: UInt16 = 0
    var groupThreshold = 0
    var groupCount = 0
    var secretLen = 0

    var groups: [CombineGroup] = []

    for (i, share) in shares.enumerated() {
        if i == 0 {
            identifier = share.identifier
            groupCount = share.groupCount
            groupThreshold = share.groupThreshold
            secretLen = share.value.count
        } else {
            if share.identifier != identifier
                || share.groupThreshold != groupThreshold
                || share.groupCount != groupCount
                || share.value.count != secretLen
            {
                throw .shareSetInvalid
            }
        }

        // Sort shares into member groups
        var groupFound = false
        for g in groups.indices {
            if share.groupIndex == groups[g].groupIndex {
                groupFound = true
                if share.memberThreshold != groups[g].memberThreshold {
                    throw .memberThresholdInvalid
                }
                if groups[g].memberIndices.contains(share.memberIndex) {
                    throw .duplicateMemberIndex
                }
                if groups[g].memberIndices.count < groups[g].memberThreshold {
                    groups[g].memberIndices.append(share.memberIndex)
                    groups[g].memberShares.append(share.value)
                }
            }
        }

        if !groupFound {
            var g = CombineGroup(
                groupIndex: share.groupIndex,
                memberThreshold: share.memberThreshold
            )
            g.memberIndices.append(share.memberIndex)
            g.memberShares.append(share.value)
            groups.append(g)
        }
    }

    // Check that we have enough groups
    if groups.count < groupThreshold {
        throw .notEnoughGroups
    }

    // Recover group secrets, then recover the master secret
    var masterIndices: [Int] = []
    var masterShares: [[UInt8]] = []
    masterIndices.reserveCapacity(16)
    masterShares.reserveCapacity(16)

    for group in groups {
        // Only attempt recovery if we have enough shares
        if group.memberIndices.count < group.memberThreshold {
            continue
        }
        let memberShareData = group.memberShares.map { $0.data }
        if let groupSecret = try? recoverSecret(
            indices: group.memberIndices,
            shares: memberShareData
        ) {
            masterIndices.append(group.groupIndex)
            masterShares.append(groupSecret)
        }
        // Stop if we have enough groups
        if masterIndices.count == groupThreshold {
            break
        }
    }

    if masterIndices.count < groupThreshold {
        throw .notEnoughGroups
    }

    let masterSecretData: [UInt8]
    do {
        masterSecretData = try recoverSecret(
            indices: masterIndices,
            shares: masterShares
        )
    } catch {
        throw .shamirError(error)
    }

    return try Secret(masterSecretData)
}
