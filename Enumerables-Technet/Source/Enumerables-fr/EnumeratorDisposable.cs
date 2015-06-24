using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Enumérateur disposable
    /// </summary>
    class EnumeratorDisposable<T> : IEnumerator<T>, IDisposable
    {
        List<T> _Source;
        int _Current = -1;

        /// <summary>
        /// Création d'un nouvel énuméraeur
        /// </summary>
        /// <param name="source">Source à énumérer</param>
        /// <param name="onDispose">Action invoqué lors du Dispose</param>
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
        /// Item en cours
        /// </summary>
        public T Current { get; private set; }
        object System.Collections.IEnumerator.Current { get { return Current; } }
        /// <summary>
        /// Déplacement vers le prochain item
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
        /// Méthode invoquée lorsque l'énnumérateur est disposé
        /// </summary>
        public Action OnDispose { get; set; }
    }

}
