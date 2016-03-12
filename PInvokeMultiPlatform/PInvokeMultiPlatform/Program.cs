using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PInvokeMultiPlatform
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Version {0}", Lua.lua_version(IntPtr.Zero));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Err ({0}): {1}", ex.GetType().Name, ex.GetBaseException().Message);
            }
            Console.Read();
        }
    }
}
