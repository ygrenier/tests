# Yield : ou comment perdre quelques neurones

Alors à la fin de la première paartie, j'en ai déjà entendus qui ronchonnaient : "bon s'il faut qu'on se cogne un énumérateur à chaque fois qu'on veut un truc particulier, on est pas sorti des ronces". C'est pas faux (côtelette power ;) ) ! En même temps c'est notre boulot :)

Mais effectivement on peut vite se retrouver avec des énumérateurs compliqués un peu tordu à faire avec interception d'erreurs, libération de ressources multiples, etc., ce qui peut compliquer rapidement les choses. 

C'est là que le compilateur C# va venir à notre rescousse via le mot clé ```yield``` (VB.NET supporte également une instruction ```Yield```).

Bien imaginons par exemple que nous voulons un énumérable auquel nous transmettons une liste de fichiers, et cet énumérable va parcourir chaque fichier et énumérer chaque ligne de texte du fichier.

Cet exemple est très intéressant car il inclus un gestion des erreurs (un fichier peut ne pas exister, ou être verrouillé, etc.) et une gestion des resources multiples (chaque fichier doit être disposé).

## Version barbare

Commençons par une version ressemblant à ce que je vois de temps en temps et qui m'a entre autre poussé à faire cette série d'articles. C'est typiquement la solution que va prendre quelqu'un qui ne comprends pas les principes des énumérables ou tout simplement qui fui devant la difficulté de faire un énumérateur correcte.

Cette solution se contente de lire tous les fichiers dans une liste, et lorsqu'on à besoin d'un énumérateur on retourne celui de la liste.

```csharp

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


```

Voilà le genre de chose que je rencontre (et encore là je charge les lignes à la première demande d'énumérateur, souvent j'ai carrément le chargement des fichiers dans le constructeur).

Alors qu'est-ce qui me chagrine dans cet énumérable ?

La première chose c'est que si les fichiers venaient à changer de contenu entre deux appels de ```GetEnumerator()``` nous retournerons à chaque fois le contenu de la lecture. On peut résoudre ce problème simplement en modifiant légèrement notre code pour reconstruire la liste des lignes à chaque appel.


```csharp

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
                    catch { }   // Si une erreur à la lecture du fichier on passe au prochain fichier
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

```

On supprime la liste des lignes de l'énumérable et on construit une nouvelle liste à chaque appel de ```GetEnumerator()``` et on retourne l'énumerateur de cette nouvelle liste.

OK ça respecte mieux les principes des énumérables toutefois ce n'est toujours pas satisfaisant d'un point de vue des performances. En effet s'il s'avère que les fichiers sont volumineux, nous chargeons tout dans une liste en mémoire. Imaginons que nous nous servons de cet énumérable pour filtrer quelques lignes sur un million, vous pouvez mettre à genoux votre machine. Pire si on n'extrait que les milles premières lignes, nous aurons chargé un 999000 lignes de trop :(

## Version plus subtile

L'idéal serait de ne lire une ligne de texte que quand on en a besoin. Bref en gros lors du ```IEnumerator.MoveNext()```. 

Nous allons donc faire preuve de subtilité et gérer la lecture en flux.

```csharp

    /// <summary>
    /// Enumérable parcourant les lignes texte d'un ensemble de fichier via un énumérateur
    /// </summary>
    public class EnumFichierSubtile : IEnumerable<String>
    {

        /// <summary>
        /// Création d'un nouvel énumérable
        /// </summary>
        public EnumFichierSubtile(String[] files)
        {
            // Initialisation des fichiers
            this.Files = files;
        }

        /// <summary>
        /// Retourne l'énumérateur des lignes
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            // Retourne un nouvel énumérateur
            return new FichierEnumerator(Files);
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
    class FichierEnumerator : IEnumerator<String>
    {
        Func<bool> _CurrentState = null;
        int _CurrentFilePos;
        String _CurrentFileName;
        TextReader _CurrentFile;

        /// <summary>
        /// Création d'un nouvel énumérateur
        /// </summary>
        public FichierEnumerator(String[] files)
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


```

Donc cette fois la partie lecture est défini dans un énumérateur. Il s'agit d'une simple machine à états : 'OpenNextFile', 'ReadNextFile' et 'Completed'.

On constate qu'il être vigilant à endroit où une erreur peut survenir, ne pas oublier de libérer les ressources en fonction de différentes situations, etc.

En revanche nous lisant nos fichier lignes par lignes, par conséquent la charge mémoire est à son minimum. En cas de dispose on libère le fichier ouvert, et on se se place sur l'état 'Completed' pour que l'énumérateur ne puisse plus rien faire.

Pour gérer tout ce petit monde on a pas mal de ligne de code.

Et pour tester tout ca (programme 'PrgPart2') on utilise le code suivant :

```csharp

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

```

Qui va parcourir une fois toutes les lignes de tous les fichiers, et une seconde fois mais uniquement les 4 premières lignes de la liste.

## Et 'yield' est arrivéééééé

Ha enfin !!! On y arrive ! :)

Donc on a deux situations : 
- soit on fait un code assez court facilement maintenable, mais qui risque de nous poser des problèmes techniques.
- soit on fait un code plus performant, mais qui est plus complexe, long et difficile à maintenir.

Et si on pouvait concilier les deux ? 

Par exemple :

