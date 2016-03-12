using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PInvokeMultiPlatform
{
    /// <summary>
    /// Lua DLL Wrapper
    /// </summary>
    public static class Lua
    {

        /// <summary>
        /// DLL Name
        /// </summary>
        const String LuaDllName = "Lua53.dll";

        /// <summary>
        /// Get Lua version
        /// </summary>
        /// <param name="L">Lua state. Can be null.</param>
        /// <returns>Number represents version</returns>
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "lua_version")]
        private extern static IntPtr _lua_version(IntPtr L);
        public static double lua_version(IntPtr L)
        {
            var ptr = _lua_version(L);
            return (double)Marshal.PtrToStructure(ptr, typeof(double));
        }

    }
}
