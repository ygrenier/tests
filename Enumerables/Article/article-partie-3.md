# LINQ et les énumérables à la chaîne

LINQ est un language de requêtage intégré au language C# ou VB.Net, le propros de cet article n'est pas d'expliquer LINQ en lui-même mais son comportement avec les énumérables. Pour plus d'informations sur LINQ je vous renvoie [à la documentation officielle](https://msdn.microsoft.com/fr-fr/library/bb397926.aspx).

## Rappel

Une requête LINQ ressemble à celà :

```csharp

var query = from p in GetPersons()
            where p.IsFamily
            orderby p.Name
            select p;

```

Bon je pense que tout le monde comprends grosso modo la requête : ```GetPersons()``` est une méthode qui retourne un ```IEnumerable<Person>```, et notre requête en filtre les éléments donc le booléen ```IsFamily``` est à ```true``` et trié par leur nom.

Si on regardes de plus près la variable ```query``` (dans VisualStudio il suffit de survoler le mot clé **var** pour afficher une info-bulle informant du type déterminé par le compilateur) on constate qu'il s'agit d'un ```IEnumerable<Person>```.

En ralité il faut savoir que le compilateur va compiler cette requête sous la forme suivante :

```csharp
var query = GetPersons()
    		.Where(p => p.IsFamily)
    		.OrderBy(p => p.Name)
    ;
```

LINQ fourni un ensemble de méthodes d'extensions (Select, Where, OrderBy, etc.) que l'on nomment "opérateur" car ils correspondent aux mot clés du language LINQ.

Si on regarde de plus près les opérateurs de notre requête on constate qu'ils renvoient tous un ```IEnumerable<T>```. En fait chaque appel de ces opérateurs va créé un objet "proxy" de type énumérable qui encapsule l'énumérable "source".

Ainsi notre variable ```query``` est en réalité une "chaîne" d'énumérables, elle contient un énumérable de type "OrderBy" qui se base ur un énumérable de type "Where" qui se base sur l'énumérable renvoyé par ```GetPersons()```.

## Mais alors, quel le problème ?

Je vous raconter une petite histoire, qui m'est arrivé :)

Un matin je reçois un mail d'un ami :

> Salut,
> Je possède une table de données extrêment volatile où je peux avoir une centaine de lignes insérées par secondes.
> Mon application affiche un tableau de bord avec des stats réactulisées régulièrement. Hors depuis que je suis en production LINQ calcul mal mon nombre de lignes actualisées je ne comprends pas !
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

Sa source de données est un Entity Framework, qui gère en réalité des ```IQueryable<T>``` mais ces derniers sont des ```IEnumerable<T>```.

Après quelques échanges je constate qu'il n'avait pas compris que sa requête ```q``` était un énumérable et que par conséquent chaque fois qu'il faisait une itération, une nouvelle requête SQL était générée. Hors l'opérateur ```Count()``` qu'il utilise pour détermine le nombre de nouvelles statistiques ne renvoie pas un ```IEnumerable``` mais provoque une itération pour compter le nombre d'éléments. Seulement ses données étant tellement volatiles qu'entre les deux requêtes de nouvelles statistiques sont apparues.

Le problème est là : en fait apparement certaines personnes pense qu'une requête LINQ est une sorte de liste tampon, qu'une fois qu'on a lancé une requête le résultat reste en mémoire, donc faire un ```Count()``` derrière se contente de compter le résultat.

C'était la logique de mon ami, habitué à utiliser des principes de **Recordset** ou de **Dataset** pour les données, il pensait qu'une requête LINQ était une sorte d'objet autoalimenté contenant le résultat de sa requête.

## Penser 'flux'

Comme nous l'avons vu en première partie, les énumérateurs sont une logique de "flux" ou on traite élément par élément. Dans le cadre d'une base de données chaque ligne étant retournée dés qu'elle est reçue du serveur.

Nous avons vu également qu'un énumérable n'est qu'un fournisseur d'énumérateur, tant que l'on provoque pas l'énumération (appel à ```GetEnumerator()```) nous n'avons pas de flux.

Je vous ai expliqué également que LINQ était en fait une chaîne d'énumérable. Notre requête en début d'article pourrait ressembler à celà :
```csharp
static IEnumerable<Person> SimulWhere()
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

Dans 'PrgPart' vous pouvez faire un pas à pas sur cette méthode pour voir que tant qu'on n'entre pas dans un **foreach**, ```SimulOrderBy()``` n'est pas appellée. En revanche dans notre exemple elle est appelée deux fois car on a deux **foreach**.

Donc il faut toujours penser nos requêtes en flux, qui se déclenchent à chaque fois que l'on provoque une itération.

Alors vous allez me dire, mais comment savoir qu'une itération est déclenchée avec les opérateurs LINQ ? En bien c'est simple, il suffit de regarder le type de retour de l'opérateur, s'il renvoi un ```IEnumerable``` il ne provoque pas d'itération, mais renvoi un nouvel objet énumérable avec son propre énumérateur pour appliquer son opération sur le flux.

Le language LINQ intégré ne génère que des énumérables, par exemple ```Count()``` n'a pas d'équivalent LINQ, il faut utiliser la méthode, mais on peut coupler les deux : 

```csharp

int count = (from p in GetPersons()
            where p.IsFamily
            orderby p.Name
            select p).Count();

```

## Conclusion

Bon j'espère que c'est plus clair sur la logique des énumérables :
- que ```IEnumerable``` ne contient pas de données, mais sert uniquement a créer un nouveau flux d'élément à chaque ```GetEnumerator()```
- qu'une requête LINQ n'est pas un ensemble de données mais un énumérable
- qu'à chaque fois que vous avez un énumérable, il faut penser flux, quelque soit l'implémentation de cet énumérable




