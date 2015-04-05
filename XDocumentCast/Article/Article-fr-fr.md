Cet article traite de sérialisation/désérialisation avec XDocument via le casting direct des valeurs pour
les types de base.

# La sérialisation avec XDocument

Depuis sa version 4.0, le Framework .Net nous fourni une nouvelle librairie pour gérer les 
documents XML: `System.Linq.XDocument`.

Cette librairie et plutôt concue pour fonctionner avec LINQ (comme son espace de nom nous l'indique). 
Elle permet de créer plus rapidement des documents, de parcourir avec des requêtes LINQ. En 
revanche on perd le standard DOM ainsi que le XPath.

L'une des particularité de XDocument est que l'on peut lui transmettre des objets comme
contenu et il s'occupe de les convertir en valeur de noeud ou d'attribut. L'inverse est également
vrai, on peut obtenir une valeur depuis un noeud ou un attribut directement en castant. Ces 
mécanismes nous permettent de sérialiser/désérialiser rapidement un fichier XML quand on
ne peut pas utiliser les mécanismes de sérialisation XML du .Net.

## Sérialisation

La sérialisation est assez simple, il suffit de créer un noeud ou un attribut avec l'objet
de sa valeur. XDocument va utiliser la méthode `ToString()` de l'objet pour convertir la
valeur en texte.

Là où c'est intéressant pour les "non anglais" qui doivent internationaliser leur fichier XML
les types de base (int, double, datetime, etc.) sont formatés avec une culture invariante, ou
en format international pour les dates.

Par exemple le document suivant :

``` CSharp
new XDocument(
    new XElement("root",
        new XAttribute("code", "Exemple 1"),
        new XAttribute("date", DateTime.Now),
        new XElement("val", 123.456),
        new XElement("created", new DateTime(2015, 4, 2, 7, 28, 0)),
        new XElement("color", System.ConsoleColor.Cyan)
        )
    );
```

génère le document XML suivant :

``` XML
<root code="Exemple 1" date="2015-04-02T07:31:13.8253366+02:00">
  <val>123.456</val>
  <created>2015-04-02T07:28:00</created>
  <color>Cyan</color>
</root>
```

Cet exemple se trouve dans la méthode `Serialize1()` du programme d'exemple.

# Désérialisation

Pour désérialiser XDocument nous fourni également une approche assez simple, il suffit de caster
un noeud ou un attribut pour qu'il convertisse la valeur dans le type demandé.
 
Si nous reprenons le fichier XML généré précédemment voilà comment on peut faire 
(méthode `Deserialize1()` du programme d'exemple) :

```CSharp
XDocument xdoc = XDocument.Load("sample-1.xml");
var root = xdoc.Root;
Console.WriteLine(" - Code : {0}", (String)root.Attribute("code"));
Console.WriteLine(" - Date : {0}", (DateTime)root.Attribute("date"));
Console.WriteLine(" - Val : {0}", (Double)root.Element("val"));
Console.WriteLine(" - Created : {0}", (DateTime)root.Element("created"));
Console.WriteLine(" - Color : {0}", Enum.Parse(typeof(System.ConsoleColor), (string)root.Element("color"), true));
```

Là où normalement on récupère un attribut ou un élément on le converti via un cast. La seule
exception est dans les énumérés dont le casting n'est pas supporté par XDocument. Il nous faudra
procédé comme d'habitude en demander l'analyse de la valeur texte.

Bien sûr si la valeur contenue dans l'élément ou l'attribut n'est pas applicable au type demandé
une exception de casting aura lieu.

Attention la recherche des attributs et des éléments par leur nom est sensible à la casse.

## Désérialisation de valeur optionnelle

Maintenant que se passe-t-il lorsque l'un des attributs ou éléments que nous voulons n'est pas
présent dans le fichier ?

Comme le noeud ou l'atttribut n'existe pas, une erreur `ArgumentNullException()` est levée.

Dans la méthode `Deserialize2()` nous essayons de lire deux valeurs qui n'existent pas :

```CSharp
XDocument xdoc = XDocument.Load("sample-1.xml");
var root = xdoc.Root;
try
{
  Console.WriteLine(" - Name : {0}", (String)root.Attribute("name"));
}
catch (Exception ex)
{
  Console.WriteLine(" - Name : Erreur => {0}", ex.Message);
}
try
{
  Console.WriteLine(" - Number : {0}", (Double)root.Element("number"));
}
catch (Exception ex)
{
  Console.WriteLine(" - Number : Erreur => {0}", ex.Message);
}
```

et voilà ce que nous renvoi l'application :

```
* Désérialisation 2
 - Name :
 - Number : Erreur => La valeur ne peut pas être null.
Nom du paramètre : element
```

En clait seule la valeur castée en DateTime provoque une erreur, pas celle que l'on cast
en chaîne.

En effet, lorsque l'on cherche à caster une valeur dont l'élément (ou l'attribut) n'existe pas, 
XDocument cherche à convertir la valeur en ```null```. Comme le type ```String``` est nullable
nous n'avons pas d'exception de levée, en revanche ce n'est pas le cas du type ```DateTime```.

Mais comme d'habitude XDocument fait bien les choses, car là où il supporte le cast vers
des types de base, il supporte également le cast vers ces types dans leur versions nullables.

Par exemple dans la méthode `Deserialize3()` nous reprenons l'exemple de `Deserialize2()`
ainsi que `Deserialize1()` mais en utilisant des types nullables, et des valeurs par défaut 
au cas où les valeurs n'existent pas dans le XML.

```CSharp
XDocument xdoc = XDocument.Load("sample-1.xml");
var root = xdoc.Root;

Console.WriteLine(" - Code : {0}", (String)root.Attribute("code") ?? "#Pas de Code#");
Console.WriteLine(" - Name : {0}", (String)root.Attribute("name") ?? "#Pas de Nom#");
Console.WriteLine(" - Date : {0}", (DateTime?)root.Attribute("date") ?? DateTime.MinValue);
Console.WriteLine(" - Val : {0}", (Double?)root.Element("val") ?? -1);
Console.WriteLine(" - Created : {0}", (DateTime?)root.Element("created") ?? DateTime.Now);
Console.WriteLine(" - Number : {0}", (Double?)root.Element("number") ?? -1);
System.ConsoleColor col;
if (!Enum.TryParse((string)root.Element("color"), true, out col))
  col = ConsoleColor.Black;
Console.WriteLine(" - Color : {0}", col);
```

De cette manière nous nous protégeons des valeurs pouvant manquer. Bien entendu si cette valeur
est obligatoire, on peut maintenir un type non nullable.

Nous constatons également que les énumérés ne sont pas toujours bien loti. Nous devons
faire des tests.

Pour résoudre ce problème nous pouvons toujours créer des méthodes d'extensions pour nous
faciliter la vie.

