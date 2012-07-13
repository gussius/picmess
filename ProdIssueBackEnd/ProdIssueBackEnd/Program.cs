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
                firstName = "Shem",
                lastName = "Edwards"
            };
            // Add the new employee to the database; then check if it was successful.
            using (var db = new ProdIssueDbDataContext())
            {
                db.Employees.InsertOnSubmit(newEmployee);
                db.SubmitChanges();
                var checkEmployee = db.Employees.SingleOrDefault(check => check.firstName == newEmployee.firstName);
                if (checkEmployee != null)
                    empId = checkEmployee.employee_Id;
            }

            ProdIssue newIssue = new ProdIssue()
            {
                openDate        = DateTime.Today,
                deptReported    = "Assembly",
                employee_Id     = empId,
                description     = "This is a description",
                deptOrigin      = "Welding",
                issueType       = "Process",
                priorityClass   = "Functional",
                qualityProcess  = "NCR",
                qualityRefNo    = 84,
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

                var rowFromDb = db.ProdIssues.Select(a => a.openDate == newRow.openDate);
                if (rowFromDb == null)
                    return false;
                
                return true;
            }

        }

    }
}