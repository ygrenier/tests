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
        /// Retourne une liste d'éléments en tant qu'enumérable
        /// </summary>
        /// <param name="asDisposable">Indique si on retourne un énumérable avec énumérateur disposable</param>
        static IEnumerable<int> GetItems(bool asDisposable = false)
        {
            var items = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            return asDisposable ? new EnumerableWithEnumeratorDisposable<int>(items, () => Console.WriteLine("Enumérateur Disposé")).AsEnumerable() : items;
        }

        #region Exemple boucles

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

        /// <summary>
        /// Exemple foreach avec des énumérateur disposable
        /// </summary>
        static void ForEachDisposable()
        {
            // Récupération de l'énumérable
            IEnumerable<Int32> enumerable = GetItems(true);

            // Parcours chaque élément dans 'enumerable'
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
            IEnumerable<Int32> enumerable = GetItems(true);

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
        /// Exemple de foreach avec un objet n'implémentant pas IEnumerable/IEnumerator
        /// </summary>
        static void ForEachWithoutIEnumerable()
        {
            // Création du faux énumérable
            var enumerable = new FakeEnumerable();

            // Parcours chaque élément dans 'enumerable'
            foreach (int elm in enumerable)
            {
                // ..
                Console.WriteLine(elm);
            }
        }

        /// <summary>
        /// Test ReverseEnumerator
        /// </summary>
        static void TestReverse()
        {
            // ReverseEnumerator attend un IList<>
            IList<Int32> list = new List<Int32>(GetItems());

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

        #endregion

        static void WaitAndPress()
        {
            Console.WriteLine("Appuyer sur une touche pour continuer...");
            Console.ReadKey();
            Console.WriteLine();
        }
        static void Main(string[] args)
        {
            #region Exemple boucles
            Console.WriteLine("* Boucle ForEach");
            ForEach();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Boucle ForEach avec un Disposable");
            ForEachDisposable();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Itération de base (simulation ForEach)");
            IterationBase();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Itération de base avec un disposable (simulation ForEach)");
            IterationBaseWithDispose();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Boucle ForEach avec un objet non IEnumerable");
            ForEachWithoutIEnumerable();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Test ReverseEnumerator");
            TestReverse();
            Console.WriteLine();
            WaitAndPress();
            #endregion

        }
    }
}
