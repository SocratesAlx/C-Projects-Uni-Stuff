using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Drawing.Drawing2D;

namespace SokProodos
{
    public partial class SupplierForm: Form
    {
        public SupplierForm()
        {
            InitializeComponent();

            comboBoxCreditRating.Items.Add(1);
            comboBoxCreditRating.Items.Add(2);
            comboBoxCreditRating.Items.Add(3);
            comboBoxCreditRating.Items.Add(4);
            comboBoxCreditRating.Items.Add(5);

            
            comboBoxCreditRating.SelectedIndex = 0; 
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
        private void buttonSaveSupplier_Click(object sender, EventArgs e)
        {
            string supplierName = textBoxSupplierName.Text.Trim();
            string website = textBoxWebsite.Text.Trim();
            bool preferred = checkBoxPreferred.Checked;
            bool active = checkBoxActive.Checked;

            
            if (comboBoxCreditRating.SelectedItem == null)
            {
                MessageBox.Show("Please select a valid Credit Rating (1-5).", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int creditRating;
            if (!int.TryParse(comboBoxCreditRating.SelectedItem.ToString(), out creditRating) || creditRating < 1 || creditRating > 5)
            {
                MessageBox.Show("Credit Rating must be between 1 and 5.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            
            InsertSupplier(supplierName, creditRating, preferred, active, website);
        }

        private void InsertSupplier(string supplierName, int creditRating, bool preferred, bool active, string website)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    
                    string businessEntityQuery = "INSERT INTO Person.BusinessEntity (rowguid, ModifiedDate) VALUES (NEWID(), GETDATE()); SELECT SCOPE_IDENTITY();";

                    int businessEntityId;
                    using (SqlCommand businessCmd = new SqlCommand(businessEntityQuery, connection))
                    {
                        businessEntityId = Convert.ToInt32(businessCmd.ExecuteScalar());
                    }

                    
                    string accountNumber = "VN" + new Random().Next(100000, 999999); 

                    
                    string vendorQuery = @"
                INSERT INTO Purchasing.Vendor (BusinessEntityID, AccountNumber, Name, CreditRating, PreferredVendorStatus, ActiveFlag, PurchasingWebServiceURL, ModifiedDate)
                VALUES (@BusinessEntityID, @AccountNumber, @Name, @CreditRating, @Preferred, @Active, @Website, GETDATE());";

                    using (SqlCommand vendorCmd = new SqlCommand(vendorQuery, connection))
                    {
                        vendorCmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                        vendorCmd.Parameters.AddWithValue("@AccountNumber", accountNumber);
                        vendorCmd.Parameters.AddWithValue("@Name", supplierName);
                        vendorCmd.Parameters.AddWithValue("@CreditRating", creditRating);
                        vendorCmd.Parameters.AddWithValue("@Preferred", preferred ? 1 : 0);
                        vendorCmd.Parameters.AddWithValue("@Active", active ? 1 : 0);
                        vendorCmd.Parameters.AddWithValue("@Website", string.IsNullOrWhiteSpace(website) ? (object)DBNull.Value : website);
                        vendorCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Supplier added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    
                    textBoxSupplierName.Clear();
                    comboBoxCreditRating.SelectedIndex = -1;
                    checkBoxPreferred.Checked = false;
                    checkBoxActive.Checked = false;
                    textBoxWebsite.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainForm MainForm = new MainForm();
            MainForm.Show();


            this.Hide();
        }
    }
}
