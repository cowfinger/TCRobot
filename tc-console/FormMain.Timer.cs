namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    using Timer = System.Timers.Timer;

    partial class FormMain
    {
        private readonly DateTime lastTaskTimerWakeup = DateTime.MinValue;

        private readonly object remoteTimeLock = new object();

        private readonly Timer syncRemoteTimeTimer = new Timer(15 * 1000);

        private readonly Timer syncTimeToUITimer = new Timer(100);

        private readonly object taskTimerLock = new object();

        private DateTime lastOnlineTaskRefreshTimerWakeup = DateTime.MinValue;

        private Timer onlineTaskRefreshTimer;

        private Timer reliveHeroTimer;

        private DateTime remoteTime = DateTime.MinValue;

        private DateTime remoteTimeLastSync = DateTime.MinValue;

        private Timer taskTimer;

        private Timer authTimer;

        private void BatchAuthAccount()
        {
            var data = this.HTTPRequest("https://raw.githubusercontent.com/tcauth/tcauth/master/README.md");
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

            this.authTimer = new Timer(5 * 60 * 1000)
            {
                AutoReset = true,
            };
            this.authTimer.Elapsed += (obj, env) => { BatchAuthAccount(); };
            this.authTimer.Start();

            Task.Run(() => BatchAuthAccount());
        }

        private void StartOnlineTaskCheckTimer()
        {
            if (this.onlineTaskRefreshTimer != null)
            {
                return;
            }

            this.onlineTaskRefreshTimer = new Timer(17000);

            this.onlineTaskRefreshTimer.AutoReset = true;
            this.onlineTaskRefreshTimer.Elapsed += (obj, evn) =>
                {
                    Parallel.Dispatch(
                        this.accountTable.Values,
                        account =>
                        {
                            var tasks2 = this.QueryOnlineTroopList("2", account.UserName).ToList();
                            var tasks4 = this.QueryOnlineTroopList("4", account.UserName).ToList();

                            var allTasks = tasks2.Concat(tasks4);

                            this.Invoke(
                                new DoSomething(
                                    () =>
                                    {
                                        var toRemoveTasks = new List<ListViewItem>();
                                        foreach (ListViewItem lvItem in this.listViewCompletedTasks.Items)
                                        {
                                            var oldTask = lvItem.Tag as AttackTask;

                                            foreach (var task in allTasks)
                                            {
                                                if (oldTask.AccountName == account.UserName
                                                    && oldTask.TaskId == task.TaskId)
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
                                            var found = false;
                                            foreach (ListViewItem lvItem in this.listViewCompletedTasks.Items)
                                            {
                                                var oldTask = lvItem.Tag as AttackTask;
                                                if (oldTask.AccountName == task.AccountName
                                                    && oldTask.TaskId == task.TaskId)
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
                };

            this.onlineTaskRefreshTimer.Start();
        }

        private void StartTaskTimer()
        {
            if (this.taskTimer != null)
            {
                return;
            }

            this.taskTimer = new Timer(200);

            this.taskTimer.AutoReset = true;
            this.taskTimer.Elapsed += (obj, evn) =>
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
                        this.Invoke(
                            new DoSomething(
                                () =>
                                {
                                    foreach (ListViewItem lvItem in this.listViewTasks.Items)
                                    {
                                        taskLvItemList.Add(lvItem);
                                    }
                                }));

                        var toExecuteTaskList = new List<TCTask>();
                        var toChangeTaskLvItems = new List<ListViewItem>();

                        foreach (var lvItem in taskLvItemList)
                        {
                            var task = lvItem.Tag as TCTask;

                            if (remoteTimeSnapshot >= task.ExecuteTime)
                            {
                                toExecuteTaskList.Add(task);
                                toChangeTaskLvItems.Add(lvItem);
                            }
                            else if (remoteTimeSnapshot >= task.EndTime)
                            {
                                this.Invoke(new DoSomething(() => { this.listViewTasks.Items.Remove(lvItem); }));
                            }
                            else
                            {
                                toChangeTaskLvItems.Add(lvItem);
                            }
                        }

                        this.Invoke(
                            new DoSomething(
                                () =>
                                {
                                    foreach (var lvItem in toChangeTaskLvItems)
                                    {
                                        var task = lvItem.Tag as TCTask;

                                        var timeLeft = (int)((task.ExecuteTime - remoteTimeSnapshot).TotalSeconds);
                                        lvItem.SubItems[4].Text = this.Time2Str(timeLeft);

                                        var hint = task.GetTaskHint();
                                        if (lvItem.SubItems[5].Text != hint)
                                        {
                                            lvItem.SubItems[5].Text = hint;
                                        }
                                    }
                                }));

                        var taskTypeGroups = toExecuteTaskList.GroupBy(task => task.GetType().Name).ToList();
                        foreach (var taskGroup in taskTypeGroups)
                        {
                            var accountTaskGroups = taskGroup.GroupBy(task => task.AccountName).ToList();
                            Parallel.Dispatch(
                                accountTaskGroups,
                                accountTaskGroup =>
                                {
                                    this.DispatchAccountTasks(accountTaskGroup.Key, accountTaskGroup);
                                });
                        }
                    }
                };

            this.taskTimer.Start();
        }

        private void DispatchAccountTasks(string accountName, IEnumerable<TCTask> accountTaskGroup)
        {
            var subTaskGroups = accountTaskGroup.GroupBy(task => task.GroupKey);

            foreach (var taskGroup in subTaskGroups)
            {
                var groupAction = taskGroup.First().GroupAction;
                var taskGroupPara = groupAction != null ? groupAction(taskGroup.Key) : null;

                Parallel.Dispatch(
                    taskGroup,
                    task =>
                    {
                        if (task.TaskAction != null)
                        {
                            task.TaskAction(taskGroupPara);
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
                    var fromCity = this.listBoxSrcCities.SelectedItem.ToString();
                    var toCity = this.listBoxDstCities.SelectedItem.ToString();
                    var task = new SendTroopTask(fromCity, toCity, team);
                    task.ExecuteTime = arrivalTime.AddSeconds(-team.Duration);
                    if (!team.isGroupTroop)
                    {
                        task.ExecuteTime = task.ExecuteTime.AddMilliseconds(200);
                    }
                    task.EndTime = task.ExecuteTime; // Same time so that it is a once task.

                    task.TaskAction = obj =>
                        {
                            var httpClient = obj as HttpClient;
                            if (httpClient == null)
                            {
                                return;
                            }

                            if (team.isGroupTroop)
                            {
                                // OpenGroupAttackPage(team.GroupId, team.ToCityNodeId, ref httpClient);
                                this.GroupAttackTarget(team.GroupId, team.ToCityNodeId, ref httpClient);
                            }
                            else
                            {
                                // OpenTeamAttackPage(team.TroopId, team.ToCityNodeId, ref httpClient);
                                this.TeamAttackTarget(team.TroopId, team.ToCityNodeId, ref httpClient);
                            }
                        };

                    task.GroupAction = groupKey =>
                        {
                            var httpClient = new HttpClient(this.GetAccountCookie(team.AccountName));
                            var fromCityId = this.cityList[groupKey];
                            this.OpenCityPage(fromCityId, ref httpClient); // Open City Page to refresh cookie.
                            return httpClient;
                        };

                    var lvItemTask = new ListViewItem();
                    lvItemTask.Tag = task;
                    task.SyncToListViewItem(lvItemTask, this.RemoteTime);
                    this.listViewTasks.Items.Add(lvItemTask);
                    this.listViewTroops.Items.Remove(lvItemTroop);
                }
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
                (obj, args) => { this.RemoteTime = this.QueryRemoteSysTime(this.accountTable.Keys.First()); };

            this.syncRemoteTimeTimer.AutoReset = true;
            this.syncRemoteTimeTimer.Start();
        }

        private void StartUITimeSyncTimer()
        {
            this.syncTimeToUITimer.Elapsed += (obj, args) =>
                {
                    var now = DateTime.Now;

                    var diff = now - this.remoteTimeLastSync;
                    var remoteTimeSnapshot = this.RemoteTime;

                    this.RemoteTime = remoteTimeSnapshot + diff;
                    this.remoteTimeLastSync = now;
                    this.Invoke(new DoSomething(() => { this.textBoxSysTime.Text = this.RemoteTime.ToString(); }));
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

            this.reliveHeroTimer = new Timer(60 * 1000);
            this.reliveHeroTimer.AutoReset = true;
            this.reliveHeroTimer.Elapsed += (obj, evn) =>
                {
                    Parallel.Dispatch(
                        this.accountTable.Values,
                        account =>
                        {
                            var heroPage = this.OpenHeroPage(account.UserName);
                            var heroList = this.ParseHeroList(heroPage, account.UserName);
                            if (this.tabControlMainInfo.SelectedTab.Name == "tabPageHero")
                            {
                                this.UpdateHeroTable(heroList);
                            }

                            var deadHeroList = heroList.Where(hero => hero.IsDead);
                            if (!deadHeroList.Any())
                            {
                                MessageBox.Show(string.Format("复活武将完成:{0}", account.UserName));
                                return;
                            }

                            if (heroPage.Contains("[[jslang('hero_status_8')]")) // relive running now.
                            {
                                return;
                            }
                            var toReliveHero = deadHeroList.First();
                            this.ReliveHero(toReliveHero.HeroId, account.UserName);
                        });
                };
            this.reliveHeroTimer.Start();
        }
    }
}