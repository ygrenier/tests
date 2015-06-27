using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Enumerable enumerates the text lines of a set of files, rebuilding the list on each call
    /// </summary>
    public class EnumFilesV2 : IEnumerable<String>
    {

        /// <summary>
        /// Create a new enumerable
        /// </summary>
        public EnumFilesV2(IEnumerable<String> files)
        {
            // Init the files
            this.Files = files.ToArray();
        }

        IList<String> LoadFiles()
        {
            // Create the lines list
            var result = new List<string>();

            if (this.Files != null)
            {
                // For each file
                foreach (var file in Files)
                {
                    try
                    {
                        // Open a file text reader
                        using (var reader = new StreamReader(file))
                        {
                            // Read each line of the file
                            String line;
                            while ((line = reader.ReadLine()) != null)
                            {
                                // Add the line of the list
                                result.Add(line);
                            }
                        }
                    }
                    catch { }   // When an error raised while reading the file, go to the next
                }
            }

            // Returns the list
            return result;
        }

        /// <summary>
        /// Returns the enumerator of the lines
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            // Build the list
            var list = LoadFiles();

            // Return the list enumerator
            return list.GetEnumerator();
        }

        /// <summary>
        /// Implements IEnumerator.GetEnumerator() (non generic version)
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// List of the files
        /// </summary>
        public String[] Files { get; private set; }

    }

}
