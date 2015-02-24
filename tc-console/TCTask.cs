﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TC
{
    abstract class TCTask
    {
        public string AccountName { get; set; }
        public string TaskId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ExecuteTime { get; set; }
        public DateTime EndTime { get; set; }
        public Action<object> TaskAction { get; set; }
        public string GroupKey { get; set; }

        public Func<string, object> GroupAction { get; set; }

        public static string Time2Str(int timeval)
        {
            int secs = timeval % 60;
            int mins = (timeval / 60) % 60;
            int hours = timeval / 3600;
            string fmt = "{0:D2}:{1:D2}:{2:D2}";
            return string.Format(fmt, hours, mins, secs);
        }

        public void SyncToListViewItem(ListViewItem lvItem, DateTime now)
        {
            // lvItem.SubItems.Clear();
            // lvItem.SubItems.Add("Reserved");
            lvItem.SubItems.Add(this.AccountName);
            lvItem.SubItems.Add(EndTime.ToString());
            lvItem.SubItems.Add(ExecuteTime.ToString());

            int totalSeconds = (int)(now - this.ExecuteTime).TotalSeconds;
            string etaString = totalSeconds >= 0 ? totalSeconds.ToString() : "N/A";
            lvItem.SubItems.Add(etaString);
            lvItem.SubItems.Add(this.GetTaskHint());
        }

        public abstract string GetTaskHint();
    }

    class SendTroopTask : TCTask
    {
        private TroopInfo taskData = null;
        private string fromCity = null;
        private string toCity = null;

        public SendTroopTask(string fromCity, string toCity, TroopInfo data)
        {
            this.taskData = data;
            this.fromCity = fromCity;
            this.toCity = toCity;
            this.AccountName = data.AccountName;
            this.TaskId = data.isGroupTroop ? data.GroupId : data.TroopId;
            this.GroupKey = fromCity;
        }

        public override string GetTaskHint()
        {
            return string.Format("{0} => {1}, duration:{2}", this.fromCity, this.toCity, this.taskData.Duration);
        }
    }

    class ReliveHeroTask : TCTask
    {
        public AccountInfo Account = null;

        public string CurrentHeroId = "";

        public ReliveHeroTask(AccountInfo account)
        {
            this.Account = account;
        }

        public override string GetTaskHint()
        {
            return string.Format("Relive {0}", this.CurrentHeroId);
        }
    }
}
