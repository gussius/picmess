using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProdIssueBackEnd
{
    class Program
    {
        static void Main(string[] args)
        {
            int count = 0;

            using (var db = new ProdIssueDbDataContext())
                count = db.ProdIssues.Count();

            Console.ReadKey(true);

            using (var db = new ProdIssueDbDataContext())
            {
                var blob = new Employee()
                {
                    firstName = "Harry",
                    lastName = "Smith"
                };
            }

        }


        static void PrintIssues(int numOfLines)
        {
            using (var db = new ProdIssueDbDataContext())
            {
                var issues = db.ProdIssues.Take(numOfLines);
                foreach (var i in issues)
                {
                    Console.WriteLine("{0}", i.description);
                }
            }

        }

    }
}