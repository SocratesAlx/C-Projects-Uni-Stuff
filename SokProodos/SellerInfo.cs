using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SokProodos
{
    public partial class SellerInfo: Form
    {
        private string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";
        public SellerInfo()
        {
            InitializeComponent();
            LoadSellers();

            comboBoxSellers.SelectedIndexChanged += new EventHandler(comboBoxSellers_SelectedIndexChanged);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(45, 50, 60),  
                Color.FromArgb(150, 155, 165), 
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        private void LoadSellers()
        {
            comboBoxSellers.Items.Clear();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT sp.BusinessEntityID, p.FirstName + ' ' + p.LastName AS SellerName 
                        FROM Sales.SalesPerson sp
                        JOIN Person.Person p ON sp.BusinessEntityID = p.BusinessEntityID
                        ORDER BY SellerName ASC;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBoxSellers.Items.Add(new KeyValuePair<int, string>(
                                reader.GetInt32(0), reader.GetString(1)));
                        }
                    }

                    comboBoxSellers.DisplayMember = "Value";
                    comboBoxSellers.ValueMember = "Key";
                    comboBoxSellers.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    comboBoxSellers.AutoCompleteSource = AutoCompleteSource.ListItems;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading sellers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadSellerInfo(int sellerId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            sp.BusinessEntityID,
                            p.FirstName + ' ' + p.LastName AS SellerName,
                            sp.SalesQuota,
                            sp.Bonus,
                            sp.CommissionPct,
                            sp.SalesYTD,
                            sp.SalesLastYear,
                            st.Name AS Territory,
                            e.NationalIDNumber,
                            e.LoginID,
                            e.JobTitle,
                            e.HireDate
                        FROM Sales.SalesPerson sp
                        JOIN Person.Person p ON sp.BusinessEntityID = p.BusinessEntityID
                        LEFT JOIN Sales.SalesTerritory st ON sp.TerritoryID = st.TerritoryID
                        LEFT JOIN HumanResources.Employee e ON sp.BusinessEntityID = e.BusinessEntityID
                        WHERE sp.BusinessEntityID = @SellerID;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@SellerID", sellerId);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            // Assign data to DataGridView
                            dataGridViewSellerInfo.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading seller info: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void comboBoxSellers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxSellers.SelectedItem != null)
            {
                int sellerId = ((KeyValuePair<int, string>)comboBoxSellers.SelectedItem).Key;

                

                LoadSellerInfo(sellerId);
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }
    }
}
