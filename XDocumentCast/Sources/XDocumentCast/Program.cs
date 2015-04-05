using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace XDocumentCast
{
    class Program
    {
        /// <summary>
        /// Premier exemple de sérialisation
        /// </summary>
        static XDocument Serialize1()
        {
            return new XDocument(
                new XElement("root",
                    new XAttribute("code", "Exemple 1"),
                    new XAttribute("date", DateTime.Now),
                    new XElement("val", 123.456),
                    new XElement("created", new DateTime(2015, 4, 2, 7, 28, 0)),
                    new XElement("color", System.ConsoleColor.Cyan)
                    )
                );
        }

        /// <summary>
        /// Premier exemple de désérialisation
        /// </summary>
        static void Deserialize1()
        {
            XDocument xdoc = XDocument.Load("sample-1.xml");
            var root = xdoc.Root;
            Console.WriteLine(" - Code : {0}", (String)root.Attribute("code"));
            Console.WriteLine(" - Date : {0}", (DateTime)root.Attribute("date"));
            Console.WriteLine(" - Val : {0}", (Double)root.Element("val"));
            Console.WriteLine(" - Created : {0}", (DateTime)root.Element("created"));
            Console.WriteLine(" - Color : {0}", Enum.Parse(typeof(System.ConsoleColor), (string)root.Element("color"), true));
        }

        static void Main(string[] args)
        {
            Console.Write("Culture en cours :");
            Console.WriteLine(System.Globalization.CultureInfo.CurrentCulture.DisplayName);

            // Sérialisation
            Console.WriteLine("* Sérialisation 1");
            XDocument xdoc = Serialize1();
            Console.WriteLine(xdoc.ToString(SaveOptions.None));
            Console.WriteLine("");

            // Désérialisation
            Console.WriteLine("* Désérialisation 1");
            Deserialize1();
            Console.WriteLine("");

            Console.Read();
        }
    }
}
