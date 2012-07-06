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
        int maxRows = 0;
        int inc = 0;
        string index = "0";
        DataTable dTable = null;

        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            btnNext.Focus();
            con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=C:\\Users\\angmcl\\Documents\\picmess\\SQL Project\\WorkerDB.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";

            ds1 = new DataSet();
                        
            con.Open();

            string sql = "SELECT * From tblWorkers";
            da = new System.Data.SqlClient.SqlDataAdapter(sql, con);

            da.Fill(ds1, "Workers");
            maxRows = ds1.Tables["Workers"].Rows.Count;
            UpdateStatusBar();
            con.Close();

            dbGridView.DataSource = ds1;
            dbGridView.DataMember = "Workers";
            dbGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            dbGridView.ReadOnly = true;
            dbGridView.MultiSelect = false;
            dbGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dbGridView.CurrentCell = dbGridView[1, 0];

            dTable = ds1.Tables["Workers"];
            DataColumn[] keys = new DataColumn[1];
            DataColumn column = new DataColumn();
            column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "tableID";
            dTable.Columns.Add(column);
            keys[0] = column;
            dTable.PrimaryKey = keys;

            ShowData();



        }

        void ShowData()
        {
            //DataRow dRow = ds1.Tables["Workers"].Rows[inc];
            DataRow dRow = dTable.Rows.Find(

            tbFirstName.Text = dRow.ItemArray.GetValue(1).ToString().TrimEnd(' ');
            tbLastName.Text = dRow.ItemArray.GetValue(2).ToString().TrimEnd(' ');
            tbJobTitle.Text = dRow.ItemArray.GetValue(3).ToString().TrimEnd(' ');
            dbGridView.CurrentCell = dbGridView.SelectedRows[0].Cells[0];
        }

        void UpdateStatusBar()
        {
            lblRecordNo.Text = "Record " + (inc + 1) + " of " + maxRows;
            lblAction.Text = "";
            lblSpace.Text = "";
        }

        #region Button Functionality
        private void btnNext_Click(object sender, EventArgs e)
        {
            if (inc < maxRows - 1)
            {
                inc++;
                ShowData();
                UpdateStatusBar();
            }
            else
            {
                lblAction.Text = "Already at last record";
            }
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            if (inc > 0)
            {
                inc--;
                ShowData();
                UpdateStatusBar();
            }
            else
            {
                lblAction.Text = "Already at first record";
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            inc = 0;
            ShowData();
            UpdateStatusBar();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            inc = maxRows - 1;
            ShowData();
            UpdateStatusBar();
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            tbFirstName.Clear();
            tbLastName.Clear();
            tbJobTitle.Clear();
            tbFirstName.Focus();
            lblRecordNo.Text = "Record " + (maxRows + 1) + " of " + (maxRows + 1);
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

            maxRows++;
            inc = maxRows - 1;
            UpdateStatusBar();
            lblAction.Text = "Saved to database";

            ds1.Clear();
            da.Fill(ds1, "Workers");
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
            lblAction.Text = "Record " + (inc + 1) + " Updated";

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            System.Data.SqlClient.SqlCommandBuilder cb;
            cb = new System.Data.SqlClient.SqlCommandBuilder(da);
            int tempInc = inc + 1;

            DialogResult dResult = MessageBox.Show("Are you sure you want to Delete record " + (inc + 1), "Delete", MessageBoxButtons.YesNo);

            if (dResult == DialogResult.Yes)
            {
                ds1.Tables["Workers"].Rows[inc].Delete();
                maxRows--;
                inc = 0;
                ShowData();
                UpdateStatusBar();

                da.Update(ds1, "Workers");

                lblAction.Text = "Record " + tempInc + " Deleted";
            }
            else
            {
                lblAction.Text = "No records deleted";
            }
        }
        #endregion

        private void dbGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

    }
}