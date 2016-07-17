using System;

namespace MK.MyMath
{
    [Flags]
    public enum DistanceMode
    {
        None = 0,
        Euclidean = 1,
        Manhattan = 2,
        Max = 4,
        Min = 8,
        Levenshtein = 16
    }
}
