namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Forms;

    public partial class FormChooseTroopHead : Form
    {
        public FormChooseTroopHead(IEnumerable<TroopInfo> teamList)
        {
            this.InitializeComponent();

            this.GroupHead = null;
            foreach (var team in teamList)
            {
                var lvItem = new ListViewItem();
                lvItem.SubItems.Add(team.AccountName);
                lvItem.SubItems.Add(team.PowerIndex.ToString());
                lvItem.SubItems.Add(team.TroopId.ToString());
                lvItem.Tag = team;
                this.listViewTroop.Items.Add(lvItem);
            }
        }

        public TroopInfo GroupHead { get; set; }

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