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
        /// First sample for serialization
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
        /// First sample for deserialization
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

        /// <summary>
        /// Second exemple de désérialisation
        /// Second sample for deserialization
        /// </summary>
        static void Deserialize2()
        {
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
        }

        /// <summary>
        /// Troisième exemple de désérialisation
        /// Third sample for deserialization
        /// </summary>
        static void Deserialize3()
        {
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
        }

        static void Main(string[] args)
        {
            Console.Write("Culture en cours :");
            Console.WriteLine(System.Globalization.CultureInfo.CurrentCulture.DisplayName);

            // Sérialisation / Serialization
            Console.WriteLine("* Serialization 1");
            XDocument xdoc = Serialize1();
            Console.WriteLine(xdoc.ToString(SaveOptions.None));
            Console.WriteLine("");

            // Désérialisation / Deserialization
            Console.WriteLine("* Deserialization 1");
            Deserialize1();
            Console.WriteLine("");

            Console.WriteLine("* Deserialization 2");
            Deserialize2();
            Console.WriteLine("");

            Console.WriteLine("* Deserialization 3");
            Deserialize3();
            Console.WriteLine("");

            Console.Read();
        }
    }
}
