
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
        private object remoteTimeLock = new object();
        private System.Timers.Timer syncTimeToUITimer = new System.Timers.Timer(500);
        private System.Timers.Timer syncRemoteTimeTimer = new System.Timers.Timer(15 * 1000);
        private DateTime remoteTime = DateTime.MinValue;
        private DateTime remoteTimeLastSync = DateTime.MinValue;

        private object taskTimerLock = new object();
        private DateTime lastTaskTimerWakeup = DateTime.MinValue;
        private System.Timers.Timer taskTimer = null;

        private DateTime lastOnlineTaskRefreshTimerWakeup = DateTime.MinValue;
        private System.Timers.Timer onlineTaskRefreshTimer = null;

        private System.Timers.Timer reliveHeroTimer = null;

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
                    var tasks2 = QueryOnlineTroopList("2", account.UserName).ToList();
                    var tasks4 = QueryOnlineTroopList("4", account.UserName).ToList();

                    var allTasks = tasks2.Concat(tasks4);

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

                    var toExecuteTaskList = new List<TCTask>();
                    var toChangeTaskLvItems = new List<ListViewItem>();

                    foreach (var lvItem in taskLvItemList)
                    {
                        var task = lvItem.Tag as TCTask;

                        if (remoteTimeSnapshot >= task.ExecuteTime)
                        {
                            toExecuteTaskList.Add(task);
                        }
                        else
                        {
                            toChangeTaskLvItems.Add(lvItem);
                        }

                        if (remoteTimeSnapshot >= task.EndTime)
                        {
                            this.Invoke(new DoSomething(() => { this.listViewTasks.Items.Remove(lvItem); }));
                        }
                    }

                    this.Invoke(new DoSomething(() =>
                    {
                        foreach (var lvItem in toChangeTaskLvItems)
                        {
                            var task = lvItem.Tag as TCTask;

                            int timeLeft = (int)((task.ExecuteTime - remoteTimeSnapshot).TotalSeconds);
                            lvItem.SubItems[4].Text = Time2Str(timeLeft);

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
                        Parallel.Dispatch(accountTaskGroups, accountTaskGroup =>
                        {
                            DispatchAccountTasks(accountTaskGroup.Key, accountTaskGroup);
                        });
                    }
                }
            });

            taskTimer.Start();
        }

        private void DispatchAccountTasks(string accountName, IEnumerable<TCTask> accountTaskGroup)
        {
            var subTaskGroups = accountTaskGroup.GroupBy(task => task.GroupKey);

            foreach (var taskGroup in subTaskGroups)
            {
                var groupAction = taskGroup.First().GroupAction;
                var taskGroupPara = groupAction != null ? groupAction(taskGroup.Key) : null;

                Parallel.Dispatch(taskGroup, task =>
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
                            GroupAttackTarget(team.GroupId, team.ToCityNodeId, ref httpClient);
                        }
                        else
                        {
                            // OpenTeamAttackPage(team.TroopId, team.ToCityNodeId, ref httpClient);
                            TeamAttackTarget(team.TroopId, team.ToCityNodeId, ref httpClient);
                        }
                    };

                    task.GroupAction = (groupKey) =>
                    {
                        var httpClient = new HttpClient(GetAccountCookie(team.AccountName));
                        var fromCityId = this.cityList[groupKey];
                        OpenCityPage(fromCityId, ref httpClient); // Open City Page to refresh cookie.
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
                var task = lvItemTask.Tag as SendTroopTask;
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
                        MessageBox.Show(string.Format("复活武将完成:{0}", account.UserName));
                        return;
                    }

                    var toReliveHero = deadHeroList.First();
                    ReliveHero(toReliveHero.HeroId, account.UserName);
                });
            });
            this.reliveHeroTimer.Start();
        }
    }
}
