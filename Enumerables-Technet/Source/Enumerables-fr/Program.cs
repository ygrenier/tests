using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// Liste des fichiers sources
        /// </summary>
        static IEnumerable<String> GetTestFileNames()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            return new String[]{
                Path.Combine(dir, "Files", "Fichier1.txt"),
                Path.Combine(dir, "Files", "Fichier2.txt"),
                Path.Combine(dir, "Files", "Fichier3.txt")
            };
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

        #region Exemple énumérateurs complexes

        /// <summary>
        /// Test de l'énumérateur de fichier première version
        /// </summary>
        static void TestFilesV1()
        {
            // Création de l'énumérable avec les fichiers de tests
            var enumerable = new EnumFilesV1(GetTestFileNames());

            // On parcours l'énumérable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

        }

        /// <summary>
        /// Test de l'énumérateur améliorant un peut la première version
        /// </summary>
        static void TestFilesV2()
        {
            // Création de l'énumérable avec les fichiers de tests
            var enumerable = new EnumFilesV2(GetTestFileNames());

            // On parcours l'énumérable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

        }

        /// <summary>
        /// Test de l'énumérateur de fichiers plus subtile
        /// </summary>
        static void TestFilesV3()
        {
            // Création de l'énumérable avec les fichiers de tests
            var enumerable = new EnumFilesV3(GetTestFileNames());

            // On parcours l'énumérable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

            // On parcours l'énumérable et provoque un arrêt prématuré
            int i = 0;
            foreach (var line in enumerable)
            {
                if (i++ >= 4) break;
                Console.WriteLine(line);
            }
        }
        
        #endregion

        #region Exemples méthode yield

        /// <summary>
        /// Ouvre un nouveau fichier ou retourne null si une erreur à lieu
        /// </summary>
        static StreamReader OpenFile(String file)
        {
            try
            {
                return new StreamReader(file);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Enumération des fichiers avec une méthode Yield
        /// </summary>
        static IEnumerable<String> EnumFilesYield(IEnumerable<String> files)
        {
            if (files != null)
            {
                // Pour chaque fichier
                foreach (var file in files)
                {
                    // Ouverture d'un lecteur de fichier texte
                    using (var reader = OpenFile(file))
                    {
                        // reader peut être null si une erreur à eu lieu
                        if (reader != null)
                        {
                            // Lecture de chaque ligne du fichier
                            String line;
                            do
                            {
                                // Lecture d'une ligne, si une erreur à lieu on arrête la boucle
                                try
                                {
                                    line = reader.ReadLine();
                                }
                                catch
                                {
                                    break;
                                }
                                // On envoi la ligne d'énumérable
                                if (line != null)
                                    yield return line;
                            } while (line != null);// Boucle tant qu'on a une ligne
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Test la méthode yield d'énumération
        /// </summary>
        static void TestFilesYield()
        {
            // Récupération d'un 'énumérable avec les fichiers de tests
            var enumerable = EnumFilesYield(GetTestFileNames());

            // On parcours l'énumérable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

            // On parcours l'énumérable et provoque un arrêt prématuré
            int i = 0;
            foreach (var line in enumerable)
            {
                if (i++ >= 4) break;
                Console.WriteLine(line);
            }

        }
        /// <summary>
        /// Même méthode que <see cref="EnumFilesYield"/> mais sans gestion des exception
        /// </summary>
        static IEnumerable<String> EnumFilesYieldUnprotected(IEnumerable<String> files)
        {
            if (files != null)
            {
                // Pour chaque fichier
                foreach (var file in files)
                {
                    // Ouverture d'un lecteur de fichier texte
                    using (var reader = new StreamReader(file))
                    {
                        // Lecture de chaque ligne du fichier
                        String line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // On ajoute la ligne dans la liste
                            yield return line;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Méthode yield complexe
        /// </summary>
        static IEnumerable<String> YieldComplex()
        {
            Random rnd = new Random();

            // Emission directe
            yield return "Start";

            // Emission dans un switch
            for (int i = 0; i < 10; i++)
            {
                switch (rnd.Next(4))
                {
                    case 1:
                        yield return "Funky 1";
                        break;
                    case 2:
                        continue;
                    case 3:
                        yield return "<<<<";
                        // Emission d'un autre énum
                        foreach (var line in EnumFilesYield(GetTestFileNames()))
                        {
                            // On place une condition
                            if (line.Contains("x"))
                                yield return line;
                        }
                        yield return ">>>>";
                        break;
                    case 0:
                    default:
                        yield return "Funky 0";
                        break;
                }
            }

            // Emission directe
            yield return "End";
        }

        /// <summary>
        /// Test la méthode yield complexe
        /// </summary>
        static void TestYieldComplex()
        {
            foreach (var item in YieldComplex())
                Console.WriteLine(item);
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

            #region Test énumérateurs complexes et méthode yield
            Console.WriteLine("* Enumérateur de fichier V1 (approche basique)");
            TestFilesV1();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Enumérateur de fichier V2 (version précédente améliorée)");
            TestFilesV2();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Enumérateur de fichier V3 (version avec une approche flux)");
            TestFilesV3();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Enumérateur de fichier avec une méthode Yield");
            TestFilesYield();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Méthode Yield complexe");
            TestYieldComplex();
            Console.WriteLine();
            WaitAndPress();
            #endregion

        }
    }
}
