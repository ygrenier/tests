using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrgPart3
{
    class Program
    {
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
        //        // Affichage de la stat
        //    }

        //    lblLastUpdate.Text = String.Format("{0} : {1} nouvelles statistiques", DateTime.Now, q.Count());
        //    lastRefresh = DateTime.Now;

        //}

        static IEnumerable<Person> SimulWhere()
        {
            foreach (var p in GetPersons())
            {
                if(p.IsFamily)
                    yield return p;
            }
        }

        static IEnumerable<Person> SimulOrderBy()
        {
            List<Person> result = new List<Person>();
            result.AddRange(SimulWhere());
            result.Sort((x, y) => String.Compare(x.Name, y.Name));
            foreach (var p in result)
                yield return p;
        }

        static void TestSimulLinq()
        {
            var query = SimulOrderBy();
            
            foreach (var p in query)
                Console.WriteLine(p.Name);

            foreach (var p in query.Take(2))
                Console.WriteLine(p.Name);
        }

        static void Main(string[] args)
        {
            //TestLinqRequest();
            //TestLinqRequestAsFluent();
            TestSimulLinq();

            Console.ReadLine();
        }
    }
}
