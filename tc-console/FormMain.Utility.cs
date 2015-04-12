using System.Security.Cryptography.X509Certificates;

namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Windows.Forms;

    using TC.TCPage.Influence;
    using TC.TCPage.WorldWar;
    using TC.TCTasks;
    using TC.TCUtility;

    using Timer = System.Timers.Timer;

    partial class FormMain
    {
        public static Dictionary<string, string> KeyWordMap = new Dictionary<string, string>();

        private static string HTTPRequest(string url)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = UserAgent;

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var content = string.Empty;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }

                    response.Close();
                    return content;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        private string HTTPRequest(string url, string account, string body = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = UserAgent;
                //request.Headers.Add("Cookie", this.GetAccountCookie(account));
                request.CookieContainer = this.GetAccountCookies(account);

                if (!string.IsNullOrEmpty(body))
                {
                    var codedBytes = new ASCIIEncoding().GetBytes(body);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = codedBytes.Length;
                    request.GetRequestStream().Write(codedBytes, 0, codedBytes.Length);
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    this.SetAccountCookie(account, request.CookieContainer);

                    var content = string.Empty;
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        content = reader.ReadToEnd();
                    }

                    response.Close();
                    return content;
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static string Time2Str(int timeval)
        {
            var secs = timeval % 60;
            var mins = (timeval / 60) % 60;
            var hours = timeval / 3600;
            var fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        private static string CalcGroupType(TroopInfo team)
        {
            if (team.isGroupTroop)
            {
                return "群组";
            }

            if (team.isDefendTroop)
            {
                return "防御";
            }

            return "攻击";
        }

        private void TrySyncTroopInfoToUI(TroopInfo team)
        {
            ListViewItem lvItemTroop = null;
            foreach (ListViewItem tempLvItem in this.listViewTroops.Items)
            {
                var troop = tempLvItem.Tag as TroopInfo;
                if (troop != null && troop == team)
                {
                    lvItemTroop = tempLvItem;
                    break;
                }
            }

            if (lvItemTroop == null)
            {
                lvItemTroop = new ListViewItem { Tag = team };
                lvItemTroop.SubItems[0].Text = team.AccountName;
                lvItemTroop.SubItems.Add(team.TroopId.ToString());
                lvItemTroop.SubItems.Add(team.PowerIndex.ToString());
                lvItemTroop.SubItems.Add(Time2Str(team.Duration));
                lvItemTroop.SubItems.Add(Time2Str(0));
                lvItemTroop.SubItems.Add(team.GroupId.ToString());
                lvItemTroop.SubItems.Add(CalcGroupType(team));

                this.listViewTroops.Items.Add(lvItemTroop);
            }
            else
            {
                lvItemTroop.SubItems[3].Text = Time2Str(team.Duration);
                lvItemTroop.SubItems[4].Text = Time2Str(0);
            }
        }

        private void RefreshTroopInfoToUi(IEnumerable<TroopInfo> troopList)
        {
            this.listViewTroops.Items.Clear();
            foreach (var team in troopList)
            {
                var newli = new ListViewItem();
                newli.SubItems[0].Text = team.AccountName;
                newli.SubItems.Add(team.TroopId.ToString());
                newli.SubItems.Add(team.PowerIndex.ToString());
                newli.SubItems.Add(Time2Str(team.Duration));
                newli.SubItems.Add(Time2Str(0));
                newli.SubItems.Add(team.GroupId.ToString());
                newli.SubItems.Add(CalcGroupType(team));
                newli.Tag = team;
                this.listViewTroops.Items.Add(newli);
            }
        }

        private static string ConvertStatusStr(string status)
        {
            switch (status)
            {
                case "on-line":
                    return "已登录";
                case "in-login":
                    return "登录中";
                case "login-failed":
                    return "登录失败";
                case "submitting":
                    return "提交中";
                case "sync-time":
                    return "同步系统时间中";
            }
            return "未登录";
        }

        private void SyncAccountsStatus()
        {
            this.listViewAccounts.Items.Clear();
            var loginnum = 0;
            foreach (var accountkey in this.accountTable.Keys)
            {
                var account = this.accountTable[accountkey];

                var newli = new ListViewItem();
                {
                    newli.SubItems[0].Text = account.UserName;
                    newli.SubItems.Add(ConvertStatusStr(account.LoginStatus));
                    newli.SubItems.Add("");
                    newli.SubItems.Add("");
                    newli.SubItems.Add("");
                }
                newli.Tag = account;
                this.listViewAccounts.Items.Add(newli);

                if (account.LoginStatus == "on-line")
                {
                    loginnum++;
                    this.hostname = account.AccountType;
                }
            }

            if (loginnum >= this.accountTable.Keys.Count)
            {
                this.ToolStripMenuItemFunctions.Enabled = true;
                this.btnQuickCreateTroop.Enabled = true;
            }
        }

        private IEnumerable<TroopInfo> QueryCityTroops(string cityId)
        {
            return this.accountTable.Values.Where(account => account.CityIdList.Contains(cityId)).Select(
                account =>
                {
                    var singleAttackTeams = this.GetActiveTroopInfo(cityId, "1", account.UserName);
                    var singleDefendTeams = this.GetActiveTroopInfo(cityId, "2", account.UserName);
                    var groupAttackteams = this.GetGroupTeamList(cityId, account.UserName);
                    return singleAttackTeams.Concat(singleDefendTeams).Concat(groupAttackteams);
                }).SelectMany(teams => teams);
        }

        private IEnumerable<string> QueryTargetCityList(string cityId)
        {
            var relatedAccountList = this.accountTable.Values.Where(account => account.CityIdList.Contains(cityId));
            foreach (var account in relatedAccountList)
            {
                var attackCityList = this.OpenAttackPage(cityId, account.UserName);
                var greoupAttackCityList = this.GetGroupAttackTargetCity(cityId, account.UserName);
                // var moveCityList = GetMoveTargetCities(cityId, account.UserName);
                var targetCityList = attackCityList.Concat(greoupAttackCityList).ToList();
                if (targetCityList.Any())
                {
                    return targetCityList;
                }
            }
            return new List<string>();
        }

        private void LoadCheckpoint()
        {
        }

        private void LoadCityList()
        {
            using (var sr = new StreamReader("cities.txt", Encoding.UTF8))
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    var strs = line.Split('|', ':');
                    if (strs.Length > 1)
                    {
                        this.cityList.Add(strs[1], strs[0]);
                    }

                    line = sr.ReadLine();
                }
            }

            CityList = this.cityList;
        }

        private static void LoadRoadInfo()
        {
            using (var streamReader = new StreamReader("road.txt", Encoding.ASCII))
            {
                var line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var strs = line.Split(',', ':');
                    if (strs.Length == 2)
                    {
                        RoadLevelToDistanceMap.Add(int.Parse(strs[0]), int.Parse(strs[1]));
                    }

                    line = streamReader.ReadLine();
                }
            }
        }

        private static void LoadSoldierInfo()
        {
            using (var streamReader = new StreamReader("soldier.txt", Encoding.ASCII))
            {
                var line = streamReader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    var strs = line.Split(',', ':');
                    if (strs.Length == 3)
                    {
                        var solderAttribute = new SoldierAttribute
                                                  {
                                                      SoldierId = int.Parse(strs[0]),
                                                      Speed = int.Parse(strs[1]),
                                                      Capacity = int.Parse(strs[2])
                                                  };

                        SoldierTable.Add(solderAttribute.SoldierId, solderAttribute);
                    }

                    line = streamReader.ReadLine();
                }
            }
        }

        private static void LoadLangChs()
        {
            const string pattern = @"'(?<key>.+?)':'(?<val>.+?)',";
            using (var streamReader = new StreamReader("lang_chs.txt", Encoding.UTF8))
            {
                var fileContent = streamReader.ReadToEnd();
                var matches = Regex.Matches(fileContent, pattern);
                var items = from Match match in matches
                            select new { key = match.Groups["key"].Value, val = match.Groups["val"].Value };

                foreach (var item in items)
                {
                    if (!KeyWordMap.ContainsKey(item.key))
                    {
                        KeyWordMap.Add(item.key, item.val);
                    }
                }
            }
        }

        private void LoadMultiLoginConf()
        {
            using (var sr = new StreamReader("multi_login.conf", Encoding.UTF8))
            {
                var line = sr.ReadLine();
                while (line != null)
                {
                    var strs = line.Split('|');
                    if (strs.Length == 6)
                    {
                        var conf = new LoginParam
                                       {
                                           Name = strs[0].Trim(' '),
                                           LoginURL = strs[1].Trim(' '),
                                           UsernameElmID = strs[2].Trim(' '),
                                           PasswordElmID = strs[3].Trim(' '),
                                           LoginTitle = strs[4].Trim(' '),
                                           HomeTitle = strs[5].Trim(' ')
                                       };

                        this.multiLoginConf.Add(conf.Name, conf);
                    }

                    line = sr.ReadLine();
                }
            }
        }

        private static string BuildSubHeroesString(ref List<string> heroList)
        {
            var validHeros = heroList.Where((hero, index) => index < 4);
            return string.Join("%7C", validHeros.ToArray());
        }

        private static string BuildSoldierString(ref List<Soldier> soldierList, int number)
        {
            if (number == 0)
            {
                return "";
            }

            foreach (var item in soldierList)
            {
                if (item.SoldierNumber >= number)
                {
                    item.SoldierNumber -= number;
                    return string.Format("{0}%3A{1}", item.SoldierType, number);
                }
            }

            return "";
        }

        private static IEnumerable<long> CalculateDonations(IList<long> resNeeds, IList<long> resHave)
        {
            for (var i = 0; i < 4; ++i)
            {
                var toDonate = resNeeds[i] > resHave[i] ? resHave[i] : resNeeds[i];
                yield return toDonate > 10000 ? toDonate : 0;
            }
        }

        private List<string> BatchDonate(List<string> accountList)
        {
            var toRemoveAccounts = new List<string>();
            foreach (var account in accountList)
            {
                var accountInfo = this.accountTable[account];
                var influenceSciencePage = TCPage.Science.ShowScience.Open(accountInfo.WebAgent);
                var resNeedTable = influenceSciencePage.ResourceTable.Zip(
                    influenceSciencePage.MaxResourceTable,
                    (x, y) => y - x
                    ).ToList();

                this.DebugLog("Donate: Resouce:wood({0}/{1}),mud({2}/{3}),iron({4}/{5}),food({6}/{7})",
                               influenceSciencePage.Wood, influenceSciencePage.MaxWood,
                               influenceSciencePage.Mud, influenceSciencePage.MaxMud,
                               influenceSciencePage.Iron, influenceSciencePage.MaxIron,
                               influenceSciencePage.Food, influenceSciencePage.MaxFood);

                var resNeeds = influenceSciencePage.MaxFood + influenceSciencePage.MaxIron
                               + influenceSciencePage.MaxWood + influenceSciencePage.MaxMud
                               - influenceSciencePage.MaxFood - influenceSciencePage.MaxIron
                               - influenceSciencePage.MaxWood - influenceSciencePage.MaxMud;
                if (resNeeds < 1000000)
                {
                    return accountList;
                }

                var donatePage = ShowInfluenceDonate.Open(accountInfo.WebAgent);
                while (donatePage.Food < 10000)
                {
                    if (!this.OpenResourceBox(accountInfo))
                    {
                        this.DebugLog("Donate: Remote Account {0} since lack of resource box.", account);
                        toRemoveAccounts.Add(account);
                        break;
                    }

                    donatePage = ShowInfluenceDonate.Open(accountInfo.WebAgent);
                    this.DebugLog("Donate: {0} Open Box: {1}", account, donatePage.Food);
                }

                var resToContribute = CalculateDonations(resNeedTable, donatePage.ResourcsTable).ToList();

                this.DebugLog("Donate: {0} gives {1}", account, resToContribute[3]);
                var doDonatePage = DoInfluenceDonate.Open(accountInfo.WebAgent, resToContribute);
                if (doDonatePage.Success)
                {
                    continue;
                }

                this.DebugLog("Donate: Remote Account {0} since {1}", account, doDonatePage.RawPage);
                toRemoveAccounts.Add(account);
            }
            return toRemoveAccounts;
        }

        private void UpdateHeroTable(IEnumerable<HeroInfo> heroList)
        {
            foreach (var hero in heroList)
            {
                this.Invoke(
                    new DoSomething(
                        () =>
                        {
                            foreach (ListViewItem lvItem in this.listViewAccountHero.Items)
                            {
                                var tabHero = lvItem.Tag as HeroInfo;
                                if (tabHero != null &&
                                    tabHero.HeroId == hero.HeroId &&
                                    tabHero.IsDead != hero.IsDead)
                                {
                                    lvItem.Tag = hero;
                                    break;
                                }
                            }
                        }));
            }
        }

        private void LoadAccountListToAccountTaskTable()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DoSomething(this.LoadAccountListToAccountTaskTable));
            }
            else
            {
                if (this.listViewTaskIdleAccount.Items.Count == 0 && this.listViewAccountActiveTask.Items.Count == 0)
                {
                    this.listViewTaskIdleAccount.Items.Clear();
                    foreach (var accountInfo in this.accountTable.Values)
                    {
                        var lvItem = new ListViewItem();
                        lvItem.Text = accountInfo.UserName;
                        lvItem.Tag = accountInfo;
                        this.listViewTaskIdleAccount.Items.Add(lvItem);
                    }
                }
            }
        }

        private void TryBuildInfluenceMaps()
        {
            Parallel.Dispatch(
                this.accountTable.Values,
                account =>
                {
                    if (account.InfluenceCityList != null)
                    {
                        return;
                    }

                    var accountCityList = TCDataType.InfluenceMap.QueryCityList(account).ToList();
                    account.InfluenceCityList = accountCityList.ToDictionary(city => city.Name);

                    if (!accountCityList.Any())
                    {
                        return;
                    }

                    account.InfluenceMap = TCDataType.InfluenceMap.BuildMap(accountCityList, account);
                    account.MainCity = accountCityList.Single(cityInfo => cityInfo.CityId == 0);
                }).Then(
                        () =>
                        {
                            this.LoadAccountListToMoveArmyTab();
                            this.LoadAccountListToAccountTaskTable();
                        });
        }

        private void LoadAccountListToMoveArmyTab()
        {
            this.Invoke(
                new DoSomething(
                    () =>
                    {
                        this.comboBoxAccount.Items.Clear();
                        foreach (var account in this.accountTable.Keys)
                        {
                            this.comboBoxAccount.Items.Add(account);
                        }
                    }));
        }

        private MoveTroopTask CreateMoveTroopTask(
            AccountInfo accountInfo,
            CityInfo fromCity,
            CityInfo toCity,
            List<Soldier> soldierList,
            List<string> heroList,
            int brickNum,
            bool sync = false)
        {
            var moveTask = new MoveTroopTask(accountInfo, fromCity, toCity, soldierList, heroList, brickNum);
            if (!moveTask.CanMove)
            {
                return null;
            }

            var threadTask = Task.Run(
                () =>
                {
                    moveTask.TryEnter();
                    moveTask.MoveTroop();
                    moveTask.Leave();
                });

            if (sync)
            {
                threadTask.Wait();
            }

            this.AddTaskToListView(moveTask);
            return moveTask;
        }

        private void AddTaskToListView(TCTask moveTask)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(
                    new DoSomething(
                        () =>
                        {
                            var lvItemTask = new ListViewItem { Tag = moveTask };
                            moveTask.SyncToListViewItem(lvItemTask, RemoteTime);
                            this.listViewTasks.Items.Add(lvItemTask);
                        }));
            }
            else
            {
                var lvItemTask = new ListViewItem { Tag = moveTask };
                moveTask.SyncToListViewItem(lvItemTask, RemoteTime);
                this.listViewTasks.Items.Add(lvItemTask);
            }
        }

        private static bool RepairCityFortress(int cityId, AccountInfo account)
        {
            ShowInfluenceCityDetail.Open(account.WebAgent, cityId);

            var cityBuildPage = ShowCityBuild.Open(account.WebAgent, cityId);

            var cityRepairFortressPage = ShowInfluenceBuild.Open(
                account.WebAgent,
                cityId,
                (int)CityBuildId.Fortress,
                cityBuildPage.Fortress.Level);

            if (cityRepairFortressPage.CompleteRepairNeeds == 0)
            {
                Logger.Verbose("Repair Fortress at {0} canceled: No Need.", cityBuildPage.CityName);
                return false;
            }

            var brickNumToUse = Math.Min(cityRepairFortressPage.BrickNum, cityRepairFortressPage.CompleteRepairNeeds);

            if (brickNumToUse > 0)
            {
                var resultPage = DoBuildRepair.Open(
                    account.WebAgent,
                    cityRepairFortressPage.CityNodeId,
                    cityRepairFortressPage.BuildId,
                    brickNumToUse);
                Logger.Verbose("[{2}] Repair Fortress({3}) at {0} Ok: {1}",
                    cityBuildPage.CityName, resultPage.RawPage, account.UserName, cityBuildPage.Fortress.Duration);
            }
            else
            {
                Logger.Verbose("[{1}]Repair Fortress({2}) at {0} canceled: No Brick.",
                    cityBuildPage.CityName, account.UserName, cityBuildPage.Fortress.Duration);
            }

            return true;
        }

        private static int CalcCarryBrickNum(IEnumerable<Soldier> army)
        {
            return army.Sum(a => SoldierTable[a.SoldierType].Capacity * a.SoldierNumber) / 50000;
        }

        private void CreateBuildDogTask(AccountInfo account)
        {
            var task = new UpgradeBuildDog(account);

            task.AddMileStone(20, 5);
            task.AddMileStone(51, 1);

            var lvItemTask = new ListViewItem { Tag = task };
            task.SyncToListViewItem(lvItemTask, RemoteTime);
            this.listViewTasks.Items.Add(lvItemTask);
        }

        private void CreateSpyTask(AccountInfo account)
        {
            var task = new SpyTask(
                account,
                () =>
                {
                    var focusList = new List<CityInfo>();
                    this.Invoke(
                        new DoSomething(
                            () =>
                            {
                                focusList.AddRange(
                                    from ListViewItem lvItem in this.listViewEnemyCityInfo.CheckedItems
                                    select lvItem.Tag as CityInfo);
                            }));
                    return focusList;
                },
                (city, log) =>
                {
                    if (city.UiItem == null)
                    {
                        this.Invoke(
                            new DoSomething(
                                () =>
                                {
                                    var lvItem = new ListViewItem(RemoteTime.ToString()) { Tag = city.RawData };
                                    lvItem.SubItems.Add(city.Name);
                                    lvItem.SubItems.Add(log);
                                    city.UiItem = lvItem;
                                    this.listViewEnemyCityInfo.Items.Add(lvItem);
                                }));
                    }
                    else
                    {
                        this.Invoke(
                            new DoSomething(
                                () =>
                                {
                                    var lvItem = city.UiItem;
                                    lvItem.SubItems[0].Text = RemoteTime.ToString();
                                    lvItem.SubItems[2].Text = log;
                                }));
                    }
                });
        }

        private void CreateInfluenceGuardTask(AccountInfo account)
        {
            var task = new InfluenceGuard(account) { RefuseTimer = new Timer(500) { AutoReset = true } };
            task.RefuseTimer.Elapsed += (sender, args) =>
                {
                    List<int> unionIdList;
                    lock (task.UnionIdSet)
                    {
                        unionIdList = task.UnionIdSet.ToList();
                    }

                    Parallel.Dispatch(
                        unionIdList,
                        unionId => { DoCheckMember.Open(account.WebAgent, DoCheckMember.Action.refuse, unionId); });
                };
            task.RefuseTimer.Start();

            var lvItemTask = new ListViewItem { Tag = task };
            task.SyncToListViewItem(lvItemTask, RemoteTime);
            this.listViewTasks.Items.Add(lvItemTask);
        }

        public void DebugLog(string format, params object[] args)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new DoSomething(() => { this.DebugLog(format, args); }));
            }
            else
            {
                if (this.listViewDebugLog.Items.Count > 65535)
                {
                    this.listViewDebugLog.Items.RemoveAt(0);
                }
                var logLine = string.Format(format, args);
                var lvItem = new ListViewItem(RemoteTime.ToString());
                lvItem.SubItems.Add(logLine);
                this.listViewDebugLog.Items.Add(lvItem);
                this.listViewDebugLog.EnsureVisible(lvItem.Index);
            }
        }

        private void CreateShipTroopTasks(AccountInfo account, CityInfo targetCity, bool carryBrick)
        {
            var fromCityList = account.CityNameList.Where(c => c != targetCity.Name).ToList();
            foreach (var cityInfo in fromCityList.Select(city => account.InfluenceCityList[city]))
            {
                var info = cityInfo;
                Task.Run(
                    () =>
                    {
                        var moveArmyPage = ShowMoveArmy.Open(account.WebAgent, info.NodeId);
                        var troop = moveArmyPage.Army.ToList();
                        var heroes = moveArmyPage.HeroList.Select(h => h.HeroId.ToString()).ToList();
                        var carryBrickNum = 0;
                        if (carryBrick)
                        {
                            carryBrickNum = Math.Min(CalcCarryBrickNum(troop), moveArmyPage.BrickNum);
                        }

                        this.CreateMoveTroopTask(account, info, targetCity, troop, heroes, carryBrickNum, true);
                    });
            }
        }

        private void CreateShipBrickTask(AccountInfo account, CityInfo targetCity)
        {
            Task.Run(() =>
            {
                var randomDist = this.randGen.Next(100, 60 * 1000);
                Thread.Sleep(randomDist);

                var task = new ShipBrickTask(account, targetCity);

                var lvItem = new ListViewItem();
                lvItem.SubItems[0].Text = task.Account.UserName;
                lvItem.SubItems.Add(task.TaskId);
                lvItem.SubItems.Add(task.GetTaskHint());
                lvItem.SubItems.Add(task.SubTasks.Count.ToString());
                lvItem.Tag = task;
                this.Invoke(new DoSomething(() =>
                {
                    this.listViewAccountActiveTask.Items.Add(lvItem);
                }));

                task.TryScheSubTasks();
            });
        }

        private delegate void DoSomething();
    }
}