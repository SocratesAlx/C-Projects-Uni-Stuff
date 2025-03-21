﻿using System;
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

            // New fields
            bool makeFlag = textBoxMakeFlag.Text == "1";  // Convert to boolean
            bool finishedGoodsFlag = textBoxFinishedGoodsFlag.Text == "1";

            int subcategoryId = comboBoxCategory.SelectedItem != null ? ((KeyValuePair<int, string>)comboBoxCategory.SelectedItem).Key : 0;
            int modelId = comboBoxModel.SelectedItem != null ? ((KeyValuePair<int, string>)comboBoxModel.SelectedItem).Key : 0;

            // ✅ Validate required fields
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(productNumber))
            {
                MessageBox.Show("Product Name and Product Number are required.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(textBoxStandardCost.Text, out standardCost) || standardCost < 0 ||
                !decimal.TryParse(textBoxListPrice.Text, out listPrice) || listPrice < 0)
            {
                MessageBox.Show("Enter valid values for Standard Cost and List Price.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!int.TryParse(textBoxSafetyStock.Text, out safetyStockLevel) || safetyStockLevel < 0 ||
                !int.TryParse(textBoxReorderPoint.Text, out reorderPoint) || reorderPoint < 0 ||
                !int.TryParse(textBoxDaysToManufacture.Text, out daysToManufacture) || daysToManufacture < 0)
            {
                MessageBox.Show("Enter valid integer values for stock and reorder fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // ✅ Insert Product
                    string insertProductQuery = @"
            INSERT INTO Production.Product 
            (Name, ProductNumber, Color, StandardCost, ListPrice, Size, Weight, SafetyStockLevel, 
             ReorderPoint, DaysToManufacture, ProductSubcategoryID, ProductModelID, SellStartDate, MakeFlag, FinishedGoodsFlag)
            VALUES 
            (@Name, @ProductNumber, @Color, @StandardCost, @ListPrice, @Size, @Weight, @SafetyStockLevel, 
             @ReorderPoint, @DaysToManufacture, @SubcategoryID, @ModelID, GETDATE(), @MakeFlag, @FinishedGoodsFlag);
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
                        command.Parameters.AddWithValue("@MakeFlag", makeFlag ? 1 : 0);
                        command.Parameters.AddWithValue("@FinishedGoodsFlag", finishedGoodsFlag ? 1 : 0);

                        newProductId = Convert.ToInt32(command.ExecuteScalar());
                    }

                    textBoxProductID.Text = newProductId.ToString(); // ✅ Set the ProductID in the textbox

                    MessageBox.Show("Product and stock added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBoxProductID.Text.Trim(), out int productId))
            {
                MessageBox.Show("Please enter a valid Product ID to update.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool makeFlag = textBoxMakeFlag.Text == "1";
            bool finishedGoodsFlag = textBoxFinishedGoodsFlag.Text == "1";

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string updateQuery = @"
                UPDATE Production.Product
                SET MakeFlag = @MakeFlag, FinishedGoodsFlag = @FinishedGoodsFlag, ModifiedDate = GETDATE()
                WHERE ProductID = @ProductID";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@ProductID", productId);
                        cmd.Parameters.AddWithValue("@MakeFlag", makeFlag ? 1 : 0);
                        cmd.Parameters.AddWithValue("@FinishedGoodsFlag", finishedGoodsFlag ? 1 : 0);

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Product updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating product: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }



        private void buttonFill_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(textBoxProductID.Text.Trim(), out int productId))
            {
                MessageBox.Show("Please enter a valid Product ID to search.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                SELECT p.Name, p.ProductNumber, p.Color, p.StandardCost, p.ListPrice, p.Size, p.Weight, 
                       p.SafetyStockLevel, p.ReorderPoint, p.DaysToManufacture, 
                       p.ProductSubcategoryID, p.ProductModelID, p.MakeFlag, p.FinishedGoodsFlag,
                       ISNULL(pi.Quantity, 0) AS StockQuantity  -- ✅ Stock Quantity from Inventory
                FROM Production.Product p
                LEFT JOIN Production.ProductInventory pi ON p.ProductID = pi.ProductID
                WHERE p.ProductID = @ProductID";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ProductID", productId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())  // ✅ Ensure a record was found
                            {
                                textBoxProductName.Text = reader["Name"] != DBNull.Value ? reader["Name"].ToString() : "";
                                textBoxProductNumber.Text = reader["ProductNumber"] != DBNull.Value ? reader["ProductNumber"].ToString() : "";
                                textBoxColor.Text = reader["Color"] != DBNull.Value ? reader["Color"].ToString() : "";
                                textBoxStandardCost.Text = reader["StandardCost"] != DBNull.Value ? reader["StandardCost"].ToString() : "0";
                                textBoxListPrice.Text = reader["ListPrice"] != DBNull.Value ? reader["ListPrice"].ToString() : "0";
                                textBoxSize.Text = reader["Size"] != DBNull.Value ? reader["Size"].ToString() : "";
                                textBoxWeight.Text = reader["Weight"] != DBNull.Value ? reader["Weight"].ToString() : "0";
                                textBoxSafetyStock.Text = reader["SafetyStockLevel"] != DBNull.Value ? reader["SafetyStockLevel"].ToString() : "0";
                                textBoxReorderPoint.Text = reader["ReorderPoint"] != DBNull.Value ? reader["ReorderPoint"].ToString() : "0";
                                textBoxDaysToManufacture.Text = reader["DaysToManufacture"] != DBNull.Value ? reader["DaysToManufacture"].ToString() : "0";
                                textBoxMakeFlag.Text = reader["MakeFlag"] != DBNull.Value ? reader["MakeFlag"].ToString() : "0";
                                textBoxFinishedGoodsFlag.Text = reader["FinishedGoodsFlag"] != DBNull.Value ? reader["FinishedGoodsFlag"].ToString() : "0";
                                textBoxStockQuantity.Text = reader["StockQuantity"].ToString(); // ✅ Fills Stock Quantity

                                // ✅ Set Product Category (Subcategory)
                                int subcategoryId = reader["ProductSubcategoryID"] != DBNull.Value ? Convert.ToInt32(reader["ProductSubcategoryID"]) : -1;
                                if (subcategoryId > 0)
                                {
                                    comboBoxCategory.SelectedIndex = comboBoxCategory.FindStringExact(
                                        comboBoxCategory.Items.Cast<KeyValuePair<int, string>>()
                                        .FirstOrDefault(kvp => kvp.Key == subcategoryId).Value);
                                }
                                else
                                {
                                    comboBoxCategory.SelectedIndex = -1;
                                }

                                // ✅ Set Product Model
                                int modelId = reader["ProductModelID"] != DBNull.Value ? Convert.ToInt32(reader["ProductModelID"]) : -1;
                                if (modelId > 0)
                                {
                                    comboBoxModel.SelectedIndex = comboBoxModel.FindStringExact(
                                        comboBoxModel.Items.Cast<KeyValuePair<int, string>>()
                                        .FirstOrDefault(kvp => kvp.Key == modelId).Value);
                                }
                                else
                                {
                                    comboBoxModel.SelectedIndex = -1;
                                }

                                MessageBox.Show("Product data loaded successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                MessageBox.Show("Product not found! Check the Product ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error retrieving product data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }




        private void textBoxProductID_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxMakeFlag_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBoxFinishedGoodsFlag_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
