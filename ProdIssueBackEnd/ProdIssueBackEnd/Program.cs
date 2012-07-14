using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace ProdIssueBackEnd
{
    class Program
    {
        static void Main(string[] args)
        {
            int empId = 0;

            // Check to see if the database is empty.
            using (var db = new ProdIssueDbDataContext())
            {
                if (db.ProdIssues.Count() < 1)
                    Console.WriteLine("ProdIssues table is empty!");
            }

            // Create a new employee instance.
            Employee newEmployee = new Employee()
            {
                firstName = "What",
                lastName = "The"
            };
            // Add the new employee to the database; then check if it was successful.
            using (var db = new ProdIssueDbDataContext())
            {
                Employee firstShem = db.Employees.First(d => d.firstName == "Shem");
                Console.WriteLine("\nfirstShem.firstName = {0}\n", firstShem.firstName);
                db.Employees.DeleteOnSubmit(firstShem);
                db.SubmitChanges();
                
                
                db.Employees.InsertOnSubmit(newEmployee);
                db.SubmitChanges();
                var checkEmployee = db.Employees.Single(check => check.employee_Id == newEmployee.employee_Id);
                if (checkEmployee != null)
                    empId = checkEmployee.employee_Id;
            }

            ProdIssue newIssue = new ProdIssue()
            {
                openDate        = DateTime.Today,
                deptReported    = "1",
                employee_Id     = empId,
                description     = "2",
                deptOrigin      = "3",
                issueType       = "4",
                priorityClass   = "5",
                qualityProcess  = "6",
                qualityRefNo    = 99,
                quarantined     = false,
                reject          = false
            };

            if (AddRow(newIssue))
                Console.WriteLine("New Issue row added successfully");

            PrintIssues(10);

            Console.ReadKey(true);
        }


        static void PrintIssues(int numOfLines)
        {
            Console.WriteLine("Issues:\n");
            
            using (var db = new ProdIssueDbDataContext())
            {
                var issues = db.ProdIssues.Take(numOfLines);
                var selection =
                    from n in issues
                    select n.Employee.firstName;

                Console.WriteLine("variable: \"selection\" holds the following...");
                foreach (var s in selection)
                {
                    Console.WriteLine("{0}", s);
                }

                foreach (var i in issues)
                {
                    Console.WriteLine(  "Date Opened:{0}\nReported In:{1}\nEmployee ID:{2}\n" +
                                        "Description:{3}\nIssue Origin{4}\nIssue Type{5}\n" + 
                                        "Priority Class:{6}\nQuality Process:{7}\nReference No:{8}\n" + 
                                        "Quarantined:{9}\nRejected:{10}\n",
                        i.openDate,
                        i.deptReported,
                        i.employee_Id,
                        i.description,
                        i.deptOrigin,
                        i.issueType,
                        i.priorityClass,
                        i.qualityProcess,
                        i.qualityRefNo,
                        i.quarantined,
                        i.reject
                        );
                }

                

                
            }

        }

        static bool AddRow(ProdIssue newRow)
        {
            using (var db = new ProdIssueDbDataContext())
            {
                db.ProdIssues.InsertOnSubmit(newRow);
                db.SubmitChanges();

                var rowFromDb = db.ProdIssues.Single(a => a.prodIssue_Id == newRow.prodIssue_Id);
                if (rowFromDb == null)
                    return false;
                
                return true;
            }

        }


    }
}