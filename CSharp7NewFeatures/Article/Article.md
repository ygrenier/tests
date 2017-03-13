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


# Références
- [https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/](https://blogs.msdn.microsoft.com/dotnet/2017/03/09/new-features-in-c-7-0/)


# Conclusion

A bientôt 

Yanos