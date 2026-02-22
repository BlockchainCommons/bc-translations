package com.blockchaincommons.sskr

/**
 * A specification for an SSKR split.
 *
 * @property groupThreshold The minimum number of groups required to reconstruct the secret.
 * @property groups The list of group specifications.
 * @throws SskrException if the threshold or group count is invalid.
 */
data class Spec(val groupThreshold: Int, val groups: List<GroupSpec>) {
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
    }

    /** The number of groups. */
    val groupCount: Int
        get() = groups.size

    /** The total number of shares across all groups. */
    val shareCount: Int
        get() = groups.sumOf { it.memberCount }
}

/**
 * A specification for a group of shares within an SSKR split.
 *
 * @property memberThreshold The minimum number of member shares required to recover this group.
 * @property memberCount The total number of member shares in this group.
 */
data class GroupSpec(val memberThreshold: Int = 1, val memberCount: Int = 1) {
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
    }

    override fun toString(): String = "$memberThreshold-of-$memberCount"

    companion object {
        /**
         * Parses a group specification from an `M-of-N` string (e.g. `"2-of-3"`).
         *
         * @param s The string to parse in `M-of-N` format.
         * @return A [GroupSpec] with the parsed threshold and count.
         * @throws SskrException.GroupSpecInvalid if the string is not valid `M-of-N` format.
         */
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
}
