using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enumerables
{
    class Program
    {

        /// <summary>
        /// Returns a list of elements as enumerable
        /// </summary>
        /// <param name="asDisposable">Indicates is we returns an enumerable with disposable enumerator</param>
        static IEnumerable<int> GetItems(bool asDisposable = false)
        {
            var items = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            return asDisposable ? new EnumerableWithEnumeratorDisposable<int>(items, () => Console.WriteLine("Enumerator disposed")).AsEnumerable() : items;
        }

        /// <summary>
        /// List of the soure files
        /// </summary>
        static IEnumerable<String> GetTestFileNames()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            return new String[]{
                Path.Combine(dir, "Files", "Fichier1.txt"),
                Path.Combine(dir, "Files", "Fichier2.txt"),
                Path.Combine(dir, "Files", "Fichier3.txt")
            };
        }

        /// <summary>
        /// List of persons
        /// </summary>
        static IEnumerable<Person> GetPersons()
        {
            yield return new Person() {
                Name = "Toto",
                BirthDay = null,
                IsFamily = true
            };
            yield return new Person() {
                Name = "Titi",
                BirthDay = new DateTime(2010, 11, 27),
                IsFamily = false
            };
            yield return new Person() {
                Name = "Tata",
                BirthDay = null,
                IsFamily = false
            };
            yield return new Person() {
                Name = "Tutu",
                BirthDay = new DateTime(1994, 8, 11),
                IsFamily = true
            };
        }

        #region Loop samples

        /// <summary>
        /// foreach sample
        /// </summary>
        static void ForEach()
        {
            // Get the enumerable
            IEnumerable<Int32> enumerable = GetItems();

            // Iterates each element in 'enumerable'
            foreach (int elm in enumerable)
            {
                // ..
                Console.WriteLine(elm);
            }
        }

        /// <summary>
        /// foreach sample with disposable enumerator
        /// </summary>
        static void ForEachDisposable()
        {
            // Get the enumerable
            IEnumerable<Int32> enumerable = GetItems(true);

            // Iterates each element in 'enumerable'
            foreach (int elm in enumerable)
            {
                // ..
                Console.WriteLine(elm);
            }
        }

        /// <summary>
        /// Basic iteration sample
        /// </summary>
        static void IterationBase()
        {
            // Get the enumerable
            IEnumerable<Int32> enumerable = GetItems();

            // Get a new enumerator
            IEnumerator<Int32> enumerator = enumerable.GetEnumerator();
            // While the enumerator move
            while (enumerator.MoveNext())
            {
                Int32 elm = enumerator.Current;
                // ..
                Console.WriteLine(elm);
            }
        }

        /// <summary>
        /// Iteration sample with IDiposable management like 'foreach' do
        /// </summary>
        static void IterationBaseWithDispose()
        {
            // Get the enumerable
            IEnumerable<Int32> enumerable = GetItems(true);

            // Get a new enumerator
            IEnumerator<Int32> enumerator = enumerable.GetEnumerator();
            try
            {
                // While the enumerator move
                while (enumerator.MoveNext())
                {
                    Int32 elm = enumerator.Current;
                    // ..
                    Console.WriteLine(elm);
                }
            }
            finally
            {
                // Check if the enumerator is disposable
                IDisposable disp = enumerator as IDisposable;
                // If true the we dispose it
                if (disp != null)
                {
                    disp.Dispose();
                }
            }
        }

        /// <summary>
        /// foreach sample with an object not implementing IEnumerable/IEnumerator
        /// </summary>
        static void ForEachWithoutIEnumerable()
        {
            // Get the enumerable
            var enumerable = new FakeEnumerable();

            // Iterates each element in 'enumerable'
            foreach (int elm in enumerable)
            {
                // ..
                Console.WriteLine(elm);
            }
        }

        /// <summary>
        /// Test ReverseEnumerator
        /// </summary>
        static void TestReverse()
        {
            // ReverseEnumerator require an IList<>
            IList<Int32> list = new List<Int32>(GetItems());

            // Create the reverse enumerable
            IEnumerable<Int32> enumerable = new ReverseEnumerable<Int32>(list);

            // Iterates each element in 'enumerable'
            foreach (int elm in enumerable)
            {
                // ..
                Console.WriteLine(elm);
            }

            // Iterates each element and stop the loop when find the '5' element
            foreach (int elm in enumerable)
            {
                if (elm == 5) break;
                // ..
                Console.WriteLine(elm);
            }

        }

        #endregion

        #region Complex enumerators samples

        /// <summary>
        /// Test of files enumerator first version
        /// </summary>
        static void TestFilesV1()
        {
            // Create the enumerable with tests files
            var enumerable = new EnumFilesV1(GetTestFileNames());

            // Iterates the enumerable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

        }

        /// <summary>
        /// Test the files enumerator better than the first version
        /// </summary>
        static void TestFilesV2()
        {
            // Create the enumerable with tests files
            var enumerable = new EnumFilesV2(GetTestFileNames());

            // Iterates the enumerable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

        }

        /// <summary>
        /// Test the files enumerator more better
        /// </summary>
        static void TestFilesV3()
        {
            // Create the enumerable with tests files
            var enumerable = new EnumFilesV3(GetTestFileNames());

            // Iterates the enumerable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

            // Iterates the enumerable and force a break
            int i = 0;
            foreach (var line in enumerable)
            {
                if (i++ >= 4) break;
                Console.WriteLine(line);
            }
        }

        #endregion

        #region Yield method sample

        /// <summary>
        /// Open a new file or null if an error raised
        /// </summary>
        static StreamReader OpenFile(String file)
        {
            try
            {
                return new StreamReader(file);
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// Enumerates the file with a Yield method
        /// </summary>
        static IEnumerable<String> EnumFilesYield(IEnumerable<String> files)
        {
            if (files != null)
            {
                // For each files
                foreach (var file in files)
                {
                    // Open a file text reader
                    using (var reader = OpenFile(file))
                    {
                        // reader can be null if an error raised
                        if (reader != null)
                        {
                            // Read each line of the file
                            String line;
                            do
                            {
                                // Read the line, if an error raised we stop the loop
                                try
                                {
                                    line = reader.ReadLine();
                                }
                                catch
                                {
                                    break;
                                }
                                // Returns the line in the 'enumerable'
                                if (line != null)
                                    yield return line;
                            } while (line != null);// Loop while we have a line
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Test the Yield iteration
        /// </summary>
        static void TestFilesYield()
        {
            // Get an enumerable with the files test
            var enumerable = EnumFilesYield(GetTestFileNames());

            // Iterates the enumerable
            foreach (var line in enumerable)
            {
                Console.WriteLine(line);
            }

            // Iterates the enumerable and force a break
            int i = 0;
            foreach (var line in enumerable)
            {
                if (i++ >= 4) break;
                Console.WriteLine(line);
            }

        }
        /// <summary>
        /// Same as <see cref="EnumFilesYield"/> but without the exceptions management
        /// </summary>
        static IEnumerable<String> EnumFilesYieldUnprotected(IEnumerable<String> files)
        {
            if (files != null)
            {
                // For each file
                foreach (var file in files)
                {
                    // Open a file text reader
                    using (var reader = new StreamReader(file))
                    {
                        // Read each line of the file
                        String line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            // Returns the line in the enumerable
                            yield return line;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Complex Yield method
        /// </summary>
        static IEnumerable<String> YieldComplex()
        {
            Random rnd = new Random();

            // Direct emit
            yield return "Start";

            // Emit in a switch
            for (int i = 0; i < 10; i++)
            {
                switch (rnd.Next(4))
                {
                    case 1:
                        yield return "Funky 1";
                        break;
                    case 2:
                        continue;
                    case 3:
                        yield return "<<<<";
                        // Emit an another enumerable
                        foreach (var line in EnumFilesYield(GetTestFileNames()))
                        {
                            // Set a condition
                            if (line.Contains("x"))
                                yield return line;
                        }
                        yield return ">>>>";
                        break;
                    case 0:
                    default:
                        yield return "Funky 0";
                        break;
                }
            }

            // Direct emit
            yield return "End";
        }

        /// <summary>
        /// Tesst the complex Yield method
        /// </summary>
        static void TestYieldComplex()
        {
            foreach (var item in YieldComplex())
                Console.WriteLine(item);
        }

        #endregion

        #region LINQ samples

        /// <summary>
        /// Simple LINQ request
        /// </summary>
        static void TestLinqRequest()
        {
            var query = from p in GetPersons()
                        where p.IsFamily
                        orderby p.Name
                        select p;

            foreach (var p in query)
                Console.WriteLine(p.Name);

            foreach (var p in query.Take(2))
                Console.WriteLine(p.Name);
        }

        /// <summary>
        /// Simple LINQ request as fluent mode
        /// </summary>
        static void TestLinqRequestAsFluent()
        {
            var query = GetPersons()
                .Where(p => p.IsFamily)
                .OrderBy(p => p.Name)
                ;

            foreach (var p in query)
                Console.WriteLine(p.Name);

            foreach (var p in query.Take(2))
                Console.WriteLine(p.Name);
        }

        //static void TestStats()
        //{
        //    var q = from stats in GetStats()
        //            where stat.Date >= lastRefresh
        //            orderby stat.Date
        //            select stats;

        //    foreach (var stats in q)
        //    {
        //        // Display the stats
        //    }

        //    lblLastUpdate.Text = String.Format("{0} : {1} new statistics", DateTime.Now, q.Count());
        //    lastRefresh = DateTime.Now;

        //}

        /// <summary>
        /// Where operator simulation
        /// </summary>
        static IEnumerable<Person> SimulWhere()
        {
            foreach (var p in GetPersons())
            {
                if (p.IsFamily)
                    yield return p;
            }
        }

        /// <summary>
        /// OrderBy operator simulation
        /// </summary>
        static IEnumerable<Person> SimulOrderBy()
        {
            List<Person> result = new List<Person>();
            result.AddRange(SimulWhere());
            result.Sort((x, y) => String.Compare(x.Name, y.Name));
            foreach (var p in result)
                yield return p;
        }

        /// <summary>
        /// LINQ simulation
        /// </summary>
        static void TestSimulLinq()
        {
            var query = SimulOrderBy();

            foreach (var p in query)
                Console.WriteLine(p.Name);

            foreach (var p in query.Take(2))
                Console.WriteLine(p.Name);
        }

        #endregion

        static void WaitAndPress()
        {
            Console.WriteLine("Press a key to continue...");
            Console.ReadKey();
            Console.WriteLine();
        }
        static void Main(string[] args)
        {
            #region Loop samples
            Console.WriteLine("* ForEach loop");
            ForEach();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* ForEach loop whith an IDisposable");
            ForEachDisposable();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Basic iteration (ForEach simulation)");
            IterationBase();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Basic iteration with a disposable (ForEach simulation)");
            IterationBaseWithDispose();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* ForEach loop with a non IEnumerable object");
            ForEachWithoutIEnumerable();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Test ReverseEnumerator");
            TestReverse();
            Console.WriteLine();
            WaitAndPress();
            #endregion

            #region Complex enumerators and yield method samples
            Console.WriteLine("* Files enumerator V1 (basic way)");
            TestFilesV1();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Files enumerator V2 (better version than the first version)");
            TestFilesV2();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Files enumerator V3 (stream way version)");
            TestFilesV3();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Files enumerator with a Yield method");
            TestFilesYield();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Complex Yield method");
            TestYieldComplex();
            Console.WriteLine();
            WaitAndPress();
            #endregion

            #region LINQ samples
            Console.WriteLine("* LINQ request");
            TestLinqRequest();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* Fluent LINQ request");
            TestLinqRequestAsFluent();
            Console.WriteLine();
            WaitAndPress();

            Console.WriteLine("* LINQ simulation with methods");
            TestSimulLinq();
            Console.WriteLine();
            WaitAndPress();
            #endregion
        }
    }
}
