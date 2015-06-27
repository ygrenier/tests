using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// IEnumerable implementation to read a list in the reverse way
    /// </summary>
    public class ReverseEnumerable<T> : IEnumerable<T>
    {

        /// <summary>
        /// New enumerable class
        /// </summary>
        public ReverseEnumerable(IList<T> source)
        {
            this.List = source;
        }

        /// <summary>
        /// IEnumerable&lt;T&gt;.GetEnumerator() implementation
        /// </summary>
        /// <returns>Create a new ReverseEnumerator</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new ReverseEnumerator<T>(List);
        }

        /// <summary>
        /// IEnumerable.GetEnumerator() (non generic version)
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Source list
        /// </summary>
        public IList<T> List { get; private set; }

    }
}
