using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Classe énumérable utilisant des énumérateurs disposable
    /// </summary>
    class EnumerableWithEnumeratorDisposable<T> : IEnumerable<T>
    {
        List<T> _Source;

        /// <summary>
        /// Création d'un nouvel énumérable
        /// </summary>
        /// <param name="source">Source d'initialisation</param>
        /// <param name="onDispose">Action invoquée lorsqu'un énumérateur est disposé</param>
        public EnumerableWithEnumeratorDisposable(IEnumerable<T> source, Action onDispose = null)
        {
            _Source = new List<T>(source);
            OnDispose = onDispose;
        }

        /// <summary>
        /// Retourne un énumérateur disposable
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
        /// Action invoquée lorsqu'un énumérateur est disposé
        /// </summary>
        public Action OnDispose { get; set; }

    }

}
