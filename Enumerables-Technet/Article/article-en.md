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

## The enumerators

To make an enumerator we need to implements `IEnumerator`. However before you implement one, if you need iterate a private array (or another IEnumerable) for example, just return the array  `GetNumerator()` method result.

The implementation is simple, but when the enumerator is created, it is in en "undefined" state, it means that it is not yet moved, therefore `Current` must returns a default value, and we need wait the first `MoveNext()` to start the iteration. Which means that if you have some initializations to do (like connect to a daatabase for example) you do it when the first `MoveNext()` optimisation reasons; we may need to instanciate an enumerator without enumerate it, which it may be the case when you link several LINQ queries (see next part of the article).

Suppose you want to create an enumerator to iterate a list in the reverse order. The `TestReverse()`method in the sample program show the use of our enumerator class.

```csharp
/// <summary>
/// Enumerator iterates the list in the reverse way
/// </summary>
public class ReverseEnumerator<T> : IEnumerator<T>
{
    IList<T> _Source;
    int _Position;
    bool _Completed;
 
    /// <summary>
    /// Create a new enumerator
    /// </summary>
    public ReverseEnumerator(IList<T> source)
    {
        this._Source = source;
        // Set -1 to indicates the iteration is not started
        this._Position = -1;
        // The iteration is not finished
        this._Completed = false;
        // Set the Current value by default
        this.Current = default(T);
    }
 
    /// <summary>
    /// Release the resources
    /// </summary>
    public void Dispose()
    {
        // Nothing to dispose, but mark the iterator as finished
        this._Completed = true;
    }
 
    /// <summary>
    /// This method is called when we want to reset the enumerator
    /// </summary>
    public void Reset()
    {
        // Set -1 to indicates the iteration is not started
        this._Position = -1;
        // The iteration is not finished
        this._Completed = false;
        // Set the Current value by default
        this.Current = default(T);
    }
 
    /// <summary>
    /// We go to the next element
    /// </summary>
    /// <returns>False when the iteration is finished</returns>
    public bool MoveNext()
    {
        // If the source is null then we have nothing to browse, the iteration is finished
        if (this._Source == null) return false;
 
        // If the iteration is finished, we stop here
        if (this._Completed) return false;
 
        // If the is -1 we get the count of the elements to iterates for starting the iteration
        if (this._Position == -1)
        {
            this._Position = _Source.Count;
        }
 
        // We move on the list
        this._Position--;
 
        // If we reach the -1 position the iteration is finished
        if (this._Position < 0)
        {
            this._Completed = true;
            return false;
        }
 
        // We set Current and continue
        Current = this._Source[this._Position];
 
        return true;
    }
 
    /// <summary>
    /// Current element
    /// </summary>
    public T Current { get; private set; }
 
    /// <summary>
    /// Current element for the non generic version
    /// </summary>
    object System.Collections.IEnumerator.Current
    {
        get { return Current; }
    }
 
}
```

As we can see, implement enumerator is simple.

However it can quickly become complicated when we have complex enumerators (browse a syntaxic tree for example). We will see in next section how to simplify our code.

Last words for this part: `IEnumerator` require to implements the `Reset()` method. You need to this method is not really used, most of the time it throws a `NotImplementedException`. So if your implementation is too complex, you can throw this exception too instead of. In fact, now we consider that if we need to restart an iteration, we call again `IEnumerable.GetEnumerator()` to create a new enumerator, so the `Reset()`is not more useful.


# The Yield methods

Previously, we see how to implement an enumerator, but probably you think it could be complicated to create a class each time we need a special enumerator. Actually it can quickly end up with complex enumerators, with catching errors, release of multiple resources, etc., which can quickly complicate matters.

It is there the C# compiler come to our rescue with the `yield` keyword (VB.NET also supports a `Yield` statement).

For example we want an enumerable whereby we pass a list of files, and this enumerable iterate each file and read all the text lines of the file.

This example is interesting because it included an error handling (a file may not exists, or be locked, etc.) and a multiple resources management (each file must be disposed).

## Classic code version

Let's start by a typically version code, like someone don't know the enumerable principle, or don't want to create an enumerator.

This solution read all the files in a list, and when we want to enumerate them, we return the enumerator of the list (look the `TestFilesV1()` method in the sample program).

```csharp
/// <summary>
/// Enumerable enumerates the text lines of a set of files
/// </summary>
public class EnumFilesV1 : IEnumerable<String>
{
    private List<String> _Lines;
    /// <summary>
    /// Create a new enumerable
    /// </summary>
    public EnumFilesV1(IEnumerable<String> files)
    {
        // Init the files
        this.Files = files.ToArray();
        // Mark the lines list as 'to load' by setting it to null
        _Lines = null;
    }
    void LoadFiles()
    {
        // Create the lines list
        _Lines = new List<string>();
        if (this.Files != null)
        {
            // For each file
            foreach (var file in Files)
            {
                try
                {
                    // Open a file text reader
                    using (var reader = new StreamReader(file))
                    {
                        // Read each line of the file
                        String line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Add the line to the list
                            _Lines.Add(line);
                        }
                    }
                }
                catch { }   // When an error raised while reading the file, go to the next
            }
        }
    }
    /// <summary>
    /// Returns the lines enumerator
    /// </summary>
    public IEnumerator<string> GetEnumerator()
    {
        // If the lines list is null then read the files
        if (_Lines == null)
        {
            // Load the files
            LoadFiles();
        }
        // Returns the list enumerator
        return _Lines.GetEnumerator();
    }
    /// <summary>
    /// Implements IEnumerator.GetEnumerator() (non generic version)
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    /// <summary>
    /// List of the files
    /// </summary>
    public String[] Files { get; private set; }
}
```

So what is wrong with this code ?

First, if some files have content changing between to calls of `GetEnumerator()` we return only the initial read. We can resolve this by changing our code by rebuild the text lines list on each call.

```csharp
/// <summary>
/// Enumerable enumerates the text lines of a set of files, rebuilding the list on each call
/// </summary>
public class EnumFilesV2 : IEnumerable<String>
{
    /// <summary>
    /// Create a new enumerable
    /// </summary>
    public EnumFilesV2(IEnumerable<String> files)
    {
        // Init the files
        this.Files = files.ToArray();
    }
    IList<String> LoadFiles()
    {
        // Create the lines list
        var result = new List<string>();
        if (this.Files != null)
        {
            // For each file
            foreach (var file in Files)
            {
                try
                {
                    // Open a file text reader
                    using (var reader = new StreamReader(file))
                    {
                        // Read each line of the file
                        String line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Add the line of the list
                            result.Add(line);
                        }
                    }
                }
                catch { }   // When an error raised while reading the file, go to the next fichier
            }
        }
        // Returns the list
        return result;
    }
    /// <summary>
    /// Returns the enumerator of the lines
    /// </summary>
    public IEnumerator<string> GetEnumerator()
    {
        // Build the list
        var list = LoadFiles();
        // Return the list enumerator
        return list.GetEnumerator();
    }
    /// <summary>
    /// Implements IEnumerator.GetEnumerator() (non generic version)
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    /// <summary>
    /// List of the files
    /// </summary>
    public String[] Files { get; private set; }
}
```

We remove the list of text lines from the enumerable, and we build a new list on each call of `GetEnumerator()` and we return the enumerator of this new list (the `TestFilesV2()` method in the sample program shows the usage).

Now we are in best respect of the enumerable principles, however is still not satisfy from a point of view performances. If we have some large files, we loading all lines in memory. If we have one million of lines and need to extract only the first thousand, we have loaded 999000 lines in memory for nothing :(

## More optimized version

The best solution would be to read a text line only when it's needed. So each time we wall  `IEnumerator.MoveNext()`. We will read as streaming.

```csharp
/// <summary>
/// Enumerable enumerates the text lines of a set of files via an enumerator
/// </summary>
public class EnumFilesV3 : IEnumerable<String>
{
    /// <summary>
    /// Create a new enumerable
    /// </summary>
    public EnumFilesV3(IEnumerable<String> files)
    {
        // Init the files
        this.Files = files.ToArray();
    }
    /// <summary>
    /// Returns the lines enumerator
    /// </summary>
    public IEnumerator<string> GetEnumerator()
    {
        // Returns a new enumerator 
        return new FileEnumerator(Files);
    }
    /// <summary>
    /// Implements IEnumerator.GetEnumerator() (non generic version)
    /// </summary>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    /// <summary>
    /// List of the files
    /// </summary>
    public String[] Files { get; private set; }
}
/// <summary>
/// File enumerator
/// </summary>
class FileEnumerator : IEnumerator<String>
{
    Func<bool> _CurrentState = null;
    int _CurrentFilePos;
    String _CurrentFileName;
    TextReader _CurrentFile;
    /// <summary>
    /// Create a new enumerator
    /// </summary>
    public FileEnumerator(String[] files)
    {
        // Init the files
        this.Files = files;
        // Init the enumerator
        Current = null;
        _CurrentFilePos = 0;
        _CurrentFileName = null;
        _CurrentFile = null;
        // The enumerator state is to open the next file to read
        _CurrentState = OpenNextFileState;
    }
    /// <summary>
    /// Dispose some resources
    /// </summary>
    public void Dispose()
    {
        // If we have a file opened we close it
        if (_CurrentFile != null)
        {
            _CurrentFile.Dispose();
            _CurrentFile = null;
        }
        // Set the state to 'Completed'
        _CurrentState = CompletedState;
    }
    /// <summary>
    /// Try to open the next file in the list
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
        // If we have a file opened
        if (file != null)
        {
            _CurrentFileName = filename;
            _CurrentFile = file;
            return true;
        }
        // Else we don't found
        return false;
    }
    /// <summary>
    /// Open the next file
    /// </summary>
    bool OpenNextFileState()
    {
        // If we don't have file, we stop all
        if (!GetNextFile())
        {
            Current = null;
            // Go to the state 'Completed'
            _CurrentState = CompletedState;
            // We finished
            return false;
        }
        // Go to the state ReadNextLine
        _CurrentState = ReadNextLineState;
        // Read the first line
        return _CurrentState();
    }
    /// <summary>
    /// Read line state
    /// </summary>
    bool ReadNextLineState()
    {
        try
        {
            // Read the next line in the file
            String line = _CurrentFile.ReadLine();
            // If the line is not null we process it
            if (line != null)
            {
                Current = line;
                return true;
            }
            // The line is null so we reach the end of the file, we release the resource
        }
        catch
        {
            // If an error raised while reading we close the current file to avoid infinite loops
        }
        // Release resources to go to the next file
        _CurrentFile.Dispose();
        _CurrentFile = null;
        _CurrentFileName = null;
        // Go to the state 'OpenNextFile'
        _CurrentState = OpenNextFileState;
        // Process the next state
        return _CurrentState();
    }
    /// <summary>
    /// The iteration is finished, so we returns always false
    /// </summary>
    bool CompletedState()
    {
        return false;
    }
    /// <summary>
    /// We don"t used this method
    /// </summary>
    public void Reset()
    {
        throw new NotSupportedException();
    }
    /// <summary>
    /// We move to the next line
    /// </summary>
    public bool MoveNext()
    {
        // Process the next state
        return _CurrentState();
    }
    /// <summary>
    /// List of the files
    /// </summary>
    public String[] Files { get; private set; }
    /// <summary>
    /// Current value
    /// </summary>
    public string Current { get; private set; }
    object System.Collections.IEnumerator.Current { get { return Current; } }
}
```

Now the reading part is defined in a enumerator. It a simple state machine: 'OpenNextFile', 'ReadNextFile' et 'Completed'.

We can see we need to be vigilant on every location where an error can occur, and don't forget to release the resources according differents situations, etc.

We read our files text line by text line, so our memory is on a minimal usage. In case of dispose we release the opened file, and we set the state to 'Completed' and the enumerator can't do nothing.

We do lot of code for a little work, and for test it, you can look the `TestFilesV3()` method in the sample program: 

```csharp
private static void TestFilesV3()
{
    // Create the enumerable with tests files
    var enumerable = new EnumFilesV3(GetTestFileNames());
    // Iterates the enumerable
    foreach (var line in enumerable)
    {
        Console.WriteLine(line);
    }
    // Iterates the enumerable and force a break
    int i = 0;
    foreach (var line in enumerable)
    {
        if (i++ >= 4) break;
        Console.WriteLine(line);
    }
}
```

This example read all the lines in a first loop, and in a second loop it read only the first 4 lines.

## 'yield' in our rescue

So we have to situations:

- either we create a small code, easily maintainable, but with some performances risks.
- either we create a more efficient code, but wich is more complex, long and sometimes difficult to maintain.

And if we can reconcile the both ? 

For example:

```csharp
/// <summary>
/// Open a new file or null if an error raised
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
        // For each files
        foreach (var file in files)
        {
            // Open a file text reader
            using (var reader = OpenFile(file))
            {
                // reader can be null if an error raised
                if (reader != null)
                {
                    // Read each line of the file
                    String line;
                    do
                    {
                        // Read the line, if an error raised we stop the loop
                        try
                        {
                            line = reader.ReadLine();
                        }
                        catch
                        {
                            break;
                        }
                        // Returns the line in the 'enumerable'
                        if (line != null)
                            yield return line;
                    } while (line != null);// Loop while we have a line
                }
            }
        }
    }
}
private static void TestFilesYield()
{
    // Get an enumerable with the files test
    var enumerable = EnumFilesYield(GetTestFileNames());
    // Iterates the enumerable
    foreach (var line in enumerable)
    {
        Console.WriteLine(line);
    }
    // Iterates the enumerable and force a break
    int i = 0;
    foreach (var line in enumerable)
    {
        if (i++ >= 4) break;
        Console.WriteLine(line);
    }
}
```

`TestFilesYield()` is a method which run two loops for a enumerable returns par the `EnumFilesYield()` method.

It this last method that interest us, so little explanation of code. This method returns an `IEnumerable<String>`, however it never returns an enumerable, instead of we have in the two loops a statement `yield return line;` where `line` is a `string`.

The `yield return` in the method indicates to the compiler that method is an enumerator and each `yield return` returns an element of this enumerator. Technically the compiler will transforms this method in a `IEnumerator` object wich simulates the method code.

In addition to the `yield return` there is a `yield break` that stop the enumerator.

Overall the compiler can convert the most code to an enumerator, however we can't use `yield return` in a try-catch (but it's possible in a try-finally). And a `yield break` can be used in a try-catch but not in a try-finally.

It's for that our code is more complex than a simple double loop to handle possible errors. But despite this there is still more short and easy to maintain than our previous enumerator.

The generated enumerator supports `IDisposable`, for example in our case if the iteration is stopped when reading a file, and because there is a `using`, the compiler create the code dispose the resource in the `using`. Same as if in a `foreach` an exception is raised, and the `yield return` is within a `try-finally` this block will be executed.

The `yield` keyword can be used in a method that returns an `IEnumerable` or an `IEnumerator`, permitting to implements the `IEnumerable.GetEnumerator()` with a `yield` method.

Last point; the enumerator generated by the compiler don't implements the `IEnumerator.Reset()`, an `NotSupportedException` will be raised.

For more informations about the `yield` and enumerable, look this links :

- [yield : C# reference](https://msdn.microsoft.com/en-us/library/9k7k7cf0.aspx)
- [Iterators (C# and VB.Net)](https://msdn.microsoft.com/en-us/library/dscyy5s0.aspx)

## The last word about 'yield'

The keyword `yield` is very useful, it permits to handle some complex scenarios with a 'classic' code. The real difficulty is about the exceptions management, wich can complicate our code, but in general our code is always mor simple the create an enumerator.

The Visual Studio compiler and debugger are very efficients by permitting to debug step-by-step within the generated enumerator by `yield`, and so tracing exactly what happens in the enumerator, event if the code in the yield method is complex, the trace follow exactly the code.  

The sample program provides the examples discussed in this part, and more other `yield` methods to show some complex things.

# LINQ and the chaining enumerables



~~~ TO TRANSLATE


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
