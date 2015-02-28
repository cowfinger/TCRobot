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

        private IEnumerable<TCTask> ActiveTaskList
        {
            get
            {
                foreach (ListViewItem lvItem in this.listViewTasks.Items)
                {
                    yield return lvItem.Tag as TCTask;
                }
            }
        }

        private Dictionary<string, string> cityList = new Dictionary<string, string>();

        private string hostname = "yw1.tc.9wee.com";
        private Dictionary<string, LoginParam> multiLoginConf = new Dictionary<string, LoginParam>();

        private AutoResetEvent loginLock = new AutoResetEvent(true);

        private Random randGen = new Random(DateTime.Now.Millisecond);

        private Dictionary<string, AccountInfo> accountTable = new Dictionary<string, AccountInfo>();
        private string activeAccount;

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

            int maxSoilderNumber = 0;
            if (!int.TryParse(this.textBoxMaxTroopNumber.Text, out maxSoilderNumber))
            {
                maxSoilderNumber = 20000;
            }

            Parallel.Dispatch(this.accountTable, account =>
            {
                if (!account.Value.CityIDList.Contains(cityId))
                {
                    return;
                }

                var page = OpenCreateTeamPage(cityId, account.Key);
                var heroList = ParseHerosInCreateTeamPage(page);
                var soldierList = ParseSoldiersInCreateTeamPage(page).ToList();

                soldierList.Sort((x, y) => { return x.SoldierNumber.CompareTo(y.SoldierNumber); });
                soldierList.Reverse();

                if (this.radioButtonFullTroop.Checked)
                {
                    int totalSolderNumber = Math.Min(soldierList.Sum(x => x.SoldierNumber), maxSoilderNumber);
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
                        CreateTeam(cityId, hero, "", soldierString, this.checkBoxDefend.Checked ? "2" : "1", account.Key);
                    }
                }
            }).Then(() =>
            {
                this.Invoke(new DoSomething(() =>
                {
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
                var targetCityNameList = QueryTargetCityList(cityId).ToList();
                var influnceCityNameList = 

                this.Invoke(new DoSomething(() =>
                {
                    this.listBoxDstCities.Items.Clear();
                    foreach (var name in targetCityNameList)
                    {
                        this.listBoxDstCities.Items.Add(name);
                    }
                }));

            });

            var relatedAccountList = this.accountTable.Values.Where(account => account.CityIDList.Contains(cityId));
            Parallel.Dispatch(relatedAccountList, account =>
            {
                var singleAttackTeams = GetActiveTroopInfo(cityId, "1", account.UserName);
                var singleDefendTeams = GetActiveTroopInfo(cityId, "2", account.UserName);
                var groupAttackteams = GetGroupTeamList(cityId, account.UserName);
                foreach (var troop in singleAttackTeams.Concat(singleDefendTeams).Concat(groupAttackteams))
                {
                    this.Invoke(new DoSomething(() =>
                    {
                        if (troop.isGroupTroop)
                        {
                            var taskGroupIdList = this.ActiveTaskList.Where(
                                task => (task as SendTroopTask) != null && (task as SendTroopTask).taskData.isGroupTroop
                                ).Select(task => (task as SendTroopTask).taskData.GroupId);
                            if (taskGroupIdList.Contains(troop.GroupId))
                            {
                                return;
                            }
                        }
                        else
                        {
                            var taskTroopIdList = this.ActiveTaskList.Where(
                                task => (task as SendTroopTask) != null && !(task as SendTroopTask).taskData.isGroupTroop
                                ).Select(task => (task as SendTroopTask).taskData.TroopId);
                            if (taskTroopIdList.Contains(troop.TroopId))
                            {
                                return;
                            }
                        }

                        TrySyncTroopInfoToUI(troop);
                    }));
                }
            }).Then(() =>
            {
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

            var srcCityName = this.listBoxSrcCities.SelectedItem.ToString();
            var srcCityID = this.cityList[srcCityName];
            var dstCityName = this.listBoxDstCities.SelectedItem.ToString();

            var troopList = this.CityTroopList.ToList().Where(troop => !troop.isDefendTroop);
            Parallel.Dispatch(troopList, team =>
            {
                string cityPage = OpenCityShowAttackPage(srcCityID, team.AccountName);
                string destCityID = ParseTargetCityID(cityPage, dstCityName);

                if (string.IsNullOrEmpty(destCityID))
                {
                    string groupAttackPage = OpenGroupTeamListPage(srcCityID, team.AccountName);
                    destCityID = ParseTargetCityID(groupAttackPage, dstCityName);
                    if (string.IsNullOrEmpty(destCityID))
                    {
                        return;
                    }
                }

                team.ToCityNodeId = destCityID;

                string attackPage = team.isGroupTroop ?
                    OpenGroupAttackPage(team.GroupId, destCityID, team.AccountName) :
                    OpenTeamAttackPage(team.TroopId, destCityID, team.AccountName);

                var durationString = ParseAttackDuration(attackPage);
                team.Duration = TimeStr2Sec(durationString);

                this.Invoke(new DoSomething(() => { TrySyncTroopInfoToUI(team); }));
            }).Then(() =>
            {
                this.Invoke(new DoSomething(() =>
                {
                    this.btnAutoAttack.Enabled = true;
                    this.btnConfirmMainTroops.Enabled = true;
                }));
            });
        }

        private void btnConfirmMainTroops_Click(object sender, EventArgs e)
        {
            if (this.listViewTroops.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择需要出发的部队.");
                return;
            }

            TimeSpan diff = this.dateTimePickerArrival.Value - this.RemoteTime;
            var selectedTroops = new List<TroopInfo>();
            foreach (ListViewItem lvItem in this.listViewTroops.CheckedItems)
            {
                var troop = lvItem.Tag as TroopInfo;
                if (troop != null)
                {
                    selectedTroops.Add(troop);
                }
            }

            int maxDuration = selectedTroops.Max(troop => troop.Duration);

            if (maxDuration > diff.TotalSeconds)
            {
                var minArrivalTime = this.RemoteTime.AddSeconds(maxDuration);
                var result = MessageBox.Show(
                    string.Format("建议到达时间必须晚于{0}", minArrivalTime),
                    "是否使用建议时间",
                    MessageBoxButtons.YesNo
                    );
                if (result == DialogResult.Yes)
                {
                    this.dateTimePickerArrival.Value = minArrivalTime.AddSeconds(60);
                }
            }

            StartSendTroopTasks();
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

        private void btnCancelTasks_Click(object sender, EventArgs e)
        {
            if (this.listViewTasks.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择需要取消的任务.");
                return;
            }

            StopSendTroopTasks();
            btnConfirmMainTroops.Text = "确认攻击";
        }

        private void ToolStripMenuItemLoadAccountFile_Click(object sender, EventArgs e)
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
                        AccountInfo accountinfo = null;
                        if (!this.accountTable.TryGetValue(strs[0], out accountinfo))
                        {
                            accountinfo = new AccountInfo();
                            this.accountTable.Add(strs[0], accountinfo);
                        }

                        accountinfo.UserName = strs[0];
                        accountinfo.Password = strs[1];
                        if (strs.Length > 2)
                            accountinfo.AccountType = strs[2];
                        else
                            accountinfo.AccountType = "tianya";

                        accountinfo.CookieStr = "";
                        accountinfo.LoginStatus = "off-line";

                    }

                    line = sr.ReadLine();
                }

                foreach (var account in this.accountTable.Values)
                {
                    TryLoadAccountCookie(account);
                }

                SyncAccountsStatus();

                if (this.accountTable.Keys.Any())
                {
                    this.ToolStripMenuItemBatchLogin.Enabled = true;
                }
            }
        }

        private void ToolStripMenuItemBatchLogin_Click(object sender, EventArgs e)
        {
            Task.Run(() => { BatchLoginProc(); });
        }

        private void ToolStripMenuItemScan_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var validCityNameList = this.accountTable.Values.SelectMany(
                    account =>
                    {
                        var cityNameList = GetAccountInflunceCityNameListWithArmy(account.UserName);
                        account.CityNameList = cityNameList;
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
                }));
            });
        }

        private void ToolStripMenuItemDonation_Click(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var accountList = this.accountTable.Keys.ToList();

                for (int i = 0; i < 1000 && accountList.Any(); ++i)
                {
                    BatchDonate(i, ref accountList);
                }
            });
        }

        private void ToolStripMenuItemReliveHero_Click(object sender, EventArgs e)
        {
            StartReliveHeroTimer();
        }

        private void ToolStripMenuItemQuickReliveHero_Click(object sender, EventArgs e)
        {
            Parallel.Dispatch(this.accountTable.Values, account =>
            {
                var heroPage = OpenHeroPage(account.UserName);
                var heroList = ParseHeroList(heroPage, account.UserName).ToList();

                if (tabControlMainInfo.SelectedTab.Name == "tabPageHero")
                {
                    UpdateHeroTable(heroList);
                }

                var deadHeroList = heroList.Where(hero => hero.IsDead).ToList();
                if (!deadHeroList.Any())
                {
                    return;
                }

                int status = 0;
                foreach (var toReliveHero in deadHeroList)
                {
                    if (status == 0) // relive running now.
                    {
                        status = 1;
                        if (!heroPage.Contains("[[jslang('hero_status_8')]")) // relive running now.
                        {
                            ReliveHero(toReliveHero.HeroId, account.UserName);
                        }
                    }
                    else
                    {
                        ReliveHero(toReliveHero.HeroId, account.UserName);
                    }

                    var tid = GetTid(account);
                    var reliveQueueId = QueryReliveQueueId(tid, account);
                    var reliveItem = QueryReliveItem(reliveQueueId, tid, account);
                    UserReliveItem(reliveItem, toReliveHero.HeroId, reliveQueueId, tid, account);
                }
            }).Then(() =>
            {
                MessageBox.Show(string.Format("复活武将完成"));
            });
        }

        private void tabControlMainInfo_Selected(object sender, TabControlEventArgs e)
        {
            if (this.tabControlMainInfo.SelectedTab.Name == "tabPageHero")
            {
                this.listViewAccountHero.Items.Clear();

                Parallel.Dispatch(this.accountTable.Values, account =>
                {
                    var heroList = QueryHeroList(account.UserName).ToList();

                    this.Invoke(new DoSomething(() =>
                    {
                        foreach (var hero in heroList)
                        {
                            var lvItem = new ListViewItem();
                            lvItem.Tag = hero;
                            lvItem.SubItems[0].Text = hero.AccountName;
                            lvItem.SubItems.Add(hero.Name);
                            lvItem.SubItems.Add(hero.IsDead.ToString());
                            this.listViewAccountHero.Items.Add(lvItem);
                        }
                    }));
                });
            }
        }

        private void tabControlTask_Selected(object sender, TabControlEventArgs e)
        {
            if (this.tabControlTask.SelectedTab.Name == "tabPageMoveArmy")
            {
                TryBuildInfluenceMaps();
            }
        }

        private void comboBoxAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedAccount = this.comboBoxAccount.SelectedItem.ToString();

            AccountInfo account = null;
            if (!this.accountTable.TryGetValue(selectedAccount, out account))
            {
                MessageBox.Show("账号不存在");
                return;
            }

            this.comboBoxFromCity.Items.Clear();
            foreach (var cityName in account.CityNameList)
            {
                this.comboBoxFromCity.Items.Add(cityName);
            }

            this.comboBoxToCity.Items.Clear();
            foreach (var cityName in account.InfluenceMap.Keys)
            {
                this.comboBoxToCity.Items.Add(cityName);
            }
        }

        private void comboBoxFromCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fromCity = this.comboBoxFromCity.SelectedItem.ToString();
            string accountName = this.comboBoxAccount.SelectedItem.ToString();
            Task.Run(() =>
            {
                string cityId = this.cityList[fromCity];
                string cityPage = OpenCityPage(cityId, accountName);
                var soldiers = ParseSoldierInfoFromCityPage(cityPage).ToList();
                var heroes = ParseHeroNameListFromCityPage(cityPage).ToList();

                if (!soldiers.Any())
                {
                    MessageBox.Show("没有空闲的部队，请先解散部队.");
                }

                if (!heroes.Any())
                {
                    MessageBox.Show("没有空闲的将领，请先解散部队.");
                }

                this.Invoke(new DoSomething(() =>
                {
                    this.listViewAccountArmy.Items.Clear();
                    foreach (var soldierInfo in soldiers)
                    {
                        var lvItem = new ListViewItem();
                        lvItem.Tag = soldierInfo;
                        lvItem.SubItems[0].Text = soldierInfo.Name;
                        lvItem.SubItems.Add(soldierInfo.SoldierNumber.ToString());
                        this.listViewAccountArmy.Items.Add(lvItem);
                    }

                    this.listViewMoveHero.Items.Clear();
                    foreach (var hero in heroes)
                    {
                        var lvItem = new ListViewItem();
                        lvItem.SubItems[0].Text = hero;
                        this.listViewMoveHero.Items.Add(lvItem);
                    }
                }));
            });
        }

        private void comboBoxToCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            string fromCity = this.comboBoxFromCity.SelectedItem.ToString();
            string toCity = this.comboBoxToCity.SelectedItem.ToString();
            string accountName = this.comboBoxAccount.SelectedItem.ToString();
            var account = this.accountTable[accountName];

            var path = new DijstraHelper(account.InfluenceMap).GetPath(fromCity, toCity).ToList();
            path.Reverse();

            this.listBoxMovePath.Items.Clear();
            foreach (var city in path)
            {
                this.listBoxMovePath.Items.Add(city);
            }
        }
    }
}
