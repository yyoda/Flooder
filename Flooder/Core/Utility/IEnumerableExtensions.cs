using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Flooder.Core.Utility
{
    public static class EnumerableExtensions
    {
        private static int _shuffleSeed = new Random().Next();

        //Thread safe.
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rnd = null)
        {
            rnd = rnd ?? new Random(Interlocked.Increment(ref _shuffleSeed));
            return ShuffleCore(source, rnd);
        }

        //Thread unsafe. but fast.
        public static IEnumerable<T> ShuffleSlim<T>(this IEnumerable<T> source, Random rnd = null)
        {
            rnd = rnd ?? new Random(_shuffleSeed);
            return ShuffleCore(source, rnd);
        }

        private static IEnumerable<T> ShuffleCore<T>(IEnumerable<T> source, Random rnd)
        {
            var arr = source.ToArray();

            for (var i = arr.Length - 1; i >= 0; --i)
            {
                var j = rnd.Next(i + 1);
                yield return arr[j];
                arr[j] = arr[i];    // mini-swap
            }
        }
    }
}
