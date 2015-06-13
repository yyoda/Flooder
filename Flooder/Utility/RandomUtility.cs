using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace Flooder.Utility
{
    public class WeightedSampleSeed<T>
    {
        public int ItemCount { get { return _items.Count; } }

        private readonly int _totalWeight;
        private readonly List<T> _items;
        private readonly List<int> _weightBounds;

        public WeightedSampleSeed(IEnumerable<T> source, Func<T, int> weightSelector)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (weightSelector == null) throw new ArgumentNullException("weightSelector");

            var enumerable = source as T[] ?? source.ToArray();
            if (!enumerable.Any()) throw new ArgumentException("source is empty");

            _items = new List<T>();
            _weightBounds = new List<int>();
            _totalWeight = 0;

            foreach (var x in enumerable)
            {
                var weight = weightSelector(x);
                if (weight <= 0)
                    continue;

                checked
                {
                    _totalWeight += weight;
                }

                _items.Add(x);
                _weightBounds.Add(_totalWeight);
            }
        }

        public T Sample()
        {
            var draw = RandomProvider.Random.Next(1, _totalWeight + 1);
            var index = _weightBounds.BinarySearch(draw);

            if (index < 0)
                index = ~index;

            return _items[index];
        }
    }

    public static class RandomProvider
    {
        private static readonly ThreadLocal<Random> ThreadLocalRandom = new ThreadLocal<Random>(() =>
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var buffer = new byte[sizeof(int)];
                rng.GetBytes(buffer);
                var seed = BitConverter.ToInt32(buffer, 0);
                return new Random(seed);
            }
        });

        public static Random Random
        {
            get { return ThreadLocalRandom.Value; }
        }

        public static int RandomNumberOf1To100
        {
            get { return Random.Next(1, 101); }
        }
    }
}
