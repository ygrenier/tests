This article is about the serialization/deserialization in XDocument with direct base type
values casting.

# Serialization with XDocument

Since his version 4.0, the .Net Framework provide a new library for managing XML documents : `System.Linq.XDocument`.

This library is more built to be used with LINQ (as the namespace say it). It allow to
quickly create documents, browse elements with LINQ query. But we lose the DOM standard
and XPath query.

One of feature of XDocument is that we can use object as content, XDocument convert the objects
to node or attribute value. The reverse is also true, we can get a value from a node or an
attribute directly by casting. These mechanisms allow us to serialize/deserialize quickly
an XML file when we can't use .Net XML serialization for example.

## Serialization

Serialization is simple, we just need to create a node or an attribute with the object
as value. XDocument will use the object method `ToString()` to convert the value to text.

Where it is interesting for the "non-english" (french for example ;)) which need internationalize their XML files, the basics types (int, double, datetime, etc.) are formatted with the invariant culture, or an international format for the dates.

Exemple the following code :

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

generate this XML document :

``` XML
<root code="Exemple 1" date="2015-04-02T07:31:13.8253366+02:00">
  <val>123.456</val>
  <created>2015-04-02T07:28:00</created>
  <color>Cyan</color>
</root>
```

This sample can be find in the `Serialize1()` method in the sample program.

# Deserialization

For deserialize XDocument provide us a simple method too, we just need to cast a node or an
attribute to convert the value to the requested type.
 
If we take the generated XML previously, how we can make to read (`Deserialize1()` method in the sample program) :

```CSharp
XDocument xdoc = XDocument.Load("sample-1.xml");
var root = xdoc.Root;
Console.WriteLine(" - Code : {0}", (String)root.Attribute("code"));
Console.WriteLine(" - Date : {0}", (DateTime)root.Attribute("date"));
Console.WriteLine(" - Val : {0}", (Double)root.Element("val"));
Console.WriteLine(" - Created : {0}", (DateTime)root.Element("created"));
Console.WriteLine(" - Color : {0}", Enum.Parse(typeof(System.ConsoleColor), (string)root.Element("color"), true));
```

We retreive the attribut or the element and we convert it by casting. The only exception
is for the enums whose casting is not supported by XDocument. We will need to process as usual
by parsing the text value.

Of course if the value contained in the element or the attribute can't be converted to the
requested type, a cast exception will be thrown.

Remark: the search of attributes or elements by name is case sensitive.

## Deserialiaation of optional value

Now, what appends when one of attributes or elements that we want is missing ?

Because the node or attribute not exists, an `ArgumentNullException()` is thrown.

In the `Deserialize2()` method we trying to read two values that not exists :

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

and that the application display :

```
* Désérialisation 2
 - Name :
 - Number : Erreur => The value can't be null.
Parameter : element
```

Apparently only the DateTime cast throw an error, not the string cast.

In fact, when we want to cast the value of a missing element (or attribute), XDocument
try to convert the value to ```null```. As the ```String``` type is nullable we don't
have exception thrown, however this is not the case of the ```DateTime``` type.

But as usual, XDocument help us, like it supports the base types casting, it supports casting to the nullable version of these types.

For example in the `Deserialize3()` method, we reuse the `Deserialize2()` code with
the `Deserialize1()` code, but this time we use nullable types, and default value in case
of the values not exists in the XML.

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

By this way, we are protected from missing values. Of course if a value is required, 
we can maintain a non-nullable type.

We are also seeing that the enums are not always well off. We need to do tests.

To resolve this problem we can create an extension methods to make or life easier.


