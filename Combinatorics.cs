using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MathUtils
{
    public static class Combinatorics
    {
        /// <summary>
        /// Determines all k-samples with repetition from the elements of pool.
        /// This is all the results you get when drawing k times from a pool of n elements with repetition; 
        /// in total there are n^k possibilities.
        /// <para>
        /// Example: a = GetPermutationsWithRept({1, 2, 3, 4}, 2) 
        /// --> {{1,1} {1,2} {1,3} {1,4} {2,1} {2,2} {2,3} {2,4} {3,1} {3,2} {3,3} {3,4} {4,1} {4,2} {4,3} {4,4}}
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection pool</typeparam>
        /// <param name="pool">A duplicate-free collection</param>
        /// <param name="k">The length of the permutations</param>
        /// <returns>An enumeration of all k-samples with repetition from the elements of pool</returns>
        public static IEnumerable<IEnumerable<T>> GetPermutationsWithRept<T>(IEnumerable<T> pool, int k)
        {
            if (k < 1)
                throw new ArgumentException("Parameter must be a positive integer", nameof(k));
            if (k == 1)
                return pool.Select(t => new T[] { t });
            else
                return GetPermutationsWithRept(pool, k - 1)
                    .SelectMany(t => pool, (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// Determines all k-samples (without repetition) from the elements of pool.
        /// This is all the results you get when drawing k times from a pool of n elements without repetition; 
        /// in total there are n*(n-1)*...*(n-k+1) possibilities.
        /// <para>
        /// Example: a = GetPermutationsWithoutRept({1, 2, 3, 4}, 2) 
        /// --> {{1,2} {1,3} {1,4} {2,1} {2,3} {2,4} {3,1} {3,2} {3,4} {4,1} {4,2} {4,3}}
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection pool</typeparam>
        /// <param name="pool">A duplicate-free collection</param>
        /// <param name="k">The length of the permutations</param>
        /// <returns>An enumeration of all k-samples without repetition from the elements of pool</returns>
        public static IEnumerable<IEnumerable<T>> GetPermutationsWithoutRept<T>(IEnumerable<T> pool, int k)
        {
            if (k < 1)
                throw new ArgumentException("Parameter must be a positive integer", nameof(k));
            if (k == 1)
                return pool.Select(t => new T[] { t });
            else
                return GetPermutationsWithoutRept(pool, k - 1)
                    .SelectMany(t => pool.Where(l => !t.Contains(l)), (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// Determines all k-combinations with repetition from the elements of pool.
        /// This is all the different results you can get when drawing k times from a pool of n elements with repetition and arrange the items in order; 
        /// in total there are Binomial(n+k-1, k) possibilities.
        /// <para>
        /// Example: a = GetCombinationsWithRept({1, 2, 3, 4}, 2) 
        /// --> {{1,1} {1,2} {1,3} {1,4} {2,2} {2,3} {2,4} {3,3} {3,4} {4,4}}
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection pool</typeparam>
        /// <param name="pool">A duplicate-free collection</param>
        /// <param name="k">The length of the combinations</param>
        /// <returns>An enumeration of all k-combinations with repetition from the elements of pool</returns>
        public static IEnumerable<IEnumerable<T>> GetCombinationsWithRept<T>(IEnumerable<T> pool, int k) where T : IComparable
        {
            if (k < 1)
                throw new ArgumentException("Parameter must be a positive integer", nameof(k));
            if (k == 1)
                return pool.Select(t => new T[] { t });
            else
                return GetCombinationsWithRept(pool, k - 1)
                    .SelectMany(t => pool.Where(l => l.CompareTo(t.Last()) >= 0), (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        /// <summary>
        /// Determines all k-combinations without repetition from the elements of pool.
        /// This is all the different results you can get when drawing k times from a pool of n elements without repetition and arrange the items in order; 
        /// in total there are Binomial(n, k) possibilities.
        /// <para>
        /// Example: a = GetCombinationsWithoutRept({1, 2, 3, 4}, 2) 
        /// --> {{1,2} {1,3} {1,4} {2,3} {2,4} {3,4}}
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of elements in the collection pool</typeparam>
        /// <param name="pool">A duplicate-free collection</param>
        /// <param name="k">The length of the combinations</param>
        /// <returns>An enumeration of all k-combinations without repetition from the elements of pool</returns>
        public static IEnumerable<IEnumerable<T>> GetCombinationsWithoutRept<T>(IEnumerable<T> pool, int k) where T : IComparable
        {
            if (k < 1)
                throw new ArgumentException("Parameter must be a positive integer", nameof(k));
            if (k == 1)
                return pool.Select(t => new T[] { t });
            else
                return GetCombinationsWithoutRept(pool, k - 1)
                    .SelectMany(t => pool.Where(l => l.CompareTo(t.Last()) > 0), (t1, t2) => t1.Concat(new T[] { t2 }));
        }


        /// <summary>
        /// Rearranges the elements of an IList collection in-place at random.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection</typeparam>
        /// <param name="list">The IList instance to shuffle</param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            int k;
            while (n > 1)
            {
                n--;
                k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        /// <summary>
        /// Shuffles a subpart of the collection, beginning at a specified index.
        /// </summary>
        /// <typeparam name="T">The type of the elements in list</typeparam>
        /// <param name="list">An IList collection of elements</param>
        /// <param name="pos">The starting index of the subpart that should be shuffled</param>
        /// <returns>A randomized List of the elements of the selected subpart of list</returns>
        public static List<T> Shuffle<T>(this IList<T> list, int pos)
        {
            List<T> shuffledList = new List<T>(list.Skip(pos));
            shuffledList.Shuffle();
            return shuffledList;
        }

        /// <summary>
        /// Shuffles a subpart of the collection.
        /// </summary>
        /// <typeparam name="T">The type of the elements in list</typeparam>
        /// <param name="list">An IList collection of elements</param>
        /// <param name="pos">The starting index of the subpart that should be shuffled</param>
        /// <param name="count">The number of elements in the subpart</param>
        /// <returns>A randomized List of the elements of the selected subpart of list</returns>
        public static List<T> Shuffle<T>(this IList<T> list, int pos, int count)
        {
            if (pos < 0 || pos >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(pos), "starting position is out of range");
            if (pos + count >= list.Count)
                throw new ArgumentOutOfRangeException(nameof(count), "count lands out of range");
            List<T> shuffledList = new List<T>(list.Skip(pos).Take(count));
            shuffledList.Shuffle();
            return shuffledList;
        }


        /// <summary>
        /// Get a randomly drawn sample (with repeated elements allowed)
        /// </summary>
        /// <typeparam name="T">The type of the elements in pool</typeparam>
        /// <param name="pool">An IList collection of elements</param>
        /// <param name="k">The length of the sample</param>
        /// <returns>A randomly drawn (with repetition) k-sequence of the elements in pool</returns>
        public static List<T> DrawRandomSampleWithRept<T>(this IList<T> pool, int k)
        {
            int n = pool.Count;
            int ix;
            List<T> sample = new List<T>();
            for (int j = 0; j < k; j++)
            {
                ix = ThreadSafeRandom.ThisThreadsRandom.Next(n);
                sample.Add(pool[ix]);
            }
            return sample;
        }

        /// <summary>
        /// Get a randomly drawn sample (without repeated elements)
        /// </summary>
        /// <typeparam name="T">The type of the elements in pool</typeparam>
        /// <param name="pool">An IList collection of elements</param>
        /// <param name="k">The length of the sample</param>
        /// <returns>A randomly drawn k-sequence of the elements in pool (without repetitions)</returns>
        public static List<T> DrawRandomSampleWithoutRept<T>(this IList<T> pool, int k)
        {
            int n = pool.Count;
            if (k > n)
                throw new ArgumentException("Parameter must not exceed the pool count", nameof(k));
            List<T> sample = new List<T>();
            List<int> positions = Enumerable.Range(0, n).ToList();
            int ix;
            for (int j = 0; j < k; j++)
            {
                ix = ThreadSafeRandom.ThisThreadsRandom.Next(positions.Count);
                sample.Add(pool[positions[ix]]);
                positions.RemoveAt(ix);
            }
            return sample;
        }
    }

    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
}
