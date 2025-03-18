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
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStockHistory)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridViewStockHistory
            // 
            this.dataGridViewStockHistory.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewStockHistory.Location = new System.Drawing.Point(218, 31);
            this.dataGridViewStockHistory.Name = "dataGridViewStockHistory";
            this.dataGridViewStockHistory.Size = new System.Drawing.Size(529, 429);
            this.dataGridViewStockHistory.TabIndex = 0;
            // 
            // buttonReresh
            // 
            this.buttonReresh.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.buttonReresh.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonReresh.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonReresh.Location = new System.Drawing.Point(864, 526);
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
            this.button1.Location = new System.Drawing.Point(29, 526);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 37);
            this.button1.TabIndex = 2;
            this.button1.Text = "Back";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // StockHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1006, 588);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.buttonReresh);
            this.Controls.Add(this.dataGridViewStockHistory);
            this.Name = "StockHistory";
            this.Text = "StockHistory";
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStockHistory)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridViewStockHistory;
        private System.Windows.Forms.Button buttonReresh;
        private System.Windows.Forms.Button button1;
    }
}