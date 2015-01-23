using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrgPart2
{
    /// <summary>
    /// Enumérable parcourant les lignes texte d'un ensemble de fichier reconstruisant une liste à chaque appel
    /// </summary>
    public class EnumFichierUnPeuMoinsBarbare : IEnumerable<String>
    {

        /// <summary>
        /// Création d'un nouvel énumérable
        /// </summary>
        public EnumFichierUnPeuMoinsBarbare(String[] files)
        {
            // Initialisation des fichiers
            this.Files = files;
        }

        IList<String> LoadFiles()
        {
            // Création de la liste des lignes
            var result = new List<string>();

            if (this.Files != null)
            {
                // Pour chaque fichier
                foreach (var file in Files)
                {
                    try
                    {
                        // Ouverture d'un lecteur de fichier texte
                        using (var reader = new StreamReader(file))
                        {
                            // Lecture de chaque ligne du fichier
                            String line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                // On ajoute la ligne dans la liste
                                result.Add(line);
                            }
                        }
                    }
                    catch { }   // Si une erreur à lecture du fichier on passe au prochain fichier
                }
            }

            // On retourne la liste
            return result;
        }

        /// <summary>
        /// Retourne l'énumérateur des lignes
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            // On construit la liste
            var list = LoadFiles();

            // Retourne l'énumérateur de la liste
            return list.GetEnumerator();
        }

        /// <summary>
        /// Implémentation de IEnumerator.GetEnumerator() (version non générique)
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Liste des fichiers
        /// </summary>
        public String[] Files { get; private set; }

    }

}
