using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Enumerable enumerates the text lines of a set of files
    /// </summary>
    public class EnumFilesV1 : IEnumerable<String>
    {

        private List<String> _Lines;

        /// <summary>
        /// Create a new enumerabl
        /// </summary>
        public EnumFilesV1(IEnumerable<String> files)
        {
            // Init the files
            this.Files = files.ToArray();

            // Mark the lines list as 'to load' by setting it to null
            _Lines = null;
        }

        void LoadFiles()
        {
            // Create the lines list
            _Lines = new List<string>();

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
                                // Add the line to the list
                                _Lines.Add(line);
                            }
                        }
                    }
                    catch { }   // When an error raised while reading the file, go to the next
                }
            }
        }

        /// <summary>
        /// Returns the lines enumertor
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            // If the lines list is null then read the files
            if (_Lines == null)
            {
                // Load the files
                LoadFiles();
            }

            // Returns the list enumerator
            return _Lines.GetEnumerator();
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
