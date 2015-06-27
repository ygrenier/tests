using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Disposable enumerator
    /// </summary>
    class EnumeratorDisposable<T> : IEnumerator<T>, IDisposable
    {
        List<T> _Source;
        int _Current = -1;

        /// <summary>
        /// Create a new enumerator
        /// </summary>
        /// <param name="source">Data source to enumerate</param>
        /// <param name="onDispose">Action invoked when disposed</param>
        public EnumeratorDisposable(IEnumerable<T> source, Action onDispose)
        {
            this._Source = new List<T>(source);
            this.OnDispose = onDispose;
        }
        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            if (OnDispose != null)
                OnDispose();
        }
        /// <summary>
        /// Current item
        /// </summary>
        public T Current { get; private set; }
        object System.Collections.IEnumerator.Current { get { return Current; } }
        /// <summary>
        /// Move to the next item
        /// </summary>
        public bool MoveNext()
        {
            if (++_Current >= _Source.Count) return false;
            Current = _Source[_Current];
            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Method invoked when the enumerator was disposed
        /// </summary>
        public Action OnDispose { get; set; }
    }

}
