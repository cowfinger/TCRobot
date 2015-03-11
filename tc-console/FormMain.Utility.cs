﻿namespace TC
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

    using TC.TCTasks;

    partial class FormMain
    {
        public static Dictionary<string, string> KeyWordMap = new Dictionary<string, string>();

        private string HTTPRequest(string url)
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
                request.Headers.Add("Cookie", this.GetAccountCookie(account));

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
                    this.SetAccountCookie(account, response.Headers["Set-Cookie"]);

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

        private string Time2Str(int timeval)
        {
            var secs = timeval % 60;
            var mins = (timeval / 60) % 60;
            var hours = timeval / 3600;
            var fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        private string CalcGroupType(TroopInfo team)
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

        private void SyncTroopInfoToUI(IEnumerable<TroopInfo> troopList)
        {
            foreach (var team in troopList)
            {
                this.TrySyncTroopInfoToUI(team);
            }
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
                lvItemTroop = new ListViewItem();
                lvItemTroop.Tag = team;
                lvItemTroop.SubItems[0].Text = team.AccountName;
                lvItemTroop.SubItems.Add(team.TroopId);
                lvItemTroop.SubItems.Add(team.PowerIndex.ToString());
                lvItemTroop.SubItems.Add(this.Time2Str(team.Duration));
                lvItemTroop.SubItems.Add(this.Time2Str(0));
                lvItemTroop.SubItems.Add(team.GroupId);
                lvItemTroop.SubItems.Add(this.CalcGroupType(team));

                this.listViewTroops.Items.Add(lvItemTroop);
            }
            else
            {
                lvItemTroop.SubItems[3].Text = this.Time2Str(team.Duration);
                lvItemTroop.SubItems[4].Text = this.Time2Str(0);
            }
        }

        private void RefreshTroopInfoToUI(IEnumerable<TroopInfo> troopList)
        {
            this.listViewTroops.Items.Clear();
            foreach (var team in troopList)
            {
                var newli = new ListViewItem();
                newli.SubItems[0].Text = team.AccountName;
                newli.SubItems.Add(team.TroopId);
                newli.SubItems.Add(team.PowerIndex.ToString());
                newli.SubItems.Add(this.Time2Str(team.Duration));
                newli.SubItems.Add(this.Time2Str(0));
                newli.SubItems.Add(team.GroupId);
                newli.SubItems.Add(this.CalcGroupType(team));
                newli.Tag = team;
                this.listViewTroops.Items.Add(newli);
            }
        }

        private string ConvertStatusStr(string status)
        {
            if (status == "on-line")
            {
                return "已登录";
            }
            if (status == "in-login")
            {
                return "登录中";
            }
            if (status == "login-failed")
            {
                return "登录失败";
            }
            if (status == "submitting")
            {
                return "提交中";
            }
            if (status == "sync-time")
            {
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
                    newli.SubItems.Add(this.ConvertStatusStr(account.LoginStatus));
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
            return this.accountTable.Values.Where(account => account.CityIDList.Contains(cityId)).Select(
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
            var relatedAccountList = this.accountTable.Values.Where(account => account.CityIDList.Contains(cityId));
            if (relatedAccountList.Any())
            {
                var account = relatedAccountList.First();
                var attackCityList = this.OpenAttackPage(cityId, account.UserName);
                var greoupAttackCityList = this.GetGroupAttackTargetCity(cityId, account.UserName);
                // var moveCityList = GetMoveTargetCities(cityId, account.UserName);
                return attackCityList.Concat(greoupAttackCityList).Distinct();
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

        private void LoadRoadInfo()
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

        private void LoadSoldierInfo()
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

        private void LoadLangChs()
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

        private void BatchLoginProc()
        {
            foreach (var key in this.accountTable.Keys)
            {
                this.LoginAccount(key);
            }
        }

        private bool IsOwnCity(string name)
        {
            var cityId = this.cityList[name];
            foreach (var account in this.accountTable.Values)
            {
                if (account.CityIDList.Contains(cityId))
                {
                    return true;
                }
            }

            return false;
        }

        private string BuildSubHeroesString(ref List<string> heroList)
        {
            var validHeros = heroList.Where((hero, index) => index < 4);
            return string.Join("%7C", validHeros.ToArray());
        }

        private string BuildSoldierString(ref List<Soldier> soldierList, int number)
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

        private IEnumerable<long> CalculateDonations(List<long> resNeeds, List<long> resHave)
        {
            for (var i = 0; i < 4; ++i)
            {
                var toDonate = resNeeds[i] > resHave[i] ? resHave[i] : resNeeds[i];
                yield return toDonate > 10000 ? toDonate : 0;
            }
        }

        private void BatchDonate(int i, ref List<string> accountList)
        {
            foreach (var account in accountList)
            {
                var influenceSciencePage = this.OpenInfluenceSciencePage(account);
                var influenceRes = this.ParseInfluenceResource(influenceSciencePage).ToList();
                var resNeeds = influenceRes.Select(resPair => resPair.Value - resPair.Key).ToList();

                if (resNeeds[3] < 1000000)
                {
                    accountList.Remove(account);
                    continue;
                }

                var accountRes = this.GetAccountResources(account).ToList();
                while (accountRes[3] < 10000)
                {
                    if (!this.OpenResourceBox(account))
                    {
                        accountList.Remove(account);
                        break;
                    }

                    accountRes = this.GetAccountResources(account).ToList();
                }

                var resToContribute = this.CalculateDonations(resNeeds, accountRes).ToList();

                this.DonateResource(
                    account,
                    resToContribute[0],
                    resToContribute[1],
                    resToContribute[2],
                    resToContribute[3]);
            }
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
                                    if (tabHero != null && tabHero.HeroId == hero.HeroId
                                        && tabHero.IsDead != hero.IsDead)
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
                        if (account.InfluenceCityList == null)
                        {
                            var accountCityList = this.QueryInfluenceCityList(account.UserName).ToList();
                            account.InfluenceCityList = accountCityList.ToDictionary(city => city.Name);

                            if (!accountCityList.Any())
                            {
                                return;
                            }

                            account.InfluenceMap = this.BuildInfluenceCityMap(accountCityList, account.UserName);
                            account.MainCity = accountCityList.Single(cityInfo => cityInfo.CityId == 0);
                        }
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

        private bool HasTroopArrived(MoveTroopTask task)
        {
            var cityNodeId = task.NextCity.CityId;
            var fromCityId = task.NextCity.NodeId;
            this.OpenCityPage(cityNodeId, task.Account.UserName);
            var movePageFrom = this.ChangeMoveFromCity(task.Account.UserName, fromCityId.ToString());

            var heroes = this.ParseHeroIdListFromMovePage(movePageFrom).ToList();

            if (!heroes.Any())
            {
                return false;
            }

            var heroMatchCount = task.HeroIdList.Sum(hero => heroes.Contains(hero) ? 1 : 0);
            return heroMatchCount == task.HeroIdList.Count();
        }

        private MoveTask MoveTroop(MoveTroopTask task)
        {
            var cityNodeId = task.CurrentCity.CityId;

            this.OpenCityPage(cityNodeId, task.Account.UserName);
            var moveQueuePage = this.OpenMoveTaskQueue(task.Account.UserName);
            var moveTaskList = this.ParseMoveTaskList(moveQueuePage).ToList();

            var heroString = string.Join("%7C", task.HeroIdList.ToArray());
            var soldiers =
                task.SoldierList.Where(s => s.SoldierNumber != 0)
                    .Select(soldier => string.Format("{0}%3A{1}", soldier.SoldierType, soldier.SoldierNumber))
                    .ToArray();
            var soldierString = string.Join("%7c", soldiers);

            this.ConfirmMoveTroop(
                task.CurrentCity.NodeId,
                task.NextCity.NodeId,
                soldierString,
                heroString,
                task.BrickNum,
                task.Account.UserName);

            for (var i = 0; i < 10; ++i)
            {
                Thread.Sleep(2000);
                this.OpenCityPage(task.NextCity.CityId, task.Account.UserName);
                var newMoveQueuePage = this.OpenMoveTaskQueue(task.Account.UserName);
                var newMoveTaskList = this.ParseMoveTaskList(newMoveQueuePage).ToList();

                if (!newMoveTaskList.Any())
                {
                    continue;
                }

                var thisMoveTask =
                    newMoveTaskList.Single(
                        taskItem => !moveTaskList.Select(item => item.TaskId).Contains(taskItem.TaskId));
                task.TaskId = thisMoveTask.TaskId;
                task.ExecutionTime = thisMoveTask.EndTime.AddSeconds(2);
                return thisMoveTask;
            }

            return null;
        }

        private int CalculateDistance(string from, string to, string account, Dictionary<string, int> roadLevelCache)
        {
            string fromCityIdValue;
            if (!this.cityList.TryGetValue(from, out fromCityIdValue))
            {
                return 2;
            }
            int fromCityId = int.Parse(fromCityIdValue);

            string toCityIdValue;
            if (!this.cityList.TryGetValue(to, out toCityIdValue))
            {
                return 120;
            }
            int toCityId = int.Parse(toCityIdValue);

            int roadLevel;
            if (!roadLevelCache.TryGetValue(from, out roadLevel))
            {
                this.OpenCityPage(fromCityId, account);
                var page = this.OpenCityBuildPage(fromCityId, account);
                roadLevel = ParseRoadLevelFromCityBuildPage(page);
                roadLevelCache.Add(from, roadLevel);
            }

            return RoadLevelToDistanceMap[roadLevel];
        }

        private MoveTroopTask CreateMoveTroopTask(
            AccountInfo accountInfo,
            CityInfo fromCity,
            CityInfo toCity,
            List<Soldier> soldierList,
            List<string> heroList,
            int brickNum)
        {
            var initialHelper = new DijstraHelper(accountInfo.InfluenceMap)
                                    {
                                        DistanceCalculate = this.CalculateDistance,
                                        account = accountInfo.UserName
                                    };
            var initialPath = initialHelper.GetPath(fromCity.Name, toCity.Name, soldierList).ToList();
            initialPath.Reverse();
            var nextCity = accountInfo.InfluenceCityList[initialPath.First()];

            var moveTask = new MoveTroopTask(accountInfo, fromCity, nextCity, toCity, brickNum, "")
                               {
                                   SoldierList =
                                       soldierList,
                                   HeroIdList =
                                       heroList,
                                   Path =
                                       initialPath
                               };

            moveTask.TaskAction = obj =>
                {
                    if (!this.HasTroopArrived(moveTask))
                    {
                        ++moveTask.RetryCount;
                        if (moveTask.RetryCount < 3)
                        {
                            moveTask.ExecutionTime = moveTask.ExecutionTime.AddSeconds(10);
                        }
                        else
                        {
                            moveTask.IsCompleted = true;
                        }
                        return;
                    }

                    if (moveTask.NextCity == moveTask.TerminalCity)
                    {
                        moveTask.IsCompleted = true;
                        return;
                    }

                    var helper = new DijstraHelper(accountInfo.InfluenceMap)
                                     {
                                         DistanceCalculate =
                                             this.CalculateDistance,
                                         account = accountInfo.UserName
                                     };

                    var path = helper.GetPath(moveTask.NextCity.Name, moveTask.TerminalCity.Name, soldierList).ToList();
                    path.Reverse();

                    moveTask.Path = path;
                    moveTask.CurrentCity = moveTask.NextCity;
                    moveTask.NextCity = moveTask.Account.InfluenceCityList[path.First()];

                    this.MoveTroop(moveTask);
                };

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

            Task.Run(
                () =>
                    {
                        moveTask.TryEnter();
                        this.MoveTroop(moveTask);
                        moveTask.Leave();
                    });

            return moveTask;
        }

        private void ShipBrickMonitorCityWall(ShipBrickTask task)
        {
            this.OpenCityPage(task.TargetCity.CityId, task.Account.UserName);
            var cityBuildPage = this.OpenCityBuildPage(task.TargetCity.CityId, task.Account.UserName);

            var cityRepairWallPageData = this.OpenCityWallPage(task.TargetCity.CityId, 10, task.Account.UserName);
            var cityRepairWallPage = new TCPage.RepairCityWallPage(cityRepairWallPageData);
            var brickNumToUse = Math.Min(cityRepairWallPage.BrickNum, cityRepairWallPage.CompleteRepairNeeds);
            if (brickNumToUse > 0)
            {
                this.RepairCityBuild(
                    cityRepairWallPage.CityNodeId, 
                    cityRepairWallPage.BuildId, 
                    brickNumToUse, 
                    task.Account.UserName);
            }
        }

        private void ShipBrickScheSubTask(ShipBrickTask task)
        {
            var account = task.Account;
            var homeCity = account.InfluenceCityList.Values.First(city => city.CityId == 0);
            var targetCity = task.TargetCity;

            this.ShipBrickMonitorCityWall(task);

            var completedTasks = task.SubTasks.Where(t => t.IsCompleted).ToList();
            if (completedTasks.Any())
            {
                foreach (var subTask in completedTasks)
                {
                    task.SubTasks.Remove(subTask);
                }

                var moveTroopTasks = completedTasks.Where(t => t.TerminalCity.Name != targetCity.Name);
                var newMoveBrickTasks =
                    moveTroopTasks.Select(
                        t => this.ShipBrickCreateMoveBrickTask(account, t.TerminalCity, task.TargetCity))
                        .Where(t => t != null)
                        .ToList();

                task.SubTasks.AddRange(newMoveBrickTasks);

                var moveBrickTasks = completedTasks.Where(t => t.TerminalCity.Name == targetCity.Name);
                if (moveBrickTasks.Any())
                {
                    task.SubTasks.Add(this.ShipBrickCreateMoveTroopTask(account, targetCity, homeCity));
                }
            }
            else
            {
                // Search bricks and move troop.
                var newTasks = account.CityNameList.Select(
                    cityName =>
                        {
                            var cityInfo = account.InfluenceCityList[cityName];

                            if (cityInfo.Name == targetCity.Name)
                            {
                                return this.ShipBrickCreateMoveTroopTask(account, targetCity, homeCity);
                            }

                            if (cityInfo.Name == homeCity.Name)
                            {
                                return this.ShipBrickCreateMoveBrickTask(account, homeCity, targetCity);
                            }

                            var cityMovePage = this.ChangeMoveFromCity(account.UserName, cityInfo.NodeId.ToString());
                            var brickNum = this.ParseBrickNumberFromMovePage(cityMovePage);

                            return brickNum == 0
                                       ? this.ShipBrickCreateMoveTroopTask(account, cityInfo, homeCity)
                                       : this.ShipBrickCreateMoveBrickTask(account, cityInfo, targetCity);
                        }).Where(t => t != null).ToList();
                task.SubTasks.AddRange(newTasks);
            }
        }

        private MoveTroopTask ShipBrickCreateMoveTroopTask(AccountInfo account, CityInfo fromCity, CityInfo toCity)
        {
            var cityMovePage = this.ChangeMoveFromCity(account.UserName, fromCity.NodeId.ToString());
            var troop = this.ParseSoldierListFromMovePage(cityMovePage).ToList();

            return this.CreateMoveTroopTask(account, fromCity, toCity, troop, new List<string>(), 0);
        }

        private MoveTroopTask ShipBrickCreateMoveBrickTask(AccountInfo account, CityInfo fromCity, CityInfo toCity)
        {
            var cityMovePage = this.ChangeMoveFromCity(account.UserName, fromCity.NodeId.ToString());
            var soldiers = this.ParseSoldierListFromMovePage(cityMovePage).ToList();
            var brickNum = this.ParseBrickNumberFromMovePage(cityMovePage);
            var carryBrickNum = Math.Min(this.CalcCarryBrickNum(soldiers), brickNum);
            var troop = this.CalcCarryTroop(soldiers, carryBrickNum).ToList();
            if (carryBrickNum == 0)
            {
                return null;
            }

            return this.CreateMoveTroopTask(account, fromCity, toCity, troop, new List<string>(), carryBrickNum);
        }

        private IEnumerable<Soldier> CalcCarryTroop(IEnumerable<Soldier> army, int brickNum)
        {
            var soldierList = army.ToList();
            soldierList.Sort(
                (x, y) =>
                    {
                        var indexX = SoldierTable[x.SoldierType].Capacity * SoldierTable[x.SoldierType].Speed;
                        var indexY = SoldierTable[y.SoldierType].Capacity * SoldierTable[y.SoldierType].Speed;
                        return indexX.CompareTo(indexY);
                    });
            soldierList.Reverse();

            var totalCap = brickNum * 50000;
            var costCap = 0;
            var resultList = new List<Soldier>();
            foreach (var soldier in soldierList)
            {
                if (costCap >= totalCap)
                {
                    break;
                }

                var cap = soldier.SoldierNumber * SoldierTable[soldier.SoldierType].Capacity;
                if (cap > 0)
                {
                    resultList.Add(soldier);
                    costCap += cap;
                }
            }

            return resultList;
        }

        private int CalcCarryBrickNum(IEnumerable<Soldier> army)
        {
            return army.Sum(a => SoldierTable[a.SoldierType].Capacity * a.SoldierNumber) / 50000;
        }

        private IEnumerable<TCTask> GetActiveTasks()
        {
            if (!this.InvokeRequired)
            {
                return from ListViewItem lvItem in this.listViewTasks.Items
                       let activeTask = lvItem.Tag as TCTask
                       select activeTask;
            }

            IEnumerable<TCTask> activeTaskList = new List<TCTask>();
            this.Invoke(new DoSomething(() => { activeTaskList = this.GetActiveTasks(); }));
            return activeTaskList;
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
                            var cityMovePage = this.ChangeMoveFromCity(account.UserName, info.NodeId.ToString());
                            var troop = this.ParseSoldierListFromMovePage(cityMovePage).ToList();
                            var heroes = this.ParseHeroIdListFromMovePage(cityMovePage).ToList();
                            var carryBrickNum = 0;
                            if (carryBrick)
                            {
                                var brickNum = this.ParseBrickNumberFromMovePage(cityMovePage);
                                carryBrickNum = Math.Min(this.CalcCarryBrickNum(troop), brickNum);
                            }

                            this.CreateMoveTroopTask(account, info, targetCity, troop, heroes, carryBrickNum);
                        });
            }
        }

        private void CreateShipBrickTask(AccountInfo account, CityInfo targetCity)
        {
            var task = new ShipBrickTask(account, targetCity);
            task.TaskAction = obj => { this.ShipBrickScheSubTask(task); };

            var lvItem = new ListViewItem();
            lvItem.SubItems[0].Text = task.Account.UserName;
            lvItem.SubItems.Add(task.TaskId);
            lvItem.SubItems.Add(task.GetTaskHint());
            lvItem.SubItems.Add(task.SubTasks.Count.ToString());
            lvItem.Tag = task;
            this.listViewAccountActiveTask.Items.Add(lvItem);

            Task.Run(() => { this.ShipBrickScheSubTask(task); });
        }

        private delegate void DoSomething();
    }
}