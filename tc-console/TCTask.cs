using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC
{
    abstract class TCTask
    {
        public string AccountName { get; set; }
        public string TaskId { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime ExecuteTime { get; set; }
        public DateTime EndTime { get; set; }
        public Action TaskAction { get; set; }

        public void ExecuteTask()
        {
            if (this.TaskAction != null)
            {
                this.TaskAction();
            }
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
        }

        public override string GetTaskHint()
        {
            return string.Format("{0} => {1}, duration:{2}", this.fromCity, this.toCity, this.taskData.Duration);
        }
    }

    class ReliveHeroTask : TCTask
    {
        private AccountInfo Account = null;

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
