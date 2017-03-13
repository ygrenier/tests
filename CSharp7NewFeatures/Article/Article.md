Avec la sortie de Visual Studio 20017, nous avons eu �galement le droit � une nouvelle
version du C#, le C# 7.0, avec son lot de nouveaut�s.

Alors ne vous attendez pas � une r�volution, mais plut�t � un ensemble d'ajout pour 
am�liorer l'�criture et l'ex�cution du code.

Faison un rapide tour de tout �a.

<!--more-->

# Les variables 'out'

Bon vous connaissez tous les param�tres "out". Par exemple la m�thode:

```csharp
static void DivAndModulo(int value, int divider, out int result, out int remainder)
{
    remainder = value % divider;
    result = value / divider;
}
```

que l'on invoke de cette mani�re

```csharp
static void CallDivAndModulo()
{
    int result, remainder;
    DivAndModulo(10, 3, out result, out remainder);
    Console.WriteLine($"10/3={result}, reste {remainder}");
}
```

L'un des points parfois aga�ant des param�tres 'out' c'est de devoir pr�d�finir toutes 
les variables qui vont �tre utilis�es par les param�tres, comme 'result' et 'remainder' 
dans l'exemple pr�c�dent.

Avec C# 7.0, nous avons maintenant les 'variables out' avec la possibilit� de d�finir nos
variables au moment o� on passe nos variables comme param�tres.

Dans notre exemple cela donne:

```csharp
static void CallDivAndModuloWithOutVariables()
{
    DivAndModulo(10, 3, out int result, out var remainder);
    Console.WriteLine($"10/3={result}, reste {remainder}");
}
```

Comme on le constate, il suffit d'ajouter le type de la variable dans l'appel pour
d�clarer les variable. Comme notre m�thode appel�e d�fini le type du param�tre, nous 
pouvons utiliser `var` a la place du type.

A noter que la port�e des variables 'out' se trouve dans le bloc englobant la d�finition,
ce qui permet d'utiliser nos variables dans les lignes qui se trouvent dans le
bloc de m�me niveau.

L'une des utilisations communes des variables 'out' est avec le mod�le **Try...**, 
par exemple:

```csharp
static void CallTryParseWithOutVariables(string s)
{
    if (int.TryParse(s, out int n)) { Console.WriteLine($"Nombre: {n}"); }
    else { Console.WriteLine("Pas un nombre"); }
}
```


On constate que l'�criture de ce code est plus court que d'habitude.

Dernier point int�ressant, on peut ignorer une variable out gr�ce � `_`.

```csharp
static void CallDivOnly()
{
    DivAndModulo(10, 3, out int result, out var _);
    Console.WriteLine($"10/3={result}");
}
```
Enfin termin�es les variables `dummy` ;)

# Pattern Matching (ou filtres)

C# 7.0 introduit la notions de "patterns" (ou "mod�les"), qui sont des �l�ments 
syntaxiques qui permettent de *tester* qu'une valeur correspond � une certaine "forme"
et d'*extraire* des informations de ce test. On utilise ces patterns dans des �l�ments
du language.

C# 7.0 g�re les patterns suivants:

- **Constant pattern**: de la forme `c` (o� `c` est une expression constante) qui test
si l'entr�e est �gale � `c`.
- **Type pattern**: de la forme `T x` (o� `T` est un type et `x` est un identificateur) qui
test si l'entr�e est du type `T` et si c'est le cas extrait la valeur de l'entr�e dans
une nouvelle variable `x` de type `T`
- **Var pattern": de la forme `var x` (o� `x` est un identificateur) qui est toujours 
valide et place simplement la valeur de l'entr�e dans une nouvelle variable `x` avec le 
m�me type que l'entr�e.

On utilise ces patterns dans deux extensions du language C# 7.0.

## Expression 'is' avec des patterns

Les expressions `is` peuvent d�sormais supporter un pattern dans la partie droite, � la place d'un simple type.

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

Dans le "constant pattern" l'utilit� r�side essentiellement dans le fait qu'il n'a pas
n�cessaire de faire appelle aux m�thodes d'�galit� des objets (je vous rappelle � toutes
fins utiles que l'op�rateur d'�galit� `==` ne s'appliquer sur un `object` avec un `string`),
var l'op�rateur `is` et les patterns nous avons un test de type, et ensuite une 
comparaison de valeur.

Le second pattern est plus utile, nous v�rifions que notre objet est d'un type en 
particulier  et si c'est le cas on l'affecte dans une variable du type en question.

Comme pour les variables out (qui ont �trangement la m�me syntaxe) la port�e des variables
d�finies de cette mani�res est du bloc englobant.

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

L'instruction `switch` a �t� modifi�e afin de:
- tester un type en particulier (et pas uniquement un type primitif)
- utiliser les patterns dans les clauses `case`
- d'avoir des conditions suppl�mentaires dans les clauses `case`

```csharp
static void SwitchWithPattern(object shape)
{
    switch (shape)
    {
        case Circle c:
            WriteLine($"Cercle d'un rayon de {c.Radius}");
            break;
        case Rectangle s when (s.Width == s.Height):
            WriteLine($"Carr� {s.Width} x {s.Height}");
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

On constate diff�rentes choses avec cette nouvelle instruction `switch`:

- *D�sormais l'ordre des clauses `case` est important*: comme pour les clauses `catch`,
les clauses `case` sont valid�es dans l'ordre, et la premi�re qui est valide est 
ex�cut�e. Dans notre exemple il est important que le test du carr� soit effectu� AVANT
celui du rectangle. De m�me le compilateur vous pr�vient si une clause `case` n'est 
jamais atteinte.
- *La clause par d�faut est TOUJOURS ex�cut�e en dernier*: malgr� qu'elle ne soit pas
la derni�re clause (on a une clause null apr�s) c'est elle qui sera toujours ex�cut�e
en dernier, principalement pour des raisons de compatibilit�. Malgr� tout il est
pr�f�rable de toujours d�finir la clause `default` en dernier.
- *La clause null � la fin est accessible*: �tant donn� que les types patterns suivent
le m�me principe que pour l'expression `is` et qu'ils ne valident par le null, ce qui
nous garanti que les valeurs null ne sont pas valid�es par n'importe quel type qui
pourrait �tre d�fini dans une clause pr�c�dente.

Les variables d�finies dans les patterns d'une clause `cause` ont une port�e limit�e au
bloc du case.

Attention les instructions `goto case ...` ne sont applicables qu'aux case constante, 
pas avec des patterns.

# Les tuples

Haa l'une de mes fonctionnalit�s pr�f�r�es du C# 7.0 :)

Il est assez courant d'avoir besoin de retourner plusieurs valeurs depuis une m�thode. Avec
les versions pr�c�dentes du C# nous avions les options peu optimales suivantes:
- Param�tres output: pas facile d'utilisation (m�me avec les am�liorations d�crites 
pr�c�demment) et ne supportent pas les m�thodes async.
- Type de retour `System.Tuple<>`: utilisation verbeuse, n�cessite d'allouer un objet tuple.
- Type personnalis� pour retourner les valeurs: �criture de beaucoup de code pour une utlisation temporaire.
- Retour de type anonyme via un `dynamic`: r�duction des performances de code  et pas de v�rification statique de type .

Pour am�liorer �a, le C# 7.0 apportent le type tuple et les tuples litt�raux.

```csharp
static (string, int, string) ReturnsTuple() // Type de retour tuple
{
    return ("un", 2, "trois"); // Tuple litt�ral
}
```

Notre m�thode renvoie trois valeurs encapsul�es dans une valeur tuple.

L'appelant de la m�thode recoit un tuple et peut acc�der aux �l�ments individuels:
```csharp
static void UseTuple()
{
    var values = ReturnsTuple();
    WriteLine($"{values.Item1}, {values.Item2}, {values.Item3}");
}
```

Les noms **Item1**, etc. sont les noms des membres par d�faut des tuples, mais ils ne 
sont vraiment descriptifs. Heureusement d�sormais on peut si on le souhaite nommer
nos �l�ments de tuple.

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

On peut �galement s�pcifier les noms explicitement lors de la d�finition des tuples.

```csharp
static void UseTupleWithExplicitNames()
{
    var values = (f: "un", second: 2, last: "trois");
    WriteLine($"{values.f}, {values.second}, {values.last}");
}
```

A savoir que les noms ne sont que des alias aux noms `Item*`, donc les noms `Item*` 
existent toujours. Ce n'est pas un nouveau type qui est g�n�r�, mais le type `ValueTuple<>`
qui est "d�cor�" par le compilateur.

De ce fait, on peut assigner n'importe quel tuple dans un autre a partir du moment o�
chacun des membres est du m�me type et que le nombre de membres eset identique, 
les noms "originaux" ne changeant pas.

**ATTENTION:** Si vous utilisez les `ValueTuple<>` dans un framework qui ne les supportent pas
(vous aurez une erreur de compilation ou un type tuple est introuvable),
il faut installer le package Nuget `System.ValueTuple`.

# D�construction

Une autre mani�re d'exploiter les tuples, est la d�construction. 

Une **d�claration de d�construction** est une syntaxe qui permet de d�composer chaque 
�l�ment du tuple dans une variable.

```csharp
static void DeconstructingDeclaration()
{
    (string f, int c, string l) = ReturnsTuple();   // deconstructing
    WriteLine($"{f}, {c}, {l}");
}
```

On peut utiliser `var` dans la d�claration individuelle des variables.
```csharp
static void DeconstructingDeclarationVarInside()
{
    (var f, var c, var l) = ReturnsTuple();   // var inside
    WriteLine($"{f}, {c}, {l}");
}
```

Mais on peut �galement d�finir `var` de mani�re plus globale en dehors des parenth�se 
comme abr�viation.
```csharp
static void DeconstructingDeclarationVarOutside()
{
    var (f, c, l) = ReturnsTuple();   // var outside
    WriteLine($"{f}, {c}, {l}");
}
```

On peut �galement d�construire dans des variables existantes, c'est ce qu'on appelle
**affectation de d�construction**.

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

La d�construction n'est pas valable uniquement pour les tuples. N'importe quel type
peut �tre d�construit a partir du moment ou une m�thode (d'instance ou d'extension)
*deconstrucor* est d�finie de la forme suivante:

```csharp
public void Deconstruct(out T1 x1, ..., out Tn xn) { ... }
```

Les param�tres de sorties constituent les valeurs qui r�sultent de la d�construction.

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

L'int�r�t de d�construire un objet plut�t que de renvoyer un tuple, est qu'on peut
surcharger la m�thode avec autant de d�clinaisons que l'on veut.

Tout comme pour les variables 'out' il est possible d'ignorer un valeur de d�construction
avec `_`.

# Fonctions locales

Parfois des m�thodes utilitaires ne sont utilis�es qu'� l'int�rieur d'une m�thode. On
peut d�finir une m�thode anonyme � l'int�rieur de la m�thode, malheureusement 
elles ne supportent pas certains choses (param�tres out, mot cl� yield, etc.).

D�sormais les *fonctions locales* vont nous permettrent de faire tout cel�. Tout comme
les m�thodes anonymes les fonctions locales ont acc�s aux param�tres et variables de la
port�e du bloc o� est d�clar�e la fonction locale.

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

# Am�lioration des litt�raux

Les nombres litt�raux ont �galement leur part d'am�lioration.

On peut d�sormais utiliser `_` dans un nombre "s�parateur digital", cel� ne repr�sente 
rien, ce caract�re sert de s�parateur afin de permettre une meilleure lecture d'un 
nombre.

De plus C# 7.0 les litt�raux binaires.

```csharp
static void Litterals()
{
    int dec = 12_34;        // decimal
    int hex = 0xA1_23;      // hexadecimal
    int boo = 0b1010_1100;  // binaire

    WriteLine($"{dec}, {hex}, {boo}");
}
```


# R�f�rences
- [https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/](https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/)
- [https://msdn.microsoft.com/en-us/magazine/mt790184.aspx](https://msdn.microsoft.com/en-us/magazine/mt790184.aspx)

# Conclusion

A bient�t 

Yanos