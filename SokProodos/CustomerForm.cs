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
    public partial class CustomerForm : Form
    {
        public CustomerForm()
        {
            InitializeComponent();
            LoadStates();
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

        private void LoadStates()
        {
            comboBoxState.Items.Clear(); // Clear previous items

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT StateProvinceID, Name FROM Person.StateProvince";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int stateId = reader.GetInt32(0);
                        string stateName = reader.GetString(1);
                        comboBoxState.Items.Add(new KeyValuePair<int, string>(stateId, stateName));
                    }
                }
            }

            comboBoxState.DisplayMember = "Value";
            comboBoxState.ValueMember = "Key";
            comboBoxState.SelectedIndex = -1; // No selection initially
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            string firstName = textBoxFirstName.Text.Trim();
            string lastName = textBoxLastName.Text.Trim();
            string billingAddress = textBoxBillingAddress.Text.Trim();
            string city = textBoxCity.Text.Trim();
            string postalCode = textBoxPostalCode.Text.Trim();

            if (comboBoxState.SelectedItem == null)
            {
                MessageBox.Show("Please select a valid state!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int stateProvinceId = ((KeyValuePair<int, string>)comboBoxState.SelectedItem).Key;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(billingAddress))
            {
                MessageBox.Show("Please fill in all required fields!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    // Step 1: Insert into Person.BusinessEntity
                    string businessEntityInsertQuery = "INSERT INTO Person.BusinessEntity DEFAULT VALUES; SELECT SCOPE_IDENTITY();";
                    int businessEntityId;
                    using (SqlCommand cmd = new SqlCommand(businessEntityInsertQuery, connection, transaction))
                    {
                        businessEntityId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Step 2: Insert into Person.Person
                    string personInsertQuery = @"
            INSERT INTO Person.Person (BusinessEntityID, FirstName, LastName, PersonType)
            VALUES (@BusinessEntityID, @FirstName, @LastName, 'IN')";

                    using (SqlCommand cmd = new SqlCommand(personInsertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                        cmd.Parameters.AddWithValue("@FirstName", firstName);
                        cmd.Parameters.AddWithValue("@LastName", lastName);
                        cmd.ExecuteNonQuery();
                    }

                    // Step 3: Insert into Person.Address
                    string addressInsertQuery = @"
            INSERT INTO Person.Address (AddressLine1, City, StateProvinceID, PostalCode)
            OUTPUT INSERTED.AddressID
            VALUES (@AddressLine1, @City, @StateProvinceID, @PostalCode)";

                    int addressId;
                    using (SqlCommand cmd = new SqlCommand(addressInsertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@AddressLine1", billingAddress);
                        cmd.Parameters.AddWithValue("@City", city);
                        cmd.Parameters.AddWithValue("@StateProvinceID", stateProvinceId);
                        cmd.Parameters.AddWithValue("@PostalCode", postalCode);
                        addressId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Step 4: Insert into Person.BusinessEntityAddress to link Customer and Address
                    string businessEntityAddressInsertQuery = @"
            INSERT INTO Person.BusinessEntityAddress (BusinessEntityID, AddressID, AddressTypeID)
            VALUES (@BusinessEntityID, @AddressID, 1)";  // AddressTypeID = 1 (Billing Address)

                    using (SqlCommand cmd = new SqlCommand(businessEntityAddressInsertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                        cmd.Parameters.AddWithValue("@AddressID", addressId);
                        cmd.ExecuteNonQuery();
                    }

                    // ✅ Step 5: Insert into Sales.Customer without BillToAddressID
                    string customerInsertQuery = @"
            INSERT INTO Sales.Customer (PersonID)
            VALUES (@PersonID)";

                    using (SqlCommand cmd = new SqlCommand(customerInsertQuery, connection, transaction))
                    {
                        cmd.Parameters.AddWithValue("@PersonID", businessEntityId);
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                    MessageBox.Show("Customer and Billing Address added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // ✅ Reset form for new entry
                    ResetForm();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error inserting customer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }







        private void ResetForm()
        {
            textBoxFirstName.Clear();
            textBoxLastName.Clear();
            textBoxBillingAddress.Clear();
            textBoxCity.Clear();
            textBoxPostalCode.Clear();
            comboBoxState.SelectedIndex = -1; // Reset state selection
            textBoxFirstName.Focus(); // Move cursor back to first input field
        }


        private void InsertCustomer(string firstName, string lastName, string email, string phoneNumber)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Step 1: Insert into Person.BusinessEntity to get a new BusinessEntityID
                    string businessEntityQuery = "INSERT INTO Person.BusinessEntity (rowguid, ModifiedDate) VALUES (NEWID(), GETDATE()); SELECT SCOPE_IDENTITY();";

                    int businessEntityId;
                    using (SqlCommand businessCmd = new SqlCommand(businessEntityQuery, connection))
                    {
                        businessEntityId = Convert.ToInt32(businessCmd.ExecuteScalar());
                    }

                    // Step 2: Insert into Person.Person with the new BusinessEntityID
                    string personQuery = @"
                INSERT INTO Person.Person (BusinessEntityID, FirstName, LastName, PersonType, ModifiedDate)
                VALUES (@BusinessEntityID, @FirstName, @LastName, 'IN', GETDATE());";

                    using (SqlCommand personCmd = new SqlCommand(personQuery, connection))
                    {
                        personCmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                        personCmd.Parameters.AddWithValue("@FirstName", firstName);
                        personCmd.Parameters.AddWithValue("@LastName", lastName);
                        personCmd.ExecuteNonQuery();
                    }

                    // Step 3: Insert Email if provided
                    if (!string.IsNullOrWhiteSpace(email))
                    {
                        string emailQuery = @"
                    INSERT INTO Person.EmailAddress (BusinessEntityID, EmailAddress)
                    VALUES (@BusinessEntityID, @Email);";

                        using (SqlCommand emailCmd = new SqlCommand(emailQuery, connection))
                        {
                            emailCmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                            emailCmd.Parameters.AddWithValue("@Email", email);
                            emailCmd.ExecuteNonQuery();
                        }
                    }

                    // Step 4: Insert Phone Number if provided
                    if (!string.IsNullOrWhiteSpace(phoneNumber))
                    {
                        string phoneQuery = @"
                    INSERT INTO Person.PersonPhone (BusinessEntityID, PhoneNumber, PhoneNumberTypeID)
                    VALUES (@BusinessEntityID, @PhoneNumber, 1);"; // 1 = Cell Phone

                        using (SqlCommand phoneCmd = new SqlCommand(phoneQuery, connection))
                        {
                            phoneCmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                            phoneCmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                            phoneCmd.ExecuteNonQuery();
                        }
                    }

                    // Step 5: Insert into Sales.Customer WITHOUT AccountNumber (SQL Server generates it)
                    string customerQuery = @"
                INSERT INTO Sales.Customer (PersonID, StoreID, TerritoryID, ModifiedDate)
                VALUES (@BusinessEntityID, NULL, NULL, GETDATE());";

                    using (SqlCommand customerCmd = new SqlCommand(customerQuery, connection))
                    {
                        customerCmd.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                        customerCmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Customer added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Clear input fields
                    textBoxFirstName.Clear();
                    textBoxLastName.Clear();
                    textBoxEmail.Clear();
                    textBoxPhone.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void buttonBack_Click(object sender, EventArgs e)
        {
            MainForm MainForm = new MainForm();
            MainForm.Show();


            this.Hide();
        }
    }
}

