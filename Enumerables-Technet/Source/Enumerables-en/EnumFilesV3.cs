using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    /// <summary>
    /// Enumerable enumerates the text lines of a set of files via an enumerator
    /// </summary>
    public class EnumFilesV3 : IEnumerable<String>
    {

        /// <summary>
        /// Create a new enumerable
        /// </summary>
        public EnumFilesV3(IEnumerable<String> files)
        {
            // Init the files
            this.Files = files.ToArray();
        }

        /// <summary>
        /// Returns the lines enumerator
        /// </summary>
        public IEnumerator<string> GetEnumerator()
        {
            // Returns a new enumerator 
            return new FileEnumerator(Files);
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

    /// <summary>
    /// File enumerator
    /// </summary>
    class FileEnumerator : IEnumerator<String>
    {
        Func<bool> _CurrentState = null;
        int _CurrentFilePos;
        String _CurrentFileName;
        TextReader _CurrentFile;

        /// <summary>
        /// Create a new enumerator
        /// </summary>
        public FileEnumerator(String[] files)
        {
            // Init the files
            this.Files = files;
            // Init the enumerator
            Current = null;
            _CurrentFilePos = 0;
            _CurrentFileName = null;
            _CurrentFile = null;
            // The enumerator state is to open the next file to read
            _CurrentState = OpenNextFileState;
        }

        /// <summary>
        /// Dispose some resources
        /// </summary>
        public void Dispose()
        {
            // If we have a file opened we close it
            if (_CurrentFile != null)
            {
                _CurrentFile.Dispose();
                _CurrentFile = null;
            }
            // Set the state to 'Completed'
            _CurrentState = CompletedState;
        }

        /// <summary>
        /// Try to open the next file in the list
        /// </summary>
        bool GetNextFile()
        {
            String filename = null;
            TextReader file = null;
            while (file == null && Files != null && _CurrentFilePos < Files.Length)
            {
                try
                {
                    filename = Files[_CurrentFilePos++];
                    file = new StreamReader(filename);
                }
                catch { }
            }
            // If we have a file opened
            if (file != null)
            {
                _CurrentFileName = filename;
                _CurrentFile = file;
                return true;
            }
            // Else we don't found
            return false;
        }

        /// <summary>
        /// Open the next file
        /// </summary>
        bool OpenNextFileState()
        {
            // If we don't have file, we stop all
            if (!GetNextFile())
            {
                Current = null;
                // Go to the state 'Completed'
                _CurrentState = CompletedState;
                // We finished
                return false;
            }

            // Go to the state ReadNextLine
            _CurrentState = ReadNextLineState;

            // Read the first line
            return _CurrentState();
        }

        /// <summary>
        /// Read line state
        /// </summary>
        bool ReadNextLineState()
        {
            try
            {
                // Read the next line in the file
                String line = _CurrentFile.ReadLine();
                // If the line is not null we process it
                if (line != null)
                {
                    Current = line;
                    return true;
                }
                // The line is null so we reach the end of the file, we release the resource
            }
            catch
            {
                // If an error raised while reading we close the current file to avoid infinite loops

            }
            // Release resources to go to the next file
            _CurrentFile.Dispose();
            _CurrentFile = null;
            _CurrentFileName = null;
            // Go to the state 'OpenNextFile'
            _CurrentState = OpenNextFileState;
            // Process the next state
            return _CurrentState();
        }

        /// <summary>
        /// The iteration is finished, so we returns always false
        /// </summary>
        bool CompletedState()
        {
            return false;
        }

        /// <summary>
        /// We don"t used this method
        /// </summary>
        public void Reset()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// We move to the next line
        /// </summary>
        public bool MoveNext()
        {
            // Process the next state
            return _CurrentState();
        }

        /// <summary>
        /// List odf the files
        /// </summary>
        public String[] Files { get; private set; }

        /// <summary>
        /// Current value
        /// </summary>
        public string Current { get; private set; }
        object System.Collections.IEnumerator.Current { get { return Current; } }

    }

}
