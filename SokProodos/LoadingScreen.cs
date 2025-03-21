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
            this.FormBorderStyle = FormBorderStyle.None; 
            this.ShowInTaskbar = false; 
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Task.Run(() => SimulateLoading()); 
        }

        private async void SimulateLoading()
        {
            await Task.Delay(1000);

            if (!this.IsDisposed && this.IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    this.Hide();
                    this.Dispose();  // Ensure disposal after hiding
                }));
            }
        }






        private void LoadingScreen_Load(object sender, EventArgs e)
        {

        }
    }
}
