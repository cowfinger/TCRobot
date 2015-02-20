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
    public partial class FormChooseTroopHead : Form
    {
        public TroopInfo GroupHead { get; set; }

        public FormChooseTroopHead(IEnumerable<TroopInfo> teamList)
        {
            InitializeComponent();

            this.GroupHead = null;
            foreach (var team in teamList)
            {
                var lvItem = new ListViewItem();
                lvItem.SubItems.Add(team.AccountName);
                lvItem.SubItems.Add(team.PowerIndex.ToString());
                lvItem.SubItems.Add(team.TroopId);
                lvItem.Tag = team;
                this.listViewTroop.Items.Add(lvItem);
            }
        }

        private void listViewTroop_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.btnOk.Enabled = this.listViewTroop.CheckedItems.Count == 1;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.GroupHead = this.listViewTroop.CheckedItems[0].Tag as TroopInfo;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.GroupHead = null;
            this.Close();
        }
    }
}
