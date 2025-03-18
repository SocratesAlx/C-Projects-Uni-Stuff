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
    public partial class SellersForm: Form
    {
        public SellersForm()
        {
            InitializeComponent();
            LoadSalesTerritories();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(45, 50, 60),   // **Dark Grey at the Top**
                Color.FromArgb(150, 155, 165), // **Lighter Grey at the Bottom**
                LinearGradientMode.Vertical)) // **Vertical transition**
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }



        private void Button_MouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(114, 137, 218); // Lighter blue on hover
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(88, 101, 242); // Normal state
        }

        private void LoadSalesTerritories()
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT TerritoryID, Name FROM Sales.SalesTerritory";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBoxTerritory.Items.Add(new KeyValuePair<int, string>(
                                reader.GetInt32(0), reader.GetString(1)));
                        }
                    }

                    comboBoxTerritory.DisplayMember = "Value";
                    comboBoxTerritory.ValueMember = "Key";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading sales territories: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InsertSeller(string sellerName, decimal salesQuota, decimal bonus, decimal commissionPct, decimal salesYTD, decimal salesLastYear, int territoryId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // ✅ Step 1: Insert into Person.BusinessEntity to get BusinessEntityID
                    string businessEntityQuery = "INSERT INTO Person.BusinessEntity (rowguid, ModifiedDate) VALUES (NEWID(), GETDATE()); SELECT SCOPE_IDENTITY();";
                    int businessEntityId;
                    using (SqlCommand businessCmd = new SqlCommand(businessEntityQuery, connection))
                    {
                        businessEntityId = Convert.ToInt32(businessCmd.ExecuteScalar());
                    }

                    // ✅ Step 2: Insert into Person.Person
                    string personQuery = @"
            INSERT INTO Person.Person (BusinessEntityID, PersonType, FirstName, LastName, ModifiedDate)
            VALUES (@BusinessEntityID, 'EM', @FirstName, @LastName, GETDATE());";

                    using (SqlCommand personCmd = new SqlCommand(personQuery, connection))
                    {
                        personCmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                        personCmd.Parameters.AddWithValue("@FirstName", sellerName.Split(' ')[0]);
                        personCmd.Parameters.AddWithValue("@LastName", sellerName.Contains(" ") ? sellerName.Split(' ')[1] : "Unknown");
                        personCmd.ExecuteNonQuery();
                    }

                    // Random ID generation gia na kanw prevent ta duplicates
                    string uniqueNationalID = new Random().Next(100000000, 999999999).ToString();

                    string employeeQuery = @"
INSERT INTO HumanResources.Employee (BusinessEntityID, NationalIDNumber, LoginID, JobTitle, BirthDate, MaritalStatus, Gender, HireDate)
VALUES (@BusinessEntityID, @NationalIDNumber, 'adventure-works\seller' + CAST(@BusinessEntityID AS NVARCHAR), 'Sales Representative', '1990-01-01', 'S', 'M', GETDATE());";

                    using (SqlCommand employeeCmd = new SqlCommand(employeeQuery, connection))
                    {
                        employeeCmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                        employeeCmd.Parameters.AddWithValue("@NationalIDNumber", uniqueNationalID); // Ensure unique ID
                        employeeCmd.ExecuteNonQuery();
                    }


                    // ✅ Step 4: Insert into Sales.SalesPerson
                    string sellerQuery = @"
            INSERT INTO Sales.SalesPerson (BusinessEntityID, SalesQuota, Bonus, CommissionPct, SalesYTD, SalesLastYear, TerritoryID, rowguid, ModifiedDate)
            VALUES (@BusinessEntityID, @SalesQuota, @Bonus, @CommissionPct, @SalesYTD, @SalesLastYear, @TerritoryID, NEWID(), GETDATE());";

                    using (SqlCommand sellerCmd = new SqlCommand(sellerQuery, connection))
                    {
                        sellerCmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                        sellerCmd.Parameters.AddWithValue("@SalesQuota", salesQuota);
                        sellerCmd.Parameters.AddWithValue("@Bonus", bonus);
                        sellerCmd.Parameters.AddWithValue("@CommissionPct", commissionPct);
                        sellerCmd.Parameters.AddWithValue("@SalesYTD", salesYTD);
                        sellerCmd.Parameters.AddWithValue("@SalesLastYear", salesLastYear);
                        sellerCmd.Parameters.AddWithValue("@TerritoryID", territoryId > 0 ? (object)territoryId : DBNull.Value);
                        sellerCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Seller added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // ✅ Reset input fields
                    textBoxSellerName.Clear();
                    textBoxSalesQuota.Clear();
                    textBoxBonus.Clear();
                    textBoxCommissionPct.Clear();
                    textBoxSalesYTD.Clear();
                    textBoxSalesLastYear.Clear();
                    comboBoxTerritory.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void buttonSaveSeller_Click(object sender, EventArgs e)
        {
            string sellerName = textBoxSellerName.Text.Trim();
            decimal salesQuota, bonus, commissionPct, salesYTD, salesLastYear;
            int territoryId = comboBoxTerritory.SelectedItem != null ? ((KeyValuePair<int, string>)comboBoxTerritory.SelectedItem).Key : 0;

            // ✅ Validate Inputs
            if (string.IsNullOrWhiteSpace(sellerName))
            {
                MessageBox.Show("Seller Name is required.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBoxSalesQuota.Text, out salesQuota) || salesQuota < 0)
            {
                MessageBox.Show("Enter a valid Sales Quota.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(textBoxBonus.Text, out bonus) || bonus < 0)
            {
                MessageBox.Show("Enter a valid Bonus Amount.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(textBoxCommissionPct.Text, out commissionPct) || commissionPct < 0 || commissionPct > 1)
            {
                MessageBox.Show("Enter a valid Commission Percentage (0 to 1).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(textBoxSalesYTD.Text, out salesYTD) || salesYTD < 0)
            {
                MessageBox.Show("Enter a valid Sales Year-To-Date.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(textBoxSalesLastYear.Text, out salesLastYear) || salesLastYear < 0)
            {
                MessageBox.Show("Enter a valid Sales Last Year.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            InsertSeller(sellerName, salesQuota, bonus, commissionPct, salesYTD, salesLastYear, territoryId);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainForm MainForm = new MainForm();
            MainForm.Show();


            this.Hide();
        }
    }
}
