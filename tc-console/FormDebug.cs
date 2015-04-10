using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TC
{
    public partial class FormDebug : Form
    {
        private delegate void DoSomething();

        public AccountInfo Account { get; set; }

        public FormDebug()
        {
            InitializeComponent();
        }

        private void textBoxUrl_TextChanged(object sender, EventArgs e)
        {
            this.buttonGet.Enabled = this.textBoxUrl.Text.Any();
            this.buttonPost.Enabled = this.textBoxUrl.Text.Any() && this.textBoxBody.Text.Any();
        }

        private void textBoxBody_TextChanged(object sender, EventArgs e)
        {
            this.buttonPost.Enabled = this.textBoxUrl.Text.Any() && this.textBoxBody.Text.Any();
        }

        private void buttonGet_Click(object sender, EventArgs e)
        {
            Parallel.Dispatch("".PadRight((int)this.numericUpDownParallel.Value),
                ch =>
                {
                    var resp = this.Account.WebAgent.WebClient.OpenUrl(this.textBoxUrl.Text);
                    this.Invoke(new DoSomething(() => { this.textBoxResponse.Text = resp; }));
                });
        }

        private void buttonPost_Click(object sender, EventArgs e)
        {
            Parallel.Dispatch("".PadRight((int)this.numericUpDownParallel.Value),
                ch =>
                {
                    var resp = this.Account.WebAgent.WebClient.OpenUrl(this.textBoxUrl.Text, this.textBoxBody.Text);
                    this.Invoke(new DoSomething(() => { this.textBoxResponse.Text = resp; }));
                });
        }
    }
}
