# Enumerable in deep

This article discusses in details about the enumerables in C#.

It based on french articles written [on this blog](http://www.yeg-projects.com/2015/01/soyons-enumerables-partie-1/) by Yan Grenier.

All code samples are in the project "Enumerable-en" in the solution.

# The enumerables

The purpose of the enumerables is to browse through a list of elements by retreiving element by element like a stream (named as iteration or enumeration). The most common usage of this principle is done with the `foreach` keyword.

In .Net the enumerables are based on the interfaces `System.Collections.IEnumerable` and `System.Collections.IEnumerator`. The generic version of these interfaces exist as `System.Collections.Generic.IEnumerable<>` and  `System.Collections.Generic.IEnumerator<>`.

When a class implements the interface `IEnumerable` then it becomes enumerable.

`IEnumerable` indicates than we can enumerate an objet that is responsible to provide an  `IEnumerator` each time we want to browse its elements.

`IEnumerator` is the "enumerator", that is the object that will be in charge of "Browsing" the elements to enumerate.

So to make a class as enumerable, we implements `IEnumerable` which will create an `IEnumerator` which will be responsible of the iteration of the elements.

Notice: all lists, collections, dictionnaries, and arrays in .Net, implement `IEnumerable`.

## IEnumerable/IEnumerable&lt;T&gt; interface

This interface indicates we can enumerate the object of the class that implements it. It provides only one method `GetEnumerator()` returning an `IEnumerator`.

```csharp
public interface IEnumerable
{
    IEnumerator GetEnumerator();
}
public interface IEnumerable<out T> : IEnumerable
{
    IEnumerator<T> GetEnumerator();
}
```

The purpose of the class is to provide a new enumerator object each time we need to start a new iteration, and permits to have several iterations in parallel on the same source (if the object logic permits it).


## IEnumerator/IEnumerator&lt;T&gt; interface 

This interface represents the iteration logic (enumerator object). This it wich indicates the current element and permits to "move" in the enumeration.


```csharp
public interface IEnumerator
{
    object Current { get; }
    bool MoveNext();
    void Reset();
}
public interface IEnumerator<out T> : IDisposable, IEnumerator
{
    T Current { get; }
}
```

The operation of the enumerator is simple: it indicates the current element of the enumeration (`Current` property), and moves to the next element with the `MoveNext()` method.

## Iteration basics

To navigate through an enumerable we use always the same principle:

- Get a new enumerator from `IEnumerable`
- While `IEnumerator.MoveNext()` returns `true`
	- Process `IEnumerator.Current`

This is what the statement `foreach` do for us.

So the next loop (`ForEach()` method in the sample program):

```csharp
// Get the enumerable
IEnumerable<Int32> enumerable = GetItems();
// Iterates each element in 'enumerable'
foreach (int elm in enumerable)
{
    // ..
}
```

is approximately compiled like this (`IterationBase()` method in the sample program):

```csharp
// Get the enumerable
IEnumerable<Int32> enumerable = GetItems();
// Get a new enumerator
IEnumerator<Int32> enumerator = enumerable.GetEnumerator();
// While the enumerator move
while (enumerator.MoveNext())
{
    Int32 elm = enumerator.Current;
    // ..
}
```

However this is not entirely exact, because the `foreach` loop handles the possibility than an enumerator be disposable (that implements `IDisposable`) (`ForEachDisposable()` method in the sample program show it), so in fact the the equivalent loop is more like this (`IterationBaseWithDispose()` method in the sample program) :

```csharp
// Get the enumerable
IEnumerable<Int32> enumerable = GetItems(true);
// Get a new enumerator
IEnumerator<Int32> enumerator = enumerable.GetEnumerator();
try
{
    // While the enumerator move
    while (enumerator.MoveNext())
    {
        Int32 elm = enumerator.Current;
        // ..
        Console.WriteLine(elm);
    }
}
finally
{
    // Check if the enumerator is disposable
    IDisposable disp = enumerator as IDisposable;
    // If true the we dispose it
    if (disp != null)
    {
        disp.Dispose();
    }
}
```

In this way that the loop terminates normally, or prematurely by a `break` statement, or by an exception, our enumerator will be disposed.

It must be understood that implement 'IDisposable' on our enumerator is the only way we have to determine that an iteration is finished, especially if it has not browsed all the elements.

Small aside: in fact `foreach` do not supports only the `IEnumerable`. What the compiler needs the `foreach` statement, is that the source to enumerate provides a `GetEnumerator()` public method that returns a type that provides a `MoveNext()` public method returning a boolean, and a `Current` property. In the sample program you can find the `ForEachWithoutIEnumerable()` method using the `FakeEnumerable` and `FakeEnumerator` objects that does not implement the interfaces.

## Les énumérateurs

To make an enumerator we need to implements `IEnumerator`. However before you implement one, if you need iterate a private array (or another IEnumerable) for example, just return the array  `GetNumerator()` method result.


L'implémentation est assez simple au final, ce qui importe c'est qu'à sa création l'énumérateur est dans un état "indéfini", c'est à dire qu'on ne s'est pas encore déplacé, donc `Current` doit se trouver avec une valeur par défaut, et on doit attendre le premier `MoveNext()` pour commencer vraiment notre itération. Ce qui implique que si vous avez des initialisations à faire (se connecter à une base de données par exemple) vous devez le faire lors du premier `MoveNext()` pour des raisons d'optimisation; on peut avoir besoin d'instancier un énumérateur sans pour autant le parcourir, ce qui peut être le cas quand on enchaîne plusieurs énumérables comme LINQ (cf prochaine partie de cet article).


~~~ TO TRANSLATE



Imaginons que nous voulons créer un énumérateur qui parcours une liste à l'envers. La méthode `TestReverse()` du programme d'exemple montre l'utilisation de notre classe énumérateur.

```csharp
/// <summary>
/// Enumérateur parcourant une liste dans le sens inverse
/// </summary>
public class ReverseEnumerator<T> : IEnumerator<T>
{
    IList<T> _Source;
    int _Position;
    bool _Completed;
 
    /// <summary>
    /// Création d'un nouvel énumérateur
    /// </summary>
    public ReverseEnumerator(IList<T> source)
    {
        this._Source = source;
        // On met -1 pour indiquer qu'on a pas commencé l'itération
        this._Position = -1;
        // L'itération n'est pas terminée
        this._Completed = false;
        // On défini Current avec la valeur par défaut
        this.Current = default(T);
    }
 
    /// <summary>
    /// Libération des ressources
    /// </summary>
    public void Dispose()
    {
        // On a rien à libérer , mais on marque notre itérateur comme terminé
        this._Completed = true;
    }
 
    /// <summary>
    /// Cette méthode est appelée lorsque l'on veut réinitialiser l'énumérateur
    /// </summary>
    public void Reset()
    {
        // On met -1 pour indiquer qu'on a pas commencer l'itération
        this._Position = -1;
        // L'itération n'est pas terminée
        this._Completed = false;
        // On défini Current avec la valeur par défaut
        this.Current = default(T);
    }
 
    /// <summary>
    /// On se déplace vers le prochain élément
    /// </summary>
    /// <returns>False lorsque l'itération est terminée</returns>
    public bool MoveNext()
    {
        // Si la source est Null alors on a rien à parcourir, donc l'itération s'arrête
        if (this._Source == null) return false;
 
        // Si l'itération est terminée alors on ne va pas plus loin
        if (this._Completed) return false;
 
        // Si la position est à -1 on récupère le nombre d'éléments à parcourir pour démarrer l'itération
        if (this._Position == -1)
        {
            this._Position = _Source.Count;
        }
 
        // On se déplace dans la liste
        this._Position--;
 
        // Si on a atteind -1 alors on a terminé l'itération
        if (this._Position < 0)
        {
            this._Completed = true;
            return false;
        }
 
        // On défini Current et on continue
        Current = this._Source[this._Position];
 
        return true;
    }
 
    /// <summary>
    /// Elément en cours de l'itération
    /// </summary>
    public T Current { get; private set; }
 
    /// <summary>
    /// Elément en cours pour la version non générique
    /// </summary>
    object System.Collections.IEnumerator.Current
    {
        get { return Current; }
    }
 
}
```

Comme on peut le voir le principe de l'implémentation est assez simple.

Toutefois ça peut vite se compliquer quand nous avons des énumérateurs complexes (parcours d'un arbre syntaxique par exemple). Nous verrons au chapitre suivant comment on peut se simplifier la vie.

Dernier point pour cette partie : `IEnumerator` demande l'implémentation de `Reset()`. Il faut savoir que cette méthode n'est pas vraiment utilisée, la plupart du temps elle lève une exception `NotImplementedException`. Donc si son implémentation est trop complexe, n'ayez pas peur de faire de même. En fait on part du principe que si on veut redémarrer une itération on fait à nouveau appel à `IEnumerable.GetEnumerator()` qui va nous instancier un nouvel énumérateur, donc le reset n'a plus lieu d'être.

# Les méthodes Yield

Précédemment, certains d'entre vous ont dû se dire qu'à chaque fois que l'on veut mettre en place un énumérateur un peu particulier, il y a beaucoup de code à mettre place.

Effectivement on peut vite se retrouver avec des énumérateurs complexes, avec interception d'erreurs, libération de ressources multiples, etc., ce qui peut rapidement compliquer les choses.

C'est là que le compilateur C# va venir à notre rescousse via le mot clé <code>yield</code> (VB.NET supporte également une instruction `Yield`).

Bien imaginons par exemple que nous voulons un énumérable auquel nous transmettons une liste de fichiers, et cet énumérable va parcourir chaque fichier et énumérer chaque ligne de texte du fichier.

Cet exemple est intéressant car il inclus un gestion des erreurs (un fichier peut ne pas exister, ou être verrouillé, etc.) et une gestion des ressources multiples (chaque fichier doit être disposé).

## Version brut de code

Commençons par une version ressemblant typiquement à la solution que va prendre quelqu'un qui ne comprends pas les principes des énumérables ou tout simplement qui cherche à éviter la difficulté de faire un énumérateur correct.

Cette solution se contente de lire tous les fichiers dans une liste, et lorsqu'on à besoin d'un énumérateur on retourne celui de la liste. (La méthode `TestFilesV1()` dans le programme exemple montre son utilisation).

```csharp
/// <summary>
/// Enumérable parcourant les lignes texte d'un ensemble de fichier
/// </summary>
public class EnumFilesV1 : IEnumerable<String>
{
    private List<String> _Lines;
    /// <summary>
    /// Création d'un nouvel énumérable
    /// </summary>
    public EnumFilesV1(IEnumerable<String> files)
    {
        // Initialisation des fichiers
        this.Files = files.ToArray();
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

Alors qu'est-ce qui ne va pas avec ce code ?

La première chose c'est que si les fichiers venaient à changer de contenu entre deux appels de `GetEnumerator()` nous retournerons à chaque fois le contenu de la lecture initiale. On peut résoudre ce problème simplement en modifiant légèrement notre code pour reconstruire la liste des lignes à chaque appel.

```csharp
/// <summary>
/// Enumérable parcourant les lignes texte d'un ensemble de fichier reconstruisant une liste à chaque appel
/// </summary>
public class EnumFilesV2 : IEnumerable<String>
{
    /// <summary>
    /// Création d'un nouvel énumérable
    /// </summary>
    public EnumFilesV2(IEnumerable<String> files)
    {
        // Initialisation des fichiers
        this.Files = files.ToArray();
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

On supprime la liste des lignes de l'énumérable et on construit une nouvelle liste à chaque appel de `GetEnumerator()` et on retourne l'énumerateur de cette nouvelle liste. (La méthode <code>TestFilesV2()</code> dans le programme exemple montre son utilisation).

Cette fois nous respectons un peu mieux les principes des énumérables toutefois ce n'est toujours pas satisfaisant d'un point de vue des performances. En effet s'il s'avère que les fichiers sont volumineux, nous chargeons tout dans une liste en mémoire. Imaginons que nous nous servons de cet énumérable pour filtrer quelques lignes sur un million, nous pouvons engorger notre mémoire inutilement. Pire si on extrait que les milles premières lignes, nous aurons chargé 999000 lignes de trop :(

## Version plus subtile

L'idéal serait de ne lire une ligne de texte que quand on en a besoin. Bref en gros lors du `IEnumerator.MoveNext()`.

Nous allons donc faire preuve de subtilité et gérer la lecture en flux.

```csharp
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
```

Donc cette fois la partie lecture est défini dans un énumérateur. Il s'agit d'une simple machine à états : 'OpenNextFile', 'ReadNextFile' et 'Completed'.

On constate qu'il faut être vigilant à chaque endroit où une erreur peut survenir, ne pas oublier de libérer les ressources en fonction de différentes situations, etc.

En revanche nous lisons nos fichiers ligne par ligne, par conséquent la charge mémoire est à son minimum. En cas de dispose on libère le fichier ouvert, et on se place sur l'état 'Completed' pour que l'énumérateur ne puisse plus rien faire.

Pour gérer tout ce petit monde on a pas mal de ligne de code.

Et pour tester tout ca (méthode `TestFilesV3()`) on utilise le code suivant :

```csharp
private static void TestFilesV3()
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
```

Qui va parcourir une fois toutes les lignes de tous les fichiers, et une seconde fois mais uniquement les 4 premières lignes de la liste.

## 'yield' à notre secours

Donc nous avons deux situations :

- soit nous faisons un code assez court facilement maintenable, mais qui risque de nous poser des problèmes techniques.
- soit nous faisons un code plus performant, mais qui est plus complexe, long et difficile à maintenir.

 
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
private static void TestFilesYield()
{
    // Récupération d'un énumérable avec les fichiers de tests
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
```

`TestFilesYield()` est une méthode qui lance deux boucles d'un énumérable renvoyé par la méthode `EnumFilesYield()`.

C'est cette dernière qui nous intéresse, alors petite explication de code. On constate que cette méthode retourne un `IEnumerable<String>`, en revanche elle ne renvoie jamais d'énumérable, a la place on a dans la double boucle de lecture de fichier une instruction `yield return line;` ou `line` est une `string`.

Le `yield return` dans une méthode indique au compilateur que cette méthode est en fait un énumérateur et que chaque `yield return` renvoi un élément de cet énumérateur. Techniquement le compilateur va transformer cette méthode en un objet `IEnumerator` qui va simuler le code de la méthode.

En plus du `yield return` il existe le `yield break` qui arrête l'énumérateur.

Globalement le compilateur est capable de convertir la plupart du code en énumérateur, toutefois on ne peut pas utiliser `yield return` dans un try-catch (mais on le peut dans un try-finally). En revanche un `yield break` peut se trouver dans un try-catch mais pas un try-finally.

C'est pour ça que notre code est un peu plus compliqué qu'une simple double boucle pour prendre en compte d'éventuelles erreurs. Mais malgré celà il reste toujours plus court et facile à maintenir que notre énumérateur précédent.

L'énumérateur généré supporte le `IDisposable` par exemple dans notre cas si l'itération se termine en cours de lecture d'un fichier, comme il y a un `using`, le compilateur se chargera de créer le code nécessaire pour disposer la ressource se trouvant dans le `using`. De même que si dans une boucle `foreach` une exception est levée, et que l'on a un bloc `finally`qui englobe le `yield return` en cours alors ce bloc `finally` sera exécuté.

Le mot clé `yield` peut être utilisé dans une méthode qui retourne `IEnumerable` mais également `IEnumerator` ce qui nous permet d'implémenter `IEnumerable.GetEnumerator()` par une méthode `yield`.

Dernier point, l'énumérateur généré par le compilateur ne prend pas en charge `IEnumerator.Reset()` une exception `NotSupportedException` est levée.

Pour plus d'informations sur `yield` et les itérateurs, voici quelques liens :

- [Référence C# de yield](https://msdn.microsoft.com/fr-fr/library/9k7k7cf0.aspx)
- [Les itérateurs en C# et VB.net](https://msdn.microsoft.com/fr-fr/library/dscyy5s0.aspx)


## Pour en finir avec 'yield'

L'utilisation de `yield` est très pratique, elle permet de gérer des scénarios complexes avec un code 'classique'. La seule vraie difficulté réside dans la gestion des exceptions, qui peut compliquer notre code, mais de manière générale notre code reste toujours plus simple.

Le compilateur et le debuggeur de Visual Studio sont extrêment performants et vous permettent de faire du pas à pas dans l'énumérateur généré par `yield`, et ainsi de tracer exactement ce qu'il se passe dans l'énumérateur, même si le code dans la méthode yield est complexe, la trace suit parfaitement votre code.

Le programme d'exemple contient les différents exemples donnés, plus quelques méthodes `yield` suplémentaires pour montrer qu'on peut faire des choses complexes.

# LINQ et les énumérables à la chaîne

LINQ est un language de requêtage intégré au language C# ou VB.Net, le propos de cette partie n'est pas d'expliquer LINQ en lui-même mais son comportement avec les énumérables dans la suite des deux précédentes parties.

Pour plus d'informations sur LINQ je vous renvoie [à la documentation officielle](https://msdn.microsoft.com/fr-fr/library/bb397926.aspx).

## Rappel

Une requête LINQ ressemble à celà (méthode `TestLinqRequest()`) :

```csharp
var query = from p in GetPersons()
            where p.IsFamily
            orderby p.Name
            select p;
```

Cette requête n'est pas très compliquée : `GetPersons()` est une méthode qui retourne un `IEnumerable<Person>`, et notre requête en filtre les éléments dont le booléen `IsFamily` est à `true` et retourne le résultat trié par leur nom.

Si on regarde de plus près la variable `query` (dans VisualStudio il suffit de survoler le mot clé **var** pour afficher une info-bulle informant du type déterminé par le compilateur) on constate qu'il s'agit d'un `IEnumerable<Person>`.

En réalité il faut savoir que le compilateur va compiler cette requête sous la forme suivante (méthode `TestLinqRequestAsFluent()`) :

```csharp
var query = GetPersons()
            .Where(p => p.IsFamily)
            .OrderBy(p => p.Name)
    ;
```

LINQ fourni un ensemble de méthodes d'extensions (Select, Where, OrderBy, etc.) que l'on nomment "opérateur" car ils correspondent aux mot clés du language LINQ.

Si on regarde de plus près les opérateurs de notre requête on constate qu'ils renvoient tous un `IEnumerable<>`. En fait chaque appel de ces opérateurs va créer un objet "proxy" de type énumérable qui encapsule l'énumérable "source".

Ainsi notre variable `query` est en réalité une "chaîne" d'énumérables, elle contient un énumérable de type "OrderBy" qui se base sur un énumérable de type "Where" qui se base sur l'énumérable renvoyé par `GetPersons()`.

## Mais alors, quel est le problème ?

Voici une petite histoire vécue: un matin réception du mail suivant :

> Je possède une table de données extrêmement volatile où je peux avoir une centaine de lignes insérées par seconde.
> 
> Mon application affiche un tableau de bord avec des stats réactualisées régulièrement. Hors depuis que je suis en production LINQ calcul un nombre de lignes actualisées différents du nombre de lignes traitées, je ne comprends pas !
> 
> Voici mon code
> 
> ```csharp
> class Stats {
>   private DateTime lastRefresh;
>   ...
>   public void Refresh(){
>     var q = from stats in GetStats()
>       where stat.Date >= lastRefresh
>       orderby stat.Date
>       select stats;
>         
>       foreach (var stats in q)
>       {
>         // Affichage de la stat
>       }
>         
>       lblLastUpdate.Text = String.Format("{0} : {1} nouvelles statistiques", DateTime.Now, q.Count());
>       lastRefresh = DateTime.Now;
>   }
> }
> ```

La source de données est un contexte Entity Framework, qui gère en réalité des `IQueryable<T>` mais ces derniers sont des `IEnumerable<T>`.

Après discussion il s'avère que cette persone n'avait pas compris que sa requête `q` était un énumérable et que par conséquent chaque fois qu'il faisait une itération, un appel à GetEnumerator() était effectué et dans son cas une nouvelle requête SQL était exécutée. Hors l'opérateur `Count()` qu'il utilise pour déterminer le nombre de nouvelles statistiques ne renvoie pas un `IEnumerable` mais provoque une itération pour compter le nombre d'éléments. Seulement ses données étant tellement volatiles qu'entre les deux requêtes de nouvelles statistiques sont apparues.

Le problème est là : en fait certaines personnes pensent qu'une requête LINQ est une sorte de liste tampon, qu'une fois qu'on a lancé une requête le résultat reste en mémoire, donc faire un `Count()` derrière se contente de compter le résultat.

C'était la logique de cette personne, habituée à utiliser des principes de **Recordset** ou de **Dataset** pour les données, elle pensait qu'une requête LINQ était une sorte d'objet auto-alimenté contenant le résultat de sa requête.

## Penser 'flux'

Comme nous l'avons vu en début d'article, les énumérateurs sont une logique de "flux" ou on traite élément par élément. Dans le cadre d'une base de données chaque ligne étant retournée dés qu'elle est reçue du serveur.

Nous avons vu également qu'un énumérable n'est qu'un fournisseur d'énumérateur, tant que l'on ne provoque pas l'énumération (appel à <code>GetEnumerator()</code>) nous n'avons pas de flux.

Il a été également expliqué que LINQ était en fait une chaîne d'énumérable. Notre requête en début de cette partie pourrait ressembler à cela :

```csharp
tatic IEnumerable<Person> SimulWhere()
{
    foreach (var p in GetPersons())
    {
        if(p.IsFamily)
            yield return p;
    }
}
static IEnumerable<Person> SimulOrderBy()
{
    List<Person> result = new List<Person>();
    result.AddRange(SimulWhere());
    result.Sort((x, y) => String.Compare(x.Name, y.Name));
    foreach (var p in result)
        yield return p;
}
static void TestSimulLinq()
{
    var query = SimulOrderBy();
    foreach (var p in query)
        Console.WriteLine(p.Name);
    foreach (var p in query.Take(2))
        Console.WriteLine(p.Name);
}
```

Dans la méthode `TestSimulLinq()` vous pouvez faire un pas à pas pour voir que tant qu'on n'entre pas dans un `foreach`, `SimulOrderBy()` n'est pas appelée. En revanche dans notre exemple elle est appelée deux fois car on a deux `foreach`.

Donc il faut toujours penser nos requêtes en flux, qui se déclenchent à chaque fois que l'on provoque une itération.

Alors une question se pose: comment savoir qu'une itération est déclenchée avec les opérateurs LINQ ? En bien c'est simple, il suffit de regarder le type de retour de l'opérateur, s'il renvoi un `IEnumerable` il ne provoque pas d'itération, mais renvoi un nouvel objet énumérable avec son propre énumérateur pour appliquer son opération sur le flux.

Le langage LINQ intégré ne génère que des énumérables, par exemple `Count()` n'a pas d'équivalent LINQ, il faut utiliser la méthode, mais on peut coupler les deux :

```csharp
int count = (from p in GetPersons()
            where p.IsFamily
            orderby p.Name
            select p).Count();
```

# Conclusion

En espérant que cet article vous a apporté plus de clarté sur la logique des énumérables :

- que `IEnumerable` ne contient pas de données, mais sert uniquement a créer un nouveau flux d'élément à chaque `GetEnumerator()`
- qu'une requête LINQ n'est pas un ensemble de données mais un énumérable
- qu'à chaque fois que vous avez un énumérable, il faut penser flux, quelque soit l'implémentation de cet énumérable

# Voir Aussi

- [Article d'origine](http://www.yeg-projects.com/2015/01/soyons-enumerables-partie-1/)
- [MSDN: Référence C# de yield](https://msdn.microsoft.com/fr-fr/library/9k7k7cf0.aspx)
- [MSDN: Les itérateurs en C# et VB.net](https://msdn.microsoft.com/fr-fr/library/dscyy5s0.aspx)
- [MSDN: Documentation officielle de LINQ](https://msdn.microsoft.com/fr-fr/library/bb397926.aspx)
