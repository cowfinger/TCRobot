namespace TC
{
    using System;
    using System.Collections.Generic;

    internal class ShipBrickTask : TCTask
    {
        public List<Soldier> SoldierList = new List<Soldier>();

        public List<MoveTroopTask> SubTasks = new List<MoveTroopTask>();

        public ShipBrickTask(AccountInfo account, CityInfo targetCity)
            : base(account, 60 * 1000)
        {
            this.TargetCity = targetCity;
        }

        public CityInfo TargetCity { get; private set; }

        public override string TaskId
        {
            get
            {
                return string.Format("{0}=>{1}", this.Account.UserName, this.TargetCity.Name);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string GetTaskHint()
        {
            return this.TaskId;
        }
    }
}