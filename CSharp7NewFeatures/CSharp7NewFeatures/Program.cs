﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
            if (o is int i || (o is string s && int.TryParse(s, out i)))
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

        #region Tuples

        static (string, int, string) ReturnsTuple() // tuple return type
        {
            return ("un", 2, "trois"); // tuple literal
        }

        static void UseTuple()
        {
            var values = ReturnsTuple();
            WriteLine($"{values.Item1}, {values.Item2}, {values.Item3}");
        }

        static (string first, int, string last) ReturnsTupleWithNames() // tuple with names
        {
            return ("un", 2, "trois"); // tuple literal
        }

        static void UseTupleWithNames()
        {
            var values = ReturnsTupleWithNames();
            WriteLine($"{values.first}, {values.Item2}, {values.last}");
        }

        static void UseTupleWithExplicitNames()
        {
            var values = (f: "un", second: 2, last: "trois");
            WriteLine($"{values.f}, {values.second}, {values.last}");
        }
        #endregion

        #region Deconstruction

        static void DeconstructingDeclaration()
        {
            (string f, int c, string l) = ReturnsTuple();   // deconstructing
            WriteLine($"{f}, {c}, {l}");
        }

        static void DeconstructingDeclarationVarInside()
        {
            (var f, var c, var l) = ReturnsTuple();   // var inside
            WriteLine($"{f}, {c}, {l}");
        }

        static void DeconstructingDeclarationVarOutside()
        {
            var (f, c, l) = ReturnsTuple();   // var outside
            WriteLine($"{f}, {c}, {l}");
        }

        static void DeconstructingAssignment()
        {
            string first = "first";
            int count = -5;
            string last = "last";

            (first, count, last) = ReturnsTuple();  // deconstructing assignment
            WriteLine($"{first}, {count}, {last}");
        }

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

        static void DeconstructingWithIgnores()
        {
            (_, var c, _) = ReturnsTuple();
            WriteLine($"{c}");
        }
        #endregion

        #region Locals functions

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

        #endregion

        #region Litterals

        static void Litterals()
        {
            int dec = 12_34;        // decimal
            int hex = 0xA1_23;      // hexadecimal
            int boo = 0b1010_1100;  // binaire

            WriteLine($"{dec}, {hex}, {boo}");
        }

        #endregion

        #region Ref returns and local

        static ref int Find(int number, int[] numbers)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                if (numbers[i] == number)
                {
                    return ref numbers[i]; // return the storage location, not the value
                }
            }
            throw new IndexOutOfRangeException($"{nameof(number)} not found");
        }

        static void RefReturns()
        {
            int[] array = { 1, 15, -39, 0, 7, 14, -12 };
            ref int place = ref Find(7, array); // aliases 7's place in the array
            place = 9; // replaces 7 with 9 in the array
            WriteLine(array[4]); // prints 9
        }

        #endregion

        #region Async

        //static ValueTask<int> AsyncOtherType()
        //{

        //}

        #endregion

        #region Expression bodies
        class Person
        {
            private static ConcurrentDictionary<int, string> names = new ConcurrentDictionary<int, string>();
            private int id = GetId();

            static int GetId() => 123;

            public Person(string name) => names.TryAdd(id, name); // constructors
            ~Person() => names.Clear();                           // destructors

            public string Name
            {
                get => names[id];                                 // getters
                set => names[id] = value;                         // setters
            }
        }
        #endregion

        class User
        {
            public string Name { get; }
            public User(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
            public string GetFirstName()
            {
                var parts = Name.Split(new string[] { " " }, StringSplitOptions.None);
                return (parts.Length > 0) ? parts[0] : throw new InvalidOperationException("No name!");
            }
            public string GetLastName() => throw new NotImplementedException();
        }

        static void Main(string[] args)
        {
            #region Out variables
            //CallDivAndModulo();
            //CallDivAndModuloWithOutVariables();
            //CallDivOnly();
            //CallTryParseWithOutVariables("12");
            //CallTryParseWithOutVariables("Test");
            #endregion

            #region Pattern matching
            //IsWithPatterns("test");
            //IsWithPatterns(12);
            //ComplexIsPattern(12);
            //ComplexIsPattern(12.3);
            //ComplexIsPattern("8");
            //SwitchWithPattern(new Circle { Radius = 4 });
            //SwitchWithPattern(new Rectangle { Width = 24, Height = 12 });
            //SwitchWithPattern(new Rectangle { Width = 12, Height = 12 });
            //SwitchWithPattern(new Line { });
            //SwitchWithPattern("test");
            #endregion

            #region Tuples
            //UseTuple();
            //UseTupleWithNames();
            //UseTupleWithExplicitNames();
            #endregion

            #region Deconstruction
            //DeconstructingDeclaration();
            //DeconstructingDeclarationVarInside();
            //DeconstructingDeclarationVarOutside();
            //DeconstructingAssignment();
            //DeconstructingObject();
            //DeconstructingWithIgnores();
            #endregion

            #region Locals functions
            //LocalFunc();
            #endregion

            #region Litterals
            //Litterals();
            #endregion

            #region Ref returns
            RefReturns();
            #endregion

            Read();
        }
    }
}
