using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace TC
{
    partial class FormMain
    {
        private object taskTimerLock = new object();
        private DateTime lastTaskTimerWakeup = DateTime.MinValue;
        private System.Timers.Timer taskTimer = null;

        private DateTime lastOnlineTaskRefreshTimerWakeup = DateTime.MinValue;
        private System.Timers.Timer onlineTaskRefreshTimer = null;

        private System.Timers.Timer reliveHeroTimer = null;

        private string HTTPRequest(string url, string account, string body = null)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.UserAgent = UserAgent;
                request.Headers.Add("Cookie", GetAccountCookie(account));

                if (!string.IsNullOrEmpty(body))
                {
                    var codedBytes = new ASCIIEncoding().GetBytes(body);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = codedBytes.Length;
                    request.GetRequestStream().Write(codedBytes, 0, codedBytes.Length);
                }

                // var response = (HttpWebResponse)request.GetResponse();
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    SetAccountCookie(account, response.Headers["Set-Cookie"]);

                    string content = string.Empty;
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
            int secs = timeval % 60;
            int mins = (timeval / 60) % 60;
            int hours = timeval / 3600;
            string fmt = "{0:D2}:{1:D2}:{2:D2}";
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
            foreach (TroopInfo team in troopList)
            {
                TrySyncTroopInfoToUI(team);
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
                lvItemTroop.SubItems.Add(Time2Str(team.Duration));
                lvItemTroop.SubItems.Add(Time2Str(0));
                lvItemTroop.SubItems.Add(team.GroupId);
                lvItemTroop.SubItems.Add(CalcGroupType(team));

                this.listViewTroops.Items.Add(lvItemTroop);
            }
            else
            {
                lvItemTroop.SubItems[3].Text = Time2Str(team.Duration);
                lvItemTroop.SubItems[4].Text = Time2Str(0);
            }
        }

        private void RefreshTroopInfoToUI(IEnumerable<TroopInfo> troopList)
        {
            listViewTroops.Items.Clear();
            foreach (TroopInfo team in troopList)
            {

                ListViewItem newli = new ListViewItem();
                newli.SubItems[0].Text = team.AccountName;
                newli.SubItems.Add(team.TroopId);
                newli.SubItems.Add(team.PowerIndex.ToString());
                newli.SubItems.Add(Time2Str(team.Duration));
                newli.SubItems.Add(Time2Str(0));
                newli.SubItems.Add(team.GroupId);
                newli.SubItems.Add(CalcGroupType(team));
                newli.Tag = team;
                listViewTroops.Items.Add(newli);
            }
        }

        private string ConvertStatusStr(string status)
        {
            if (status == "on-line")
                return "已登录";
            if (status == "in-login")
                return "登录中";
            if (status == "login-failed")
                return "登录失败";
            if (status == "submitting")
                return "提交中";
            if (status == "sync-time")
                return "同步系统时间中";
            return "未登录";
        }

        private void SyncAccountsStatus()
        {

            listViewAccounts.Items.Clear();
            int loginnum = 0;
            foreach (string accountkey in this.accountTable.Keys)
            {
                AccountInfo account = this.accountTable[accountkey];

                ListViewItem newli = new ListViewItem();
                {
                    newli.SubItems[0].Text = account.UserName;
                    newli.SubItems.Add(ConvertStatusStr(account.LoginStatus));
                }
                newli.Tag = account;
                listViewAccounts.Items.Add(newli);

                if (account.LoginStatus == "on-line")
                {
                    loginnum++;
                    hostname = account.AccountType;
                }
            }


            if (loginnum >= this.accountTable.Keys.Count)
            {
                btnLoginAll.Text = "登录所有";
                btnAutoAttack.Enabled = true;
                btnLoginAll.Enabled = false;
                btnScanCity.Enabled = true;
                btnQuickCreateTroop.Enabled = true;
            }
        }

        delegate void DoSomething();

        private static int CompareTroopInfo(TroopInfo x, TroopInfo y)
        {
            if (x.Duration == y.Duration)
            {
                return 0;
            }
            else if (x.Duration > y.Duration)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }

        private void StartOnlineTaskCheckTimer()
        {
            if (this.onlineTaskRefreshTimer != null)
            {
                return;
            }

            this.onlineTaskRefreshTimer = new System.Timers.Timer(17000);

            this.onlineTaskRefreshTimer.AutoReset = true;
            this.onlineTaskRefreshTimer.Elapsed += new System.Timers.ElapsedEventHandler((obj, evn) =>
            {
                Parallel.Dispatch(this.accountTable.Values, account =>
                {
                    var tasks2 = QueryOnlineTroopList("2", account.UserName);
                    var tasks4 = QueryOnlineTroopList("4", account.UserName);

                    var allTasks = tasks2.Concat(tasks4).ToList().ToList();

                    this.Invoke(new DoSomething(() =>
                    {
                        var toRemoveTasks = new List<ListViewItem>();
                        foreach (ListViewItem lvItem in this.listViewCompletedTasks.Items)
                        {
                            var oldTask = lvItem.Tag as AttackTask;

                            foreach (var task in allTasks)
                            {
                                if (oldTask.AccountName == account.UserName && oldTask.TaskId == task.TaskId)
                                {
                                    toRemoveTasks.Add(lvItem);
                                    break;
                                }
                            }
                        }

                        foreach (var item in toRemoveTasks)
                        {
                            this.listViewCompletedTasks.Items.Remove(item);
                        }

                        foreach (var task in allTasks)
                        {
                            bool found = false;
                            foreach (ListViewItem lvItem in this.listViewCompletedTasks.Items)
                            {
                                var oldTask = lvItem.Tag as AttackTask;
                                if (oldTask.AccountName == task.AccountName && oldTask.TaskId == task.TaskId)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                var newLvItem = new ListViewItem();
                                newLvItem.Tag = task;
                                newLvItem.SubItems[0].Text = task.AccountName;
                                newLvItem.SubItems.Add(task.FromCity);
                                newLvItem.SubItems.Add(task.ToCity);
                                newLvItem.SubItems.Add(task.EndTime.ToString());
                                newLvItem.SubItems.Add(task.TaskId);
                                newLvItem.SubItems.Add(task.TaskType);
                                this.listViewCompletedTasks.Items.Add(newLvItem);
                            }
                        }

                    }));
                });
            });

            this.onlineTaskRefreshTimer.Start();
        }

        private void StartTaskTimer()
        {
            if (this.taskTimer != null)
            {
                return;
            }

            this.taskTimer = new System.Timers.Timer(500);

            this.taskTimer.AutoReset = true;
            this.taskTimer.Elapsed += new System.Timers.ElapsedEventHandler((obj, evn) =>
            {
                lock (this.taskTimerLock)
                {
                    var remoteTimeSnapshot = this.RemoteTime;

                    var diff = remoteTimeSnapshot - this.lastTaskTimerWakeup;
                    if (diff.TotalSeconds < 1.0)
                    {
                        return;
                    }

                    var taskLvItemList = new List<ListViewItem>();
                    this.Invoke(new DoSomething(() =>
                    {
                        foreach (ListViewItem lvItem in this.listViewTasks.Items)
                        {
                            taskLvItemList.Add(lvItem);
                        }
                    }));

                    var toExecuteTaskList = new List<AttackTask>();
                    var toChangeTaskLvItems = new List<ListViewItem>();

                    foreach (var lvItem in taskLvItemList)
                    {
                        var task = lvItem.Tag as AttackTask;

                        if (remoteTimeSnapshot >= task.EndTime)
                        {
                            toExecuteTaskList.Add(task);
                            this.Invoke(new DoSomething(() => { this.listViewTasks.Items.Remove(lvItem); }));
                        }
                        else
                        {
                            toChangeTaskLvItems.Add(lvItem);
                        }
                    }

                    this.Invoke(new DoSomething(() =>
                    {
                        foreach (var lvItem in toChangeTaskLvItems)
                        {
                            var task = lvItem.Tag as AttackTask;
                            int timeLeft = (int)((task.EndTime - remoteTimeSnapshot).TotalSeconds);
                            lvItem.SubItems[6].Text = Time2Str(timeLeft);
                        }
                    }));

                    var accountTaskGroups = toExecuteTaskList.GroupBy(task => task.AccountName).ToList();
                    if (accountTaskGroups.Any())
                    {
                        Parallel.Dispatch(accountTaskGroups, taskGroup =>
                        {
                            DispatchSendTroopTasks(taskGroup.Key, taskGroup);
                        });
                    }
                }
            });

            taskTimer.Start();
        }

        private void DispatchSendTroopTasks(string accountName, IEnumerable<AttackTask> accountTaskGroup)
        {
            var cityTaskGroups = accountTaskGroup.GroupBy(task => task.FromCity);

            foreach (var taskGroup in cityTaskGroups)
            {
                var httpClient = new HttpClient(GetAccountCookie(accountName));

                var fromCityId = this.cityList[taskGroup.Key];
                OpenCityPage(fromCityId, ref httpClient); // Open City Page to refresh cookie.

                Parallel.Dispatch(taskGroup, task =>
                {
                    var team = task.Troop;
                    if (team.isGroupTroop)
                    {
                        // OpenGroupAttackPage(team.GroupId, team.ToCityNodeId, ref httpClient);
                        GroupAttackTarget(team.GroupId, team.ToCityNodeId, ref httpClient);
                    }
                    else
                    {
                        // OpenTeamAttackPage(team.TroopId, team.ToCityNodeId, ref httpClient);
                        TeamAttackTarget(team.TroopId, team.ToCityNodeId, ref httpClient);
                    }
                });
            }
        }

        private void StartSendTroopTasks()
        {
            var timeSnapshot = this.RemoteTime;
            var arrivalTime = this.dateTimePickerArrival.Value;
            foreach (ListViewItem lvItemTroop in this.listViewTroops.CheckedItems)
            {
                var team = lvItemTroop.Tag as TroopInfo;
                if (team == null)
                {
                    continue;
                }

                if (!team.isDefendTroop && ((team.isGroupTroop && team.IsGroupHead) || !team.isGroupTroop))
                {
                    var task = new AttackTask()
                    {
                        AccountName = team.AccountName,
                        FromCity = this.listBoxSrcCities.SelectedItem.ToString(),
                        ToCity = this.listBoxDstCities.SelectedItem.ToString(),
                        StartTime = arrivalTime,
                        EndTime = arrivalTime.AddSeconds(-team.Duration),
                        Troop = team,
                    };

                    var lvItemTask = new ListViewItem();
                    lvItemTask.SubItems[0].Text = "Attack";
                    lvItemTask.SubItems.Add(task.AccountName);
                    lvItemTask.SubItems.Add(task.FromCity);
                    lvItemTask.SubItems.Add(task.ToCity);
                    lvItemTask.SubItems.Add(task.StartTime.ToString());
                    lvItemTask.SubItems.Add(task.EndTime.ToString());
                    lvItemTask.SubItems.Add("00:00:00");
                    lvItemTask.Tag = task;
                    this.listViewTasks.Items.Add(lvItemTask);
                    this.listViewTroops.Items.Remove(lvItemTroop);
                }
            }
        }

        private void StopSendTroopTasks()
        {
            foreach (ListViewItem lvItemTask in this.listViewTasks.CheckedItems)
            {
                var task = lvItemTask.Tag as AttackTask;
                if (task == null)
                {
                    continue;
                }

                this.listViewTasks.Items.Remove(lvItemTask);
            }
        }

        private void StartRemoteTimeSyncTimer()
        {
            this.syncRemoteTimeTimer.Elapsed += new System.Timers.ElapsedEventHandler(
                (obj, args) =>
                {
                    this.RemoteTime = QueryRemoteSysTime(this.accountTable.Keys.First());
                });

            this.syncRemoteTimeTimer.AutoReset = true;
            this.syncRemoteTimeTimer.Start();
        }

        private void StartUITimeSyncTimer()
        {
            this.syncTimeToUITimer.Elapsed += new System.Timers.ElapsedEventHandler(
                (obj, args) =>
                {
                    DateTime now = DateTime.Now;

                    TimeSpan diff = now - this.remoteTimeLastSync;
                    DateTime remoteTimeSnapshot = this.RemoteTime;

                    this.RemoteTime = remoteTimeSnapshot + diff;
                    this.remoteTimeLastSync = now;
                    this.Invoke(new DoSomething(() =>
                    {
                        this.textBoxSysTime.Text = this.RemoteTime.ToString();
                    }));
                });

            this.syncTimeToUITimer.AutoReset = true;
            this.syncTimeToUITimer.Start();
        }

        private void StopTimeSyncTimer()
        {
            syncTimeToUITimer.Stop();
        }

        private IEnumerable<TroopInfo> QueryCityTroops(string cityId)
        {
            return this.accountTable.Values.Where(account => account.CityIDList.Contains(cityId)).Select(
                account =>
                {
                    var singleAttackTeams = GetActiveTroopInfo(cityId, "1", account.UserName);
                    var singleDefendTeams = GetActiveTroopInfo(cityId, "2", account.UserName);
                    var groupAttackteams = GetGroupTeamList(cityId, account.UserName);
                    return singleAttackTeams.Concat(singleDefendTeams).Concat(groupAttackteams);
                }).SelectMany(teams => teams);
        }

        private IEnumerable<string> QueryTargetCityList(string cityId)
        {
            var relatedAccountList = this.accountTable.Values.Where(account => account.CityIDList.Contains(cityId));
            if (relatedAccountList.Any())
            {
                var account = relatedAccountList.First();
                var attackCityList = OpenAttackPage(cityId, account.UserName);
                var greoupAttackCityList = GetGroupAttackTargetCity(cityId, account.UserName);
                // var moveCityList = GetMoveTargetCities(cityId, account.UserName);
                return attackCityList.Concat(greoupAttackCityList).Distinct();
            }
            else
            {
                return new List<string>();
            }
        }

        private void LoadCheckpoint()
        {

        }

        private void LoadCityList()
        {
            using (var sr = new StreamReader("cities.txt", System.Text.Encoding.Default))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] strs = line.Split('|', ':');
                    if (strs.Length > 1)
                    {
                        cityList.Add(strs[1], strs[0]);
                    }

                    line = sr.ReadLine();
                }
            }
        }

        private void LoadMultiLoginConf()
        {
            using (var sr = new StreamReader("multi_login.conf", Encoding.Default))
            {
                string line = sr.ReadLine();
                while (line != null)
                {
                    string[] strs = line.Split('|');
                    if (strs.Length == 6)
                    {
                        var conf = new LoginParam()
                        {
                            Name = strs[0].Trim(' '),
                            LoginURL = strs[1].Trim(' '),
                            UsernameElmID = strs[2].Trim(' '),
                            PasswordElmID = strs[3].Trim(' '),
                            LoginTitle = strs[4].Trim(' '),
                            HomeTitle = strs[5].Trim(' '),
                        };

                        this.multiLoginConf.Add(conf.Name, conf);
                    }

                    line = sr.ReadLine();
                }
            }
        }

        private void BatchLoginProc()
        {
            foreach (string key in this.accountTable.Keys)
            {
                LoginAccount(key);
            }
        }

        private bool IsOwnCity(string name)
        {
            string cityId = cityList[name];
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
            for (int i = 0; i < 4; ++i)
            {
                long toDonate = resNeeds[i] > resHave[i] ? resHave[i] : resNeeds[i];
                yield return toDonate > 10000 ? toDonate : 0;
            }
        }

        private void BatchDonate(int i, ref List<string> accountList)
        {
            foreach (var account in accountList)
            {
                string influenceSciencePage = OpenInfluenceSciencePage(account);
                var influenceRes = ParseInfluenceResource(influenceSciencePage).ToList();
                var resNeeds = influenceRes.Select(resPair => resPair.Value - resPair.Key).ToList();

                this.Invoke(new DoSomething(() =>
                {
                    foreach (ListViewItem lvItem in this.listViewInfluence.Items)
                    {
                        if (lvItem.SubItems[0].Text == account)
                        {
                            for (int j = 1; j < 5; ++j)
                            {
                                lvItem.SubItems[j].Text = influenceRes[j - 1].Key.ToString();
                            }

                            return;
                        }
                    }

                    var newLvItem = new ListViewItem(account);
                    newLvItem.SubItems.AddRange(influenceRes.Select(ar => ar.Key.ToString()).ToArray());
                    this.listViewInfluence.Items.Add(newLvItem);

                }));

                if (resNeeds[3] < 1000000)
                {
                    accountList.Remove(account);
                    continue;
                }

                var accountRes = GetAccountResources(account).ToList();
                while (accountRes[3] < 10000)
                {
                    if (!OpenResourceBox(account))
                    {
                        accountList.Remove(account);
                        break;
                    }

                    accountRes = GetAccountResources(account).ToList();
                }

                var resToContribute = CalculateDonations(resNeeds, accountRes).ToList();

                this.Invoke(new DoSomething(() =>
                {
                    this.txtInfo.Text = string.Format("Donate: {0}:{1}/{2} - {3}", account, accountRes[3], resNeeds[3], i);
                }));

                DonateResource(account, resToContribute[0], resToContribute[1], resToContribute[2], resToContribute[3]);
            }
        }

        private void QuickReliveAllHeroes()
        {
            Parallel.Dispatch(this.accountTable.Values, account =>
            {
                var heroPage = OpenHeroPage(account.UserName);
                var deadHeroList = ParseHeroList(heroPage, account.UserName).Where(hero => hero.IsDead).ToList();
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
            });
        }

        private void StartReliveHeroTimer()
        {
            if (this.reliveHeroTimer != null)
            {
                this.reliveHeroTimer.Start();
                return;
            }

            this.reliveHeroTimer = new System.Timers.Timer(60 * 1000);
            this.reliveHeroTimer.AutoReset = true;
            this.reliveHeroTimer.Elapsed += new System.Timers.ElapsedEventHandler((obj, evn) =>
            {
                Parallel.Dispatch(this.accountTable.Values, account =>
                {
                    var heroPage = OpenHeroPage(account.UserName);
                    if (heroPage.Contains("[[jslang('hero_status_8')]")) // relive running now.
                    {
                        return;
                    }

                    var deadHeroList = ParseHeroList(heroPage, account.UserName).Where(hero => hero.IsDead);
                    if (!deadHeroList.Any())
                    {
                        return;
                    }

                    var toReliveHero = deadHeroList.First();
                    ReliveHero(toReliveHero.HeroId, account.UserName);
                });
            });
        }
    }
}
