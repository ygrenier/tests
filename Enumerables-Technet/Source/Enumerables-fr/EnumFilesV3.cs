using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Enumérable parcourant les lignes texte d'un ensemble de fichier via un énumérateur
    /// </summary>
    public class EnumFilesV3 : IEnumerable<String>
    {

        /// <summary>
        /// Création d'un nouvel énumérable
        /// </summary>
        public EnumFilesV3(IEnumerable<String> files)
        {
            // Initialisation des fichiers
            this.Files = files.ToArray();
        }

        /// <summary>
        /// Retourne l'énumérateur des lignes
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            // Retourne un nouvel énumérateur
            return new FileEnumerator(Files);
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

    /// <summary>
    /// Enumérateur de fichier
    /// </summary>
    class FileEnumerator : IEnumerator<String>
    {
        Func<bool> _CurrentState = null;
        int _CurrentFilePos;
        String _CurrentFileName;
        TextReader _CurrentFile;

        /// <summary>
        /// Création d'un nouvel énumérateur
        /// </summary>
        public FileEnumerator(String[] files)
        {
            // Initialisation des fichiers
            this.Files = files;
            // Initialisation de l'énumérateur
            Current = null;
            _CurrentFilePos = 0;
            _CurrentFileName = null;
            _CurrentFile = null;
            // L'état de l'énumérateur est à l'ouverture du prochain fichier à traiter
            _CurrentState = OpenNextFileState;
        }

        /// <summary>
        /// Libération des ressources éventuelles
        /// </summary>
        public void Dispose()
        {
            // Si on a un fichier encore d'ouvert on libère la mémoire
            if (_CurrentFile != null)
            {
                _CurrentFile.Dispose();
                _CurrentFile = null;
            }
            // On défini l'état 'Completed'
            _CurrentState = CompletedState;
        }

        /// <summary>
        /// Essayes d'ouvrir le prochain fichier de la liste
        /// </summary>
        bool GetNextFile()
        {
            String filename = null;
            TextReader file = null;
            while (file == null && Files != null && _CurrentFilePos < Files.Length)
            {
                try
                {
                    filename = Files[_CurrentFilePos++];
                    file = new StreamReader(filename);
                }
                catch { }
            }
            // Si on a un fichier d'ouvert
            if (file != null)
            {
                _CurrentFileName = filename;
                _CurrentFile = file;
                return true;
            }
            // Sinon on a rien trouvé
            return false;
        }

        /// <summary>
        /// Ouverture du prochain fichier
        /// </summary>
        bool OpenNextFileState()
        {
            // Si on a pas ou plus de fichier on arrête tout
            if (!GetNextFile())
            {
                Current = null;
                // On passe à l'état 'Completed'
                _CurrentState = CompletedState;
                // On termine
                return false;
            }

            // On passe à l'état ReadNextLine
            _CurrentState = ReadNextLineState;

            // On lit la première ligne
            return _CurrentState();
        }

        /// <summary>
        /// On est en cours de lecture
        /// </summary>
        bool ReadNextLineState()
        {
            try
            {
                // On lit la prochaine ligne du fichier
                String line = _CurrentFile.ReadLine();
                // Si la ligne n'est pas null on la traite
                if (line != null)
                {
                    Current = line;
                    return true;
                }
                // La ligne est null alors on a atteint la fin du fichier, on libère sa ressource
            }
            catch
            {
                // Si une erreur survient à la lecture on ferme le fichier en cours pour éviter les boucles infinies

            }
            // Libération des ressources pour passer au fichier suivant
            _CurrentFile.Dispose();
            _CurrentFile = null;
            _CurrentFileName = null;
            // On passe à l'état 'OpenNextFile'
            _CurrentState = OpenNextFileState;
            // On traite le prochain état
            return _CurrentState();
        }

        /// <summary>
        /// L'itération est terminée on retourne toujours false
        /// </summary>
        bool CompletedState()
        {
            return false;
        }

        /// <summary>
        /// On ne s'occupe pas de cette méthode
        /// </summary>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// On se déplace
        /// </summary>
        public bool MoveNext()
        {
            // Exécution de l'état en cours
            return _CurrentState();
        }

        /// <summary>
        /// Liste des fichiers
        /// </summary>
        public String[] Files { get; private set; }

        /// <summary>
        /// Valeur en cours
        /// </summary>
        public string Current { get; private set; }
        object System.Collections.IEnumerator.Current { get { return Current; } }

    }

}
