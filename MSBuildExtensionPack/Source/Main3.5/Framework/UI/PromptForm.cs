﻿//-----------------------------------------------------------------------
// <copyright file="PromptForm.cs">(c) http://www.codeplex.com/MSBuildExtensionPack. This source is subject to the Microsoft Permissive License. See http://www.microsoft.com/resources/sharedsource/licensingbasics/sharedsourcelicenses.mspx. All other rights reserved.</copyright>
//-----------------------------------------------------------------------
namespace MSBuild.ExtensionPack.UI.Extended
{
    using System.Drawing;
    using System.Windows.Forms;

    /// <summary>
    /// PromptForm
    /// </summary>
    public partial class PromptForm : Form
    {
        private string buttonClickedText = "None";
        private string userText = string.Empty;

        public PromptForm()
        {
            this.InitializeComponent();
        }

        public PromptForm(string messageText, string messageColour, bool messageBold, string button1Text, string button2Text, string button3Text, bool maskText)
        {
            this.InitializeComponent();
            this.labelText.Text = messageText;
            
            if (messageBold)
            {
                this.labelText.Font = new Font(this.labelText.Font, FontStyle.Bold);
            }

            if (maskText)
            {
                this.textBoxUser.UseSystemPasswordChar = true;
            }
            
            if (!string.IsNullOrEmpty(messageColour))
            {
                this.labelText.ForeColor = Color.FromName(messageColour);
            }

            if (!string.IsNullOrEmpty(button1Text))
            {
                this.button1.Visible = true;
                this.button1.Text = button1Text;
            }

            if (!string.IsNullOrEmpty(button2Text))
            {
                this.button2.Visible = true;
                this.button2.Text = button2Text;
            }

            if (!string.IsNullOrEmpty(button3Text))
            {
                this.button3.Visible = true;
                this.button3.Text = button3Text;
            }
        }

        public string ButtonClickedText
        {
            get { return this.buttonClickedText; }
            set { this.buttonClickedText = value; }
        }

        public string UserText
        {
            get { return this.userText; }
            set { this.userText = value; }
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            this.ButtonClickedText = this.button1.Text;
            this.UserText = this.textBoxUser.Text;
            this.Close();
        }

        private void Button2_Click(object sender, System.EventArgs e)
        {
            this.ButtonClickedText = this.button2.Text;
            this.UserText = this.textBoxUser.Text;
            this.Close();
        }

        private void Button3_Click(object sender, System.EventArgs e)
        {
            this.ButtonClickedText = this.button3.Text;
            this.UserText = this.textBoxUser.Text;
            this.Close();
        }
    }
}
