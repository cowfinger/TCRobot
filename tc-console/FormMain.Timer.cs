﻿namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Forms;

    using Timer = System.Timers.Timer;

    partial class FormMain
    {
        private readonly DateTime lastTaskTimerWakeup = DateTime.MinValue;

        private static object remoteTimeLock = new object();

        private static DateTime remoteTime = DateTime.MinValue;

        private readonly Timer syncRemoteTimeTimer = new Timer(15 * 1000);

        private readonly Timer syncTimeToUITimer = new Timer(100);

        private readonly object taskTimerLock = new object();

        private DateTime lastOnlineTaskRefreshTimerWakeup = DateTime.MinValue;

        private Timer onlineTaskRefreshTimer;

        private Timer reliveHeroTimer;

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
                            var tasks1 = this.QueryOnlineTroopList("1", account.UserName).ToList();
                            var tasks2 = this.QueryOnlineTroopList("2", account.UserName).ToList();
                            var tasks4 = this.QueryOnlineTroopList("4", account.UserName).ToList();

                            var allTasks = tasks1.Concat(tasks2).Concat(tasks4).ToList();

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
                        var remoteTimeSnapshot = RemoteTime;

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
                                    var toRemoveList = new List<ListViewItem>();
                                    foreach (ListViewItem lvItem in this.listViewTasks.Items)
                                    {
                                        var task = lvItem.Tag as TCTask;

                                        var timeLeft = (int)((task.ExecutionTime - remoteTimeSnapshot).TotalSeconds);
                                        lvItem.SubItems[4].Text = this.Time2Str(timeLeft);

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

                                    foreach (ListViewItem lvItem in toRemoveList)
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

                if (!team.isDefendTroop && ((team.isGroupTroop && team.IsGroupHead) || !team.isGroupTroop))
                {
                    var account = this.accountTable[team.AccountName];
                    var fromCityName = this.listBoxSrcCities.SelectedItem.ToString();
                    var toCityName = this.listBoxDstCities.SelectedItem.ToString();

                    var fromCity = account.InfluenceCityList[fromCityName];
                    var toCity = new CityInfo()
                    {
                        Name = toCityName,
                        CityId = int.Parse(this.cityList[toCityName]),
                        NodeId = int.Parse(team.ToCityNodeId),
                    };

                    var task = new SendTroopTask(account, fromCity, toCity, team, arrivalTime);

                    task.TaskAction = obj =>
                        {
                            switch (task.Status)
                            {
                                case SendTroopTask.TaskStatus.OpenAttackPage:
                                    var requestPerfTimer = DateTime.Now;
                                    this.OpenCityPage(team.ToCityNodeId, ref task.webClient);
                                    var cost = DateTime.Now - requestPerfTimer;
                                    var attackTime = task.ExecutionTime.AddSeconds(SendTroopTask.OpenAttackPageTime);
                                    task.ExecutionTime = attackTime.AddMilliseconds(-(cost.TotalMilliseconds / 2));
                                    task.Status = SendTroopTask.TaskStatus.ConfirmAttack;
                                    break;

                                case SendTroopTask.TaskStatus.ConfirmAttack:
                                    if (team.isGroupTroop)
                                    {
                                        this.GroupAttackTarget(team.GroupId, team.ToCityNodeId, ref task.webClient);
                                    }
                                    else
                                    {
                                        this.TeamAttackTarget(team.TroopId, team.ToCityNodeId, ref task.webClient);
                                    }
                                    task.IsCompleted = true;
                                    break;
                            }
                        };

                    var lvItemTask = new ListViewItem();
                    lvItemTask.Tag = task;
                    task.SyncToListViewItem(lvItemTask, timeSnapshot);
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
                    this.Invoke(new DoSomething(() => { this.textBoxSysTime.Text = RemoteTime.ToString(); }));
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