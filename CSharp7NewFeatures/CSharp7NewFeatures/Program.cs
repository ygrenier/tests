using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        static void Main(string[] args)
        {
            //CallDivAndModulo();
            //CallDivAndModuloWithOutVariables();
            //CallDivOnly();
            //CallTryParseWithOutVariables("12");
            //CallTryParseWithOutVariables("Test");

            Console.Read();
        }
    }
}
