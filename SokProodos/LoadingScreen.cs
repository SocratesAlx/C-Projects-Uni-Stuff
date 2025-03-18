using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SokProodos
{
    public partial class LoadingScreen: Form
    {
        public LoadingScreen()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None; // Remove border
            this.ShowInTaskbar = false; // Hide from taskbar
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Task.Run(() => SimulateLoading()); // Run loading in background
        }

        private async void SimulateLoading()
        {
            await Task.Delay(3000); // Simulate 3 seconds loading

            if (this.IsHandleCreated && !this.IsDisposed) // Ensure the form is still active
            {
                this.Invoke(new Action(() => this.Close())); // Close the loading form safely
            }
        }




        private void LoadingScreen_Load(object sender, EventArgs e)
        {

        }
    }
}
