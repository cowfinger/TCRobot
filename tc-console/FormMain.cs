using System.Security.AccessControl;

namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;

    using TC.TCPage.Depot;
    using TC.TCPage.Influence;
    using TC.TCPage.Politics;
    using TC.TCPage.Prop;
    using TC.TCPage.Union;
    using TC.TCPage.WorldWar;
    using TC.TCTasks;
    using TC.TCUtility;

    using DoCheckMember = TC.TCPage.Union.DoCheckMember;

    public partial class FormMain : Form
    {
        private const string UserAgent =
            "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 6.1; Trident/5.0; SLCC2; .NET CLR 2.0.50727; .NET CLR 3.5.30729; .NET CLR 3.0.30729; Media Center PC 6.0; InfoPath.2; .NET4.0C; .NET4.0E)";

        public static Dictionary<string, string> CityList;

        public static Dictionary<int, SoldierAttribute> SoldierTable = new Dictionary<int, SoldierAttribute>();

        public static Dictionary<int, int> RoadLevelToDistanceMap = new Dictionary<int, int>();

        private readonly Dictionary<string, AccountInfo> accountTable = new Dictionary<string, AccountInfo>();

        private readonly Dictionary<string, string> cityList = new Dictionary<string, string>();

        private readonly AutoResetEvent loginLock = new AutoResetEvent(true);

        private readonly Dictionary<string, LoginParam> multiLoginConf = new Dictionary<string, LoginParam>();

        private readonly Random randGen = new Random(DateTime.Now.Millisecond);

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
                return from ListViewItem lvItem in this.listViewTasks.Items select lvItem.Tag as TCTask;
            }
        }

        public static DateTime RemoteTime
        {
            get
            {
                lock (RemoteTimeLock)
                {
                    return remoteTime;
                }
            }

            set
            {
                lock (RemoteTimeLock)
                {
                    remoteTime = value;
                }
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            this.LoadMultiLoginConf();
            this.LoadCityList();
            this.LoadCheckpoint();
            LoadLangChs();
            LoadSoldierInfo();
            LoadRoadInfo();
            Logger.Initialize(new FormMainLogger(this));
        }

        private void btnQuickCreateTroop_Click(object sender, EventArgs e)
        {
            var cityName = this.listBoxSrcCities.SelectedItem as string;
            if (string.IsNullOrEmpty(cityName))
            {
                return;
            }

            string cityId;
            if (!this.cityList.TryGetValue(cityName, out cityId))
            {
                return;
            }

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
                    if (!account.Value.CityIdList.Contains(cityId))
                    {
                        return;
                    }

                    var page = this.OpenCreateTeamPage(cityId, account.Key);
                    var heroList = ParseHerosInCreateTeamPage(page);
                    var soldierList = ParseSoldiersInCreateTeamPage(page).ToList();

                    soldierList.Sort((x, y) => x.SoldierNumber.CompareTo(y.SoldierNumber));
                    soldierList.Reverse();

                    if (this.radioButtonFullTroop.Checked)
                    {
                        var totalSolderNumber = Math.Min(soldierList.Sum(x => x.SoldierNumber), maxSoilderNumber);
                        var soldierString = BuildSoldierString(ref soldierList, totalSolderNumber);

                        var heroRawList = heroList.ToList();
                        var headHero = heroRawList.First();
                        heroRawList.RemoveAt(0);
                        var subHeroes = BuildSubHeroesString(ref heroRawList);

                        this.CreateTeam(
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
                            var soldierString = BuildSoldierString(
                                ref soldierList,
                                this.radioButtonSmallTroop.Checked ? 1000 : 0);
                            this.CreateTeam(
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

                            this.Invoke(new DoSomething(() => { this.RefreshTroopInfoToUi(troopList); }));
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
                    this.Invoke(
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

            var relatedAccountList = this.accountTable.Values.Where(account => account.CityIdList.Contains(cityId));
            Parallel.Dispatch(
                relatedAccountList,
                account =>
                {
                    var singleAttackTeams = this.GetActiveTroopInfo(cityId, "1", account.UserName).ToList();
                    var singleDefendTeams = this.GetActiveTroopInfo(cityId, "2", account.UserName).ToList();
                    var groupAttackteams = this.GetGroupTeamList(cityId, account.UserName);
                    var teamList = singleAttackTeams.Concat(singleDefendTeams).Concat(groupAttackteams);
                    foreach (var troop in teamList)
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
                                                && (task as SendTroopTask).TaskData.isGroupTroop)
                                                .Select(task => (task as SendTroopTask).TaskData.GroupId);
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
                                                && !(task as SendTroopTask).TaskData.isGroupTroop)
                                                .Select(task => (task as SendTroopTask).TaskData.TroopId);
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
                        var account = this.accountTable[troop.AccountName];
                        if (troop.isGroupTroop)
                        {
                            if (troop.IsGroupHead)
                            {
                                DoDisbandGroup.Open(account.WebAgent, troop.GroupId);
                            }
                        }
                        else
                        {
                            DoDisbandTeam.Open(account.WebAgent, troop.TroopId);
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
            this.labelCheckedTroopCount.Text = string.Format("部队计数:{0}", candidateTroopCount);
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
                    var destCityID = ParseTargetCityId(cityPage, dstCityName);

                    if (string.IsNullOrEmpty(destCityID))
                    {
                        var groupAttackPage = this.OpenGroupTeamListPage(srcCityID, team.AccountName);
                        destCityID = ParseTargetCityId(groupAttackPage, dstCityName);
                        if (string.IsNullOrEmpty(destCityID))
                        {
                            return;
                        }
                    }

                    team.ToCityNodeId = destCityID;

                    var attackPage = team.isGroupTroop
                                         ? this.OpenGroupAttackPage(team.GroupId, destCityID, team.AccountName)
                                         : this.OpenTeamAttackPage(team.TroopId, destCityID, team.AccountName);
                    if (attackPage.Contains("您占领出发地不足24小时，不能出征"))
                    {
                        return;
                    }

                    var durationString = ParseAttackDuration(attackPage);
                    team.Duration = TimeStr2Sec(durationString);

                    this.Invoke(new DoSomething(() => { this.TrySyncTroopInfoToUI(team); }));
                }).Then(
                        resultSet =>
                        {
                            if (resultSet != null && resultSet.Sum(r => r ? 0 : 1) > 0)
                            {
                                MessageBox.Show("您占领出发地不足24小时，不能出征");
                                return 0;
                            }

                            this.Invoke(
                                new DoSomething(
                                    () =>
                                    {
                                        this.btnAutoAttack.Enabled = true;
                                        this.btnConfirmMainTroops.Enabled = true;
                                    }));
                            return 0;
                        });
        }

        private void btnConfirmMainTroops_Click(object sender, EventArgs e)
        {
            if (this.listViewTroops.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择需要出发的部队.");
                return;
            }

            var diff = this.dateTimePickerArrival.Value - RemoteTime;
            var selectedTroops =
                (from ListViewItem lvItem in this.listViewTroops.CheckedItems select lvItem.Tag).OfType<TroopInfo>()
                    .ToList();

            var maxDuration = selectedTroops.Max(troop => troop.Duration);

            if (maxDuration > diff.TotalSeconds)
            {
                var minArrivalTime = RemoteTime.AddSeconds(maxDuration);
                var result =
                    MessageBox.Show(
                        string.Format("建议到达时间必须晚于{0}", minArrivalTime.AddSeconds(SendTroopTask.OpenAttackPageTime)),
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
            var candidateTroops =
                (from ListViewItem item in this.listViewTroops.CheckedItems select item.Tag as TroopInfo).ToList();

            var dialog = new FormChooseTroopHead(candidateTroops);
            dialog.ShowDialog();
            if (dialog.GroupHead == null)
            {
                return;
            }

            var headTroop = dialog.GroupHead;
            candidateTroops.Remove(headTroop);

            var cityId = this.cityList[this.listBoxSrcCities.SelectedItem.ToString()];
            var cityIdValue = int.Parse(cityId);

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

                    var headAccount = this.accountTable[headTroop.AccountName];

                    if (!headTroop.isGroupTroop)
                    {
                        var groupName = this.randGen.Next(1000000, 9999999).ToString();
                        TCPage.Influence.ShowInfluenceCityDetail.Open(
                            headAccount.WebAgent, cityIdValue);
                        TCPage.WorldWar.DoCreateGroup.Open(
                            headAccount.WebAgent, headTroop.TroopId, groupName);
                        var groupTroops = this.GetGroupTeamList(cityId, headTroop.AccountName);
                        headTroop = groupTroops.FirstOrDefault(troop => troop.Name == groupName);
                        if (headTroop == null || headTroop.Name != groupName)
                        {
                            return;
                        }
                    }

                    foreach (var troop in troopGroup)
                    {
                        var account = this.accountTable[troop.AccountName];
                        DoJoinGroup.Open(account.WebAgent, troop.TroopId, troop.GroupId);
                    }

                    var troopList = this.QueryCityTroops(cityId).ToList();

                    this.Invoke(new DoSomething(() => { this.RefreshTroopInfoToUi(troopList); }));
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

                        accountinfo.LoginStatus = "off-line";
                    }

                    line = sr.ReadLine();
                }

                foreach (var account in this.accountTable.Values)
                {
                    TryLoadAccountCookie(account);
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
            // Task.Run(this.BatchLoginProc);
            // var accounts = this.accountTable.Keys.ToList();
            var accounts = (from ListViewItem item in this.listViewAccounts.Items
                            select item.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                while (accounts.Any())
                {
                    var toAuthAccounts = accounts.Take(10).ToList();
                    Parallel.ForEach(toAuthAccounts, this.LoginAccount);
                    accounts.RemoveRange(0, toAuthAccounts.Count());
                }
            });
        }

        private void ToolStripMenuItemScan_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    var validCityNameList = this.accountTable.Values.SelectMany(
                        account =>
                        {
                            var cityArmyPage = TCPage.Influence.ShowInfluenceCityArmy.Open(account.WebAgent);
                            account.CityNameList = cityArmyPage.Cities.ToList();
                            account.CityIdList = account.CityNameList.Select(cityName => this.cityList[cityName]).ToList();
                            return account.CityNameList;
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
                }).Then(() => { this.Invoke(new DoSomething(() => { this.tabControlTask.Enabled = true; })); });
        }

        private void ToolStripMenuItemDonation_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    var accountList = this.accountTable.Keys.ToList();

                    for (var i = 0; i < 100 && accountList.Any(); ++i)
                    {
                        var toRemoveAccounts = this.BatchDonate(accountList);
                        foreach (var account in toRemoveAccounts)
                        {
                            accountList.Remove(account);
                        }
                    }
                });
        }

        private void ToolStripMenuItemReliveHero_Click(object sender, EventArgs e)
        {
            this.StartReliveHeroTimer();
        }

        private void ToolStripMenuItemQuickReliveHero_Click(object sender, EventArgs e)
        {
            var accountList = from ListViewItem item in this.listViewAccounts.CheckedItems
                              let account = item.Tag as AccountInfo
                              select account;
            Parallel.Dispatch(
                accountList,
                account =>
                {
                    var heroPage = TCPage.Hero.ShowMyHeroes.Open(account.WebAgent);
                    var heroList = heroPage.Heroes.ToList();

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
                            if (heroList.Sum(hero => hero.Status == 8 ? 1 : 0) == 0) // relive running now.
                            {
                                TCPage.Hero.DoReliveHero.Open(account.WebAgent, toReliveHero.HeroId);
                            }
                        }
                        else
                        {
                            TCPage.Hero.DoReliveHero.Open(account.WebAgent, toReliveHero.HeroId);
                        }

                        Thread.Sleep(1000);

                        var reliveQueueId = this.QueryReliveQueueId(account.Tid, account);
                        var allowPropPage = ShowAllowProp.Open(account.WebAgent, account.Tid, reliveQueueId);
                        var reliveItem = allowPropPage.Item;
                        if (reliveItem == null)
                        {
                            MessageBox.Show(string.Format("复活药用完了."));
                            return;
                        }

                        var page = DoUseProp.OpenOnHero(
                            account.WebAgent,
                            reliveItem.PropertyId,
                            reliveItem.UserPropertyId,
                            toReliveHero.HeroId,
                            reliveQueueId,
                            account.Tid);
                        Logger.Verbose("{1} : QuickReliveHero:{0} : {2}",
                            toReliveHero.HeroId, account.UserName, page.RawPage);
                    }
                }).Then(() => { MessageBox.Show(string.Format("复活武将完成")); });
        }

        private void tabControlMainInfo_Selected(object sender, TabControlEventArgs e)
        {
            if (this.tabControlMainInfo.SelectedTab.Name != "tabPageHero")
            {
                return;
            }

            this.listViewAccountHero.Items.Clear();
            Parallel.Dispatch(
                this.accountTable.Values,
                account =>
                {
                    var heroList = TCPage.Hero.ShowMyHeroes.Open(account.WebAgent).Heroes.ToList();

                    this.Invoke(
                        new DoSomething(
                            () =>
                            {
                                foreach (var hero in heroList)
                                {
                                    var lvItem = new ListViewItem { Tag = hero };
                                    lvItem.SubItems[0].Text = account.UserName;
                                    lvItem.SubItems.Add(hero.Name);
                                    lvItem.SubItems.Add(hero.IsDead.ToString());
                                    this.listViewAccountHero.Items.Add(lvItem);
                                }
                            }));
                });
        }

        private void tabControlTask_Selected(object sender, TabControlEventArgs e)
        {
            switch (this.tabControlTask.SelectedTab.Name)
            {
                case "tabPageMoveArmy":
                    this.TryBuildInfluenceMaps();
                    break;
                case "tabPageAccountTask":
                    this.TryBuildInfluenceMaps();
                    break;
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

            var cityArmyPage = TCPage.Influence.ShowInfluenceCityArmy.Open(account.WebAgent);
            account.CityNameList = cityArmyPage.Cities.ToList();
            account.CityIdList = account.CityNameList.Select(cityName => this.cityList[cityName]).ToList();

            this.comboBoxFromCity.Items.Clear();
            foreach (var cityName in account.CityNameList)
            {
                this.comboBoxFromCity.Items.Add(cityName);
            }
            if (account.MainCity != null)
            {
                this.comboBoxFromCity.Items.Add(account.MainCity.Name);
                this.comboBoxFromCity.Text = "";
            }

            this.comboBoxToCity.Items.Clear();
            if (account.InfluenceMap != null)
            {
                foreach (var cityName in account.InfluenceMap.Keys)
                {
                    this.comboBoxToCity.Items.Add(cityName);
                }
            }
            this.comboBoxToCity.Text = "";
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

                    var cityId = accountInfo.InfluenceCityList[fromCity].NodeId;

                    var moveArmyPage = ShowMoveArmy.Open(accountInfo.WebAgent, cityId);
                    var heroList = moveArmyPage.HeroList.Where(hero => !hero.IsBusy).ToList();
                    var soldiers = moveArmyPage.Army.ToList();
                    var brickNum = moveArmyPage.BrickNum;

                    this.Invoke(
                        new DoSomething(
                            () =>
                            {
                                this.listViewAccountArmy.Items.Clear();
                                foreach (var soldierInfo in soldiers)
                                {
                                    var lvItem = new ListViewItem { Tag = soldierInfo };
                                    lvItem.SubItems[0].Text = soldierInfo.Name;
                                    lvItem.SubItems.Add(soldierInfo.SoldierNumber.ToString());
                                    lvItem.SubItems.Add("0");
                                    this.listViewAccountArmy.Items.Add(lvItem);
                                }

                                this.listViewMoveHero.Items.Clear();
                                for (var i = 0; i < heroList.Count(); ++i)
                                {
                                    var lvItem = new ListViewItem();
                                    lvItem.SubItems[0].Text = heroList[i].Name;
                                    lvItem.SubItems.Add(heroList[i].HeroId.ToString());
                                    this.listViewMoveHero.Items.Add(lvItem);
                                }

                                this.numericUpDownBrickNum.Maximum = brickNum;
                                this.numericUpDownBrickNum.Value = 0;
                            }));
                });
        }

        private void comboBoxToCity_SelectedIndexChanged(object sender, EventArgs e)
        {
            var fromCity = this.comboBoxFromCity.Text;
            var toCity = this.comboBoxToCity.Text;
            var accountName = this.comboBoxAccount.Text;
            var account = this.accountTable[accountName];

            var helper = new DijstraHelper(account.InfluenceMap) { Account = account };

            var path = helper.GetPath(fromCity, toCity, null).ToList();
            path.Reverse();

            this.listBoxMovePath.Items.Clear();
            foreach (var city in path)
            {
                this.listBoxMovePath.Items.Add(city.Name);
            }
        }

        private void buttonConfirmMove_Click(object sender, EventArgs e)
        {
            var accountName = this.comboBoxAccount.Text;
            var fromCityName = this.comboBoxFromCity.Text;
            var toCityName = this.comboBoxToCity.Text;

            var heroList =
                (from ListViewItem lvItem in this.listViewMoveHero.CheckedItems select lvItem.SubItems[1].Text).ToList();

            var soldierList = (from ListViewItem lvItem in this.listViewAccountArmy.CheckedItems
                               let soldier = lvItem.Tag as Soldier
                               let soldierNumber = int.Parse(lvItem.SubItems[2].Text)
                               where soldierNumber > 0
                               select
                                   new Soldier
                                       {
                                           Name = soldier.Name,
                                           SoldierType = soldier.SoldierType,
                                           SoldierNumber = soldierNumber
                                       }).ToList();

            if (heroList.Count + soldierList.Count == 0)
            {
                MessageBox.Show("必须选择部队或将领");
                return;
            }

            var accountInfo = this.accountTable[accountName];
            var fromCity = accountInfo.InfluenceCityList[fromCityName];
            var toCity = accountInfo.InfluenceCityList[toCityName];

            this.CreateMoveTroopTask(
                accountInfo,
                fromCity,
                toCity,
                soldierList,
                heroList,
                (int)this.numericUpDownBrickNum.Value);

            this.comboBoxFromCity.Text = "";
            this.comboBoxAccount.Text = "";
            this.listViewMoveHero.Items.Clear();
            this.listViewAccountArmy.Items.Clear();
            this.listBoxMovePath.Items.Clear();
            this.numericUpDownBrickNum.Value = 0;
            this.numericUpDownBrickNum.Maximum = 0;
        }

        private void listViewAccountArmy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listViewAccountArmy.SelectedItems.Count == 1)
            {
                var lvItem = this.listViewAccountArmy.SelectedItems[0];
                this.labelSelectedArmyName.Text = lvItem.Text;
                this.labelSelectedArmyName.Visible = true;
                this.numericUpDownToMoveArmy.Maximum = int.Parse(lvItem.SubItems[1].Text);
                this.numericUpDownToMoveArmy.Value = int.Parse(lvItem.SubItems[2].Text);
                this.numericUpDownToMoveArmy.Visible = true;
            }
            else
            {
                this.labelSelectedArmyName.Visible = false;
                this.numericUpDownToMoveArmy.Visible = false;
            }
        }

        private void numericUpDownToMoveArmy_ValueChanged(object sender, EventArgs e)
        {
            if (this.listViewAccountArmy.SelectedItems.Count == 1)
            {
                var lvItem = this.listViewAccountArmy.SelectedItems[0];
                lvItem.SubItems[2].Text = this.numericUpDownToMoveArmy.Value.ToString();
            }
        }

        private void checkBoxDirectTarget_CheckedChanged(object sender, EventArgs e)
        {
            this.comboBoxToCity.Text = "";
            this.comboBoxToCity.Items.Clear();
            var accountName = this.comboBoxAccount.Text;
            AccountInfo account;
            if (!this.accountTable.TryGetValue(accountName, out account))
            {
                return;
            }

            if (this.checkBoxDirectTarget.Checked)
            {
                foreach (var i in account.InfluenceMap[this.comboBoxFromCity.Text])
                {
                    this.comboBoxToCity.Items.Add(i);
                }
            }
            else
            {
                foreach (var i in account.InfluenceCityList.Keys)
                {
                    this.comboBoxToCity.Items.Add(i);
                }
            }
        }

        private void checkBoxSelecteAllMoveHeroes_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in this.listViewMoveHero.Items)
            {
                lvItem.Checked = this.checkBoxSelecteAllMoveHeroes.Checked;
            }
        }

        private void comboBoxAccountTaskType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewTaskIdleAccount.CheckedItems
                               let accountInfo = lvItem.Tag as AccountInfo
                               select accountInfo).ToList();

            this.comboBoxAccountTaskTarget.Text = "";
            this.comboBoxAccountTaskTarget.Items.Clear();

            if (string.IsNullOrEmpty(this.comboBoxAccountTaskType.Text))
            {
                this.comboBoxAccountTaskTarget.Enabled = false;
            }
            var targetList = accountList.SelectMany(info => info.InfluenceCityList.Keys).ToList().Distinct();
            this.comboBoxAccountTaskTarget.Items.AddRange(targetList.Select(val => val as object).ToArray());
            this.comboBoxAccountTaskTarget.Enabled = true;

            switch (this.comboBoxAccountTaskType.Text)
            {
                case "运砖":
                    this.checkBoxAccountTaskOption.Text = "使用将领";
                    this.checkBoxAccountTaskOption.Visible = false;
                    break;
                case "调兵":
                    this.checkBoxAccountTaskOption.Text = "带砖";
                    this.checkBoxAccountTaskOption.Visible = true;
                    break;
                case "拒绝联盟":
                    this.buttonAssignTask.Enabled = true;
                    break;
                case "间谍":
                    this.buttonAssignTask.Enabled = true;
                    break;
                case "小号练级":
                    this.buttonAssignTask.Enabled = true;
                    break;
            }
        }

        private void buttonAssignTask_Click(object sender, EventArgs e)
        {
            var targetCityName = this.comboBoxAccountTaskTarget.Text;

            var accountList = (from ListViewItem lvItem in this.listViewTaskIdleAccount.CheckedItems
                               let accountInfo = lvItem.Tag as AccountInfo
                               select accountInfo).ToList();
            if (!accountList.Any())
            {
                return;
            }

            foreach (var account in accountList)
            {
                CityInfo targetCity;
                switch (this.comboBoxAccountTaskType.Text)
                {
                    case "运砖":
                        if (!account.InfluenceCityList.TryGetValue(targetCityName, out targetCity))
                        {
                            continue;
                        }
                        this.CreateShipBrickTask(account, targetCity);
                        break;
                    case "调兵":
                        if (!account.InfluenceCityList.TryGetValue(targetCityName, out targetCity))
                        {
                            continue;
                        }
                        this.CreateShipTroopTasks(account, targetCity, this.checkBoxAccountTaskOption.Checked);
                        break;
                    case "拒绝联盟":
                        this.CreateInfluenceGuardTask(account);
                        break;
                    case "间谍":
                        this.CreateSpyTask(account);
                        break;
                    case "小号练级":
                        this.CreateBuildDogTask(account);
                        break;
                }
            }
        }

        private void listViewTaskIdleAccount_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.comboBoxAccountTaskType.Enabled = this.listViewTaskIdleAccount.CheckedItems.Count > 0;
        }

        private void listViewAccountActiveTask_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            this.buttonCancelAccountTask.Enabled = this.listViewAccountActiveTask.CheckedItems.Count > 0;
        }

        private void comboBoxAccountTaskTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.buttonAssignTask.Enabled = !string.IsNullOrEmpty(this.comboBoxAccountTaskTarget.Text);
        }

        private void checkBoxSelectAllActiveTask_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvitem in this.listViewTasks.Items)
            {
                lvitem.Checked = this.checkBoxSelectAllActiveTask.Checked;
            }
        }

        private void buttonCancelAccountTask_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in this.listViewAccountActiveTask.CheckedItems)
            {
            }
        }

        private void checkBoxSelectAllTaskAccount_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in this.listViewTaskIdleAccount.Items)
            {
                lvItem.Checked = this.checkBoxSelectAllTaskAccount.Checked;
            }
        }

        private void QuitUnionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in this.listViewAccounts.CheckedItems)
            {
                var account = lvItem.Tag as AccountInfo;
                Task.Run(() => { DoOutUnion.Open(account.WebAgent); });
            }
        }

        private void checkBoxSelectAllInAccountTable_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem lvItem in this.listViewAccounts.Items)
            {
                lvItem.Checked = this.checkBoxSelectAllInAccountTable.Checked;
            }
        }

        private void joinUnionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var form = new FormJoinUnion();
            form.ShowDialog();
            if (!form.IsOk)
            {
                return;
            }
            var unionId = form.UnionId;
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select new { Account = lvItem.Tag as AccountInfo, lvItem }).ToList();

            Task.Run(() =>
                {
                    foreach (var account in accountList)
                    {
                        DoApplyUnion.Open(account.Account.WebAgent, unionId);
                        this.Invoke(new DoSomething(() => { account.lvItem.SubItems[2].Text = unionId.ToString(); }));
                    }
                });
        }

        private void repairCityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var allCityList =
                this.accountTable.Values.SelectMany(account => account.InfluenceCityList.Keys).Distinct().ToList();
            var dlg = new FormSelectCity { CityList = allCityList };
            dlg.ShowDialog();

            if (!dlg.IsOk)
            {
                return;
            }

            var list = dlg.CheckedCityList;
            var accountList = this.accountTable.Values.ToList();

            var cityAccountTable = from city in list
                                   from account in accountList
                                   where account.InfluenceCityList.ContainsKey(city)
                                   select new { city, account };
            var cityAccountGroups = cityAccountTable.GroupBy(item => item.city).ToList();
            if (!cityAccountGroups.Any())
            {
                return;
            }

            this.repairCityToolStripMenuItem.Enabled = false;
            Task.Run(() =>
            {
                foreach (var group in cityAccountGroups)
                {
                    string cityIdStr;
                    if (!this.cityList.TryGetValue(group.Key, out cityIdStr))
                    {
                        return;
                    }

                    var cityId = int.Parse(cityIdStr);
                    foreach (var account in group)
                    {
                        if (!RepairCityFortress(cityId, account.account))
                        {
                            return;
                        }
                        Thread.Sleep(1000);
                    }

                }
            }).Then(() =>
            {
                MessageBox.Show("所有城市修理完毕.");
                this.Invoke(new DoSomething(() => { this.repairCityToolStripMenuItem.Enabled = true; }));
            });
        }

        private void enlistTroopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(
                () =>
                {
                    foreach (var account in accountList)
                    {
                        var infoPage = ShowDraft.Open(account.WebAgent);
                        if (infoPage.LeftTimes <= 0)
                        {
                            this.DebugLog("Cannot Enlist since no Left times.");
                            continue;
                        }

                        var soldierId = 0;
                        var maxSpeed = 0;
                        foreach (var soldier in SoldierTable.Values)
                        {
                            if (!infoPage.SoldierIdSet.Contains(soldier.SoldierId))
                            {
                                continue;
                            }

                            if (soldier.Speed > maxSpeed && soldier.Capacity > 0)
                            {
                                maxSpeed = soldier.Speed;
                                soldierId = soldier.SoldierId;
                            }
                        }

                        if (soldierId == 0)
                        {
                            break;
                        }

                        for (var i = 0; i < infoPage.LeftTimes; i++)
                        {
                            var doPage = DoDraft.Open(account.WebAgent, infoPage.EfficientHeroId, soldierId, 2);
                            if (!doPage.Success)
                            {
                                break;
                            }
                            this.DebugLog("Enlist {0}: {1}", account.UserName, KeyWordMap["soldier_" + soldierId]);
                        }
                    }
                });
        }

        private void flushToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string Url =
                "http://yw1.tc.9wee.com/index.php?mod=military/attack&op=show&func=military_event_list&type=1&r=0.6564584287467245";
            Parallel.Dispatch(
                this.accountTable.Values,
                account =>
                {
                    var loopTable = "".PadRight(1000, '1');

                    try
                    {
                        var webClient = new HttpClient(account.WebAgent.WebClient.Cookies);
                        Parallel.Dispatch(
                            loopTable,
                            ch =>
                            {
                                try
                                {
                                    webClient.OpenUrl(Url);
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }
                            });
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                });
        }

        private void listViewAccountArmy_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked)
            {
                e.Item.SubItems[2].Text = e.Item.SubItems[1].Text;
            }
        }

        private void exportLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Task.Run(
                () =>
                {
                    using (var stream = new StreamWriter("debug_log.txt"))
                    {
                        var logList = new List<string>();
                        this.Invoke(
                            new DoSomething(
                                () =>
                                {
                                    logList.AddRange(
                                        from ListViewItem logItem in this.listViewDebugLog.Items
                                        select logItem.SubItems[0].Text + ":" + logItem.SubItems[1].Text);
                                }));

                        foreach (var logLine in logList)
                        {
                            stream.WriteLine(logLine);
                        }
                        stream.Flush();
                    }
                });
        }

        private void getWarPointToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                foreach (var account in accountList)
                {
                    var page = TCPage.Resource.DoGetWarPoint.Open(account.WebAgent);
                    Logger.Verbose("GetWarPoint[{0}]:Total:{1}, Get{2}",
                        account.UserName, page.TotalWarPoint, page.GetResultPoint);
                }
            });
        }

        private void developArmyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                foreach (var account in accountList)
                {
                    // var soldierId = CalcEffectiveSoldierId(account);
                    // if (soldierId == 0)
                    // {
                    //     continue;
                    // }
                    var soldierId = 204;
                    var page = TCPage.Train.DoDevelop.Open(account.WebAgent, soldierId);
                    Logger.Verbose("{0} Train:{1}", account.UserName, page.RawPage);
                }
            });
        }

        private static int CalcEffectiveSoldierId(AccountInfo account)
        {
            int soldierId = 0;
            var infoPage = ShowDraft.Open(account.WebAgent);
            var maxSpeed = 0;
            foreach (var soldier in SoldierTable.Values)
            {
                if (!infoPage.SoldierIdSet.Contains(soldier.SoldierId))
                {
                    continue;
                }

                if (soldier.Speed > maxSpeed && soldier.Capacity > 0)
                {
                    maxSpeed = soldier.Speed;
                    soldierId = soldier.SoldierId;
                }
            }
            return soldierId;
        }

        private void hackGetSoldierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var leadAccount = this.accountTable["clairchen001"];
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                while (accountList.Any())
                {
                    var toAuthAccounts = accountList.Take(10).ToList();
                    Parallel.ForEach(toAuthAccounts, account =>
                    {
                        Logger.Verbose("Hack Buy Soldier:{0}", account.UserName);

                        var soldierId = CalcEffectiveSoldierId(account);
                        if (soldierId == 0)
                        {
                            Logger.Verbose("Canceled Since No Valid Soldier");
                            // continue;
                            return;
                        }

                        // DoApplyUnion.Open(account.WebAgent, 22757);

                        // var memberPage = ShowUnionMember.Open(leadAccount.WebAgent, 22757);
                        // var requestUsers = memberPage.RequestUsers.ToList();

                        // foreach (var usrId in requestUsers)
                        // {
                        //     var checkMemberPage = DoCheckMember.Open(
                        //         leadAccount.WebAgent, usrId, DoCheckMember.Result.pass);
                        //     if (!checkMemberPage.Success)
                        //     {
                        //         Logger.Verbose("Check Member{0} Failed:{1}", usrId, checkMemberPage.RawPage);
                        //     }
                        // }

                        for (var i = 0; i < 1000; i++)
                        {
                            TCPage.Build.DoBrick.Open(account.WebAgent, -100);
                            while (true)
                            {
                                var doPage = DoBuySoldierFromUnion.Open(account.WebAgent, soldierId, 30, 1);
                                if (doPage.Success)
                                {
                                    continue;
                                }

                                if (doPage.RawPage.Contains("当前资源不够"))
                                {
                                    break;
                                }
                                else
                                {
                                    Logger.Verbose("Buy failed:{0}", doPage.RawPage);
                                    i = 1000;
                                    break;
                                }
                            }
                        }

                        // DoOutUnion.Open(account.WebAgent);
                    });
                    accountList.RemoveRange(0, toAuthAccounts.Count());
                }
            });
        }

        private void contributeUnionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                foreach (var account in accountList)
                {
                    for (var i = 0; i < 10; ++i)
                    {
                        var dataPage = TCPage.DoGetData.Open(account.WebAgent, account.Tid, account.Tid);
                        DoDonateRes.Open(account.WebAgent, dataPage.ResourceTabe);
                        Logger.Verbose("Donate:{0}", string.Join("|", dataPage.ResourceTabe));

                        var resBoxes = ShowMyDepot.EnumDepotItems(account.WebAgent)
                                .Where(prop => prop.PropertyId == (int)DepotItem.PropId.ResourceBox)
                                .ToList();
                        if (!resBoxes.Any())
                        {
                            return;
                        }
                        var firstBox = resBoxes.First();
                        DoUseProp.Open(account.WebAgent, firstBox.PropertyId, firstBox.UserPropertyId, 5);
                    }
                }
            });
        }

        private void buildBrickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var leadAccount = this.accountTable["clairchen001"];
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                foreach (var account in accountList)
                {
                    Logger.Verbose("Start Build Brick:{0}", account.UserName);
                    if (DoOutUnion.Open(account.WebAgent).RawPage.Contains("才能退出"))
                    {
                        Logger.Verbose("Cannot out union");
                        continue;
                    }

                    DoApplyUnion.Open(account.WebAgent, 22757);
                    AcceptMemberRequest(leadAccount);

                    TCPage.Build.DoBrick.Open(account.WebAgent, -1000);
                    DoOutUnion.Open(account.WebAgent);

                    DoApplyUnion.Open(account.WebAgent, 22757);
                    AcceptMemberRequest(leadAccount);
                    TCPage.Build.DoBrick.Open(account.WebAgent, 12);
                }
            });
        }

        private static void AcceptMemberRequest(AccountInfo leadAccount)
        {
            var memberPage = ShowUnionMember.Open(leadAccount.WebAgent, 22757);
            var requestUsers = memberPage.RequestUsers.ToList();

            foreach (var usrId in requestUsers)
            {
                var checkMemberPage = DoCheckMember.Open(
                    leadAccount.WebAgent, usrId, DoCheckMember.Result.pass);
                if (!checkMemberPage.Success)
                {
                    Logger.Verbose("Check Member{0} Failed:{1}", usrId, checkMemberPage.RawPage);
                }
            }
        }

        private void trainAccountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                foreach (var account in accountList)
                {
                    this.CreateBuildDogTask(account);
                    Thread.Sleep(1 * 1000);
                }
            });
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new FormDebug { Account = this.accountTable.Values.First() };
            dlg.ShowDialog();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.taskTimer != null)
            {
                this.taskTimer.Stop();
            }
        }

        private void safeBuildBrickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                foreach (var account in accountList)
                {
                    Logger.Verbose("Start Safe Build Brick:{0}", account.UserName);

                    var dataPage = TCPage.DoGetData.Open(account.WebAgent, account.Tid, account.Tid);
                    var minRes = dataPage.ResourceTabe.Take(4).Min();
                    Logger.Verbose("Res:{0}=>{1}", string.Join("|", dataPage.ResourceTabe.Take(4)), minRes);
                    var brickCount = minRes / 7500;
                    if (brickCount <= 0)
                    {
                        continue;
                    }

                    Logger.Verbose("Safe Build Brick:{0}", brickCount);
                    TCPage.Build.DoBrick.Open(account.WebAgent, brickCount);
                }
            });
        }

        private void hackDonateInfluenceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                while (true)
                {
                    foreach (var account in accountList)
                    {
                        var resPage = TCPage.DoGetData.Open(account.WebAgent, account.Tid, account.Tid);
                        var resToDonate = resPage.MaxResourceTable[0];
                        for (var i = 0; i < 4; i++)
                        {
                            HackDonateOneRes(account, i, resToDonate);
                        }
                    }
                    Thread.Sleep(10 * 60 * 1000);
                }
            });
        }

        private static void HackDonateOneRes(AccountInfo account, int resType, int resToDonate)
        {
            while (true)
            {
                Logger.Verbose("Start HackDonateInfluence:{0}:{1}-{2}", account.UserName, resType, resToDonate);
                TCPage.Build.DoBrick.Open(account.WebAgent, -100);
                var toDonateList = new List<long>() { 0, 0, 0, 0, 0, 0 };
                toDonateList[resType] = resToDonate;
                var donatePage = DoInfluenceDonate.Open(account.WebAgent, toDonateList);
                Logger.Verbose("HackDonateInfluence:{0}", donatePage.RawPage);
                if (!donatePage.Success && !donatePage.RawPage.Contains("资源不"))
                {
                    break;
                }
            }
        }

        private void returnHomeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var accountList = (from ListViewItem lvItem in this.listViewAccounts.CheckedItems
                               select lvItem.Tag as AccountInfo).ToList();
            Task.Run(() =>
            {
                foreach (var account in accountList)
                {
                    Logger.Verbose("Start Return Home:{0}", account.UserName);
                    foreach (var city in account.CityNameList)
                    {
                        var fromCity = account.InfluenceCityList[city];
                        var moveArmyPage = ShowMoveArmy.Open(account.WebAgent, fromCity.NodeId);
                        this.CreateMoveTroopTask(
                            account,
                            fromCity,
                            account.MainCity,
                            moveArmyPage.Army.ToList(),
                            moveArmyPage.HeroList.Select(h => h.HeroId.ToString()).ToList(),
                            0);
                    }
                }
            });
        }
    }
}
