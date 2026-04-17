using System;

/// <summary>
/// Provides helper methods for working with <see cref="System.Random"/>
/// in gameplay systems.
/// </summary>
public static class RngUtility
{
    /// <summary>
    /// Returns a random integer in the range [min, max).
    /// </summary>
    public static int Range(System.Random rng, int min, int max)
    {
        if (rng == null) throw new ArgumentNullException(nameof(rng));
        if (min > max) throw new ArgumentException("min cannot be greater than max.");

        return rng.Next(min, max);
    }

    /// <summary>
    /// Returns a random float in the range [min, max).
    /// </summary>
    public static float Range(System.Random rng, float min, float max)
    {
        if (rng == null) throw new ArgumentNullException(nameof(rng));
        if (min > max) throw new ArgumentException("min cannot be greater than max.");

        return (float)(min + rng.NextDouble() * (max - min));
    }

    /// <summary>
    /// Performs a probability check using the provided random number generator.
    /// </summary>
    public static bool Chance(System.Random rng, float probability)
    {
        if (rng == null) throw new ArgumentNullException(nameof(rng));
        if (probability < 0f || probability > 1f)
            throw new ArgumentOutOfRangeException(nameof(probability), "Probability must be between 0 and 1.");

        return rng.NextDouble() < probability;
    }
}