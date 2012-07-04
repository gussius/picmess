using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SQLExample2
{
    public partial class Form1 : Form
    {   
        System.Data.SqlClient.SqlConnection con;
        DataSet ds1;
        System.Data.SqlClient.SqlDataAdapter da;
        int MaxRows = 0;
        int inc = 0;


        
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            btnNext.Focus();
            con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=C:\\WorkerDB.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";

            ds1 = new DataSet();
                        
            con.Open();

            string sql = "SELECT * From tblWorkers";
            da = new System.Data.SqlClient.SqlDataAdapter(sql, con);

            da.Fill(ds1, "Workers");
            NavigateRecords();

            MaxRows = ds1.Tables["Workers"].Rows.Count;
            UpdateStatusBar();

            con.Close();
        }

        void NavigateRecords()
        {
            DataRow dRow = ds1.Tables["Workers"].Rows[inc];

            tbFirstName.Text = dRow.ItemArray.GetValue(1).ToString();
            tbLastName.Text = dRow.ItemArray.GetValue(2).ToString();
            tbJobTitle.Text = dRow.ItemArray.GetValue(3).ToString();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (inc < MaxRows - 1)
            {
                inc++;
                NavigateRecords();
                UpdateStatusBar();
            }
            else
            {
                toolStripStatusLabel1.Text = "There are no more records";
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (inc > 0)
            {
                inc--;
                NavigateRecords();
                UpdateStatusBar();
            }
            else
            {
                toolStripStatusLabel1.Text = "Already at first record";
            }
        }

        void UpdateStatusBar()
        {
            toolStripStatusLabel1.Text = "Record " + (inc + 1) + " of " + MaxRows;
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            inc = 0;
            NavigateRecords();
            UpdateStatusBar();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            inc = MaxRows - 1;
            NavigateRecords();
            UpdateStatusBar();
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            tbFirstName.Clear();
            tbLastName.Clear();
            tbJobTitle.Clear();
            tbFirstName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            System.Data.SqlClient.SqlCommandBuilder cb;
            cb = new System.Data.SqlClient.SqlCommandBuilder(da);
            
            DataRow dRow = ds1.Tables["Workers"].NewRow();
            dRow[1] = tbFirstName.Text;
            dRow[2] = tbLastName.Text;
            dRow[3] = tbJobTitle.Text;
            ds1.Tables["Workers"].Rows.Add(dRow);
            da.Update(ds1, "Workers");

            MaxRows++;
            inc = MaxRows - 1;
            UpdateStatusBar();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            System.Data.SqlClient.SqlCommandBuilder cb;
            cb = new System.Data.SqlClient.SqlCommandBuilder(da);

            System.Data.DataRow dRow2 = ds1.Tables["Workers"].Rows[inc];

            dRow2[1] = tbFirstName.Text;
            dRow2[2] = tbLastName.Text;
            dRow2[3] = tbJobTitle.Text;

            da.Update(ds1, "Workers");
            toolStripStatusLabel1.Text = "Current Record Updated";

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            System.Data.SqlClient.SqlCommandBuilder cb;
            cb = new System.Data.SqlClient.SqlCommandBuilder(da);

            DialogResult dResult = MessageBox.Show("Are you sure you want to Delete record " + (inc + 1), "Delete", MessageBoxButtons.YesNo);

            if (dResult == DialogResult.Yes)
            {
                ds1.Tables["Workers"].Rows[inc].Delete();
                MaxRows--;
                inc = 0;
                NavigateRecords();

                da.Update(ds1, "Workers");

                toolStripStatusLabel1.Text = "Record Deleted";
            }
        }
    }
}
