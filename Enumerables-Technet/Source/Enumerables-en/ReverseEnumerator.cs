using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{

    /// <summary>
    /// Enumerator iterates the list in the reverse way
    /// </summary>
    public class ReverseEnumerator<T> : IEnumerator<T>
    {
        IList<T> _Source;
        int _Position;
        bool _Completed;

        /// <summary>
        /// Create a new enumerator
        /// </summary>
        public ReverseEnumerator(IList<T> source)
        {
            this._Source = source;
            // Set -1 to indicates the iteration is not started
            this._Position = -1;
            // The iteration is not finished
            this._Completed = false;
            // Set the Current value by default
            this.Current = default(T);
        }

        /// <summary>
        /// Release the resources
        /// </summary>
        public void Dispose()
        {
            // Nothing to dispose, but mark the iterator as finished
            this._Completed = true;
            Console.WriteLine("Enumerator disposed");
        }

        /// <summary>
        /// This method is called when we want to reset the enumerator
        /// </summary>
        public void Reset()
        {
            // Set -1 to indicates the iteration is not started
            this._Position = -1;
            // The iteration is not finished
            this._Completed = false;
            // Set the Current value by default
            this.Current = default(T);
        }

        /// <summary>
        /// We go to the next element
        /// </summary>
        /// <returns>False when the iteration is finished</returns>
        public bool MoveNext()
        {
            // If the source is null then we have nothing to browse, the iteration is finished
            if (this._Source == null) return false;

            // If the iteration is finished, we stop here
            if (this._Completed) return false;

            // If the is -1 we get the count of the elements to iterates for starting the iteration
            if (this._Position == -1)
            {
                Console.WriteLine("Iteration started");
                this._Position = _Source.Count;
            }

            // We move on the list
            this._Position--;

            // If we reach the -1 position the iteration is finished
            if (this._Position < 0)
            {
                this._Completed = true;
                Console.WriteLine("Iteration finished");
                return false;
            }

            // We set Current and continue
            Current = this._Source[this._Position];

            return true;
        }

        /// <summary>
        /// Current element
        /// </summary>
        public T Current { get; private set; }

        /// <summary>
        /// Current element for the non generic version
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

    }

}
