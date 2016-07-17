using System;
using System.Collections.Generic;
using System.Linq;
using MK.Utilities;

namespace MK.MyMath
{
    public static class MyMath
    {
        public static IEnumerable<int> GetRandomNumbers(int numberOfRandomNumbers, int maxValue)
        {
            if (maxValue < numberOfRandomNumbers)
                numberOfRandomNumbers = maxValue;

            var mySet = new HashSet<int>();
            var r = new Random((int)DateTime.Now.Ticks);

            for (var i = 0; i < numberOfRandomNumbers; i++)
            {
                var randomNo = r.Next(maxValue);
                while (!mySet.Add(randomNo))
                    randomNo = r.Next(maxValue);

                yield return randomNo;
            }
        }

        public static IEnumerable<T> PickSomeInRandomOrder<T>(IEnumerable<T> elements, int maxCount)
        {
            var random = new Random(DateTime.Now.Millisecond);

            var dict = elements.ToDictionary(x => random.NextDouble(), x => x);

            return dict.OrderBy(kvp => kvp.Key).Take(maxCount).Select(kvp => kvp.Value);
        }

        public static IList<T> PickSomeInRandomOrderWithWeights<T>(IList<Tuple<double, T>> data, int maxCount)
        {
            if (maxCount >= data.Count)
                return data.Select(t => t.Item2).ToList();

            var result = new List<T>();
            var flag = false;

            if (maxCount > data.Count / 2)
            {
                maxCount = data.Count - maxCount;
                flag = true;
            }

            double sum = 0;
            data.ForEach(a => sum = sum + a.Item1);

            var rand = new Random((int)DateTime.Now.Ticks);

            while (result.Count < maxCount)
            {
                double weight = rand.NextDouble() * sum;
                double tempSum = 0;

                foreach (var e in data)
                {
                    tempSum += e.Item1;
                    if (tempSum > weight)
                    {
                        if (!result.Contains(e.Item2))
                            result.Add(e.Item2);

                        break;
                    }
                }
            }

            if (flag)
                return data.Where(t => !result.Contains(t.Item2)).Select(kvp => kvp.Item2).ToList();

            return result;
        }

        public static double CalculateDistance(VectorD vectorA, VectorD vectorB, DistanceMode mode)
        {
            if (vectorA.Count != vectorB.Count)
                throw new Exception("Both vectors should have the same number of elemetns in order to calculate a distance between them!");

            switch (mode)
            {
                case DistanceMode.Euclidean:
                    var sum = vectorA.Select((t, i) => Math.Pow(t - vectorB[i], 2.0)).Sum();
                    return Math.Sqrt(sum);

                case DistanceMode.Manhattan:
                    var sum2 = vectorA.Select((t, i) => Math.Abs(t - vectorB[i])).Sum();
                    return sum2;


                case DistanceMode.Max:
                    var max = vectorA.Select((t, i) => Math.Abs(t - vectorB[i])).Max();
                    return max;


                case DistanceMode.Min:
                    var min = vectorA.Select((t, i) => Math.Abs(t - vectorB[i])).Min();
                    return min;

                case DistanceMode.Levenshtein:
                    var l = LevenshteinDistance(vectorA, vectorB);
                    return l;

                default:
                    throw new Exception("Unknown distance mode!");
            }
        }

        /// <summary>
        /// http://www.codeproject.com/Articles/13525/Fast-memory-efficient-Levenshtein-algorithm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static int LevenshteinDistance<T>(IList<T> v1, IList<T> v2)
        {
            if (v1.Count == 0) 
                return v2.Count;

            if (v2.Count == 0) 
                return v1.Count;

            var dist1 = new int[v2.Count + 1];
            var dist2 = new int[v2.Count + 1];

            for (var i = 0; i < dist1.Length; i++)
                dist1[i] = i;

            for (var i = 0; i < v1.Count; i++)
            {
                dist2[0] = i + 1;

                for (var j = 0; j < v2.Count; j++)
                {
                    var cost = Equals(v1[i], v2[j]) ? 0 : 1;
                    dist2[j + 1] = Math.Min(dist2[j] + 1, Math.Min(dist1[j + 1] + 1, dist1[j] + cost));
                }

                for (var j = 0; j < dist1.Length; j++)
                    dist1[j] = dist2[j];
            }

            return dist2[v2.Count];
        }
    }
}
