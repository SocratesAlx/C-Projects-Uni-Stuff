namespace SokProodos
{
    partial class StockHistory
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dataGridViewStockHistory = new System.Windows.Forms.DataGridView();
            this.buttonReresh = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBoxProductName = new System.Windows.Forms.ComboBox();
            this.textBoxProductID = new System.Windows.Forms.TextBox();
            this.labelProductName = new System.Windows.Forms.Label();
            this.labelProductID = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStockHistory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewStockHistory
            // 
            this.dataGridViewStockHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewStockHistory.Location = new System.Drawing.Point(337, 41);
            this.dataGridViewStockHistory.Name = "dataGridViewStockHistory";
            this.dataGridViewStockHistory.Size = new System.Drawing.Size(657, 296);
            this.dataGridViewStockHistory.TabIndex = 0;
            // 
            // buttonReresh
            // 
            this.buttonReresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.buttonReresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReresh.Location = new System.Drawing.Point(855, 462);
            this.buttonReresh.Name = "buttonReresh";
            this.buttonReresh.Size = new System.Drawing.Size(130, 38);
            this.buttonReresh.TabIndex = 1;
            this.buttonReresh.Text = "Refresh";
            this.buttonReresh.UseVisualStyleBackColor = false;
            this.buttonReresh.Click += new System.EventHandler(this.buttonReresh_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(12, 463);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 37);
            this.button1.TabIndex = 2;
            this.button1.Text = "Back";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBoxProductName
            // 
            this.comboBoxProductName.FormattingEnabled = true;
            this.comboBoxProductName.Location = new System.Drawing.Point(165, 41);
            this.comboBoxProductName.Name = "comboBoxProductName";
            this.comboBoxProductName.Size = new System.Drawing.Size(138, 21);
            this.comboBoxProductName.TabIndex = 3;
            this.comboBoxProductName.SelectedIndexChanged += new System.EventHandler(this.comboBoxProductName_SelectedIndexChanged);
            // 
            // textBoxProductID
            // 
            this.textBoxProductID.Location = new System.Drawing.Point(165, 89);
            this.textBoxProductID.Name = "textBoxProductID";
            this.textBoxProductID.Size = new System.Drawing.Size(138, 20);
            this.textBoxProductID.TabIndex = 4;
            this.textBoxProductID.TextChanged += new System.EventHandler(this.textBoxProductID_TextChanged);
            // 
            // labelProductName
            // 
            this.labelProductName.AutoSize = true;
            this.labelProductName.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProductName.Location = new System.Drawing.Point(32, 40);
            this.labelProductName.Name = "labelProductName";
            this.labelProductName.Size = new System.Drawing.Size(116, 18);
            this.labelProductName.TabIndex = 5;
            this.labelProductName.Text = "Product Name";
            // 
            // labelProductID
            // 
            this.labelProductID.AutoSize = true;
            this.labelProductID.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProductID.Location = new System.Drawing.Point(32, 91);
            this.labelProductID.Name = "labelProductID";
            this.labelProductID.Size = new System.Drawing.Size(88, 18);
            this.labelProductID.TabIndex = 6;
            this.labelProductID.Text = "Product ID";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SokProodos.Properties.Resources.awc_logo;
            this.pictureBox1.Location = new System.Drawing.Point(333, 401);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(516, 100);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 7;
            this.pictureBox1.TabStop = false;
            // 
            // StockHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 512);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.labelProductID);
            this.Controls.Add(this.labelProductName);
            this.Controls.Add(this.textBoxProductID);
            this.Controls.Add(this.comboBoxProductName);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonReresh);
            this.Controls.Add(this.dataGridViewStockHistory);
            this.Name = "StockHistory";
            this.Text = "StockHistory";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStockHistory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewStockHistory;
        private System.Windows.Forms.Button buttonReresh;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBoxProductName;
        private System.Windows.Forms.TextBox textBoxProductID;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelProductID;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}