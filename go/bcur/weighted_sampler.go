package bcur

// weightedSampler implements Vose's alias method for O(1) weighted random sampling.
type weightedSampler struct {
	probs   []float64
	aliases []int
}

func newWeightedSampler(weights []float64) *weightedSampler {
	for _, w := range weights {
		if w < 0 {
			panic("negative probability encountered")
		}
	}

	summed := 0.0
	for _, w := range weights {
		summed += w
	}
	if summed <= 0.0 {
		panic("probabilities don't sum to a positive value")
	}

	count := len(weights)
	normalized := make([]float64, count)
	for i, w := range weights {
		normalized[i] = w * float64(count) / summed
	}

	small := make([]int, 0)
	large := make([]int, 0)
	for j := count - 1; j >= 0; j-- {
		if normalized[j] < 1.0 {
			small = append(small, j)
		} else {
			large = append(large, j)
		}
	}

	probs := make([]float64, count)
	aliases := make([]int, count)

	for len(small) > 0 && len(large) > 0 {
		a := small[len(small)-1]
		small = small[:len(small)-1]
		g := large[len(large)-1]
		large = large[:len(large)-1]

		probs[a] = normalized[a]
		aliases[a] = g
		normalized[g] += normalized[a] - 1.0

		if normalized[g] < 1.0 {
			small = append(small, g)
		} else {
			large = append(large, g)
		}
	}

	for len(large) > 0 {
		g := large[len(large)-1]
		large = large[:len(large)-1]
		probs[g] = 1.0
	}

	for len(small) > 0 {
		a := small[len(small)-1]
		small = small[:len(small)-1]
		probs[a] = 1.0
	}

	return &weightedSampler{probs: probs, aliases: aliases}
}

func (w *weightedSampler) next(rng *xoshiro256) int {
	r1 := rng.nextDouble()
	r2 := rng.nextDouble()
	n := len(w.probs)
	i := int(float64(n) * r1)
	if r2 < w.probs[i] {
		return i
	}
	return w.aliases[i]
}
