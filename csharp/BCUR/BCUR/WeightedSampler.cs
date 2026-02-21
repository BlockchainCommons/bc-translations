namespace BlockchainCommons.BCUR;

/// <summary>
/// Weighted random sampling using the alias method (Vose's algorithm).
/// Used internally for fountain code degree selection.
/// </summary>
internal sealed class WeightedSampler
{
    private readonly int[] _aliases;
    private readonly double[] _probs;

    internal WeightedSampler(double[] weights)
    {
        if (weights.Any(w => w < 0.0))
            throw new ArgumentException("negative probability encountered");

        var summed = weights.Sum();
        if (summed <= 0.0)
            throw new ArgumentException("probabilities don't sum to a positive value");

        var count = weights.Length;
        var scaled = new double[count];
        for (int i = 0; i < count; i++)
        {
            scaled[i] = weights[i] * count / summed;
        }

        var small = new Stack<int>();
        var large = new Stack<int>();
        for (int j = count - 1; j >= 0; j--)
        {
            if (scaled[j] < 1.0)
                small.Push(j);
            else
                large.Push(j);
        }

        _probs = new double[count];
        _aliases = new int[count];

        while (small.Count > 0 && large.Count > 0)
        {
            var a = small.Pop();
            var g = large.Pop();

            _probs[a] = scaled[a];
            _aliases[a] = g;
            scaled[g] += scaled[a] - 1.0;

            if (scaled[g] < 1.0)
                small.Push(g);
            else
                large.Push(g);
        }

        while (large.Count > 0)
        {
            _probs[large.Pop()] = 1.0;
        }

        while (small.Count > 0)
        {
            _probs[small.Pop()] = 1.0;
        }
    }

    internal int Next(Xoshiro256 rng)
    {
        var r1 = rng.NextDouble();
        var r2 = rng.NextDouble();
        var n = _probs.Length;
        var i = (int)((double)n * r1);
        return r2 < _probs[i] ? i : _aliases[i];
    }
}
