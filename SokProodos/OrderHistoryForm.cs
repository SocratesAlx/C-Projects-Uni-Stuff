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
    public partial class OrderHistoryForm: Form
    {
        private string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";
        public OrderHistoryForm()
        {
            InitializeComponent();
            LoadFilters();
            LoadOrderHistory();
            

            comboBoxYear.SelectedIndexChanged += new EventHandler(FilterOrders);
            comboBoxMonth.SelectedIndexChanged += new EventHandler(FilterOrders);
            comboBoxCustomer.SelectedIndexChanged += new EventHandler(FilterOrders);

        }

        private void LoadFilters()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // 🎯 Load Unique Years
                    string yearQuery = "SELECT DISTINCT YEAR(OrderDate) AS OrderYear FROM Sales.SalesOrderHeader ORDER BY OrderYear DESC;";
                    using (SqlCommand command = new SqlCommand(yearQuery, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        comboBoxYear.Items.Add("All");
                        while (reader.Read()) comboBoxYear.Items.Add(reader.GetInt32(0).ToString());
                    }

                    // 🎯 Load Unique Months
                    comboBoxMonth.Items.Add("All");
                    string[] months = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.MonthNames;
                    for (int i = 0; i < months.Length - 1; i++)
                    {
                        comboBoxMonth.Items.Add(months[i]);
                    }

                    // 🎯 Load Customers
                    string customerQuery = @"
                        SELECT DISTINCT c.CustomerID, ISNULL(p.FirstName + ' ' + p.LastName, s.Name) AS CustomerName
                        FROM Sales.Customer c
                        LEFT JOIN Person.Person p ON c.PersonID = p.BusinessEntityID
                        LEFT JOIN Sales.Store s ON c.StoreID = s.BusinessEntityID
                        ORDER BY CustomerName ASC;";

                    using (SqlCommand command = new SqlCommand(customerQuery, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        comboBoxCustomer.Items.Add("All");
                        while (reader.Read())
                        {
                            comboBoxCustomer.Items.Add(new KeyValuePair<int, string>(reader.GetInt32(0), reader.GetString(1)));
                        }
                    }

                    // Set Display for Customer ComboBox
                    comboBoxCustomer.DisplayMember = "Value";
                    comboBoxCustomer.ValueMember = "Key";

                    // Enable typing/search in combo boxes
                    comboBoxYear.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    comboBoxYear.AutoCompleteSource = AutoCompleteSource.ListItems;

                    comboBoxMonth.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    comboBoxMonth.AutoCompleteSource = AutoCompleteSource.ListItems;

                    comboBoxCustomer.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    comboBoxCustomer.AutoCompleteSource = AutoCompleteSource.ListItems;

                    // Default selections
                    comboBoxYear.SelectedIndex = 0;
                    comboBoxMonth.SelectedIndex = 0;
                    comboBoxCustomer.SelectedIndex = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading filters: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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

        private void LoadOrderHistory()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = @"
                        SELECT 
                            soh.SalesOrderID AS 'Order ID',
                            soh.OrderDate AS 'Order Date',
                            soh.DueDate AS 'Due Date',
                            c.CustomerID AS 'Customer ID',
                            ISNULL(p.FirstName + ' ' + p.LastName, s.Name) AS 'Customer Name',
                            sp.BusinessEntityID AS 'Seller ID',
                            per.FirstName + ' ' + per.LastName AS 'Seller Name',
                            soh.TotalDue AS 'Total Amount',
                            sm.Name AS 'Shipping Method',
                            soh.BillToAddressID AS 'Billing Address ID',
                            a.AddressLine1 + ', ' + a.City AS 'Billing Address',
                            sod.SpecialOfferID AS 'Special Offer ID',
                            so.Description AS 'Special Offer',
                            sod.OrderQty AS 'Order Quantity',
                            sod.UnitPrice AS 'Unit Price',
                            (sod.UnitPrice * sod.OrderQty) AS 'Total Price'
                        FROM Sales.SalesOrderHeader soh
                        JOIN Sales.Customer c ON soh.CustomerID = c.CustomerID
                        LEFT JOIN Person.Person p ON c.PersonID = p.BusinessEntityID
                        LEFT JOIN Sales.Store s ON c.StoreID = s.BusinessEntityID
                        LEFT JOIN Sales.SalesPerson sp ON soh.SalesPersonID = sp.BusinessEntityID
                        LEFT JOIN Person.Person per ON sp.BusinessEntityID = per.BusinessEntityID
                        LEFT JOIN Sales.SalesOrderDetail sod ON soh.SalesOrderID = sod.SalesOrderID
                        LEFT JOIN Sales.SpecialOffer so ON sod.SpecialOfferID = so.SpecialOfferID
                        LEFT JOIN Purchasing.ShipMethod sm ON soh.ShipMethodID = sm.ShipMethodID
                        LEFT JOIN Person.Address a ON soh.BillToAddressID = a.AddressID
                        ORDER BY soh.OrderDate DESC;";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridViewOrderHistory.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading order history: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void FilterOrders(object sender, EventArgs e)
        {
            string yearFilter = comboBoxYear.SelectedItem.ToString();
            string monthFilter = comboBoxMonth.SelectedItem.ToString();
            var customerFilter = comboBoxCustomer.SelectedItem;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Base query
                    string query = @"
                SELECT soh.SalesOrderID, soh.OrderDate, soh.DueDate, 
                       c.CustomerID, ISNULL(p.FirstName + ' ' + p.LastName, s.Name) AS CustomerName,
                       sp.BusinessEntityID, per.FirstName + ' ' + per.LastName AS SellerName,
                       soh.TotalDue, sm.Name AS ShippingMethod
                FROM Sales.SalesOrderHeader soh
                JOIN Sales.Customer c ON soh.CustomerID = c.CustomerID
                LEFT JOIN Person.Person p ON c.PersonID = p.BusinessEntityID
                LEFT JOIN Sales.Store s ON c.StoreID = s.BusinessEntityID
                LEFT JOIN Sales.SalesPerson sp ON soh.SalesPersonID = sp.BusinessEntityID
                LEFT JOIN Person.Person per ON sp.BusinessEntityID = per.BusinessEntityID
                LEFT JOIN Purchasing.ShipMethod sm ON soh.ShipMethodID = sm.ShipMethodID
                WHERE 1 = 1";

                    // Add filters dynamically
                    List<SqlParameter> parameters = new List<SqlParameter>();

                    if (yearFilter != "All")
                    {
                        query += " AND YEAR(soh.OrderDate) = @Year";
                        parameters.Add(new SqlParameter("@Year", int.Parse(yearFilter)));
                    }

                    if (monthFilter != "All")
                    {
                        int monthValue = DateTime.ParseExact(monthFilter, "MMMM", null).Month;
                        query += " AND MONTH(soh.OrderDate) = @Month";
                        parameters.Add(new SqlParameter("@Month", monthValue));
                    }

                    if (customerFilter != null && customerFilter.ToString() != "All")
                    {
                        int customerId = ((KeyValuePair<int, string>)customerFilter).Key;
                        query += " AND c.CustomerID = @CustomerID";
                        parameters.Add(new SqlParameter("@CustomerID", customerId));
                    }

                    query += " ORDER BY soh.OrderDate DESC;";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        // Add parameters
                        command.Parameters.AddRange(parameters.ToArray());

                        using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dataGridViewOrderHistory.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error filtering orders: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }




        private void buttonClose_Click_Click(object sender, EventArgs e)
        {
            MainForm MainForm = new MainForm();
            MainForm.Show();


            this.Hide();
        }

        private void dataGridViewOrderHistory_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
