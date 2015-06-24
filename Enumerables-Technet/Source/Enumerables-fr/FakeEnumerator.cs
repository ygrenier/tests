using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Enumerables
{
    /// <summary>
    /// Faux énumérateur n'implémentant pas IEnumerator mais supporté par foreach
    /// </summary>
    /// <remarks>
    /// Doit implémenter une méthode publique MoveNext() et une propriété Current
    /// </remarks>
    class FakeEnumerator
    {
        List<int> _Source = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 });
        int _Current = -1;
        /// <summary>
        /// Item en cours
        /// </summary>
        public int Current { get; private set; }
        /// <summary>
        /// Déplacement vers le prochain item
        /// </summary>
        public bool MoveNext()
        {
            if (++_Current >= _Source.Count) return false;
            Current = _Source[_Current];
            return true;
        }
    }
}
