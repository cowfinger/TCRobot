namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    using TC.TCPage.Influence;
    using TC.TCPage.WorldWar;
    using TC.TCTasks;
    using TC.TCUtility;

    using Timer = System.Timers.Timer;

    partial class FormMain
    {
        private static readonly object RemoteTimeLock = new object();

        private static DateTime remoteTime = DateTime.MinValue;

        private readonly DateTime lastTaskTimerWakeup = DateTime.MinValue;

        private readonly Timer syncRemoteTimeTimer = new Timer(15 * 1000);

        private readonly Timer syncTimeToUITimer = new Timer(100);

        private readonly object taskTimerLock = new object();

        private Timer authTimer;

        private DateTime lastOnlineTaskRefreshTimerWakeup = DateTime.MinValue;

        private Timer onlineTaskRefreshTimer;

        private Timer reliveHeroTimer;

        private DateTime remoteTimeLastSync = DateTime.MinValue;

        private Timer taskTimer;

        private void BatchAuthAccount()
        {
            var data = HTTPRequest("https://raw.githubusercontent.com/tcauth/tcauth/master/README.md");
            var lines = data.Split(',').ToLookup(val => int.Parse(val));

            foreach (var account in this.accountTable.Values)
            {
                if (!lines.Contains(account.UnionId))
                {
                    this.Invoke(new DoSomething(this.Close));
                    throw new NullReferenceException();
                }
            }
        }

        private void StartAuthTimer()
        {
            if (this.authTimer != null)
            {
                return;
            }

            this.authTimer = new Timer(5 * 60 * 1000) { AutoReset = true };
            this.authTimer.Elapsed += (obj, env) => { this.BatchAuthAccount(); };
            this.authTimer.Start();

            Task.Run(this.BatchAuthAccount);
        }

        private void StartOnlineTaskCheckTimer()
        {
            if (this.onlineTaskRefreshTimer != null)
            {
                return;
            }

            this.onlineTaskRefreshTimer = new Timer(17000) { AutoReset = true };

            this.onlineTaskRefreshTimer.Elapsed +=
                (obj, evn) => { Parallel.Dispatch(this.accountTable.Values, this.RefreshAccountOnlineTasks); };

            this.onlineTaskRefreshTimer.Start();
        }

        private void RefreshAccountOnlineTasks(AccountInfo account)
        {
            var allTasks =
                new[] { "1", "2", "4" }.SelectMany(s => this.QueryOnlineTroopList(s, account.UserName)).ToList();
            this.SyncOnlineTaskListToUI(allTasks);
        }

        private void SyncOnlineTaskListToUI(List<AttackTask> allTasks)
        {
            this.Invoke(
                new DoSomething(
                    () =>
                        {
                            var toRemoveTasks = new List<ListViewItem>();
                            foreach (ListViewItem lvItem in this.listViewCompletedTasks.Items)
                            {
                                var oldTask = lvItem.Tag as AttackTask;

                                if (allTasks.Find(task => task.TaskId == oldTask.TaskId) == null)
                                {
                                    toRemoveTasks.Add(lvItem);
                                }
                            }

                            foreach (var item in toRemoveTasks)
                            {
                                this.listViewCompletedTasks.Items.Remove(item);
                            }

                            foreach (var task in allTasks)
                            {
                                var found = false;
                                foreach (ListViewItem lvItem in this.listViewCompletedTasks.Items)
                                {
                                    var oldTask = lvItem.Tag as AttackTask;
                                    if (oldTask.TaskId == task.TaskId)
                                    {
                                        found = true;
                                        break;
                                    }
                                }

                                if (found)
                                {
                                    continue;
                                }

                                var newLvItem = new ListViewItem { Tag = task };
                                newLvItem.SubItems[0].Text = task.AccountName;
                                newLvItem.SubItems.Add(task.FromCity);
                                newLvItem.SubItems.Add(task.ToCity);
                                newLvItem.SubItems.Add(task.EndTime.ToString());
                                newLvItem.SubItems.Add(task.TaskId);
                                newLvItem.SubItems.Add(task.TaskType);
                                this.listViewCompletedTasks.Items.Add(newLvItem);
                            }
                        }));
        }

        private void StartTaskTimer()
        {
            if (this.taskTimer != null)
            {
                return;
            }

            this.taskTimer = new Timer(200) { AutoReset = true };

            this.taskTimer.Elapsed += (obj, evn) =>
                {
                    lock (this.taskTimerLock)
                    {
                        var remoteTimeSnapshot = RemoteTime;

                        var diff = remoteTimeSnapshot - this.lastTaskTimerWakeup;
                        if (diff.TotalSeconds < 1.0)
                        {
                            return;
                        }

                        this.Invoke(
                            new DoSomething(
                                () =>
                                    {
                                        var toRemoveList = new List<ListViewItem>();
                                        foreach (ListViewItem lvItem in this.listViewTasks.Items)
                                        {
                                            var task = lvItem.Tag as TCTask;
                                            if (task == null)
                                            {
                                                continue;
                                            }

                                            var timeLeft = (int)((task.ExecutionTime - remoteTimeSnapshot).TotalSeconds);
                                            lvItem.SubItems[4].Text = Time2Str(timeLeft);

                                            var hint = task.GetTaskHint();
                                            if (lvItem.SubItems[5].Text != hint)
                                            {
                                                lvItem.SubItems[5].Text = hint;
                                            }

                                            if (task.IsCompleted)
                                            {
                                                toRemoveList.Add(lvItem);
                                            }
                                        }

                                        foreach (var lvItem in toRemoveList)
                                        {
                                            this.listViewTasks.Items.Remove(lvItem);
                                        }
                                    }));
                    }
                };

            this.taskTimer.Start();
        }

        private void StartSendTroopTasks()
        {
            var timeSnapshot = RemoteTime;
            var arrivalTime = this.dateTimePickerArrival.Value;
            foreach (ListViewItem lvItemTroop in this.listViewTroops.CheckedItems)
            {
                var team = lvItemTroop.Tag as TroopInfo;
                if (team == null)
                {
                    continue;
                }

                if (team.isDefendTroop || ((!team.isGroupTroop || !team.IsGroupHead) && team.isGroupTroop))
                {
                    continue;
                }

                var account = this.accountTable[team.AccountName];
                var fromCityName = this.listBoxSrcCities.SelectedItem.ToString();
                var toCityName = this.listBoxDstCities.SelectedItem.ToString();

                var fromCity = account.InfluenceCityList[fromCityName];
                var toCity = new CityInfo
                                 {
                                     Name = toCityName,
                                     CityId = int.Parse(this.cityList[toCityName]),
                                     NodeId = int.Parse(team.ToCityNodeId)
                                 };

                var task = new SendTroopTask(account, fromCity, toCity, team, arrivalTime);

                var lvItemTask = new ListViewItem { Tag = task };
                task.SyncToListViewItem(lvItemTask, timeSnapshot);
                this.listViewTasks.Items.Add(lvItemTask);
                this.listViewTroops.Items.Remove(lvItemTroop);
            }
        }

        private void StopSendTroopTasks()
        {
            foreach (ListViewItem lvItemTask in this.listViewTasks.CheckedItems)
            {
                this.listViewTasks.Items.Remove(lvItemTask);
            }
        }

        private void StartRemoteTimeSyncTimer()
        {
            this.syncRemoteTimeTimer.Elapsed +=
                (obj, args) => { RemoteTime = this.QueryRemoteSysTime(this.accountTable.Keys.First()); };

            this.syncRemoteTimeTimer.AutoReset = true;
            this.syncRemoteTimeTimer.Start();
        }

        private void StartUITimeSyncTimer()
        {
            this.syncTimeToUITimer.Elapsed += (obj, args) =>
                {
                    var now = DateTime.Now;

                    var diff = now - this.remoteTimeLastSync;
                    var remoteTimeSnapshot = RemoteTime;

                    RemoteTime = remoteTimeSnapshot + diff;
                    this.remoteTimeLastSync = now;
                    try
                    {
                        this.Invoke(new DoSomething(() => { this.textBoxSysTime.Text = RemoteTime.ToString(); }));
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                };

            this.syncTimeToUITimer.AutoReset = true;
            this.syncTimeToUITimer.Start();
        }

        private void StartReliveHeroTimer()
        {
            if (this.reliveHeroTimer != null)
            {
                this.reliveHeroTimer.Start();
                return;
            }

            this.reliveHeroTimer = new Timer(60*1000) {AutoReset = true};
            this.reliveHeroTimer.Elapsed += (obj, evn) =>
                {
                    Parallel.Dispatch(
                        this.accountTable.Values,
                        account =>
                            {
                                var heroPage = TCPage.Hero.ShowMyHeroes.Open(account.WebAgent);
                                var heroList = heroPage.Heroes.ToList();
                                if (this.tabControlMainInfo.SelectedTab.Name == "tabPageHero")
                                {
                                    this.UpdateHeroTable(heroList);
                                }

                                var deadHeroList = heroList.Where(hero => hero.IsDead).ToList();
                                if (!deadHeroList.Any())
                                {
                                    MessageBox.Show(string.Format("复活武将完成:{0}", account.UserName));
                                    return;
                                }

                                if (heroList.Any(hero => hero.Status == 8)) // relive running now.
                                {
                                    return;
                                }
                                var toReliveHero = deadHeroList.First();
                                TCPage.Hero.DoReliveHero.Open(account.WebAgent, toReliveHero.HeroId);
                            });
                };
            this.reliveHeroTimer.Start();
        }
    }
}