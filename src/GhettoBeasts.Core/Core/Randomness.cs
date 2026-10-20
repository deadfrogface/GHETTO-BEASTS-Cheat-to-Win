using System;

namespace GhettoBeasts.Core
{
    public interface IRandom
    {
        /// <summary>Returns a value in [0, 1).</summary>
        double NextDouble();
    }

    public sealed class SystemRandom : IRandom
    {
        private readonly Random random;

        public SystemRandom(int? seed = null)
        {
            random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public double NextDouble() => random.NextDouble();
    }

    public static class RandomExtensions
    {
        public static bool Chance(this IRandom random, double percent) => random.NextDouble() * 100.0 < percent;

        public static int Range(this IRandom random, int minInclusive, int maxExclusive) =>
            minInclusive + (int)Math.Floor(random.NextDouble() * (maxExclusive - minInclusive));
    }
}
