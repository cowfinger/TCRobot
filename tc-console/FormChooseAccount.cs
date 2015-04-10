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
    public partial class FormChooseAccount : Form
    {
        private readonly Dictionary<string, int> accountIdMap = new Dictionary<string, int>();

        public int TargetAccountId { get; private set; }

        public void AddAccount(int id, string name)
        {
            this.comboBoxAccounts.Items.Add(name);
            this.accountIdMap.Add(name, id);
        }

        public FormChooseAccount()
        {
            InitializeComponent();
        }

        private void FormChooseAccount_Load(object sender, EventArgs e)
        {
            this.comboBoxAccounts.Text = this.comboBoxAccounts.Items[0].ToString();
            this.TargetAccountId = this.accountIdMap[this.comboBoxAccounts.Text];
        }

        private void comboBoxAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.TargetAccountId = this.accountIdMap[this.comboBoxAccounts.Text];
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
