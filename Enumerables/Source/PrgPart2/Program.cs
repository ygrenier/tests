using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrgPart2
{
    class Program
    {

        static String[] GetTestFileNames()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            return new String[]{
                Path.Combine(dir, "Files", "Fichier1.txt"),
                Path.Combine(dir, "Files", "Fichier2.txt"),
                Path.Combine(dir, "Files", "Fichier3.txt")
            };
        }

        private static void TestFichierBarbare()
        {
            // Création de l'énumérable avec les fichiers de tests
            var enumerable = new EnumFichierBarbare(GetTestFileNames());

            // On parcours l'énumérable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

        }

        private static void TestFichierUnPeuMoinsBarbare()
        {
            // Création de l'énumérable avec les fichiers de tests
            var enumerable = new EnumFichierUnPeuMoinsBarbare(GetTestFileNames());

            // On parcours l'énumérable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

        }

        private static void TestFichierSubtile()
        {
            // Création de l'énumérable avec les fichiers de tests
            var enumerable = new EnumFichierSubtile(GetTestFileNames());

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

        static IEnumerable<String> EnumFichierYield(String[] files)
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

        static IEnumerable<String> EnumFichierYieldUnprotected(String[] files)
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

        private static void TestFichierYield()
        {
            // Récupération d'un 'énumérable avec les fichiers de tests
            var enumerable = EnumFichierYield(GetTestFileNames());

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

        static IEnumerable<String> EnumComplex()
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
                        foreach (var line in EnumFichierYield(GetTestFileNames()))
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

        static void TestEnumComplex()
        {
            foreach (var item in EnumComplex())
                Console.WriteLine(item);
        }

        static void Main(string[] args)
        {
            //TestFichierBarbare();
            //TestFichierUnPeuMoinsBarbare();
            //TestFichierSubtile();
            //TestFichierYield();
            TestEnumComplex();

            Console.ReadLine();
        }

    }
}
