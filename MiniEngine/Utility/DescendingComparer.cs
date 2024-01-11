using System.Collections.Generic;

namespace MiniEngine.Utility
{
    internal class DescendingComparer<T> : IComparer<T>
    {
        public int Compare(T? x, T? y)
        {
            if (x == null)
                return -1;
            if (y == null)
                return 1;
            return Comparer<T>.Default.Compare(y, x);
        }
    }
}
