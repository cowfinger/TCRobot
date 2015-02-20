using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading;

namespace TC
{
    public partial class FormMain : Form
    {
        private static string UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)";

        // private List<TroopInfo> troopList = new List<TroopInfo>();

        private IEnumerable<TroopInfo> CityTroopList
        {
            get
            {
                foreach (ListViewItem lvItem in this.listViewTroops.Items)
                {
                    yield return lvItem.Tag as TroopInfo;
                }
            }
        }

        private IEnumerable<AttackTask> ActiveTaskList
        {
            get
            {
                foreach (ListViewItem lvItem in this.listViewTasks.Items)
                {
                    yield return lvItem.Tag as AttackTask;
                }
            }
        }

        private Dictionary<string, string> cityList = new Dictionary<string, string>();

        private string hostname = "yw1.tc.9wee.com";
        private Dictionary<string, LoginParam> multiLoginConf = new Dictionary<string, LoginParam>();

        private string m_srcCityID = "";
        private string m_srcCityName = "";
        private string destCityID = "";
        private string m_dstCityName = "";

        private AutoResetEvent loginLock = new AutoResetEvent(true);
        private AutoResetEvent m_sendTroopLock = new AutoResetEvent(true);

        private Random randGen = new Random(DateTime.Now.Millisecond);

        private Dictionary<string, AccountInfo> accountTable = new Dictionary<string, AccountInfo>();
        private string activeAccount;

        private object remoteTimeLock = new object();
        private System.Timers.Timer syncTimeToUITimer = new System.Timers.Timer(500);
        private System.Timers.Timer syncRemoteTimeTimer = new System.Timers.Timer(15 * 1000);
        private DateTime remoteTime = DateTime.MinValue;

        private DateTime RemoteTime
        {
            get
            {
                lock (this.remoteTimeLock)
                {
                    return this.remoteTime;
                }
            }

            set
            {
                lock (this.remoteTimeLock)
                {
                    this.remoteTime = value;
                }
            }
        }

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            LoadMultiLoginConf();
            LoadCityList();
            LoadCheckpoint();
        }

        private void btnScanCity_Click(object sender, EventArgs e)
        {
            btnScanCity.Enabled = false;
            Task.Run(() =>
            {
                var validCityNameList = this.accountTable.Values.SelectMany(
                    account =>
                    {
                        this.Invoke(new DoSomething(() =>
                        {
                            this.txtInfo.Text = string.Format("Scan Account:{0}", account.UserName);
                        }));

                        var cityNameList = GetAccountInflunceCityNameListWithArmy(account.UserName);
                        account.CityIDList = cityNameList.Select(cityName => this.cityList[cityName]);

                        return cityNameList;
                    }).ToList().Distinct();

                this.Invoke(new DoSomething(() =>
                {
                    listBoxSrcCities.Items.Clear();
                    foreach (var city in validCityNameList)
                    {
                        listBoxSrcCities.Items.Add(city);
                    }

                    this.txtInfo.Text = string.Format("Scan Completed");
                    btnScanCity.Enabled = true;
                }));
            });
        }

        private void btnLoginAll_Click(object sender, EventArgs e)
        {
            btnLoginAll.Enabled = false;
            btnLoginAll.Text = "登录中";
            Task.Run(() => { BatchLoginProc(); });
        }

        private void btnQuickCreateTroop_Click(object sender, EventArgs e)
        {
            var cityName = this.listBoxSrcCities.SelectedItem as string;
            if (string.IsNullOrEmpty(cityName))
            {
                return;
            }

            if (!cityList.ContainsKey(cityName))
            {
                return;
            }

            var cityId = cityList[cityName];
            this.btnQuickCreateTroop.Enabled = false;

            Task.Run(() =>
            {
                foreach (var account in this.accountTable)
                {
                    if (!account.Value.CityIDList.Contains(cityId))
                    {
                        continue;
                    }

                    this.Invoke(new DoSomething(() =>
                    {
                        this.txtInfo.Text = string.Format("Create Troop for Account: {0} at {1}", account.Key, cityName);
                    }));

                    var page = OpenCreateTeamPage(cityId, account.Key);
                    var heroList = ParseHerosInCreateTeamPage(page);
                    var soldierList = ParseSoldiersInCreateTeamPage(page).ToList();

                    soldierList.Sort((x, y) => { return x.SoldierNumber.CompareTo(y.SoldierNumber); });
                    soldierList.Reverse();

                    if (this.radioButtonFullTroop.Checked)
                    {
                        int totalSolderNumber = soldierList.Sum(x => x.SoldierNumber);
                        string soldierString = BuildSoldierString(ref soldierList, totalSolderNumber);

                        var heroRawList = heroList.ToList();
                        string headHero = heroRawList.First();
                        heroRawList.RemoveAt(0);
                        string subHeroes = BuildSubHeroesString(ref heroRawList);

                        CreateTeam(cityId, headHero, subHeroes, soldierString, this.checkBoxDefend.Checked ? "2" : "1", account.Key);
                    }
                    else
                    {
                        foreach (var hero in heroList)
                        {
                            string soldierString = BuildSoldierString(ref soldierList, this.radioButtonSmallTroop.Checked ? 1000 : 0);
                            this.Invoke(new DoSomething(() =>
                            {
                                this.txtInfo.Text = string.Format(
                                    "Create Troop for Account: {0} at {1}, Hero={2}, Soldier={3}",
                                    account.Key,
                                    cityName,
                                    hero,
                                    soldierString
                                    );
                            }));

                            CreateTeam(cityId, hero, "", soldierString, this.checkBoxDefend.Checked ? "2" : "1", account.Key);
                        }
                    }
                }

                this.Invoke(new DoSomething(() =>
                {
                    this.txtInfo.Text = string.Format("Create Troop Completed");
                    this.btnQuickCreateTroop.Enabled = true;
                }));

                var troopList = QueryCityTroops(cityId);

                this.Invoke(new DoSomething(() => { RefreshTroopInfoToUI(troopList); }));
            });
        }

        private void listBoxDstCities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBoxSrcCities.SelectedItems.Count > 0 &&
                this.listBoxDstCities.SelectedItems.Count > 0 &&
                this.listViewTroops.CheckedItems.Count > 0)
            {
                btnAutoAttack.Enabled = true;
            }
        }

        private void listBoxSrcCities_SelectedIndexChanged(object sender, EventArgs e)
        {
            string cityname = this.listBoxSrcCities.SelectedItem as string;
            if (cityname == null)
            {
                return;
            }

            if (!cityList.ContainsKey(cityname))
            {
                return;
            }

            this.listViewTroops.Items.Clear();
            this.listViewTroops.Enabled = false;
            this.listBoxDstCities.Enabled = false;
            this.listBoxSrcCities.Enabled = false;

            string cityId = cityList[cityname];
            Task.Run(() =>
            {
                this.Invoke(new DoSomething(() =>
                {
                    this.listBoxDstCities.Items.Clear();
                }));

                var targetCityNameList = QueryTargetCityList(cityId);

                foreach (var name in targetCityNameList)
                {
                    this.Invoke(new DoSomething(() =>
                    {
                        this.listBoxDstCities.Items.Add(name);
                    }));
                }

                var troopList = QueryCityTroops(cityId);
                foreach (var troop in troopList)
                {
                    this.Invoke(new DoSomething(() =>
                    {
                        if (troop.isGroupTroop)
                        {
                            var taskGroupIdList = this.ActiveTaskList.Where(task => task.Troop.isGroupTroop).Select(task => task.Troop.GroupId);
                            if (taskGroupIdList.Contains(troop.GroupId))
                            {
                                return;
                            }
                        }
                        else
                        {
                            var taskTroopIdList = this.ActiveTaskList.Where(task => !task.Troop.isGroupTroop).Select(task => task.Troop.TroopId);
                            if (taskTroopIdList.Contains(troop.TroopId))
                            {
                                return;
                            }
                        }

                        TrySyncTroopInfoToUI(troop);
                    }));
                }

                this.Invoke(new DoSomething(() =>
                {
                    this.listViewTroops.Enabled = true;
                    this.listBoxDstCities.Enabled = true;
                    this.listBoxSrcCities.Enabled = true;
                }));
            });
        }

        private void btnDismissTroop_Click(object sender, EventArgs e)
        {
            if (this.listViewTroops.CheckedItems.Count <= 0)
            {
                return;
            }

            var targetTroops = new List<TroopInfo>();
            foreach (ListViewItem item in this.listViewTroops.CheckedItems)
            {
                var troop = item.Tag as TroopInfo;
                if (troop == null)
                {
                    continue;
                }

                targetTroops.Add(troop);
            }

            if (targetTroops.Count == 0)
            {
                return;
            }

            this.btnDismissTroop.Enabled = false;
            this.listViewTroops.Enabled = false;

            Task.Run(() =>
            {
                foreach (var troop in targetTroops)
                {
                    if (troop.isGroupTroop)
                    {
                        if (troop.IsGroupHead)
                        {
                            DismissGroup(troop.GroupId, troop.AccountName);
                        }
                    }
                    else
                    {
                        DismissTeam(troop.TroopId, troop.AccountName);
                    }

                    this.Invoke(new DoSomething(() =>
                    {
                        foreach (ListViewItem item in this.listViewTroops.CheckedItems)
                        {
                            var troopInfo = item.Tag as TroopInfo;
                            if (troopInfo == troop)
                            {
                                this.listViewTroops.Items.Remove(item);
                            }
                        }
                    }));
                }

                this.Invoke(new DoSomething(() =>
                {
                    btnDismissTroop.Enabled = true;
                    this.listViewTroops.Enabled = true;
                }));
            });
        }

        private void listViewTasks_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            int candidateTroopCount = 0;
            foreach (ListViewItem item in this.listViewTroops.CheckedItems)
            {
                var troop = item.Tag as TroopInfo;
                if (troop == null)
                {
                    continue;
                }

                ++candidateTroopCount;
            }

            this.btnDismissTroop.Enabled = candidateTroopCount > 0;
            this.btnGroupTroop.Enabled = candidateTroopCount >= 2;
        }

        private void checkBoxSelectAllTasks_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listViewTroops.Items)
            {
                item.Checked = this.checkBoxSelectAllTasks.Checked;
            }
        }

        private void AutoAttack_Click(object sender, EventArgs e)
        {
            if (listBoxSrcCities.SelectedItem == null ||
                listBoxDstCities.SelectedItem == null)
            {
                MessageBox.Show("Please select source city and destination city");
                return;
            }

            btnAutoAttack.Enabled = false;

            m_srcCityName = listBoxSrcCities.SelectedItem.ToString();
            m_srcCityID = cityList[m_srcCityName];
            m_dstCityName = listBoxDstCities.SelectedItem.ToString();

            var troopList = this.CityTroopList.ToList();

            Task.Run(() => AutoAttackProc(troopList));
        }

        private void btnConfirmMainTroops_Click(object sender, EventArgs e)
        {
            if (btnConfirmMainTroops.Text != "取消")
            {
                TimeSpan diff = this.dateTimePickerArrival.Value - this.RemoteTime;
                int maxDuration = this.CityTroopList.Max(troop => troop.Duration);

                if (maxDuration > diff.TotalSeconds)
                {
                    MessageBox.Show(string.Format("到达时间必须晚于{0}", this.RemoteTime.AddSeconds(maxDuration)));
                    return;
                }

                btnConfirmMainTroops.Text = "取消";
                StartSendTroopTasks();
            }
            else
            {
                StopSendTroopTasks();
                btnConfirmMainTroops.Text = "确认攻击";
            }
        }

        private void btnLoadProfile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            DialogResult ret = dlg.ShowDialog();
            if (ret == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(dlg.FileName, System.Text.Encoding.Default);
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] strs = line.Split('|', ':');
                    if (strs.Length > 1)
                    {
                        AccountInfo accountinfo = new AccountInfo();
                        {
                            accountinfo.UserName = strs[0];
                            accountinfo.Password = strs[1];
                            if (strs.Length > 2)
                                accountinfo.AccountType = strs[2];
                            else
                                accountinfo.AccountType = "tianya";

                            accountinfo.CookieStr = "";
                            accountinfo.LoginStatus = "off-line";
                        }
                        this.accountTable.Add(strs[0], accountinfo);
                    }
                    line = sr.ReadLine();
                }

                SyncAccountsStatus();
                if (this.accountTable.Keys.Count > 0)
                {
                    btnLoadProfile.Enabled = false;
                    btnLoginAll.Enabled = true;
                }

            }
        }

        private void btnContribute_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var accountList = this.accountTable.Keys.ToList();

                for (int i = 0; i < 1000 && accountList.Any(); ++i)
                {
                    BatchDonate(i, ref accountList);
                }

                this.Invoke(new DoSomething(() =>
                {
                    this.txtInfo.Text = string.Format("Donate: Completed");
                }));
            });
        }

        private void btnGroupTroop_Click(object sender, EventArgs e)
        {
            var candidateTroops = new List<TroopInfo>();
            foreach (ListViewItem item in this.listViewTroops.CheckedItems)
            {
                candidateTroops.Add(item.Tag as TroopInfo);
            }

            var dialog = new FormChooseTroopHead(candidateTroops);
            dialog.ShowDialog();
            if (dialog.GroupHead == null)
            {
                return;
            }

            TroopInfo headTroop = dialog.GroupHead;
            candidateTroops.Remove(headTroop);

            string cityId = this.cityList[this.listBoxSrcCities.SelectedItem.ToString()];

            this.btnGroupTroop.Enabled = false;
            Task.Run(() =>
                {
                    var troopGroup = candidateTroops.GroupBy(troop => troop.AccountName).Select(troops => troops.First()).ToList();
                    if (!troopGroup.Any())
                    {
                        return;
                    }

                    if (!headTroop.isGroupTroop)
                    {
                        string groupName = CreateGroupHead(cityId, headTroop.TroopId, headTroop.AccountName);
                        var groupTroops = GetGroupTeamList(cityId, headTroop.AccountName);
                        headTroop = groupTroops.Where(troop => troop.Name == groupName).FirstOrDefault();
                        if (headTroop == null || headTroop.Name != groupName)
                        {
                            return;
                        }
                    }

                    foreach (var troop in troopGroup)
                    {
                        JoinGroup(cityId, headTroop.GroupId, troop.TroopId, troop.AccountName);
                    }

                    var troopList = QueryCityTroops(cityId).ToList();

                    this.Invoke(new DoSomething(() =>
                    {
                        RefreshTroopInfoToUI(troopList);
                    }));
                });
        }
    }
}
