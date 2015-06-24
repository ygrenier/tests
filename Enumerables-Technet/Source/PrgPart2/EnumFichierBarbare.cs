using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrgPart2
{
    /// <summary>
    /// Enumérable parcourant les lignes texte d'un ensemble de fichier
    /// </summary>
    public class EnumFichierBarbare : IEnumerable<String>
    {

        private List<String> _Lines;

        /// <summary>
        /// Création d'un nouvel énumérable
        /// </summary>
        public EnumFichierBarbare(String[] files)
        {
            // Initialisation des fichiers
            this.Files = files;

            // On marque la liste des lignes à charger en la mettant à null
            _Lines = null;
        }

        void LoadFiles()
        {
            // Création de la liste des lignes
            _Lines = new List<string>();

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
                                _Lines.Add(line);
                            }
                        }
                    }
                    catch { }   // Si une erreur à la lecture du fichier on passe au prochain fichier
                }
            }
        }

        /// <summary>
        /// Retourne l'énumérateur des lignes
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            // Si la liste des lignes est null alors il faut lire les fichiers
            if (_Lines == null)
            {
                // Chargement des fichiers
                LoadFiles();
            }

            // Retourne l'énumérateur de la liste
            return _Lines.GetEnumerator();
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
