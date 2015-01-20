Ces derniers temps je me suis battu à expliquer le fonctionnement des énumérables en .Net (en C# plus précisément) à plusieurs personnes et notamment des conséquences sur des librairies comme LINQ.

Alors je n'ai pas la prétention de tout maîtriser sur le sujet, mais il y a quelques bases qui, me sembles-t-il, devrait tout de même être connues. 

Aussi au lieu de m'énerver un peu à force de répéter les mêmes choses, j'ai décidé de prendre un peu de temps pour faire un récapitulatif de ce que j'en sais. Et ainsi la prochaine fois je n'aurais qu'à faire un lien sur l'article, et en plus ca boostera mes stats :p

## Les énumérables

Le principe des énumérables c'est de permettre de parcourir une liste d'éléments en se récupérant élément par élément, a la manière d'un flux (on parle d'itération). L'utilisation de ce principe la plus courante étant l'utilisation du mot clé ```foreach```.

Dans .Net les énumérables fonctionnent via les interfaces ```System.Collections.IEnumerable``` et ```System.Collections.IEnumerator```. Ces interfaces existent en version génériques ```System.Collections.Generic.IEnumerable<T>``` et ```System.Collections.Generic.IEnumerator<T>```.

A partir du moment où une classe implémente l'interface ```IEnumerable``` alors elle devient énumérable.

```IEnumerable``` indique que l'on peut énumérer un objet et à la charge de fournir un ```IEnumerator``` à chaque fois que l'on veut parcourir les éléments.

```IEnumerator``` est "l'énumérateur", c'est à dire l'objet qui va se charger de "parcourir" les éléments à énumérer.

Donc pour rendre une classe énumérable, on implémente ```IEnumerable``` qui va créer un ```IEnumerator``` qui aura la charge de l'itération des éléments.

A savoir: toutes les listes, collections, dictionnaires, ainsi que les tableaux du .Net, implémentent ```IEnumerable```.

### Interface IEnumerable/IEnumerable<T>

Cette interface indique qu'on peut énumérer l'objet de la classe qu'il l'implémente. Elle ne possède qu'une méthode ```GetEnumerator()``` retournant un ```IEnumerator```.

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

Le principe de cette interface et de fournir un nouvel objet énumérateur à chaque appel permettant ainsi de démarrer une nouvelle itération, et permettre également d'avoir plusieurs itérations en parallèle sur la même source.

### Interface IEnumerator/IEnumerator<T>

Cette interface représente la logique d'itération (objet énumérateur). C'est elle qui indique l'élément en cours et qui permet de se "déplacer" dans l'énumération.

```csharp

public interface IEnumerator
{
    object Current { get; }
    bool MoveNext();
    void Reset();
}

public interface IEnumerator&lt;out T&gt; : IDisposable, IEnumerator
{
    T Current { get; }
}

```

Le fonctionnement de l'énumérateur est assez simple : il indique l'élément en cours d'énuémration (propriété ```Current```), et se déplace vers le prochain élément avec la méthode ```MoveNext()```.

### Principe de l'itération

Pour parcourir un énumérable on utilise toujours le même principe :
- On demande à ```IEnumerable``` un nouvel énumérateur
- Tant que ```IEnumerator.MoveNext()``` retourne ```true```
  - On traite ```IEnumerator.Current```

C'est ce que fait l'instruction ```foreach``` pour nous.

Ainsi la boucle suivante

```csharp
// Récupération de l'énumérable
IEnumerable <Int32> enumerable = GetItems();

// Parcours chaque élément dans elm
foreach (int elm in enumerable)
{
    // ..
}
```

est compilée comme ceci (approximativement ;) )

```csharp
// Récupération de l'énumérable
IEnumerable<Int32> enumerable = GetItems();

// Récupère un nouvel énumérateur
IEnumerator<Int32> enumerator = enumerable.GetEnumerator();
// Tant que l'énumérateur se déplace
while (enumerator.MoveNext())
{
    Int32 elm = enumerator.Current;
    // ..
}
```

Toutefois ce n'est pas tout à fait exact, car ce que beaucoup de savent pas (apparement) c'est que la boucle ```foreach``` gère également la possibilité qu'un énumérateur soit disposable (implémentant ```IDisposable```), donc la boucle équivalente est en réalité plus comme ceci :


```csharp
// Récupération de l'énumérable
IEnumerable<Int32> enumerable = GetItems();

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
```

De cette manière que la boucle s'arrête normalement, ou prématurément via un ```break```, ou à cause d'une exception, notre énumérateur sera disposé.

Il faut comprendre qu'implémenter ```IDisposable``` sur notre énumérateur est le seul moyen que nous ayons pour déterminer qu'une itération est terminée, surtout si on a pas parcouru l'ensemble des éléments.

Petit apparté : en réalité ```foreach``` ne prends pas en charge que des ```IEnumerable```. Ce qui importe pour ```foreach``` c'est que sa source possède une méthode ```GetEnumerator()``` publique qui retourne un type qui possède une méthode ```MoveNext()``` retournant un booléen, et une propriété ```Current```.

### Les énumérateurs

Pour faire un énumérateur il faut donc implémenter ```IEnumerator```. Toutefois avant d'en implémenter un, si vous voulez énumérer un tableau privé par exemple, il vous suffit de renvoyer le ```GetNumerator()``` du tableau.

L'implémentation est assez simple au final, ce qui importe c'est qu'a sa création l'énumérateur est dans un état "indéfini", c'est à dire qu'on ne s'est pas encore déplacé, donc ```Current``` doit se trouver avec une valeur par défaut, et on doit attendre le premier ```MoveNext()``` pour commencer vraiment notre itération. Ce qui implique que si vous avez des initialisations à faire (se connecter à la base de données par exemple) vous devez le faire lors du premier ```MoveNext()``` pour des raisons d'optimisation ; on peut avoir besoin d'instancier un énumérateur sans pour autant le parcourir, ce qui peut être le cas quand on enchaîne plusieurs énumérables comme le LINQ (cf prochaine partie de cet article).

Imaginons que nous voulons créer un énumérateur qui parcours une liste à l'envers.

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

Toutefois ça peut vite se compliquer quand nous avons des énumérateurs un peu tordus (parcours d'un arbre syntaxique par exemple). Nous verrons dans la partie 2 comment on peut se simplifier la vie.

Dernier point pour cette partie : ```IEnumerator``` demande l'implémentation de ```Reset()```. Il faut savoir que cette méthode n'est pas vraiment utilisée, la plupart du temps elle lève une exception ```NotImplementedException```. Donc si son implémentation est trop complexe, n'ayez pas peur de faire de même. En fait on part du principe que si on veut redémarrer une itération on fait à nouveau appel à ```IEnumerable.GetEnumerator()``` qui va nous instancier un nouvel énumérateur, donc le reset n'a plus lieu d'être.  

## Conclusion

Nous avons vu la mécanique de base des énumérables, ainsi que le pattern d'une itération.

Ce qu'il faut retenir :
- ```IEnumerable.GetEnumerator()``` doit toujours retourner un nouvel ```IEnumerator``` permettant ainsi le démarrage d'une nouvelle itération.
- l'itération ne doit démarrer réellement qu'au premier appel de ```IEnumerator.MoveNext()```.
- S'il est nécessaire de "détecter" la fin d'une itération quelque soit la situation, l'énumerateur doit implémenter l'interface ```IDisposable```.

Le programme "PrgPart1" implémente des exemples des différents codes que j'ai fourni dans cet article. 

La seconde partie de cette article montrera comment générer des énumérateurs complexes de manière assez simple grâce au mot clé ```yield```.

 