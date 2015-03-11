namespace TC
{
    using System.Collections.Generic;

    class MoveTroopTask : TCTask
    {
        public CityInfo CurrentCity {get; set;}
        public CityInfo NextCity {get; set;}
        public CityInfo TerminalCity {get;set;}
        public List<string> HeroIdList = new List<string>();
        public List<Soldier> SoldierList = new List<Soldier>();
        public List<string> Path = null;
        public int BrickNum = 0;
        public int RetryCount = 0;
        private string taskId = "";

        public MoveTroopTask(
            AccountInfo account,
            CityInfo from,
            CityInfo next,
            CityInfo terminal,
            int BrickNum,
            string taskId)
            : base(account, FormMain.RemoteTime.AddMinutes(2))
        {
            this.CurrentCity = from;
            this.NextCity = next;
            this.TerminalCity = terminal;
            this.BrickNum = BrickNum;
            this.taskId = taskId;
        }

        public override string GetTaskHint()
        {
            var pathString = this.Path == null ? "" : string.Join("=>", this.Path.ToArray());
            return string.Format("Move Troop: {0}=>{1}", this.CurrentCity.Name, pathString);
        }

        public override string TaskId
        {
            get
            {
                return this.taskId;
            }
            set
            {
                this.taskId = value;
            }
        }
    }
}