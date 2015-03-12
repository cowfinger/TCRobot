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
    public partial class FormSelectCity : Form
    {
        public bool IsOk { get; private set; }

        public List<string> CityList { get; set; }

        public List<string> CheckedCityList { get; private set; }

        public FormSelectCity()
        {
            InitializeComponent();
            this.IsOk = false;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.CheckedCityList = this.listViewCityList.CheckedItems.Cast<ListViewItem>().Select(lvItem => lvItem.Text).ToList();
            this.IsOk = true;
            this.Close();
        }

        private void FormSelectCity_Load(object sender, EventArgs e)
        {
            this.listViewCityList.Items.AddRange(
                this.CityList.Select(city => new ListViewItem(city)).ToArray()
                );
        }

    }
}
