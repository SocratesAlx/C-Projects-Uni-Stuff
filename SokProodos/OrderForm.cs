using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SokProodos
{
    public partial class OrderForm : Form
    {
        private string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

        private List<KeyValuePair<int, string>> allCustomers = new List<KeyValuePair<int, string>>(); // ✅ FIXED: Proper declaration
        private List<KeyValuePair<int, string>> allProducts = new List<KeyValuePair<int, string>>(); // ✅ FIXED: Proper declaration


        public OrderForm()
        {

            InitializeComponent();
            InitializeDataGridView();
            LoadCustomers();
            LoadSellers();
            LoadProducts();
            LoadShipMethods();
            LoadSpecialOffers();

            comboBoxCustomer.DropDownStyle = ComboBoxStyle.DropDown;
            comboBoxCustomer.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            comboBoxCustomer.AutoCompleteSource = AutoCompleteSource.ListItems;

            this.Load += new EventHandler(SalesForm_Load);
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(114, 137, 218); // Lighter blue on hover
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(88, 101, 242); // Normal state
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

        private void SalesForm_Load(object sender, EventArgs e)
        {
            textBoxDueDate.Text = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd"); // Default Due Date
        }

        private int GetCustomerPaymentTerms(int customerId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                SELECT TOP 1 DATEDIFF(DAY, OrderDate, DueDate) AS PaymentDays 
                FROM Sales.SalesOrderHeader
                WHERE CustomerID = @CustomerID
                ORDER BY OrderDate DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    object result = command.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 7;
                }
            }
        }

        private void LoadCustomerBillingAddress(int customerId)
        {
            textBoxBillingAddress.Text = ""; // Clear before searching

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
        SELECT a.AddressLine1 + ', ' + a.City AS BillingAddress
        FROM Sales.Customer c
        JOIN Person.BusinessEntityAddress bea ON c.PersonID = bea.BusinessEntityID
        JOIN Person.Address a ON bea.AddressID = a.AddressID
        WHERE c.CustomerID = @CustomerID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);

                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        textBoxBillingAddress.Text = result.ToString();  // ✅ Fill billing address textbox
                        textBoxBillingAddress.Refresh(); // Force UI update
                    }
                    else
                    {
                        textBoxBillingAddress.Text = "";  // ❌ No address found
                        MessageBox.Show("No billing address found for this customer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }









        private void LoadBillingAddressForCustomer(int customerId)
        {
            textBoxBillingAddress.Text = ""; // Clear previous data

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // First, retrieve the BusinessEntityID for this customer
                string query = @"
            SELECT c.PersonID 
            FROM Sales.Customer c
            WHERE c.CustomerID = @CustomerID";

                int businessEntityId = 0;
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        businessEntityId = Convert.ToInt32(result);
                    }
                }

                if (businessEntityId == 0)
                {
                    MessageBox.Show("No BusinessEntityID found for this customer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Now, find the Billing Address from BusinessEntityAddress
                query = @"
            SELECT a.AddressLine1 + ', ' + a.City AS BillingAddress
            FROM Person.BusinessEntityAddress bea
            JOIN Person.Address a ON bea.AddressID = a.AddressID
            WHERE bea.BusinessEntityID = @BusinessEntityID
            AND bea.AddressTypeID = 1";  // AddressTypeID = 1 is Billing Address

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@BusinessEntityID", businessEntityId);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        textBoxBillingAddress.Text = result.ToString();  // ✅ Fill the billing address textbox
                    }
                    else
                    {
                        MessageBox.Show("No billing address found for this customer!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }







        private Dictionary<int, int> customerBillingAddresses = new Dictionary<int, int>();

        private void LoadCustomers()
        {
            comboBoxCustomer.Items.Clear();
            customerBillingAddresses.Clear();

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
        SELECT c.CustomerID, 
               ISNULL(p.FirstName + ' ' + p.LastName, s.Name) AS CustomerName,
               (SELECT TOP 1 soh.BillToAddressID 
                FROM Sales.SalesOrderHeader soh
                WHERE soh.CustomerID = c.CustomerID
                ORDER BY soh.OrderDate DESC) AS BillToAddressID
        FROM Sales.Customer c
        LEFT JOIN Person.Person p ON c.PersonID = p.BusinessEntityID
        LEFT JOIN Sales.Store s ON c.StoreID = s.BusinessEntityID";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int customerId = reader.GetInt32(0);
                        string customerName = reader.GetString(1);
                        int billToAddressId = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);

                        // ✅ Store billing address ID (NO POPUP!)
                        customerBillingAddresses[customerId] = billToAddressId;

                        // ✅ Add customer to combo box
                        comboBoxCustomer.Items.Add(new KeyValuePair<int, string>(customerId, customerName));
                    }
                }
            }

            comboBoxCustomer.DisplayMember = "Value";
            comboBoxCustomer.ValueMember = "Key";
        }


        private void LoadSellers()
        {
            string query = "SELECT BusinessEntityID, FirstName + ' ' + LastName AS FullName FROM Person.Person WHERE BusinessEntityID IN (SELECT BusinessEntityID FROM Sales.SalesPerson)";
            LoadComboBox(query, comboBoxSeller);
        }

        private void LoadShipMethods()
        {
            string query = "SELECT ShipMethodID, Name FROM Purchasing.ShipMethod";
            LoadComboBox(query, comboBoxShipMethod);
        }

        private void LoadSpecialOffers()
        {
            string query = "SELECT SpecialOfferID, Description FROM Sales.SpecialOffer";
            LoadComboBox(query, comboBoxSpecialOffer);
        }

        private void LoadComboBox(string query, ComboBox comboBox)
        {
            comboBox.Items.Clear();
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        comboBox.Items.Add(new KeyValuePair<int, string>(reader.GetInt32(0), reader.GetString(1)));
                    }
                }
            }

            comboBox.DisplayMember = "Value";
            comboBox.ValueMember = "Key";
        }



        private void comboBoxCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxCustomer.SelectedItem == null)
            {
                textBoxBillingAddress.Text = "";
                return;
            }

            // ✅ Debug: Confirm event fires
            MessageBox.Show("Customer Selected Event Triggered!", "Debug", MessageBoxButtons.OK, MessageBoxIcon.Information);

            int customerId = ((KeyValuePair<int, string>)comboBoxCustomer.SelectedItem).Key;

            if (customerBillingAddresses.ContainsKey(customerId))
            {
                int billToAddressId = customerBillingAddresses[customerId];

                if (billToAddressId > 0)
                {
                    LoadBillingAddress(billToAddressId);
                }
                else
                {
                    textBoxBillingAddress.Text = "No Address Found";
                    textBoxBillingAddress.Refresh();
                }
            }
        }


















        private void LoadBillingAddress(int billToAddressId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
        SELECT AddressLine1 + ', ' + City AS FullAddress
        FROM Person.Address
        WHERE AddressID = @AddressID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AddressID", billToAddressId);
                    object result = command.ExecuteScalar();

                    if (result != DBNull.Value && result != null)
                    {
                        textBoxBillingAddress.Text = result.ToString(); // ✅ Set address
                        textBoxBillingAddress.Refresh(); // ✅ Force UI refresh
                    }
                    else
                    {
                        textBoxBillingAddress.Text = "No Address Found";
                        textBoxBillingAddress.Refresh(); // ✅ Ensure UI updates
                    }
                }
            }
        }







        private void SetDueDate(int customerId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                SELECT TOP 1 DATEDIFF(DAY, OrderDate, DueDate) AS PaymentDays 
                FROM Sales.SalesOrderHeader
                WHERE CustomerID = @CustomerID
                ORDER BY OrderDate DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    object result = command.ExecuteScalar();
                    int paymentDays = result != DBNull.Value ? Convert.ToInt32(result) : 7;
                    textBoxDueDate.Text = DateTime.Now.AddDays(paymentDays).ToString("yyyy-MM-dd");
                }
            }
        }

        private bool IsValidSpecialOffer(int productId, int specialOfferId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Sales.SpecialOfferProduct WHERE ProductID = @ProductID AND SpecialOfferID = @SpecialOfferID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.Parameters.AddWithValue("@SpecialOfferID", specialOfferId);
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private int GetValidSpecialOfferId(int productId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT TOP 1 SpecialOfferID 
            FROM Sales.SpecialOfferProduct 
            WHERE ProductID = @ProductID
            ORDER BY SpecialOfferID DESC";  // Prioritize the latest valid offer

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    object result = command.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 1; // Default to "No Discount"
                }
            }
        }





        private void LoadProducts()
        {
            allProducts.Clear();
            comboBoxProduct.Items.Clear();

            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ProductID, Name FROM Production.Product WHERE FinishedGoodsFlag = 1";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int productId = reader.GetInt32(0);
                        string productName = reader.GetString(1);
                        allProducts.Add(new KeyValuePair<int, string>(productId, productName));
                    }
                }
            }

            comboBoxProduct.DataSource = new BindingSource(allProducts, null);
            comboBoxProduct.DisplayMember = "Value";
            comboBoxProduct.ValueMember = "Key";
            comboBoxProduct.SelectedIndex = -1;
        }

        private void InitializeDataGridView()
        {
            dataGridViewInvoiceItems.Columns.Clear();
            dataGridViewInvoiceItems.Columns.Add("ProductID", "Product ID");
            dataGridViewInvoiceItems.Columns.Add("ProductName", "Product Name");
            dataGridViewInvoiceItems.Columns.Add("Quantity", "Quantity");
            dataGridViewInvoiceItems.Columns.Add("UnitPrice", "Unit Price");
            dataGridViewInvoiceItems.Columns.Add("TotalPrice", "Total Price");
            dataGridViewInvoiceItems.Columns.Add("BillingAddress", "Billing Address");
            dataGridViewInvoiceItems.Columns.Add("ShippingMethod", "Shipping Method");
            dataGridViewInvoiceItems.Columns.Add("SpecialOffer", "Special Offer"); // ✅ Ensuring Special Offer Column Exists
            dataGridViewInvoiceItems.Columns.Add("SpecialOfferID", "Special Offer ID"); // ✅ Ensure Special Offer ID is correctly placed
        }




        private int CreateSalesOrder(int customerId, int sellerId, DateTime orderDate, DateTime dueDate, int billToAddressId, int shipMethodId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
        INSERT INTO Sales.SalesOrderHeader (CustomerID, SalesPersonID, OrderDate, DueDate, BillToAddressID, ShipToAddressID, ShipMethodID)
        VALUES (@CustomerID, @SalesPersonID, @OrderDate, @DueDate, @BillToAddressID, @ShipToAddressID, @ShipMethodID);
        SELECT SCOPE_IDENTITY();"; // Get the newly inserted Order ID

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    command.Parameters.AddWithValue("@SalesPersonID", sellerId);
                    command.Parameters.AddWithValue("@OrderDate", orderDate);
                    command.Parameters.AddWithValue("@DueDate", dueDate);
                    command.Parameters.AddWithValue("@BillToAddressID", billToAddressId);
                    command.Parameters.AddWithValue("@ShipToAddressID", billToAddressId); // ✅ Set ShipTo same as BillTo if missing
                    command.Parameters.AddWithValue("@ShipMethodID", shipMethodId);

                    return Convert.ToInt32(command.ExecuteScalar()); // Return new SalesOrderID
                }
            }
        }





        private void AddSalesOrderDetail(int salesOrderId, int productId, int quantity, decimal unitPrice, int specialOfferId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = @"
        INSERT INTO Sales.SalesOrderDetail (SalesOrderID, ProductID, OrderQty, UnitPrice, SpecialOfferID)
        VALUES (@SalesOrderID, @ProductID, @OrderQty, @UnitPrice, @SpecialOfferID);";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SalesOrderID", salesOrderId);
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.Parameters.AddWithValue("@OrderQty", quantity);
                    command.Parameters.AddWithValue("@UnitPrice", unitPrice);
                    command.Parameters.AddWithValue("@SpecialOfferID", specialOfferId); // ✅ Prevent NULL error

                    command.ExecuteNonQuery();
                }
            }
        }
        private int GetProductStock(int productId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT SUM(Quantity) FROM Production.ProductInventory WHERE ProductID = @ProductID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    object result = command.ExecuteScalar();

                    return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }




        private decimal GetProductPrice(int productId)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT ListPrice FROM Production.Product WHERE ProductID = @ProductID AND FinishedGoodsFlag = 1";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    object result = command.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDecimal(result) : 0;
                }
            }
        }

        private void UpdateTotalOrderPrice()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dataGridViewInvoiceItems.Rows)
            {
                total += Convert.ToDecimal(row.Cells["TotalPrice"].Value);
            }
            textBoxTotalPrice.Text = total.ToString("F2");
        }

        private void printPreviewDialog1_Load(object sender, EventArgs e)
        {

        }

        private void ReduceStock(int productId, int quantity)
        {
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            UPDATE Production.ProductInventory
            SET Quantity = Quantity - @Quantity, 
                ModifiedDate = GETDATE()  -- ✅ Ensure the last updated date is set to today
            WHERE ProductID = @ProductID 
            AND Quantity >= @Quantity";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        MessageBox.Show($"Stock update failed for ProductID: {productId}", "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }



        private void buttonAddToOrder_Click(object sender, EventArgs e)
        {
            if (comboBoxProduct.SelectedItem == null || string.IsNullOrWhiteSpace(textBoxQuantity.Text))
            {
                MessageBox.Show("Select a product and enter quantity!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int productId = ((KeyValuePair<int, string>)comboBoxProduct.SelectedItem).Key;
            string productName = ((KeyValuePair<int, string>)comboBoxProduct.SelectedItem).Value;

            // ✅ Ensure ProductID is valid
            if (productId <= 0)
            {
                MessageBox.Show("Invalid Product! Please select a valid product.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int quantity;
            if (!int.TryParse(textBoxQuantity.Text, out quantity) || quantity <= 0)
            {
                MessageBox.Show("Quantity must be a positive integer!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int availableStock = GetProductStock(productId);
            if (availableStock < quantity)
            {
                MessageBox.Show($"Insufficient stock! Available: {availableStock}", "Stock Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal unitPrice = GetProductPrice(productId);
            decimal totalPrice = unitPrice * quantity;

            // ✅ Fetch a valid Special Offer ID
            int specialOfferId = GetValidSpecialOfferId(productId);
            string specialOffer = specialOfferId == 1 ? "No Discount" : $"Offer {specialOfferId}";

            // ✅ Ensure Special Offer ID is valid
            if (specialOfferId <= 0)
            {
                MessageBox.Show($"Invalid Special Offer for Product {productId}. Adjusting to default.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                specialOfferId = 1; // Default "No Discount"
            }

            // ✅ Debugging output (check if ProductID is valid)
            Console.WriteLine($"Adding ProductID: {productId}, SpecialOfferID: {specialOfferId}");

            // ✅ Ensure correct column mapping when adding a new row
            dataGridViewInvoiceItems.Rows.Add(
                productId,
                productName,
                quantity,
                unitPrice.ToString("F2"),
                totalPrice.ToString("F2"),
                textBoxBillingAddress.Text, // Billing Address
                comboBoxShipMethod.SelectedItem != null ? ((KeyValuePair<int, string>)comboBoxShipMethod.SelectedItem).Value : "Not Selected", // Shipping Method
                specialOffer,  // Special Offer Description
                specialOfferId // Special Offer ID
            );

            UpdateTotalOrderPrice();
        }




        private int GetBillingAddressFromDatabase(int customerId)
        {
            int billToAddressId = 0;
            string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
        SELECT TOP 1 a.AddressID
        FROM Sales.Customer c
        JOIN Person.BusinessEntityAddress bea ON c.PersonID = bea.BusinessEntityID
        JOIN Person.Address a ON bea.AddressID = a.AddressID
        WHERE c.CustomerID = @CustomerID";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerID", customerId);
                    object result = command.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        billToAddressId = Convert.ToInt32(result);
                    }
                }
            }

            return billToAddressId;
        }




        private async void buttonCompleteOrder_Click(object sender, EventArgs e)
        {
            buttonCompleteOrder.Enabled = false; // Disable to prevent duplicate clicks
            await Task.Run(() =>
            {
                CompleteOrder(); // Run the order completion process in the background
            });
            buttonCompleteOrder.Enabled = true; // Re-enable after completion
        }

        private void CompleteOrder()
        {
            // ✅ Access UI elements safely
            int customerId = 0, sellerId = 0, shipMethodId = 0;
            string dueDateText = "";

            this.Invoke((MethodInvoker)delegate
            {
                if (comboBoxCustomer.SelectedItem == null || comboBoxSeller.SelectedItem == null || comboBoxShipMethod.SelectedItem == null)
                {
                    MessageBox.Show("Please select a customer, seller, and shipping method before completing the order.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                customerId = ((KeyValuePair<int, string>)comboBoxCustomer.SelectedItem).Key;
                sellerId = ((KeyValuePair<int, string>)comboBoxSeller.SelectedItem).Key;
                shipMethodId = ((KeyValuePair<int, string>)comboBoxShipMethod.SelectedItem).Key;
                dueDateText = textBoxDueDate.Text;
            });

            DateTime orderDate = DateTime.Now;
            DateTime dueDate = DateTime.TryParse(dueDateText, out dueDate) ? dueDate : orderDate.AddDays(7);

            int billToAddressId = customerBillingAddresses.ContainsKey(customerId) ? customerBillingAddresses[customerId] : 0;
            if (billToAddressId == 0) billToAddressId = GetBillingAddressFromDatabase(customerId);
            if (billToAddressId == 0)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Customer does not have a valid billing address!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                });
                return;
            }

            int salesOrderId = CreateSalesOrder(customerId, sellerId, orderDate, dueDate, billToAddressId, shipMethodId);
            if (salesOrderId > 0)
            {
                foreach (DataGridViewRow row in dataGridViewInvoiceItems.Rows)
                {
                    if (row.IsNewRow) continue;

                    if (!int.TryParse(row.Cells["ProductID"].Value?.ToString(), out int productId) || productId <= 0)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show("Invalid Product detected. Please check the order details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        });
                        continue;
                    }

                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);
                    decimal unitPrice = Convert.ToDecimal(row.Cells["UnitPrice"].Value);

                    if (quantity <= 0)
                    {
                        this.Invoke((MethodInvoker)delegate
                        {
                            MessageBox.Show($"Invalid quantity for Product {productId}. It must be greater than 0.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        });
                        continue;
                    }

                    int specialOfferId = 1;
                    if (int.TryParse(row.Cells["SpecialOfferID"].Value?.ToString(), out int tempSpecialOfferId) && tempSpecialOfferId > 0)
                    {
                        specialOfferId = tempSpecialOfferId;
                    }

                    AddSalesOrderDetail(salesOrderId, productId, quantity, unitPrice, specialOfferId);
                    ReduceStock(productId, quantity);
                }

                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show($"Order {salesOrderId} completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // ✅ Reset UI safely after async operation
                    dataGridViewInvoiceItems.Rows.Clear();
                    comboBoxCustomer.SelectedIndex = -1;
                    comboBoxProduct.SelectedIndex = -1;
                    textBoxQuantity.Clear();
                    textBoxBillingAddress.Clear();
                    textBoxDueDate.Text = DateTime.Now.AddDays(7).ToString("yyyy-MM-dd");
                    buttonCompleteOrder.Enabled = true; // Re-enable button
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate
                {
                    MessageBox.Show("Order creation failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                });
            }
        }




        private void buttonCancel_Click(object sender, EventArgs e)
        {
            MainForm mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }

        private void buttonFindBillingAddress_Click(object sender, EventArgs e)
        {
            if (comboBoxCustomer.SelectedItem == null)
            {
                MessageBox.Show("Please select a customer first!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int customerId = ((KeyValuePair<int, string>)comboBoxCustomer.SelectedItem).Key;
            LoadCustomerBillingAddress(customerId);
        }

    }
}
