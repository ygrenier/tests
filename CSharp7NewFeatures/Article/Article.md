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

# Références
- [https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/](https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/)
- [https://msdn.microsoft.com/en-us/magazine/mt790184.aspx](https://msdn.microsoft.com/en-us/magazine/mt790184.aspx)

# Conclusion

A bientôt 

Yanos