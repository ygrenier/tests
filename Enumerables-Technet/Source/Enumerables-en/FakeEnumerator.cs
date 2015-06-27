using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enumerables
{
    /// <summary>
    /// Fake enumerator that don't implements IEnumerator but supported by 'foreach'
    /// </summary>
    /// <remarks>
    /// Need to implements a public method MoveNext() and a public property Current
    /// </remarks>
    class FakeEnumerator
    {
        List<int> _Source = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        int _Current = -1;
        /// <summary>
        /// Current item 
        /// </summary>
        public int Current { get; private set; }
        /// <summary>
        /// Move to the next item
        /// </summary>
        public bool MoveNext()
        {
            if (++_Current >= _Source.Count) return false;
            Current = _Source[_Current];
            return true;
        }
    }
}
