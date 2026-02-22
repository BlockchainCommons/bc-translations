package com.blockchaincommons.sskr

/** A specification for an SSKR split. */
class Spec(groupThreshold: Int, groups: List<GroupSpec>) {
    /** The minimum number of groups required to reconstruct the secret. */
    val groupThreshold: Int

    /** The list of group specifications. */
    val groups: List<GroupSpec>

    init {
        if (groupThreshold == 0) {
            throw SskrException.GroupThresholdInvalid()
        }
        if (groupThreshold > groups.size) {
            throw SskrException.GroupThresholdInvalid()
        }
        if (groups.size > MAX_SHARE_COUNT) {
            throw SskrException.GroupCountInvalid()
        }

        this.groupThreshold = groupThreshold
        this.groups = groups.toList()
    }

    /** Returns the number of groups. */
    val groupCount: Int
        get() = groups.size

    /** Returns the total number of shares across all groups. */
    val shareCount: Int
        get() = groups.sumOf { it.memberCount }

    override fun equals(other: Any?): Boolean {
        if (this === other) {
            return true
        }
        if (other !is Spec) {
            return false
        }
        return groupThreshold == other.groupThreshold && groups == other.groups
    }

    override fun hashCode(): Int = 31 * groupThreshold + groups.hashCode()

    override fun toString(): String = "Spec(groupThreshold=$groupThreshold, groups=$groups)"
}

/** A specification for a group of shares within an SSKR split. */
class GroupSpec(memberThreshold: Int = 1, memberCount: Int = 1) {
    /** The minimum number of member shares required to recover this group. */
    val memberThreshold: Int

    /** The total number of member shares in this group. */
    val memberCount: Int

    init {
        if (memberCount == 0) {
            throw SskrException.MemberCountInvalid()
        }
        if (memberCount > MAX_SHARE_COUNT) {
            throw SskrException.MemberCountInvalid()
        }
        if (memberThreshold > memberCount) {
            throw SskrException.MemberThresholdInvalid()
        }

        this.memberThreshold = memberThreshold
        this.memberCount = memberCount
    }

    /** Parses a group specification from an `M-of-N` string. */
    companion object {
        fun parse(s: String): GroupSpec {
            val parts = s.split('-')
            if (parts.size != 3) {
                throw SskrException.GroupSpecInvalid()
            }
            val parsedThreshold = parts[0].toIntOrNull() ?: throw SskrException.GroupSpecInvalid()
            if (parts[1] != "of") {
                throw SskrException.GroupSpecInvalid()
            }
            val parsedCount = parts[2].toIntOrNull() ?: throw SskrException.GroupSpecInvalid()
            return GroupSpec(parsedThreshold, parsedCount)
        }
    }

    override fun equals(other: Any?): Boolean {
        if (this === other) {
            return true
        }
        if (other !is GroupSpec) {
            return false
        }
        return memberThreshold == other.memberThreshold && memberCount == other.memberCount
    }

    override fun hashCode(): Int = 31 * memberThreshold + memberCount

    override fun toString(): String = "$memberThreshold-of-$memberCount"
}
