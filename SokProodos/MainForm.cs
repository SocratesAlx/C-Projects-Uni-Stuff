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
using System.Windows.Forms.DataVisualization.Charting;




namespace SokProodos
{
    public partial class MainForm : Form
    {

        private Chart chartOrders;
        private Chart chartTopProducts;
        private Chart chartSalesCategories;
        string connectionString = @"Server=SOCHAX\SQLEXPRESS;Database=AdventureWorks2022;Trusted_Connection=True;";
        public MainForm()
        {
            InitializeComponent();
            InitializeDashboard();
            LoadOrderChart();
            LoadStockChart();
            LoadTopProductsChart();
            Timer timer = new Timer();
            timer.Interval = 1000; 
            timer.Tick += Timer_Tick;
            timer.Start();
            labelClock.Text = DateTime.Now.ToString("hh:mm:ss tt - dddd, MMM dd yyyy");
            labelClock.Font = new Font("Consolas", 12, FontStyle.Bold);
            labelClock.ForeColor = Color.White;



        }

        protected override void OnPaint(PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                this.ClientRectangle,
                Color.FromArgb(15, 25, 80),   // Darker Desaturated Blue (Top)
                Color.FromArgb(40, 100, 150), // Muted Light Blue (Bottom)
                LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }



        private void Timer_Tick(object sender, EventArgs e)
        {
            labelClock.Text = DateTime.Now.ToString("HH:mm:ss - dddd, MMM dd yyyy");
        }


        private void InitializeDashboard()
        {
            
            Panel panelDashboard = new Panel
            {
                Size = new Size(750, 400),  
                Location = new Point(200, 50),
                BackColor = Color.Transparent
            };
            this.Controls.Add(panelDashboard);

            
            Label lblDashboard = new Label
            {
                Text = "📊 Business Dashboard",
                Font = new Font("Arial", 16, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 5),
                AutoSize = true
            };
            panelDashboard.Controls.Add(lblDashboard);

            
            Chart chartOrders = CreateChart("chartOrders", "Orders", SeriesChartType.Column, Color.DodgerBlue);
            chartOrders.Location = new Point(10, 40);
            chartOrders.Size = new Size(300, 200);  
            panelDashboard.Controls.Add(chartOrders);

            
            Chart chartStock = CreateChart("chartStock", "Stock", SeriesChartType.Pie, Color.Orange);
            chartStock.Series["Stock"]["PieLabelStyle"] = "Disabled"; 
            chartStock.Legends[0].Enabled = true;
            chartStock.Legends[0].Docking = Docking.Bottom;  
            chartStock.Location = new Point(320, 40);
            chartStock.Size = new Size(280, 200); 
            panelDashboard.Controls.Add(chartStock);

            
            Chart chartTopProducts = CreateChart("chartTopProducts", "TopProducts", SeriesChartType.Bar, Color.MediumSeaGreen);
            chartTopProducts.Location = new Point(10, 250);
            chartTopProducts.Size = new Size(600, 150);  
            panelDashboard.Controls.Add(chartTopProducts);
        }

        private Chart CreateChart(string name, string seriesName, SeriesChartType chartType, Color color)
        {
            Chart chart = new Chart
            {
                Name = name,
                Size = new Size(320, 200),
                BackColor = Color.FromArgb(50, 50, 50),  
                ForeColor = Color.White
            };
            ChartArea area = new ChartArea("MainArea")
            {
                BackColor = Color.Transparent
            };
            area.AxisX.LabelStyle.ForeColor = Color.White;
            area.AxisX.LabelStyle.Font = new Font("Arial", 9, FontStyle.Bold);
            area.AxisY.LabelStyle.ForeColor = Color.White;
            area.AxisY.LabelStyle.Font = new Font("Arial", 9, FontStyle.Bold);

            
            area.AxisX.LabelStyle.Angle = chartType == SeriesChartType.Bar ? 0 : -45;

            chart.ChartAreas.Add(area);
            Series series = new Series(seriesName)
            {
                ChartType = chartType,
                Color = color
            };
            chart.Series.Add(series);

            
            series["DrawingStyle"] = "Cylinder"; 
            series["PieDrawingStyle"] = "Concave"; 
            series["PixelPointWidth"] = chartType == SeriesChartType.Bar ? "50" : "100";

            
            series.ToolTip = "#VALX: #VALY";

            chart.Legends.Add(new Legend("Legend")
            {
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Docking = Docking.Bottom  
            });

            return chart;
        }

        
        private void LoadOrderChart()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT FORMAT(OrderDate, 'yyyy-MM') AS Month, COUNT(SalesOrderID) AS TotalOrders
                    FROM Sales.SalesOrderHeader
                    WHERE OrderDate >= DATEADD(MONTH, -6, GETDATE()) -- Last 6 months
                    GROUP BY FORMAT(OrderDate, 'yyyy-MM')
                    ORDER BY Month ASC;";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Chart chart = (Chart)this.Controls.Find("chartOrders", true)[0];
                    chart.Series["Orders"].Points.Clear();

                    while (reader.Read())
                    {
                        string month = reader.GetString(0);
                        int totalOrders = reader.GetInt32(1);
                        chart.Series["Orders"].Points.AddXY(month, totalOrders);
                    }
                }
            }
        }

        
        private void LoadStockChart()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
                    SELECT TOP 8 p.Name, SUM(i.Quantity) AS StockCount
                    FROM Production.Product p
                    JOIN Production.ProductInventory i ON p.ProductID = i.ProductID
                    WHERE p.FinishedGoodsFlag = 1
                    GROUP BY p.Name
                    ORDER BY StockCount DESC;";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Chart chart = (Chart)this.Controls.Find("chartStock", true)[0];
                    chart.Series["Stock"].Points.Clear();

                    while (reader.Read())
                    {
                        string productName = reader.GetString(0);
                        int stockCount = reader.GetInt32(1);
                        chart.Series["Stock"].Points.AddXY(productName, stockCount);
                    }
                }
            }
        }

        
        private void LoadTopProductsChart()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = @"
            SELECT TOP 5 p.Name, SUM(d.OrderQty) AS TotalSold
            FROM Sales.SalesOrderDetail d
            JOIN Production.Product p ON d.ProductID = p.ProductID
            GROUP BY p.Name
            ORDER BY TotalSold DESC;";

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    Chart chart = (Chart)this.Controls.Find("chartTopProducts", true)[0];
                    chart.Series["TopProducts"].Points.Clear();

                    while (reader.Read())
                    {
                        string productName = reader.GetString(0);
                        int totalSold = reader.GetInt32(1);
                        chart.Series["TopProducts"].Points.AddXY(productName, totalSold);
                    }

                    
                    chart.Series["TopProducts"]["PixelPointWidth"] = "10";  
                    chart.Series["TopProducts"]["PointWidth"] = "0.4";      

                    
                    chart.ChartAreas[0].AxisX.LabelStyle.Angle = 0;  
                    chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Arial", 9, FontStyle.Bold);  
                    chart.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Arial", 9, FontStyle.Bold);  
                }
            }
        }



        private void Button_MouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(114, 137, 218); 
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = Color.FromArgb(88, 101, 242); 
        }


        private void button1_Click(object sender, EventArgs e)
        {
            CustomerForm CustomerForm = new CustomerForm();
            CustomerForm.Show();


            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SupplierForm SupplierForm = new SupplierForm();
            SupplierForm.Show();


            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            StockForm StockForm = new StockForm();
            StockForm.Show();


            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SellersForm SellersForm = new SellersForm();
            SellersForm.Show();


            this.Hide();
        }

        

        private void button5_Click(object sender, EventArgs e)
        {
            Form1 Form1 = new Form1();
            Form1.Show();


            this.Hide();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            OrderHistoryForm OrderHistoryForm = new OrderHistoryForm();
            OrderHistoryForm.Show();


            this.Hide();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OrderForm OrderForm = new OrderForm();
            OrderForm.Show();


            this.Hide();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            StockHistory StockHistory = new StockHistory();
            StockHistory.Show();


            this.Hide();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            SellerInfo SellerInfo = new SellerInfo();
            SellerInfo.Show();


            this.Hide();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            CustomerInfo CustomerInfo = new CustomerInfo();
            CustomerInfo.Show();


            this.Hide();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            SupplierInfo SupplierInfo = new SupplierInfo();
            SupplierInfo.Show();


            this.Hide();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            StockInfo StockInfo = new StockInfo();
            StockInfo.Show();


            this.Hide();
        }
    }
}
