using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrgPart1
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
            IEnumerable <Int32> enumerable = GetItems();

            // Parcours chaque élément dans elm
            foreach (int elm in enumerable)
            {
                // ..
                Console.WriteLine(elm);
            }
        }

        /// <summary>
        /// Exemple d'itération de base
        /// </summary>
        static void IterationBase()
        {
            // Récupération de l'énumérable
            IEnumerable<Int32> enumerable = GetItems();

            // Récupère un nouvel énumérateur
            IEnumerator<Int32> enumerator = enumerable.GetEnumerator();
            // Tant que l'énumérateur se déplace
            while (enumerator.MoveNext())
            {
                Int32 elm = enumerator.Current;
                // ..
                Console.WriteLine(elm);
            }
        }

        /// <summary>
        /// Exemple d'itération avec gestion du IDisposable tel que le fait un foreach
        /// </summary>
        static void IterationBaseWithDispose()
        {
            // Récupération de l'énumérable
            IEnumerable<Int32> enumerable = GetItems();

            // Récupère un nouvel énumérateur
            IEnumerator<Int32> enumerator = enumerable.GetEnumerator();
            try
            {
                // Tant que l'énumérateur se déplace
                while (enumerator.MoveNext())
                {
                    Int32 elm = enumerator.Current;
                    // ..
                    Console.WriteLine(elm);
                }
            }
            finally
            {
                // On détermine si l'énumérateur est disposable
                IDisposable disp = enumerator as IDisposable;
                // Si c'est le cas on dispose l'énumérateur
                if (disp != null)
                {
                    disp.Dispose();
                }
            }
        }

        /// <summary>
        /// Test ReverseEnumerator
        /// </summary>
        static void TestReverse()
        {
            // ReverseEnumerator attend un IList<>, aussi on récupère le tableau comme IList.
            // Tous les tableaux .Net sont des IList<> donc il n'y a pas de problème de cast
            IList<Int32> list = GetItems();

            // Création de l'énumerable reverse
            IEnumerable<Int32> enumerable = new ReverseEnumerable<Int32>(list);

            // Parcours chaque élément dans elm
            foreach (int elm in enumerable)
            {
                // ..
                Console.WriteLine(elm);
            }

            // On parcours chaque élément mais en arrêtant la boucle quand on trouve l'élément 5
            foreach (int elm in enumerable)
            {
                if (elm == 5) break;
                // ..
                Console.WriteLine(elm);
            }

        }

        static void Main(string[] args)
        {
            //ForEach();
            //IterationBase();
            //IterationBaseWithDispose();
            TestReverse();

            Console.ReadLine();
        }

    }
}
