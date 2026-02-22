package com.blockchaincommons.bcur

/**
 * Weighted random sampler using Vose's alias method.
 *
 * Allows O(1) sampling from a discrete probability distribution
 * after O(n) preprocessing.
 */
internal class WeightedSampler(weights: List<Double>) {
    private val aliases: IntArray
    private val probs: DoubleArray

    init {
        require(weights.none { it < 0.0 }) { "negative probability encountered" }
        val summed = weights.sum()
        require(summed > 0.0) { "probabilities don't sum to a positive value" }

        val count = weights.size
        val normalized = weights.map { it * count / summed }.toDoubleArray()

        probs = DoubleArray(count)
        aliases = IntArray(count)

        val small = mutableListOf<Int>()
        val large = mutableListOf<Int>()

        for (j in count - 1 downTo 0) {
            if (normalized[j] < 1.0) small.add(j) else large.add(j)
        }

        while (small.isNotEmpty() && large.isNotEmpty()) {
            val a = small.removeAt(small.lastIndex)
            val g = large.removeAt(large.lastIndex)
            probs[a] = normalized[a]
            aliases[a] = g
            normalized[g] += normalized[a] - 1.0
            if (normalized[g] < 1.0) small.add(g) else large.add(g)
        }

        while (large.isNotEmpty()) {
            probs[large.removeAt(large.lastIndex)] = 1.0
        }
        while (small.isNotEmpty()) {
            probs[small.removeAt(small.lastIndex)] = 1.0
        }
    }

    /** Draws a random sample index using the provided Xoshiro256 PRNG. */
    fun next(xoshiro: Xoshiro256): Int {
        val r1 = xoshiro.nextDouble()
        val r2 = xoshiro.nextDouble()
        val n = probs.size
        val i = (n * r1).toInt()
        return if (r2 < probs[i]) i else aliases[i]
    }
}
