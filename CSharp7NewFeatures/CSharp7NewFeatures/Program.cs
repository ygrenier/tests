﻿using System;
using static System.Console;

namespace CSharp7NewFeatures
{
    class Program
    {

        #region Out variables

        static void DivAndModulo(int value, int divider, out int result, out int remainder)
        {
            remainder = value % divider;
            result = value / divider;
        }

        static void CallDivAndModulo()
        {
            int result, remainder;
            DivAndModulo(10, 3, out result, out remainder);
            Console.WriteLine($"10/3={result}, reste {remainder}");
        }

        static void CallDivAndModuloWithOutVariables()
        {
            DivAndModulo(10, 3, out int result, out var remainder);
            Console.WriteLine($"10/3={result}, reste {remainder}");
        }

        static void CallDivOnly()
        {
            DivAndModulo(10, 3, out int result, out var _);
            Console.WriteLine($"10/3={result}");
        }

        static void CallTryParseWithOutVariables(string s)
        {
            if (int.TryParse(s, out int n)) { Console.WriteLine($"Nombre: {n}"); }
            else { Console.WriteLine("Pas un nombre"); }
        }

        #endregion

        #region Pattern matching

        static void IsWithPatterns(object o)
        {
            // constant pattern "null"
            if (o is null) return;
            if (o is "test") return;

            // type pattern "int i"
            if (!(o is int i)) return; 
            WriteLine(new string('*', i));
        }

        static void ComplexIsPattern(object o)
        {
            if(o is int i || (o is string s && int.TryParse(s,out i)))
            {
                WriteLine(new string('*', i));
            }
        }

        class Line
        {
            public int X1 { get; set; }
            public int Y1 { get; set; }
            public int X2 { get; set; }
            public int Y2 { get; set; }
        }
        class Circle
        {
            public int Radius { get; set; }
        }
        class Rectangle
        {
            public int Height { get; set; }
            public int Width { get; set; }
        }

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

        #endregion

        static void Main(string[] args)
        {
            // Out variables
            //CallDivAndModulo();
            //CallDivAndModuloWithOutVariables();
            //CallDivOnly();
            //CallTryParseWithOutVariables("12");
            //CallTryParseWithOutVariables("Test");

            // Pattern matching
            //IsWithPatterns("test");
            //IsWithPatterns(12);
            //ComplexIsPattern(12);
            //ComplexIsPattern(12.3);
            //ComplexIsPattern("8");
            SwitchWithPattern(new Circle { Radius = 4 });
            SwitchWithPattern(new Rectangle { Width = 24, Height = 12 });
            SwitchWithPattern(new Rectangle { Width = 12, Height = 12 });
            SwitchWithPattern(new Line { });
            SwitchWithPattern("test");

            Read();
        }
    }
}
