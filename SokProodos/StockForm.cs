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
    public partial class StockForm : Form
    {
        public StockForm()
        {
            InitializeComponent();
            LoadProductCategories();
            LoadProductModels();
            LoadSpecialOffers();
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

        private void LoadSpecialOffers()
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT SpecialOfferID, Description FROM Sales.SpecialOffer";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        comboBoxSpecialOffer.Items.Clear();
                        comboBoxSpecialOffer.Items.Add(new KeyValuePair<int, string>(1, "No Discount")); // ✅ Default

                        while (reader.Read())
                        {
                            int specialOfferId = reader.GetInt32(0);
                            string description = reader.GetString(1);
                            comboBoxSpecialOffer.Items.Add(new KeyValuePair<int, string>(specialOfferId, description));
                        }
                    }

                    comboBoxSpecialOffer.DisplayMember = "Value";
                    comboBoxSpecialOffer.ValueMember = "Key";
                    comboBoxSpecialOffer.SelectedIndex = 0; // Default to "No Discount"
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading special offers: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        private void LoadProductCategories()
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ProductSubcategoryID, Name FROM Production.ProductSubcategory";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBoxCategory.Items.Add(new KeyValuePair<int, string>(
                                reader.GetInt32(0), reader.GetString(1)));
                        }
                    }

                    comboBoxCategory.DisplayMember = "Value";
                    comboBoxCategory.ValueMember = "Key";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading subcategories: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadProductModels()
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT ProductModelID, Name FROM Production.ProductModel";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            comboBoxModel.Items.Add(new KeyValuePair<int, string>(
                                reader.GetInt32(0), reader.GetString(1)));
                        }
                    }

                    comboBoxModel.DisplayMember = "Value";
                    comboBoxModel.ValueMember = "Key";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading models: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InsertStock(string productName, string productNumber, string color, decimal standardCost, decimal listPrice,
                           string size, decimal weight, int safetyStockLevel, int reorderPoint,
                           int daysToManufacture, int subcategoryId, int modelId, int initialStockQuantity, int specialOfferId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // ✅ Step 1: Insert into `Production.Product`
                    string insertProductQuery = @"
                INSERT INTO Production.Product 
                (Name, ProductNumber, Color, StandardCost, ListPrice, Size, Weight, SafetyStockLevel, 
                 ReorderPoint, DaysToManufacture, ProductSubcategoryID, ProductModelID, SellStartDate, FinishedGoodsFlag)
                VALUES 
                (@Name, @ProductNumber, @Color, @StandardCost, @ListPrice, @Size, @Weight, @SafetyStockLevel, 
                 @ReorderPoint, @DaysToManufacture, @SubcategoryID, @ModelID, GETDATE(), 1);
                SELECT SCOPE_IDENTITY();";  // ✅ Get the new ProductID

                    int newProductId;
                    using (SqlCommand command = new SqlCommand(insertProductQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Name", productName);
                        command.Parameters.AddWithValue("@ProductNumber", productNumber);
                        command.Parameters.AddWithValue("@Color", string.IsNullOrWhiteSpace(color) ? (object)DBNull.Value : color);
                        command.Parameters.AddWithValue("@StandardCost", standardCost);
                        command.Parameters.AddWithValue("@ListPrice", listPrice);
                        command.Parameters.AddWithValue("@Size", string.IsNullOrWhiteSpace(size) ? (object)DBNull.Value : size);
                        command.Parameters.AddWithValue("@Weight", weight > 0 ? (object)weight : DBNull.Value);
                        command.Parameters.AddWithValue("@SafetyStockLevel", safetyStockLevel);
                        command.Parameters.AddWithValue("@ReorderPoint", reorderPoint);
                        command.Parameters.AddWithValue("@DaysToManufacture", daysToManufacture);
                        command.Parameters.AddWithValue("@SubcategoryID", subcategoryId > 0 ? (object)subcategoryId : DBNull.Value);
                        command.Parameters.AddWithValue("@ModelID", modelId > 0 ? (object)modelId : DBNull.Value);

                        newProductId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    // ✅ Step 2: Insert the Special Offer ID into `Sales.SpecialOfferProduct`
                    if (specialOfferId > 0)
                    {
                        string insertSpecialOfferQuery = @"
                    INSERT INTO Sales.SpecialOfferProduct (SpecialOfferID, ProductID)
                    VALUES (@SpecialOfferID, @ProductID);";

                        using (SqlCommand specialOfferCommand = new SqlCommand(insertSpecialOfferQuery, connection))
                        {
                            specialOfferCommand.Parameters.AddWithValue("@ProductID", newProductId);
                            specialOfferCommand.Parameters.AddWithValue("@SpecialOfferID", specialOfferId);
                            specialOfferCommand.ExecuteNonQuery();
                        }
                    }

                    // ✅ Step 3: Insert Initial Stock into `Production.ProductInventory`
                    if (initialStockQuantity > 0)
                    {
                        string insertInventoryQuery = @"
                    INSERT INTO Production.ProductInventory 
                    (ProductID, LocationID, Shelf, Bin, Quantity, ModifiedDate)
                    VALUES 
                    (@ProductID, 1, 'A', 1, @Quantity, GETDATE());";  // ✅ Default LocationID = 1, Shelf 'A', Bin 1

                        using (SqlCommand inventoryCommand = new SqlCommand(insertInventoryQuery, connection))
                        {
                            inventoryCommand.Parameters.AddWithValue("@ProductID", newProductId);
                            inventoryCommand.Parameters.AddWithValue("@Quantity", initialStockQuantity);
                            inventoryCommand.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("Product and stock added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // ✅ Reset form fields
                    textBoxProductName.Clear();
                    textBoxProductNumber.Clear();
                    textBoxColor.Clear();
                    textBoxStandardCost.Clear();
                    textBoxListPrice.Clear();
                    textBoxSize.Clear();
                    textBoxWeight.Clear();
                    textBoxSafetyStock.Clear();
                    textBoxReorderPoint.Clear();
                    textBoxDaysToManufacture.Clear();
                    textBoxStockQuantity.Clear();
                    comboBoxCategory.SelectedIndex = -1;
                    comboBoxModel.SelectedIndex = -1;
                    comboBoxSpecialOffer.SelectedIndex = 0; // Reset Special Offer
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }





        private void buttonSaveProduct_Click(object sender, EventArgs e)
        {
            string productName = textBoxProductName.Text.Trim();
            string productNumber = textBoxProductNumber.Text.Trim();
            string color = textBoxColor.Text.Trim();
            decimal standardCost, listPrice, weight;
            int safetyStockLevel, reorderPoint, daysToManufacture, initialStockQuantity;
            string size = textBoxSize.Text.Trim();
            int subcategoryId = comboBoxCategory.SelectedItem != null ? ((KeyValuePair<int, string>)comboBoxCategory.SelectedItem).Key : 0;
            int modelId = comboBoxModel.SelectedItem != null ? ((KeyValuePair<int, string>)comboBoxModel.SelectedItem).Key : 0;

            // ✅ Validate required fields
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(productNumber))
            {
                MessageBox.Show("Product Name and Product Number are required.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBoxStandardCost.Text, out standardCost) || standardCost < 0)
            {
                MessageBox.Show("Enter a valid Standard Cost.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(textBoxListPrice.Text, out listPrice) || listPrice < 0)
            {
                MessageBox.Show("Enter a valid List Price.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(textBoxSafetyStock.Text, out safetyStockLevel) || safetyStockLevel < 0)
            {
                MessageBox.Show("Enter a valid Safety Stock Level.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(textBoxReorderPoint.Text, out reorderPoint) || reorderPoint < 0)
            {
                MessageBox.Show("Enter a valid Reorder Point.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(textBoxDaysToManufacture.Text, out daysToManufacture) || daysToManufacture < 0)
            {
                MessageBox.Show("Enter a valid Days to Manufacture value.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!decimal.TryParse(textBoxWeight.Text, out weight) && !string.IsNullOrWhiteSpace(textBoxWeight.Text))
            {
                MessageBox.Show("Enter a valid numeric Weight or leave it empty.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ✅ Validate Stock Quantity
            if (!int.TryParse(textBoxStockQuantity.Text, out initialStockQuantity) || initialStockQuantity < 0)
            {
                MessageBox.Show("Enter a valid stock quantity (must be 0 or greater).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // ✅ Read the Special Offer ID from the ComboBox
            int specialOfferId = comboBoxSpecialOffer.SelectedItem != null
                ? ((KeyValuePair<int, string>)comboBoxSpecialOffer.SelectedItem).Key
                : 1; // Default to "No Discount" (SpecialOfferID = 1)

            // ✅ Insert product with stock and special offer
            InsertStock(productName, productNumber, color, standardCost, listPrice, size, weight,
                        safetyStockLevel, reorderPoint, daysToManufacture, subcategoryId, modelId,
                        initialStockQuantity, specialOfferId);
        }



        private void button1_Click(object sender, EventArgs e)
        {
            MainForm MainForm = new MainForm();
            MainForm.Show();


            this.Hide();
        }
    }
}
