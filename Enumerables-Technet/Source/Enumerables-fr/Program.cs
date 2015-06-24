using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    class Program
    {

        /// <summary>
        /// Retourne une liste d'éléments
        /// </summary>
        static int[] GetItems()
        {
            return new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        }

        /// <summary>
        /// Exemple foreach
        /// </summary>
        static void ForEach()
        {
            // Récupération de l'énumérable
            IEnumerable<Int32> enumerable = GetItems();

            // Parcours chaque élément dans 'enumerable'
            foreach (int elm in enumerable)
            {
                // ..
                Console.WriteLine(elm);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("* Exemple ForEach");
            ForEach();
            Console.WriteLine();

            Console.ReadKey();
        }
    }
}
