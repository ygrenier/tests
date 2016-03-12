using System;
using System.Collections.Generic;
using System.IO;
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
        /// Preload the Lua DLL
        /// </summary>
        static Lua()
        {
            // Check the size of the pointer
            String folder = IntPtr.Size == 8 ? "x64" : "x86";
            // Build the full library file name
            String libraryFile = Path.Combine(Path.GetDirectoryName(typeof(Lua).Assembly.Location), folder, LuaDllName);
            // Load the library
            var res = LoadLibrary(libraryFile);
            if (res == IntPtr.Zero)
                throw new InvalidOperationException("Failed to load the library.");
            System.Diagnostics.Debug.WriteLine(libraryFile);
        }

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, SetLastError = false)] 
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);

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
