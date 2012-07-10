using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


namespace SqlLearnConsole
{
    class Program
    {
        static DataSet ds;

        static void Main(string[] args)
        {

        }

        static DataSet Initialise()
        {
            string conString = "SELECT * FROM EntryLog";
            System.Data.SqlClient.SqlConnection connection;
            connection = new System.Data.SqlClient.SqlConnection(
            
        }

        static void Display()
        {

        }
    }
}
