using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Faux énumérable n'implémentant pas IEnumerable mais supporté par foreach
    /// </summary>
    class FakeEnumerable
    {
        /// <summary>
        /// foreach requierd uniquement une méthode publique GetEnumerator() qui retourne un objet qui contient une méthode publique
        /// MoveNext() et une propriété Current.
        /// </summary>
        public FakeEnumerator GetEnumerator()
        {
            return new FakeEnumerator();
        }
    }

}
