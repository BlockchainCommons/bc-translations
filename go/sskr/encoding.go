package sskr

import (
	bcrand "github.com/nickel-blockchaincommons/bcrand-go"
	bcshamir "github.com/nickel-blockchaincommons/bcshamir-go"
)

// SSKRGenerate generates SSKR shares for the given spec and master secret.
func SSKRGenerate(spec *Spec, masterSecret *Secret) ([][][]byte, error) {
	rng := bcrand.NewSecureRandomNumberGenerator()
	return SSKRGenerateUsing(spec, masterSecret, rng)
}

// SSKRGenerateUsing generates SSKR shares using a supplied random generator.
func SSKRGenerateUsing(
	spec *Spec,
	masterSecret *Secret,
	randomGenerator bcrand.RandomNumberGenerator,
) ([][][]byte, error) {
	groupsShares, err := generateShares(spec, masterSecret, randomGenerator)
	if err != nil {
		return nil, err
	}

	result := make([][][]byte, len(groupsShares))
	for i, group := range groupsShares {
		serialized := make([][]byte, len(group))
		for j, share := range group {
			serialized[j] = serializeShare(share)
		}
		result[i] = serialized
	}

	return result, nil
}

// SSKRCombine combines SSKR shares into the original secret.
func SSKRCombine(shares [][]byte) (Secret, error) {
	sskrShares := make([]sskrShare, 0, len(shares))
	for _, share := range shares {
		deserialized, err := deserializeShare(share)
		if err != nil {
			return Secret{}, err
		}
		sskrShares = append(sskrShares, deserialized)
	}
	return combineShares(sskrShares)
}

func serializeShare(share sskrShare) []byte {
	// pack the id, group and member data into 5 bytes:
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

	result := make([]byte, 0, share.value.Len()+MetadataSizeBytes)
	id := share.identifier
	gt := (share.groupThreshold - 1) & 0xf
	gc := (share.groupCount - 1) & 0xf
	gi := share.groupIndex & 0xf
	mt := (share.memberThreshold - 1) & 0xf
	mi := share.memberIndex & 0xf

	result = append(result,
		byte(id>>8),
		byte(id&0xff),
		byte((gt<<4)|gc),
		byte((gi<<4)|mt),
		byte(mi),
	)
	result = append(result, share.value.bytes()...)

	return result
}

func deserializeShare(source []byte) (sskrShare, error) {
	if len(source) < MetadataSizeBytes {
		return sskrShare{}, ErrShareLengthInvalid
	}

	groupThreshold := int((source[2] >> 4) + 1)
	groupCount := int((source[2] & 0xf) + 1)
	if groupThreshold > groupCount {
		return sskrShare{}, ErrGroupThresholdInvalid
	}

	identifier := (uint16(source[0]) << 8) | uint16(source[1])
	groupIndex := int(source[3] >> 4)
	memberThreshold := int((source[3] & 0xf) + 1)
	reserved := source[4] >> 4
	if reserved != 0 {
		return sskrShare{}, ErrShareReservedBitsInvalid
	}
	memberIndex := int(source[4] & 0xf)

	value, err := NewSecret(source[MetadataSizeBytes:])
	if err != nil {
		return sskrShare{}, err
	}

	return newSSKRShare(
		identifier,
		groupIndex,
		groupThreshold,
		groupCount,
		memberIndex,
		memberThreshold,
		value,
	), nil
}

func generateShares(
	spec *Spec,
	masterSecret *Secret,
	randomGenerator bcrand.RandomNumberGenerator,
) ([][]sskrShare, error) {
	identifierBytes := [2]byte{}
	randomGenerator.FillRandomData(identifierBytes[:])
	identifier := (uint16(identifierBytes[0]) << 8) | uint16(identifierBytes[1])

	groupSecrets, err := bcshamir.SplitSecret(
		spec.GroupThreshold(),
		spec.GroupCount(),
		masterSecret.bytes(),
		randomGenerator,
	)
	if err != nil {
		return nil, wrapShamirError(err)
	}

	groupsShares := make([][]sskrShare, 0, spec.GroupCount())
	for groupIndex, group := range spec.Groups() {
		groupSecret := groupSecrets[groupIndex]
		memberSecretsRaw, err := bcshamir.SplitSecret(
			group.MemberThreshold(),
			group.MemberCount(),
			groupSecret,
			randomGenerator,
		)
		if err != nil {
			return nil, wrapShamirError(err)
		}

		memberShares := make([]sskrShare, 0, len(memberSecretsRaw))
		for memberIndex, memberSecretRaw := range memberSecretsRaw {
			memberSecret, err := NewSecret(memberSecretRaw)
			if err != nil {
				return nil, err
			}
			memberShares = append(memberShares, newSSKRShare(
				identifier,
				groupIndex,
				spec.GroupThreshold(),
				spec.GroupCount(),
				memberIndex,
				group.MemberThreshold(),
				memberSecret,
			))
		}
		groupsShares = append(groupsShares, memberShares)
	}

	return groupsShares, nil
}

type recoveryGroup struct {
	groupIndex      int
	memberThreshold int
	memberIndexes   []int
	memberShares    []Secret
}

func newRecoveryGroup(groupIndex int, memberThreshold int) recoveryGroup {
	return recoveryGroup{
		groupIndex:      groupIndex,
		memberThreshold: memberThreshold,
		memberIndexes:   make([]int, 0, 16),
		memberShares:    make([]Secret, 0, 16),
	}
}

func combineShares(shares []sskrShare) (Secret, error) {
	if len(shares) == 0 {
		return Secret{}, ErrSharesEmpty
	}

	identifier := uint16(0)
	groupThreshold := 0
	groupCount := 0
	secretLen := 0

	groups := make([]recoveryGroup, 0, 16)

	for i, share := range shares {
		if i == 0 {
			identifier = share.identifier
			groupCount = share.groupCount
			groupThreshold = share.groupThreshold
			secretLen = share.value.Len()
		} else {
			if share.identifier != identifier ||
				share.groupThreshold != groupThreshold ||
				share.groupCount != groupCount ||
				share.value.Len() != secretLen {
				return Secret{}, ErrShareSetInvalid
			}
		}

		groupFound := false
		for j := range groups {
			group := &groups[j]
			if share.groupIndex == group.groupIndex {
				groupFound = true
				if share.memberThreshold != group.memberThreshold {
					return Secret{}, ErrMemberThresholdInvalid
				}
				for _, index := range group.memberIndexes {
					if share.memberIndex == index {
						return Secret{}, ErrDuplicateMemberIndex
					}
				}
				if len(group.memberIndexes) < group.memberThreshold {
					group.memberIndexes = append(group.memberIndexes, share.memberIndex)
					group.memberShares = append(group.memberShares, share.value.Clone())
				}
			}
		}

		if !groupFound {
			group := newRecoveryGroup(share.groupIndex, share.memberThreshold)
			group.memberIndexes = append(group.memberIndexes, share.memberIndex)
			group.memberShares = append(group.memberShares, share.value.Clone())
			groups = append(groups, group)
		}
	}

	if len(groups) < groupThreshold {
		return Secret{}, ErrNotEnoughGroups
	}

	masterIndexes := make([]int, 0, 16)
	masterShares := make([][]byte, 0, 16)

	for _, group := range groups {
		if len(group.memberIndexes) < group.memberThreshold {
			continue
		}

		memberShares := make([][]byte, len(group.memberShares))
		for i := range group.memberShares {
			memberShares[i] = group.memberShares[i].bytes()
		}

		groupSecret, err := bcshamir.RecoverSecret(group.memberIndexes, memberShares)
		if err == nil {
			masterIndexes = append(masterIndexes, group.groupIndex)
			masterShares = append(masterShares, groupSecret)
		}

		if len(masterIndexes) == groupThreshold {
			break
		}
	}

	if len(masterIndexes) < groupThreshold {
		return Secret{}, ErrNotEnoughGroups
	}

	masterSecretRaw, err := bcshamir.RecoverSecret(masterIndexes, masterShares)
	if err != nil {
		return Secret{}, wrapShamirError(err)
	}

	masterSecret, err := NewSecret(masterSecretRaw)
	if err != nil {
		return Secret{}, err
	}
	return masterSecret, nil
}
