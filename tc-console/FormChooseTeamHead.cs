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
    public partial class FormChooseTeamHead : Form
    {
        public TeamInfo GroupHead { get; set; }

        public FormChooseTeamHead(IEnumerable<TeamInfo> teamList)
        {
            InitializeComponent();

            this.GroupHead = null;
            foreach (var team in teamList)
            {
                var lvItem = new ListViewItem();
                lvItem.SubItems.Add(team.AccountName);
                lvItem.SubItems.Add(team.PowerIndex.ToString());
                lvItem.SubItems.Add(team.TeamId);
                lvItem.Tag = team;
                this.listViewTeam.Items.Add(lvItem);
            }
        }

        private void listViewTeam_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.btnOk.Enabled = this.listViewTeam.CheckedItems.Count == 1;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.GroupHead = this.listViewTeam.CheckedItems[0].Tag as TeamInfo;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.GroupHead = null;
            this.Close();
        }
    }
}
