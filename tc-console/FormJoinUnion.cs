namespace TC
{
    using System;
    using System.Windows.Forms;

    public partial class FormJoinUnion : Form
    {
        public bool IsOk;

        public int UnionId;

        public FormJoinUnion()
        {
            this.InitializeComponent();
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