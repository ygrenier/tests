# Yield : ou comment perdre quelques neurones

Alors à la fin de la première paartie, j'en ai déjà entendus qui ronchonnaient : "bon s'il faut qu'on se cogne un énumérateur à chaque fois qu'on veut un truc particulier, on est pas sorti des ronces". C'est pas faux (côtelette power ;) ) ! En même temps c'est notre boulot :)

Mais effectivement on peut vite se retrouver avec des énumérateurs compliqués un peu tordu à faire avec interception d'erreurs, libération de ressources multiples, etc., ce qui peut compliquer rapidement les choses. 

C'est là que le compilateur C# va venir à notre rescousse via le mot clé ```yield``` (VB.NET supporte également une instruction ```Yield```).

Bien imaginons par exemple que nous voulons un énumérable auquel nous transmettons une liste de fichiers, et cet énumérable va parcourir chaque fichier et énumérer chaque ligne de texte du fichier.

Cet exemple est intéressant car il inclus un gestion des erreurs (un fichier peut ne pas exister, ou être verrouillé, etc.) et une gestion des resources multiples (chaque fichier doit être disposé).

## Version barbare

Commençons par une version ressemblant à ce que je vois de temps en temps et qui m'a entre autre poussé à faire cette série d'articles. C'est typiquement la solution que va prendre quelqu'un qui ne comprends pas les principes des énumérables ou tout simplement qui fuit devant la difficulté de faire un énumérateur correct.

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

La première chose c'est que si les fichiers venaient à changer de contenu entre deux appels de ```GetEnumerator()``` nous retournerons à chaque fois le contenu de la lecture initiale. On peut résoudre ce problème simplement en modifiant légèrement notre code pour reconstruire la liste des lignes à chaque appel.


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

OK ça respecte mieux les principes des énumérables toutefois ce n'est toujours pas satisfaisant d'un point de vue des performances. En effet s'il s'avère que les fichiers sont volumineux, nous chargeons tout dans une liste en mémoire. Imaginons que nous nous servons de cet énumérable pour filtrer quelques lignes sur un million, vous pouvez mettre à genoux votre machine. Pire si on n'extrait que les milles premières lignes, nous aurons chargé 999000 lignes de trop :(

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

On constate qu'il faut être vigilant à endroit où une erreur peut survenir, ne pas oublier de libérer les ressources en fonction de différentes situations, etc.

En revanche nous lisons nos fichiers ligne par ligne, par conséquent la charge mémoire est à son minimum. En cas de dispose on libère le fichier ouvert, et on se se place sur l'état 'Completed' pour que l'énumérateur ne puisse plus rien faire.

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

```csharp

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

        private static void TestFichierYield()
        {
            // Récupération d'un énumérable avec les fichiers de tests
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

```

```TestFichierYield()``` est une méthode qui lance deux boucles d'un énumérable renvoyé par la méthode ```EnumFichierYield()```. 

C'est cette dernière qui nous intéresse, alors petite explication de texte. On constate que cette méthode retourne un ```IEnumerable<String>```, en revanche elle ne renvoie jamais d'énumérable, a la place on constate dans la double boucle de lecture de fichier une instruction ```yield return line;``` ou ```line``` est une ```string```.

Le ```yield return``` dans une méthode indique au compilateur que cette méthode est en fait un énumérateur et que chaque ```yield return``` renvoi un élément de cet énumérateur. Techniquement le compilateur va transformer cette méthode en un objet ```IEnumerator``` qui va simuler le code de la méthode.

En plus du ```yield return``` il existe le ```yield break``` qui arrête l'énumérateur.

Globalement le compilateur est capable de convertir la plupart du code en énumérateur, toutefois on ne peut pas utiliser ```yield return``` dans un try-catch (mais on le peut dans un try-finally). En revanche un ```yield break``` peut se trouver dans un try-catch mais pas un try-finally.

C'est pour ça que mon code est un peu plus compliqué qu'une simple double boucle pour prendre en compte d'éventuelles erreurs. Mais malgré celà il reste toujours plus court et facile à maintenir que notre énumérateur précédent.

L'énumérateur généré supporte le ```IDisposable``` par exemple dans notre cas si l'itération se termine en cours de lecture d'un fichier, comme il y a un using, le compilateur se chargera de créer le code nécessaire pour disposer la ressource se trouvant dans le using. De même que si dans une boucle ```foreach``` une exception est levée, et que l'on a un bloc **finally** qui englobe le ```yield return``` en cours alors ce bloc **finally** sera exécuté.

Le mot clé ```yield``` peut être utilisé dans une méthode qui retourne ```IEnumerable``` mais également ```IEnumerator``` ce qui nous permet d'implémenter ```IEnumerable.GetEnumerator()``` par une méthode ```yield```.

Dernier point, l'énumérateur généré par le compilateur ne prend pas en charge ```IEnumerator.Reset()``` une exception ```NotSupportedException``` est levée.

Pour plus d'informations sur ```yield``` et les itérateurs, voici quelques liens :
- [Référence C# de yield](https://msdn.microsoft.com/fr-fr/library/9k7k7cf0.aspx)
- [Les itérateurs en C# et VB.net](https://msdn.microsoft.com/fr-fr/library/dscyy5s0.aspx)

## Conclusion

L'utilisation de ```yield``` est très pratique, elle permet de gérer des scénarios complexes avec un code 'classique'. La seule vraie difficulté réside dans la gestion des exceptions, qui peut compliquer notre code, mais de manière générale notre code est toujours plus simple.

Le compilateur et le debuggeur de Visual Studio sont extrêment performants et vous permettent de faire du pas à pas dans l'énumérateur généré par ```yield```, et ainsi tracer exactement ce qu'il se passe dans l'énumérateur, même si la plomberie est complexe, la trace suit parfaitement votre code.

Le programme 'PrgPart2' contient les différents exemples donnés, plus quelques méthodes ```yield``` suplémentaires pour montrer qu'on peut faire des choses complexes.

Pour la dernière partie nous allons voir le comportement des énumérables avec LINQ, car là également il y a parfois un peu d'incompréhension.

