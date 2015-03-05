namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    public partial class FormMain : Form
    {
        private const string UserAgent = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)";

        private readonly Dictionary<string, AccountInfo> accountTable = new Dictionary<string, AccountInfo>();

        private readonly Dictionary<string, string> cityList = new Dictionary<string, string>();

        private readonly AutoResetEvent loginLock = new AutoResetEvent(true);

        private readonly Dictionary<string, LoginParam> multiLoginConf = new Dictionary<string, LoginParam>();

        private readonly Random randGen = new Random(DateTime.Now.Millisecond);

        private string activeAccount;

        private string hostname = "yw1.tc.9wee.com";

        public FormMain()
        {
            this.InitializeComponent();
        }

        private IEnumerable<TroopInfo> CityTroopList
        {
            get
            {
                return from ListViewItem lvItem in this.listViewTroops.Items select lvItem.Tag as TroopInfo;
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

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.LoadMultiLoginConf();
            this.LoadCityList();
            this.LoadCheckpoint();
        }

        private void btnQuickCreateTroop_Click(object sender, EventArgs e)
        {
            var cityName = this.listBoxSrcCities.SelectedItem as string;
            if (string.IsNullOrEmpty(cityName))
            {
                return;
            }

            if (!this.cityList.ContainsKey(cityName))
            {
                return;
            }

            var cityId = this.cityList[cityName];
            this.btnQuickCreateTroop.Enabled = false;

            var maxSoilderNumber = 0;
            if (!int.TryParse(this.textBoxMaxTroopNumber.Text, out maxSoilderNumber))
            {
                maxSoilderNumber = 20000;
            }

            Parallel.Dispatch(
                this.accountTable,
                account =>
                {
                    if (!account.Value.CityIDList.Contains(cityId))
                    {
                        return;
                    }

                    var page = this.OpenCreateTeamPage(cityId, account.Key);
                    var heroList = this.ParseHerosInCreateTeamPage(page);
                    var soldierList = this.ParseSoldiersInCreateTeamPage(page).ToList();

                    soldierList.Sort((x, y) => { return x.SoldierNumber.CompareTo(y.SoldierNumber); });
                    soldierList.Reverse();

                    if (this.radioButtonFullTroop.Checked)
                    {
                        var totalSolderNumber = Math.Min(soldierList.Sum(x => x.SoldierNumber), maxSoilderNumber);
                        var soldierString = this.BuildSoldierString(ref soldierList, totalSolderNumber);

                        var heroRawList = heroList.ToList();
                        var headHero = heroRawList.First();
                        heroRawList.RemoveAt(0);
                        var subHeroes = this.BuildSubHeroesString(ref heroRawList);

                        this.CreateTeam(
                            cityId,
                            headHero,
                            subHeroes,
                            soldierString,
                            this.checkBoxDefend.Checked ? "2" : "1",
                            account.Key);
                    }
                    else
                    {
                        foreach (var hero in heroList)
                        {
                            var soldierString = this.BuildSoldierString(
                                ref soldierList,
                                this.radioButtonSmallTroop.Checked ? 1000 : 0);
                            this.CreateTeam(
                                cityId,
                                hero,
                                "",
                                soldierString,
                                this.checkBoxDefend.Checked ? "2" : "1",
                                account.Key);
                        }
                    }
                }).Then(
                        () =>
                        {
                            this.Invoke(new DoSomething(() => { this.btnQuickCreateTroop.Enabled = true; }));

                            var troopList = this.QueryCityTroops(cityId);

                            this.Invoke(new DoSomething(() => { this.RefreshTroopInfoToUI(troopList); }));
                        });
        }

        private void listBoxDstCities_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listBoxSrcCities.SelectedItems.Count > 0 && this.listBoxDstCities.SelectedItems.Count > 0
                && this.listViewTroops.CheckedItems.Count > 0)
            {
                this.btnAutoAttack.Enabled = true;
            }
        }

        private void listBoxSrcCities_SelectedIndexChanged(object sender, EventArgs e)
        {
            var cityname = this.listBoxSrcCities.SelectedItem as string;
            if (cityname == null)
            {
                return;
            }

            if (!this.cityList.ContainsKey(cityname))
            {
                return;
            }

            this.listViewTroops.Items.Clear();
            this.listViewTroops.Enabled = false;
            this.listBoxDstCities.Enabled = false;
            this.listBoxSrcCities.Enabled = false;

            var cityId = this.cityList[cityname];

            Task.Run(
                () =>
                {
                    var targetCityNameList = this.QueryTargetCityList(cityId).ToList();
                    var influnceCityNameList = this.Invoke(
                        new DoSomething(
                            () =>
                            {
                                this.listBoxDstCities.Items.Clear();
                                foreach (var name in targetCityNameList)
                                {
                                    this.listBoxDstCities.Items.Add(name);
                                }
                            }));
                });

            var relatedAccountList = this.accountTable.Values.Where(account => account.CityIDList.Contains(cityId));
            Parallel.Dispatch(
                relatedAccountList,
                account =>
                {
                    var singleAttackTeams = this.GetActiveTroopInfo(cityId, "1", account.UserName);
                    var singleDefendTeams = this.GetActiveTroopInfo(cityId, "2", account.UserName);
                    var groupAttackteams = this.GetGroupTeamList(cityId, account.UserName);
                    foreach (var troop in singleAttackTeams.Concat(singleDefendTeams).Concat(groupAttackteams))
                    {
                        this.Invoke(
                            new DoSomething(
                                () =>
                                {
                                    if (troop.isGroupTroop)
                                    {
                                        var taskGroupIdList =
                                            this.ActiveTaskList.Where(
                                                task =>
                                                (task as SendTroopTask) != null
                                                && (task as SendTroopTask).taskData.isGroupTroop)
                                                .Select(task => (task as SendTroopTask).taskData.GroupId);
                                        if (taskGroupIdList.Contains(troop.GroupId))
                                        {
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        var taskTroopIdList =
                                            this.ActiveTaskList.Where(
                                                task =>
                                                (task as SendTroopTask) != null
                                                && !(task as SendTroopTask).taskData.isGroupTroop)
                                                .Select(task => (task as SendTroopTask).taskData.TroopId);
                                        if (taskTroopIdList.Contains(troop.TroopId))
                                        {
                                            return;
                                        }
                                    }

                                    this.TrySyncTroopInfoToUI(troop);
                                }));
                    }
                }).Then(
                        () =>
                        {
                            this.Invoke(
                                new DoSomething(
                                    () =>
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

            Task.Run(
                () =>
                {
                    foreach (var troop in targetTroops)
                    {
                        if (troop.isGroupTroop)
                        {
                            if (troop.IsGroupHead)
                            {
                                this.DismissGroup(troop.GroupId, troop.AccountName);
                            }
                        }
                        else
                        {
                            this.DismissTeam(troop.TroopId, troop.AccountName);
                        }

                        this.Invoke(
                            new DoSomething(
                                () =>
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

                    this.Invoke(
                        new DoSomething(
                            () =>
                            {
                                this.btnDismissTroop.Enabled = true;
                                this.listViewTroops.Enabled = true;
                            }));
                });
        }

        private void listViewTasks_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            var candidateTroopCount = 0;
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
            if (this.listBoxSrcCities.SelectedItem == null || this.listBoxDstCities.SelectedItem == null)
            {
                MessageBox.Show("Please select source city and destination city");
                return;
            }

            this.btnAutoAttack.Enabled = false;

            var srcCityName = this.listBoxSrcCities.SelectedItem.ToString();
            var srcCityID = this.cityList[srcCityName];
            var dstCityName = this.listBoxDstCities.SelectedItem.ToString();

            var troopList = this.CityTroopList.ToList().Where(troop => !troop.isDefendTroop);
            Parallel.Dispatch(
                troopList,
                team =>
                {
                    var cityPage = this.OpenCityShowAttackPage(srcCityID, team.AccountName);
                    var destCityID = this.ParseTargetCityID(cityPage, dstCityName);

                    if (string.IsNullOrEmpty(destCityID))
                    {
                        var groupAttackPage = this.OpenGroupTeamListPage(srcCityID, team.AccountName);
                        destCityID = this.ParseTargetCityID(groupAttackPage, dstCityName);
                        if (string.IsNullOrEmpty(destCityID))
                        {
                            return;
                        }
                    }

                    team.ToCityNodeId = destCityID;

                    var attackPage = team.isGroupTroop
                                         ? this.OpenGroupAttackPage(team.GroupId, destCityID, team.AccountName)
                                         : this.OpenTeamAttackPage(team.TroopId, destCityID, team.AccountName);

                    var durationString = this.ParseAttackDuration(attackPage);
                    team.Duration = this.TimeStr2Sec(durationString);

                    this.Invoke(new DoSomething(() => { this.TrySyncTroopInfoToUI(team); }));
                }).Then(
                        () =>
                        {
                            this.Invoke(
                                new DoSomething(
                                    () =>
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

            var diff = this.dateTimePickerArrival.Value - this.RemoteTime;
            var selectedTroops = new List<TroopInfo>();
            foreach (ListViewItem lvItem in this.listViewTroops.CheckedItems)
            {
                var troop = lvItem.Tag as TroopInfo;
                if (troop != null)
                {
                    selectedTroops.Add(troop);
                }
            }

            var maxDuration = selectedTroops.Max(troop => troop.Duration);

            if (maxDuration > diff.TotalSeconds)
            {
                var minArrivalTime = this.RemoteTime.AddSeconds(maxDuration);
                var result = MessageBox.Show(
                    string.Format("建议到达时间必须晚于{0}", minArrivalTime),
                    "是否使用建议时间",
                    MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    this.dateTimePickerArrival.Value = minArrivalTime.AddSeconds(60);
                }
            }

            this.StartSendTroopTasks();
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

            var headTroop = dialog.GroupHead;
            candidateTroops.Remove(headTroop);

            var cityId = this.cityList[this.listBoxSrcCities.SelectedItem.ToString()];

            this.btnGroupTroop.Enabled = false;
            Task.Run(
                () =>
                {
                    var troopGroup =
                        candidateTroops.GroupBy(troop => troop.AccountName)
                            .Select(troops => troops.First())
                            .ToList();
                    if (!troopGroup.Any())
                    {
                        return;
                    }

                    if (!headTroop.isGroupTroop)
                    {
                        var groupName = this.CreateGroupHead(cityId, headTroop.TroopId, headTroop.AccountName);
                        var groupTroops = this.GetGroupTeamList(cityId, headTroop.AccountName);
                        headTroop = groupTroops.Where(troop => troop.Name == groupName).FirstOrDefault();
                        if (headTroop == null || headTroop.Name != groupName)
                        {
                            return;
                        }
                    }

                    foreach (var troop in troopGroup)
                    {
                        this.JoinGroup(cityId, headTroop.GroupId, troop.TroopId, troop.AccountName);
                    }

                    var troopList = this.QueryCityTroops(cityId).ToList();

                    this.Invoke(new DoSomething(() => { this.RefreshTroopInfoToUI(troopList); }));
                });
        }

        private void btnCancelTasks_Click(object sender, EventArgs e)
        {
            if (this.listViewTasks.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择需要取消的任务.");
                return;
            }

            this.StopSendTroopTasks();
            this.btnConfirmMainTroops.Text = "确认攻击";
        }

        private void ToolStripMenuItemLoadAccountFile_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            var ret = dlg.ShowDialog();
            if (ret == DialogResult.OK)
            {
                var sr = new StreamReader(dlg.FileName, Encoding.Default);
                var line = sr.ReadLine();
                while (line != null)
                {
                    var strs = line.Split('|', ':');
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
                        {
                            accountinfo.AccountType = strs[2];
                        }
                        else
                        {
                            accountinfo.AccountType = "tianya";
                        }

                        accountinfo.CookieStr = "";
                        accountinfo.LoginStatus = "off-line";
                    }

                    line = sr.ReadLine();
                }

                foreach (var account in this.accountTable.Values)
                {
                    this.TryLoadAccountCookie(account);
                }

                this.SyncAccountsStatus();

                if (this.accountTable.Keys.Any())
                {
                    this.ToolStripMenuItemBatchLogin.Enabled = true;
                }
            }
        }

        private void ToolStripMenuItemBatchLogin_Click(object sender, EventArgs e)
        {
            Task.Run(() => { this.BatchLoginProc(); });
        }

        private void ToolStripMenuItemScan_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    var validCityNameList = this.accountTable.Values.SelectMany(
                        account =>
                        {
                            var cityNameList = this.GetAccountInflunceCityNameListWithArmy(account.UserName);
                            account.CityNameList = cityNameList;
                            account.CityIDList = cityNameList.Select(cityName => this.cityList[cityName]);

                            return cityNameList;
                        }).ToList().Distinct();

                    this.Invoke(
                        new DoSomething(
                            () =>
                            {
                                this.listBoxSrcCities.Items.Clear();
                                foreach (var city in validCityNameList)
                                {
                                    this.listBoxSrcCities.Items.Add(city);
                                }
                            }));
                });
        }

        private void ToolStripMenuItemDonation_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    var accountList = this.accountTable.Keys.ToList();

                    for (var i = 0; i < 1000 && accountList.Any(); ++i)
                    {
                        this.BatchDonate(i, ref accountList);
                    }
                });
        }

        private void ToolStripMenuItemReliveHero_Click(object sender, EventArgs e)
        {
            this.StartReliveHeroTimer();
        }

        private void ToolStripMenuItemQuickReliveHero_Click(object sender, EventArgs e)
        {
            Parallel.Dispatch(
                this.accountTable.Values,
                account =>
                {
                    var heroPage = this.OpenHeroPage(account.UserName);
                    var heroList = this.ParseHeroList(heroPage, account.UserName).ToList();

                    this.Invoke(
                        new DoSomething(
                            () =>
                            {
                                if (this.tabControlMainInfo.SelectedTab.Name == "tabPageHero")
                                {
                                    this.UpdateHeroTable(heroList);
                                }
                            }));

                    var deadHeroList = heroList.Where(hero => hero.IsDead).ToList();
                    if (!deadHeroList.Any())
                    {
                        return;
                    }

                    var status = 0;
                    foreach (var toReliveHero in deadHeroList)
                    {
                        if (status == 0) // relive running now.
                        {
                            status = 1;
                            if (!heroPage.Contains("[[jslang('hero_status_8')]")) // relive running now.
                            {
                                this.ReliveHero(toReliveHero.HeroId, account.UserName);
                            }
                        }
                        else
                        {
                            this.ReliveHero(toReliveHero.HeroId, account.UserName);
                        }

                        var tid = this.GetTid(account);
                        var reliveQueueId = this.QueryReliveQueueId(tid, account);
                        var reliveItem = this.QueryReliveItem(reliveQueueId, tid, account);
                        this.UserReliveItem(reliveItem, toReliveHero.HeroId, reliveQueueId, tid, account);
                    }
                }).Then(() => { MessageBox.Show(string.Format("复活武将完成")); });
        }

        private void tabControlMainInfo_Selected(object sender, TabControlEventArgs e)
        {
            if (this.tabControlMainInfo.SelectedTab.Name == "tabPageHero")
            {
                this.listViewAccountHero.Items.Clear();

                Parallel.Dispatch(
                    this.accountTable.Values,
                    account =>
                    {
                        var heroList = this.QueryHeroList(account.UserName).ToList();

                        this.Invoke(
                            new DoSomething(
                                () =>
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
                this.TryBuildInfluenceMaps();
            }
        }

        private void comboBoxAccount_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedAccount = this.comboBoxAccount.Text;

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
            var fromCity = this.comboBoxFromCity.Text;
            var accountName = this.comboBoxAccount.Text;
            this.listViewMoveHero.Items.Clear();
            this.listViewAccountArmy.Items.Clear();
            this.listBoxMovePath.Items.Clear();

            Task.Run(
                () =>
                {
                    var accountInfo = this.accountTable[accountName];

                    var cityNodeId = this.cityList[fromCity];
                    var cityId = accountInfo.InfluenceCityList[fromCity].NodeId;

                    var cityPage = this.OpenCityPage(cityNodeId, accountName);
                    var soldiers = this.ParseSoldierInfoFromCityPage(cityPage).ToList();
                    var heroNameList = this.ParseHeroNameListFromCityPage(cityPage).ToList();

                    var cityMovePage = this.ChangeMoveFromCity(accountName, cityId.ToString());
                    var heroIdList = this.ParseHeroIdListFromMovePage(cityMovePage).ToList();
                    var brickNum = this.ParseBrickNumberFromMovePage(cityMovePage);

                    if (!heroIdList.Any())
                    {
                        MessageBox.Show("没有空闲的将领，不能移动部队.");
                        return;
                    }

                    if (heroIdList.Count != heroNameList.Count)
                    {
                        MessageBox.Show("天策数据错误.");
                        return;
                    }

                    this.Invoke(
                        new DoSomething(
                            () =>
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
                                for (int i = 0; i < heroNameList.Count(); ++i)
                                {
                                    var lvItem = new ListViewItem();
                                    lvItem.SubItems[0].Text = heroNameList[i];
                                    lvItem.SubItems.Add(heroIdList[i]);
                                    this.listViewMoveHero.Items.Add(lvItem);
                                }

                                this.numericUpDownBrickNum.Value = brickNum;
                                this.numericUpDownBrickNum.Maximum = brickNum;

                            }));
                });
        }

        private void comboBoxToCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            var fromCity = this.comboBoxFromCity.Text;
            var toCity = this.comboBoxToCity.Text;
            var accountName = this.comboBoxAccount.Text;
            var account = this.accountTable[accountName];

            var helper = new DijstraHelper(account.InfluenceMap);
            helper.DistanceCalculate = this.CalculateDistance;
            helper.account = accountName;

            var path = helper.GetPath(fromCity, toCity).ToList();
            path.Reverse();

            this.listBoxMovePath.Items.Clear();
            foreach (var city in path)
            {
                this.listBoxMovePath.Items.Add(city);
            }
        }

        private void buttonConfirmMove_Click(object sender, EventArgs e)
        {
            string accountName = this.comboBoxAccount.Text;
            string fromCity = this.comboBoxFromCity.Text;
            string toCity = this.comboBoxToCity.Text;
            string nextCity = this.listBoxMovePath.Items[0].ToString();
            bool halfArmy = !this.radioButtonAllArmy.Checked;

            var heroList = (from ListViewItem lvItem in this.listViewMoveHero.CheckedItems select lvItem.SubItems[1].Text).ToList();
            var soldierList = (from ListViewItem lvItem in this.listViewAccountArmy.CheckedItems
                               let soldier = lvItem.Tag as Soldier
                               where soldier.SoldierNumber > 0
                               select new Soldier()
                               {
                                   Name = soldier.Name,
                                   SoldierType = soldier.SoldierType,
                                   SoldierNumber = halfArmy ? soldier.SoldierNumber / 2 : soldier.SoldierNumber,
                               }).ToList();

            var accountInfo = this.accountTable[accountName];
            var moveTask = new MoveTroopTask(accountInfo, fromCity, nextCity, toCity);
            moveTask.BrickNum = (int)this.numericUpDownBrickNum.Value;
            moveTask.SoldierList = soldierList;
            moveTask.HeroIdList = heroList;
            moveTask.Path = (from object lvitem in this.listBoxMovePath.Items select lvitem.ToString()).ToList();
            moveTask.TaskAction = obj =>
                {
                    if (!moveTask.TryEnter())
                    {
                        return;
                    }

                    if (!this.HasTroopArrived(moveTask))
                    {
                        ++moveTask.RetryCount;
                        if (moveTask.RetryCount < 3)
                        {
                            moveTask.ExecuteTime = moveTask.ExecuteTime.AddSeconds(10);
                            moveTask.EndTime = moveTask.ExecuteTime.AddMinutes(1);
                        }
                        else
                        {
                            moveTask.EndTime = this.RemoteTime.AddSeconds(10); // So the task will be deleted next time.
                            moveTask.ExecuteTime = moveTask.EndTime.AddSeconds(1);
                        }
                        moveTask.Leave();
                        return;
                    }

                    if (moveTask.NextCity == moveTask.TerminalCity)
                    {
                        moveTask.EndTime = this.RemoteTime.AddSeconds(10); // So the task will be deleted next time.
                        moveTask.ExecuteTime = moveTask.EndTime.AddSeconds(1);
                        moveTask.Leave();
                        return;
                    }

                    var helper = new DijstraHelper(accountInfo.InfluenceMap);
                    helper.DistanceCalculate = this.CalculateDistance;
                    helper.account = accountName;
                    var path = helper.GetPath(moveTask.NextCity, moveTask.TerminalCity).ToList();
                    path.Reverse();
                    moveTask.Path = path;

                    moveTask.CurrentCity = moveTask.NextCity;
                    moveTask.NextCity = moveTask.Path.First();

                    this.MoveTroop(moveTask);
                    moveTask.Leave();
                };
            moveTask.AccountName = accountInfo.UserName;
            moveTask.CreationTime = this.RemoteTime;
            moveTask.ExecuteTime = this.RemoteTime.AddMinutes(1);
            moveTask.EndTime = moveTask.ExecuteTime.AddMinutes(1);
            moveTask.TaskId = string.Format("Move:{0}", this.randGen.NextDouble());

            var lvItemTask = new ListViewItem();
            lvItemTask.Tag = moveTask;
            moveTask.SyncToListViewItem(lvItemTask, this.RemoteTime);
            this.listViewTasks.Items.Add(lvItemTask);

            Task.Run(() =>
            {
                moveTask.TryEnter();
                this.MoveTroop(moveTask);
                moveTask.Leave();
            });

            this.comboBoxFromCity.Text = "";
            this.comboBoxAccount.Text = "";
            this.listViewMoveHero.Items.Clear();
            this.listViewAccountArmy.Items.Clear();
            this.listBoxMovePath.Items.Clear();
            this.numericUpDownBrickNum.Value = 0;
            this.numericUpDownBrickNum.Maximum = 0;
        }
    }
}