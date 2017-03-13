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


# R�f�rences
- [https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/](https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/)


# Conclusion

A bient�t 

Yanos