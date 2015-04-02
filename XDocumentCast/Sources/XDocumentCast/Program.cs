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

        static void Main(string[] args)
        {
            Console.Write("Culture en cours :");
            Console.WriteLine(System.Globalization.CultureInfo.CurrentCulture.DisplayName);

            Console.WriteLine("* Sérialisation 1");
            XDocument xdoc = Serialize1();
            Console.WriteLine(xdoc.ToString(SaveOptions.None));

            Console.Read();
        }
    }
}
