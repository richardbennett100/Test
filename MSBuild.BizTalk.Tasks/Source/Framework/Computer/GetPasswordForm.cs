﻿//-----------------------------------------------------------------------
// <copyright file="GetPasswordForm.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.Computer.Extended
{
    using System;
    using System.DirectoryServices.AccountManagement;
    using System.Windows.Forms;

    /// <summary>
    /// Gets a user's AD validated password
    /// </summary>
    public partial class GetPasswordForm : Form
    {
        private readonly string user;
        private readonly string domain;
        private readonly ContextOptions contextOptions;
        private readonly ContextType contextType;
        private string password;
        private Exception exception;
        
        public GetPasswordForm(string user, string domain, ContextType type, ContextOptions options)
        {
            this.InitializeComponent();

            if (!string.IsNullOrEmpty(domain))
            {
                this.Text += domain + @"\";
            }
            
            this.Text += user;
            this.user = user;
            this.domain = domain;
            this.contextType = type;
            this.contextOptions = options;
        }

        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public Exception Exception
        {
            get { return this.exception; }
            set { this.exception = value; }
        }

        private void ButtonOk_Click(object sender, System.EventArgs e)
        {
            this.ProcessOk();
        }

        private void ProcessOk()
        {
            try
            {
                PrincipalContext pcontext = new PrincipalContext(this.contextType, this.domain);
                using (pcontext)
                {
                    if (pcontext.ValidateCredentials(this.user, this.textBoxPassword.Text, this.contextOptions) == false)
                    {
                        this.labelPassword.ForeColor = System.Drawing.Color.DarkRed;
                        this.textBoxPassword.BackColor = System.Drawing.Color.Coral;
                        this.labelPassword.Focus();
                    }
                    else
                    {
                        this.password = this.textBoxPassword.Text;
                        this.labelPassword.ForeColor = System.Drawing.Color.DarkGreen;
                        this.textBoxPassword.BackColor = System.Drawing.Color.WhiteSmoke;
                        this.Refresh();
                        System.Threading.Thread.Sleep(300);
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                this.exception = ex;
                this.Close();
            }
        }

        private void ButtonCancel_Click(object sender, System.EventArgs e)
        {
            this.ProcessCancel();
        }

        private void ProcessCancel()
        {
            this.password = string.Empty;
            this.Close();
        }

        private void CheckBoxMask_CheckedChanged(object sender, System.EventArgs e)
        {
            this.textBoxPassword.UseSystemPasswordChar = this.checkBoxMask.Checked;
        }

        private void TextBoxPassword_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    this.ProcessOk();
                    break;
                case Keys.Escape:
                    this.ProcessCancel();
                    break;
            }
        }
    }
}
