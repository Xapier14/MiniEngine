using System.Collections.Generic;
using System.Linq;

namespace MiniEngine
{
    // ReSharper disable once InconsistentNaming
    public static class IEnumerableExtensions
    {
        public static bool StartsWith(this IEnumerable<object> enumerable, IEnumerable<object> startsWith)
        {
            var data = enumerable.ToArray();
            var with = startsWith.ToArray();
            if (!with.Any())
                return true;
            return !with.Where((t, i) => data[i] != t).Any();
        }
    }
}
