using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Enumerable class using disposable enumerators
    /// </summary>
    class EnumerableWithEnumeratorDisposable<T> : IEnumerable<T>
    {
        List<T> _Source;

        /// <summary>
        /// Create a new enumerable
        /// </summary>
        /// <param name="source">Data source</param>
        /// <param name="onDispose">Action invoked when an enumerator was disposed</param>
        public EnumerableWithEnumeratorDisposable(IEnumerable<T> source, Action onDispose = null)
        {
            _Source = new List<T>(source);
            OnDispose = onDispose;
        }

        /// <summary>
        /// Return a disposable enumerator
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return new EnumeratorDisposable<T>(_Source, OnDispose);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Action invoked when an enumerator was disposed
        /// </summary>
        public Action OnDispose { get; set; }

    }

}
