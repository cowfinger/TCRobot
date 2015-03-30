namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    public partial class FormSelectCity : Form
    {
        public FormSelectCity()
        {
            this.InitializeComponent();
            this.IsOk = false;
        }

        public bool IsOk { get; private set; }

        public List<string> CityList { get; set; }

        public List<string> CheckedCityList { get; private set; }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.CheckedCityList =
                this.listViewCityList.CheckedItems.Cast<ListViewItem>().Select(lvItem => lvItem.Text).ToList();
            this.IsOk = true;
            this.Close();
        }

        private void FormSelectCity_Load(object sender, EventArgs e)
        {
            this.listViewCityList.Items.AddRange(this.CityList.Select(city => new ListViewItem(city)).ToArray());
        }

        private void checkBoxSelecteAllCities_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in this.listViewCityList.Items)
            {
                lvItem.Checked = this.checkBoxSelecteAllCities.Checked;
            }
        }
    }
}