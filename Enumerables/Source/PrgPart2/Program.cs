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

        static IEnumerable<String> EnumFichierYield(String[] files)
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
                            // On envoi la ligne d'énumérable
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

        static void Main(string[] args)
        {
            //TestFichierBarbare();
            //TestFichierUnPeuMoinsBarbare();
            //TestFichierSubtile();
            TestFichierYield();

            Console.ReadLine();
        }

    }
}
