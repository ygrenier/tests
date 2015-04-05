# Le sérialisation avec XDocument

Depuis sa version 4.0, le Framework .Net nous fourni une nouvelle librairie pour gérer les 
documents XML: `System.Linq.XDocument`.

Cette librairie et plutôt concue pour fonctionner avec LINQ (comme son espace de nom nous l'indique). 
Elle permet de créer plus rapidement des documents, de parcourir avec des requêtes LINQ. En 
revanche on perd le standard DOM ainsi que le XPath.

L'un des particularité de XDocument est que l'on peut lui transmettre des objets comme
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

