﻿using System;
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
    public partial class Form1: Form
    {

        private ConnectToDB dbConnection;
        public Form1()
        {
            InitializeComponent();
            dbConnection = new ConnectToDB();
            textBox2.UseSystemPasswordChar = true;
            checkBoxShowPassword.CheckedChanged += checkBoxShowPassword_CheckedChanged;
        }

        private void checkBoxShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            // ✅ Toggle Password Visibility
            textBox2.UseSystemPasswordChar = !checkBoxShowPassword.Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!checkBoxShowPassword.Checked)
            {
                textBox2.UseSystemPasswordChar = true;
            }
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text;
            string password = textBox2.Text;

            if (dbConnection.CheckLogin(username, password))
            {
                GlobalSession.LoggedInUser = username;  // ✅ Store the username globally

                MessageBox.Show("Login successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                using (LoadingScreen loadingScreen = new LoadingScreen())
                {
                    loadingScreen.Show();
                    await Task.Delay(1500);
                    loadingScreen.Close();
                }

                MainForm mainForm = new MainForm();
                mainForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }




        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void checkBoxShowPassword_CheckedChanged_1(object sender, EventArgs e)
        {
            textBox2.UseSystemPasswordChar = !checkBoxShowPassword.Checked;
        }
    }
}
