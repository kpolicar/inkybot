using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Windows.Forms;

namespace WindowsFormsApp
{
    public partial class LoginForm : Form
    {
        private Auth auth;

        public LoginForm()
        {
            InitializeComponent();
            this.auth = (Auth) Program.Services.GetService(typeof(Auth));
        }
        
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            throw new System.NotImplementedException();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var success = await auth.Login(usernameTextBox.Text, passwordTextBox.Text);
                if (!success)  {
                    errorMessage.Text = "Incorrect username or password!";
                    return;
                }
            }
            catch (HttpRequestException requestException)
            {
                errorMessage.Text = "Something went wrong on our end.\nPlease try again later.";
                return;
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}