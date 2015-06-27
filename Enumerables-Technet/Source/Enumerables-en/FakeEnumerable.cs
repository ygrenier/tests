using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Fake enumerable that don't implements IEnumerable but supported by 'foreach'
    /// </summary>
    class FakeEnumerable
    {
        /// <summary>
        /// foreach require the class have only a public method GetEnumerator() returning an object with a public method
        /// MoveNext() and e public property Current.
        /// </summary>
        public FakeEnumerator GetEnumerator()
        {
            return new FakeEnumerator();
        }
    }

}
