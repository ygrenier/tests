using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Implémentation de IEnumerable permettant de parcourir une liste dans le sens inverse
    /// </summary>
    public class ReverseEnumerable<T> : IEnumerable<T>
    {

        /// <summary>
        /// Nouvelle classe énumérable
        /// </summary>
        public ReverseEnumerable(IList<T> source)
        {
            this.List = source;
        }

        /// <summary>
        /// Implémentation de IEnumerable&lt;T&gt;.GetEnumerator()
        /// </summary>
        /// <returns>Créé un nouveau ReverseEnumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new ReverseEnumerator<T>(List);
        }

        /// <summary>
        /// IEnumerable.GetEnumerator() (version non générique)
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Liste source
        /// </summary>
        public IList<T> List { get; private set; }

    }
}
