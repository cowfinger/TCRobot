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
    public partial class FormJoinUnion : Form
    {
        public bool IsOk = false;
        public int UnionId = 0;

        public FormJoinUnion()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.IsOk = true;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            this.buttonOk.Enabled = int.TryParse(this.textBox1.Text, out this.UnionId);
        }
    }
}
