namespace TC.TCTasks
{
    using System.Collections.Generic;

    internal class MoveTroopTask : TCTask
    {
        public int BrickNum;

        public List<string> HeroIdList = new List<string>();

        public List<string> Path = null;

        public int RetryCount = 0;

        public List<Soldier> SoldierList = new List<Soldier>();

        private string taskId;

        public MoveTroopTask(
            AccountInfo account,
            CityInfo from,
            CityInfo next,
            CityInfo terminal,
            int brickNum,
            string taskId)
            : base(account, FormMain.RemoteTime.AddMinutes(2))
        {
            this.CurrentCity = from;
            this.NextCity = next;
            this.TerminalCity = terminal;
            this.BrickNum = brickNum;
            this.taskId = taskId;
        }

        public CityInfo CurrentCity { get; set; }

        public CityInfo NextCity { get; set; }

        public CityInfo TerminalCity { get; set; }

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

        public override string GetTaskHint()
        {
            var pathString = this.Path == null ? "" : string.Join("=>", this.Path.ToArray());
            return string.Format("Move Troop: {0}=>{1}", this.CurrentCity.Name, pathString);
        }
    }
}