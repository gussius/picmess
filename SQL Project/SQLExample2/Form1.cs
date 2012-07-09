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
        // Fields required for setting up access to an SQL database.
        System.Data.SqlClient.SqlConnection con;
        System.Data.SqlClient.SqlDataAdapter da;
        DataSet ds1;
        

        // Some helper fields.
        int maxRows = 0;
        int inc = 0;
        DataRow selectedRow;

        public Form1()
        {
            InitializeComponent();
        }

        
        // This section is only run once. i.e. Initialisation.
        private void Form1_Load(object sender, EventArgs e)
        {
            // Create connection to database.
            con = new System.Data.SqlClient.SqlConnection();
            con.ConnectionString = "Data Source=.\\SQLEXPRESS;AttachDbFilename=C:\\Users\\angmcl\\Documents\\picmess\\" +
                "SQL Project\\WorkerDB.mdf;Integrated Security=True;Connect Timeout=30;User Instance=True";

            // Create a new dataset (Subset of SQL database information in memory).
            ds1 = new DataSet();
            
            // Open the connection to the SQL database.
            con.Open();

            // This is the SQL query in string form.
            string sql = "SELECT Worker_ID, RTRIM(first_Name), RTRIM(last_Name), RTRIM(job_Type) From tblWorkers";
            // Create a new SqlDataAdapter for the SQL database. This give us access to the data in the DB based on
            // the query.
            da = new System.Data.SqlClient.SqlDataAdapter(sql, con);

            // Now we want to populate the memory resident dataset with the queried data from the SqlDataAdapter.
            da.Fill(ds1, "Workers");

            // maxRows is initialised here to give us easy access to the number of rows in the "Workers" table in the
            // dataset. Not sure is this is the best way to account for number of rows or if I should just interrogate
            // the dataset.
            maxRows = ds1.Tables["Workers"].Rows.Count;

            // Update the form with the first instance of data that we wish to view. Default: first row etc.
            // Show server version started in status bar.
            UpdateStatusBar();
            lblAction.Text = "SQL Server Version " + con.ServerVersion + " started";
            
            // Close the connection to the SQL database. Do we need to open the connection again during the life of
            // this form?
            con.Close();

            // Initialise the DataGridView in the form. Not sure where this form should reference.
            dbGridView.DataSource = ds1;
            dbGridView.DataMember = "Workers";
            dbGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);
            dbGridView.ReadOnly = true;
            dbGridView.MultiSelect = false;
            dbGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dbGridView.AutoResizeColumns();
            dbGridView.CurrentCell = dbGridView[1, 0];

            // Clears the formatting label in the statusbar.
            lblSpace.Text = "";

            // Initialise DataRow for use with selecting rows in the data grid.
            selectedRow = GetSelectedRow();

            // Fill text boxes with data from selected row.
            FillTextBoxes();
        }

        // Gets the data row in the data set which the data grid view has selected.
        private DataRow GetSelectedRow()
        {
            DataRowView selectedRowView = (DataRowView)dbGridView.SelectedRows[0].DataBoundItem;
            return selectedRowView.Row;
        }

        // This method fills the text boxes with the information from selected row of the datagrid
        // based on the data set information.
        void FillTextBoxes()
        {
            tbFirstName.Text = selectedRow.ItemArray.GetValue(1).ToString();
            tbLastName.Text = selectedRow.ItemArray.GetValue(2).ToString();
            tbJobTitle.Text = selectedRow.ItemArray.GetValue(3).ToString();
        }

        // Updates the status bar and clears action description.
        void UpdateStatusBar()
        {
            lblRecordNo.Text = "Record " + (inc + 1) + " of " + maxRows;
            lblAction.Text = "";
        }

        #region Button Functionality

        // Prepares the text boxes for adding another record.
        private void btnAddNew_Click(object sender, EventArgs e)
        {
            tbFirstName.Clear();
            tbLastName.Clear();
            tbJobTitle.Clear();
            tbFirstName.Focus();
            lblRecordNo.Text = "Record " + (maxRows + 1) + " of " + (maxRows + 1);
        }

        // Adds a new row to the data set based on the contents of the text boxes
        // then updates the database via the data adapter based on the modified data set.
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Command builder to register changes though the data adapter.
            System.Data.SqlClient.SqlCommandBuilder cb;
            cb = new System.Data.SqlClient.SqlCommandBuilder(da);

            // Create a row to add to the dataset.
            DataRow dRow = ds1.Tables["Workers"].NewRow();
            dRow[1] = tbFirstName.Text;
            dRow[2] = tbLastName.Text;
            dRow[3] = tbJobTitle.Text;
            
            // Add row to dataset and update database though data adapter.
            ds1.Tables["Workers"].Rows.Add(dRow);
            da.Update(ds1, "Workers");

            // Increment helper fields to appropriate values, and update status bar.
            maxRows++;
            inc = maxRows - 1;
            UpdateStatusBar();
            lblAction.Text = "Saved to database as Record " + inc;

            // Refresh dataset to allow access to autonumber identity.
            ds1.Clear();
            da.Fill(ds1, "Workers");
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            // Create command builder.
            System.Data.SqlClient.SqlCommandBuilder cb;
            cb = new System.Data.SqlClient.SqlCommandBuilder(da);

            // Creates a new data row based on the selected 
            DataRow row = ds1.Tables["Workers"].Rows[inc];

            row[1] = tbFirstName.Text;
            row[2] = tbLastName.Text;
            row[3] = tbJobTitle.Text;

            da.Update(ds1, "Workers");
            lblAction.Text = "Record " + (inc + 1) + " Updated";

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            System.Data.SqlClient.SqlCommandBuilder cb;
            cb = new System.Data.SqlClient.SqlCommandBuilder(da);

            int row = ds1.Tables["Workers"].Rows.IndexOf(selectedRow);
            DialogResult dResult = MessageBox.Show("Are you sure you want to Delete record " + (row + 1), "Delete", MessageBoxButtons.YesNo);

            if (dResult == DialogResult.Yes)
            {
                ds1.Tables["Workers"].Rows[row].Delete();
                da.Update(ds1, "Workers");

                dbGridView.CurrentCell = dbGridView[0, 0];
                GetSelectedRow();
                FillTextBoxes();
                UpdateStatusBar();

                lblAction.Text = "Record " + (row + 1) + " Deleted";
            }
            else
            {
                lblAction.Text = "No records deleted";
            }
        }

        #endregion

        private void dbGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            selectedRow = GetSelectedRow();
            UpdateStatusBar();
            FillTextBoxes();
        }

    }
}