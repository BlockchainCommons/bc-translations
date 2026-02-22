package com.blockchaincommons.sskr

import com.blockchaincommons.bcrand.RandomNumberGenerator
import com.blockchaincommons.bcrand.SecureRandomNumberGenerator
import com.blockchaincommons.bcshamir.ShamirException
import com.blockchaincommons.bcshamir.recoverSecret
import com.blockchaincommons.bcshamir.splitSecret
import com.blockchaincommons.bcshamir.MAX_SECRET_LEN as BC_SHAMIR_MAX_SECRET_LEN
import com.blockchaincommons.bcshamir.MAX_SHARE_COUNT as BC_SHAMIR_MAX_SHARE_COUNT
import com.blockchaincommons.bcshamir.MIN_SECRET_LEN as BC_SHAMIR_MIN_SECRET_LEN

/**
 * # Sharded Secret Key Reconstruction (SSKR)
 *
 * SSKR splits a secret into shares distributed across one or more groups. The
 * secret can be reconstructed from any share set that satisfies both per-group
 * member thresholds and the overall group threshold.
 */
/** The minimum length of a secret. */
const val MIN_SECRET_LEN: Int = BC_SHAMIR_MIN_SECRET_LEN

/** The maximum length of a secret. */
const val MAX_SECRET_LEN: Int = BC_SHAMIR_MAX_SECRET_LEN

/** The maximum number of shares that can be generated from a secret. */
const val MAX_SHARE_COUNT: Int = BC_SHAMIR_MAX_SHARE_COUNT

/** The maximum number of groups in a split. */
const val MAX_GROUPS_COUNT: Int = MAX_SHARE_COUNT

/** The number of bytes used to encode metadata for a share. */
const val METADATA_SIZE_BYTES: Int = 5

/** The minimum number of bytes required to encode a share. */
const val MIN_SERIALIZE_SIZE_BYTES: Int = METADATA_SIZE_BYTES + MIN_SECRET_LEN

/**
 * Generates SSKR shares for the given [spec] and [masterSecret].
 */
fun sskrGenerate(spec: Spec, masterSecret: Secret): List<List<ByteArray>> {
    val rng = SecureRandomNumberGenerator()
    return sskrGenerateUsing(spec, masterSecret, rng)
}

/**
 * Generates SSKR shares for the given [spec] and [masterSecret] using the
 * provided [randomGenerator].
 */
fun sskrGenerateUsing(
    spec: Spec,
    masterSecret: Secret,
    randomGenerator: RandomNumberGenerator,
): List<List<ByteArray>> {
    val groupsShares = generateShares(spec, masterSecret, randomGenerator)
    return groupsShares.map { group -> group.map(::serializeShare) }
}

/**
 * Combines serialized SSKR shares into a recovered [Secret].
 */
fun sskrCombine(shares: List<ByteArray>): Secret {
    val sskrShares = ArrayList<SskrShare>(shares.size)
    for (share in shares) {
        sskrShares.add(deserializeShare(share))
    }
    return combineShares(sskrShares)
}

private fun serializeShare(share: SskrShare): ByteArray {
    // pack id, group, and member metadata into 5 bytes:
    // identifier: 16 bits
    // group-threshold-1: 4 bits
    // group-count-1: 4 bits
    // group-index: 4 bits
    // member-threshold-1: 4 bits
    // reserved: 4 bits (must be zero)
    // member-index: 4 bits
    val value = share.value.dataRef()
    val result = ByteArray(value.size + METADATA_SIZE_BYTES)

    val gt = (share.groupThreshold - 1) and 0xF
    val gc = (share.groupCount - 1) and 0xF
    val gi = share.groupIndex and 0xF
    val mt = (share.memberThreshold - 1) and 0xF
    val mi = share.memberIndex and 0xF

    result[0] = (share.identifier ushr 8).toByte()
    result[1] = (share.identifier and 0xFF).toByte()
    result[2] = ((gt shl 4) or gc).toByte()
    result[3] = ((gi shl 4) or mt).toByte()
    result[4] = mi.toByte()
    value.copyInto(result, destinationOffset = METADATA_SIZE_BYTES)

    return result
}

private fun deserializeShare(source: ByteArray): SskrShare {
    if (source.size < METADATA_SIZE_BYTES) {
        throw SskrException.ShareLengthInvalid()
    }

    val groupThreshold = ((source[2].toInt() ushr 4) and 0xF) + 1
    val groupCount = (source[2].toInt() and 0xF) + 1

    if (groupThreshold > groupCount) {
        throw SskrException.GroupThresholdInvalid()
    }

    val identifier = ((source[0].toInt() and 0xFF) shl 8) or (source[1].toInt() and 0xFF)
    val groupIndex = (source[3].toInt() ushr 4) and 0xF
    val memberThreshold = (source[3].toInt() and 0xF) + 1
    val reserved = (source[4].toInt() ushr 4) and 0xF
    if (reserved != 0) {
        throw SskrException.ShareReservedBitsInvalid()
    }
    val memberIndex = source[4].toInt() and 0xF
    val value = Secret(source.copyOfRange(METADATA_SIZE_BYTES, source.size))

    return SskrShare(
        identifier = identifier,
        groupIndex = groupIndex,
        groupThreshold = groupThreshold,
        groupCount = groupCount,
        memberIndex = memberIndex,
        memberThreshold = memberThreshold,
        value = value,
    )
}

private fun generateShares(
    spec: Spec,
    masterSecret: Secret,
    randomGenerator: RandomNumberGenerator,
): List<List<SskrShare>> {
    // assign a random identifier
    val identifierBytes = ByteArray(2)
    randomGenerator.fillRandomData(identifierBytes)
    val identifier = ((identifierBytes[0].toInt() and 0xFF) shl 8) or (identifierBytes[1].toInt() and 0xFF)

    val groupsShares = ArrayList<List<SskrShare>>(spec.groupCount)

    val groupSecrets = wrapShamir {
        splitSecret(
            threshold = spec.groupThreshold,
            shareCount = spec.groupCount,
            secret = masterSecret.dataRef(),
            randomGenerator = randomGenerator,
        )
    }

    for ((groupIndex, group) in spec.groups.withIndex()) {
        val groupSecret = groupSecrets[groupIndex]
        val memberSecrets = wrapShamir {
            splitSecret(
                threshold = group.memberThreshold,
                shareCount = group.memberCount,
                secret = groupSecret,
                randomGenerator = randomGenerator,
            )
        }.map { Secret(it) }

        val memberSskrShares = memberSecrets.mapIndexed { memberIndex, memberSecret ->
            SskrShare(
                identifier = identifier,
                groupIndex = groupIndex,
                groupThreshold = spec.groupThreshold,
                groupCount = spec.groupCount,
                memberIndex = memberIndex,
                memberThreshold = group.memberThreshold,
                value = memberSecret,
            )
        }

        groupsShares.add(memberSskrShares)
    }

    return groupsShares
}

private class Group(
    val groupIndex: Int,
    val memberThreshold: Int,
) {
    val memberIndexes: MutableList<Int> = ArrayList(16)
    val memberShares: MutableList<Secret> = ArrayList(16)
}

private fun combineShares(shares: List<SskrShare>): Secret {
    if (shares.isEmpty()) {
        throw SskrException.SharesEmpty()
    }

    var identifier = 0
    var groupThreshold = 0
    var groupCount = 0
    var secretLength = 0

    var nextGroup = 0
    val groups = ArrayList<Group>(16)

    for ((index, share) in shares.withIndex()) {
        if (index == 0) {
            // On the first share, establish expected common metadata values.
            identifier = share.identifier
            groupCount = share.groupCount
            groupThreshold = share.groupThreshold
            secretLength = share.value.length
        } else {
            // Subsequent shares must match common metadata.
            if (
                share.identifier != identifier ||
                share.groupThreshold != groupThreshold ||
                share.groupCount != groupCount ||
                share.value.length != secretLength
            ) {
                throw SskrException.ShareSetInvalid()
            }
        }

        // Sort shares into member groups.
        var groupFound = false
        for (group in groups) {
            if (share.groupIndex == group.groupIndex) {
                groupFound = true
                if (share.memberThreshold != group.memberThreshold) {
                    throw SskrException.MemberThresholdInvalid()
                }
                if (group.memberIndexes.contains(share.memberIndex)) {
                    throw SskrException.DuplicateMemberIndex()
                }
                if (group.memberIndexes.size < group.memberThreshold) {
                    group.memberIndexes.add(share.memberIndex)
                    group.memberShares.add(share.value)
                }
            }
        }

        if (!groupFound) {
            val group = Group(share.groupIndex, share.memberThreshold)
            group.memberIndexes.add(share.memberIndex)
            group.memberShares.add(share.value)
            groups.add(group)
            nextGroup += 1
        }
    }

    // Check that we have enough groups to recover the master secret.
    if (nextGroup < groupThreshold) {
        throw SskrException.NotEnoughGroups()
    }

    // Recover each group secret that has enough member shares.
    val masterIndexes = ArrayList<Int>(16)
    val masterShares = ArrayList<ByteArray>(16)

    for (group in groups) {
        if (group.memberIndexes.size < group.memberThreshold) {
            continue
        }

        val groupSecret = try {
            recoverSecret(
                indexes = group.memberIndexes,
                shares = group.memberShares.map { it.dataRef() },
            )
        } catch (_: ShamirException) {
            null
        }

        if (groupSecret != null) {
            masterIndexes.add(group.groupIndex)
            masterShares.add(groupSecret)
        }

        if (masterIndexes.size == groupThreshold) {
            break
        }
    }

    if (masterIndexes.size < groupThreshold) {
        throw SskrException.NotEnoughGroups()
    }

    val masterSecret = wrapShamir {
        recoverSecret(masterIndexes, masterShares)
    }

    return Secret(masterSecret)
}

private inline fun <T> wrapShamir(block: () -> T): T {
    try {
        return block()
    } catch (error: ShamirException) {
        throw SskrException.ShamirError(error)
    }
}
