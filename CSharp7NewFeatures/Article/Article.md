Avec la sortie de Visual Studio 20017, nous avons eu également le droit à une nouvelle
version du C#, le C# 7.0, avec son lot de nouveautés.

Alors ne vous attendez pas à une révolution, mais plutôt à un ensemble d'ajout pour 
améliorer l'écriture et l'exécution du code.

Faison un rapide tour de tout ça.

<!--more-->

# Les variables 'out'

Bon vous connaissez tous les paramètres "out". Par exemple la méthode:

```csharp
static void DivAndModulo(int value, int divider, out int result, out int remainder)
{
    remainder = value % divider;
    result = value / divider;
}
```

que l'on invoke de cette manière

```csharp
static void CallDivAndModulo()
{
    int result, remainder;
    DivAndModulo(10, 3, out result, out remainder);
    Console.WriteLine($"10/3={result}, reste {remainder}");
}
```

L'un des points parfois agaçant des paramètres 'out' c'est de devoir prédéfinir toutes 
les variables qui vont être utilisées par les paramètres, comme 'result' et 'remainder' 
dans l'exemple précédent.

Avec C# 7.0, nous avons maintenant les 'variables out' avec la possibilité de définir nos
variables au moment où on passe nos variables comme paramètres.

Dans notre exemple cela donne:

```csharp
static void CallDivAndModuloWithOutVariables()
{
    DivAndModulo(10, 3, out int result, out var remainder);
    Console.WriteLine($"10/3={result}, reste {remainder}");
}
```

Comme on le constate, il suffit d'ajouter le type de la variable dans l'appel pour
déclarer les variable. Comme notre méthode appelée défini le type du paramètre, nous 
pouvons utiliser `var` a la place du type.

A noter que la portée des variables 'out' se trouve dans le bloc englobant la définition,
ce qui permet d'utiliser nos variables dans les lignes qui se trouvent dans le
bloc de même niveau.

L'une des utilisations communes des variables 'out' est avec le modèle **Try...**, 
par exemple:

```csharp
static void CallTryParseWithOutVariables(string s)
{
    if (int.TryParse(s, out int n)) { Console.WriteLine($"Nombre: {n}"); }
    else { Console.WriteLine("Pas un nombre"); }
}
```


On constate que l'écriture de ce code est plus court que d'habitude.

Dernier point intéressant, on peut ignorer une variable out grâce à `_`.

```csharp
static void CallDivOnly()
{
    DivAndModulo(10, 3, out int result, out var _);
    Console.WriteLine($"10/3={result}");
}
```
Enfin terminées les variables `dummy` ;)

# Pattern Matching (ou filtres)

C# 7.0 introduit la notions de "patterns" (ou "modèles"), qui sont des éléments 
syntaxiques qui permettent de *tester* qu'une valeur correspond à une certaine "forme"
et d'*extraire* des informations de ce test. On utilise ces patterns dans des éléments
du language.

C# 7.0 gère les patterns suivants:

- **Constant pattern**: de la forme `c` (où `c` est une expression constante) qui test
si l'entrée est égale à `c`.
- **Type pattern**: de la forme `T x` (où `T` est un type et `x` est un identificateur) qui
test si l'entrée est du type `T` et si c'est le cas extrait la valeur de l'entrée dans
une nouvelle variable `x` de type `T`
- **Var pattern": de la forme `var x` (où `x` est un identificateur) qui est toujours 
valide et place simplement la valeur de l'entrée dans une nouvelle variable `x` avec le 
même type que l'entrée.

On utilise ces patterns dans deux extensions du language C# 7.0.

## Expression 'is' avec des patterns

Les expressions `is` peuvent désormais supporter un pattern dans la partie droite, à la place d'un simple type.

```csharp
static void IsWithPatterns(object o)
{
    // constant pattern "null"
    if (o is null) return;
    if (o is "test") return;

    // type pattern "int i"
    if (!(o is int i)) return; 
    WriteLine(new string('*', i));
}
```

Voici deux exemples d'utilisation des patterns dans une expression `is`.

Dans le "constant pattern" l'utilité réside essentiellement dans le fait qu'il n'a pas
nécessaire de faire appelle aux méthodes d'égalité des objets (je vous rappelle à toutes
fins utiles que l'opérateur d'égalité `==` ne s'appliquer sur un `object` avec un `string`),
var l'opérateur `is` et les patterns nous avons un test de type, et ensuite une 
comparaison de valeur.

Le second pattern est plus utile, nous vérifions que notre objet est d'un type en 
particulier  et si c'est le cas on l'affecte dans une variable du type en question.

Comme pour les variables out (qui ont étrangement la même syntaxe) la portée des variables
définies de cette manières est du bloc englobant.

On peut combiner `is` avec des patterns et `Try...`:

```csharp
static void ComplexIsPattern(object o)
{
    if(o is int i || (o is string s && int.TryParse(s,out i)))
    {
        WriteLine(new string('*', i));
    }
}
```

## Instruction 'switch' avec des patterns

L'instruction `switch` a été modifiée afin de:
- tester un type en particulier (et pas uniquement un type primitif)
- utiliser les patterns dans les clauses `case`
- d'avoir des conditions supplémentaires dans les clauses `case`

```csharp
static void SwitchWithPattern(object shape)
{
    switch (shape)
    {
        case Circle c:
            WriteLine($"Cercle d'un rayon de {c.Radius}");
            break;
        case Rectangle s when (s.Width == s.Height):
            WriteLine($"Carré {s.Width} x {s.Height}");
            break;
        case Rectangle r:
            WriteLine($"Rectangle {r.Width} x {r.Height}");
            break;
        case "test":
            WriteLine("C'est un test");
            break;
        default:
            WriteLine("<forme inconnue>");
            break;
        case null:
            throw new ArgumentNullException(nameof(shape));
    }
}
```

On constate différentes choses avec cette nouvelle instruction `switch`:

- *Désormais l'ordre des clauses `case` est important*: comme pour les clauses `catch`,
les clauses `case` sont validées dans l'ordre, et la première qui est valide est 
exécutée. Dans notre exemple il est important que le test du carré soit effectué AVANT
celui du rectangle. De même le compilateur vous prévient si une clause `case` n'est 
jamais atteinte.
- *La clause par défaut est TOUJOURS exécutée en dernier*: malgré qu'elle ne soit pas
la dernière clause (on a une clause null après) c'est elle qui sera toujours exécutée
en dernier, principalement pour des raisons de compatibilité. Malgré tout il est
préférable de toujours définir la clause `default` en dernier.
- *La clause null à la fin est accessible*: étant donné que les types patterns suivent
le même principe que pour l'expression `is` et qu'ils ne valident par le null, ce qui
nous garanti que les valeurs null ne sont pas validées par n'importe quel type qui
pourrait être défini dans une clause précédente.

Les variables définies dans les patterns d'une clause `cause` ont une portée limitée au
bloc du case.

Attention les instructions `goto case ...` ne sont applicables qu'aux case constante, 
pas avec des patterns.

# Les tuples

Haa l'une de mes fonctionnalités préférées du C# 7.0 :)

Il est assez courant d'avoir besoin de retourner plusieurs valeurs depuis une méthode. Avec
les versions précédentes du C# nous avions les options peu optimales suivantes:
- Paramètres output: pas facile d'utilisation (même avec les améliorations décrites 
précédemment) et ne supportent pas les méthodes async.
- Type de retour `System.Tuple<>`: utilisation verbeuse, nécessite d'allouer un objet tuple.
- Type personnalisé pour retourner les valeurs: écriture de beaucoup de code pour une utlisation temporaire.
- Retour de type anonyme via un `dynamic`: réduction des performances de code  et pas de vérification statique de type .

Pour améliorer ça, le C# 7.0 apportent le type tuple et les tuples littéraux.

```csharp
static (string, int, string) ReturnsTuple() // Type de retour tuple
{
    return ("un", 2, "trois"); // Tuple littéral
}
```

Notre méthode renvoie trois valeurs encapsulées dans une valeur tuple.

L'appelant de la méthode recoit un tuple et peut accéder aux éléments individuels:
```csharp
static void UseTuple()
{
    var values = ReturnsTuple();
    WriteLine($"{values.Item1}, {values.Item2}, {values.Item3}");
}
```

Les noms **Item1**, etc. sont les noms des membres par défaut des tuples, mais ils ne 
sont vraiment descriptifs. Heureusement désormais on peut si on le souhaite nommer
nos éléments de tuple.

```csharp
static (string first, int, string last) ReturnsTupleWithNames() // tuple with names
{
    return ("un", 2, "trois"); // tuple literal
}
```

maintenant nous pouvons utiliser des noms plus explicites

```csharp
static void UseTupleWithNames()
{
    var values = ReturnsTupleWithNames();
    WriteLine($"{values.first}, {values.Item2}, {values.last}");
}
```

On peut également sépcifier les noms explicitement lors de la définition des tuples.

```csharp
static void UseTupleWithExplicitNames()
{
    var values = (f: "un", second: 2, last: "trois");
    WriteLine($"{values.f}, {values.second}, {values.last}");
}
```

A savoir que les noms ne sont que des alias aux noms `Item*`, donc les noms `Item*` 
existent toujours. Ce n'est pas un nouveau type qui est généré, mais le type `ValueTuple<>`
qui est "décoré" par le compilateur.

De ce fait, on peut assigner n'importe quel tuple dans un autre a partir du moment où
chacun des membres est du même type et que le nombre de membres eset identique, 
les noms "originaux" ne changeant pas.

**ATTENTION:** Si vous utilisez les `ValueTuple<>` dans un framework qui ne les supportent pas
(vous aurez une erreur de compilation ou un type tuple est introuvable),
il faut installer le package Nuget `System.ValueTuple`.

# Déconstruction

Une autre manière d'exploiter les tuples, est la déconstruction. 

Une **déclaration de déconstruction** est une syntaxe qui permet de décomposer chaque 
élément du tuple dans une variable.

```csharp
static void DeconstructingDeclaration()
{
    (string f, int c, string l) = ReturnsTuple();   // deconstructing
    WriteLine($"{f}, {c}, {l}");
}
```

On peut utiliser `var` dans la déclaration individuelle des variables.
```csharp
static void DeconstructingDeclarationVarInside()
{
    (var f, var c, var l) = ReturnsTuple();   // var inside
    WriteLine($"{f}, {c}, {l}");
}
```

Mais on peut également définir `var` de manière plus globale en dehors des parenthèse 
comme abréviation.
```csharp
static void DeconstructingDeclarationVarOutside()
{
    var (f, c, l) = ReturnsTuple();   // var outside
    WriteLine($"{f}, {c}, {l}");
}
```

On peut également déconstruire dans des variables existantes, c'est ce qu'on appelle
**affectation de déconstruction**.

```csharp
static void DeconstructingAssignment()
{
    string first = "first";
    int count = -5;
    string last = "last";

    (first, count, last) = ReturnsTuple();  // deconstructing assignment
    WriteLine($"{first}, {count}, {last}");
}
```

La déconstruction n'est pas valable uniquement pour les tuples. N'importe quel type
peut être déconstruit a partir du moment ou une méthode (d'instance ou d'extension)
*deconstrucor* est définie de la forme suivante:

```csharp
public void Deconstruct(out T1 x1, ..., out Tn xn) { ... }
```

Les paramètres de sorties constituent les valeurs qui résultent de la déconstruction.

```csharp
class DeconstructObject
{
    public void Deconstruct(out string name, out int count)
    {
        name = Name;
        count = Count;
    }
    public string Name { get; set; }
    public int Count { get; set; }
}

static void DeconstructingObject()
{
    var dobj = new DeconstructObject { Name = "Nom", Count = 2 };
    var (n, c) = dobj;
    WriteLine($"{n}, {c}");
}
```

L'intérêt de déconstruire un objet plutôt que de renvoyer un tuple, est qu'on peut
surcharger la méthode avec autant de déclinaisons que l'on veut.

Tout comme pour les variables 'out' il est possible d'ignorer un valeur de déconstruction
avec `_`.

# Fonctions locales

Parfois des méthodes utilitaires ne sont utilisées qu'à l'intérieur d'une méthode. On
peut définir une méthode anonyme à l'intérieur de la méthode, malheureusement 
elles ne supportent pas certains choses (paramètres out, mot clé yield, etc.).

Désormais les *fonctions locales* vont nous permettrent de faire tout celà. Tout comme
les méthodes anonymes les fonctions locales ont accès aux paramètres et variables de la
portée du bloc où est déclarée la fonction locale.

```csharp
static void LocalFunc()
{
    foreach (var line in GetLines())
    {
        WriteLine(line);
    }

    void Pow(int value, out int result)
    {
        result = value * value;
    }

    IEnumerable<string> GetLines()
    {
        for (int i = 0; i < 5; i++)
        {
            Pow(i, out int r);
            yield return $"{i}*{i}={r}";
        }
    }
}
```

# Amélioration des littéraux

Les nombres littéraux ont également leur part d'amélioration.

On peut désormais utiliser `_` dans un nombre "séparateur digital", celà ne représente 
rien, ce caractère sert de séparateur afin de permettre une meilleure lecture d'un 
nombre.

De plus C# 7.0 les littéraux binaires.

```csharp
static void Litterals()
{
    int dec = 12_34;        // decimal
    int hex = 0xA1_23;      // hexadecimal
    int boo = 0b1010_1100;  // binaire

    WriteLine($"{dec}, {hex}, {boo}");
}
```


# Références
- [https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/](https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/)
- [https://msdn.microsoft.com/en-us/magazine/mt790184.aspx](https://msdn.microsoft.com/en-us/magazine/mt790184.aspx)

# Conclusion

A bientôt 

Yanos