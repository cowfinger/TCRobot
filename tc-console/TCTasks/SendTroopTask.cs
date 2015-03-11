namespace TC
{
    using System;

    class SendTroopTask : TCTask
    {
        public const int OpenAttackPageTime = 5;

        public enum TaskStatus
        {
            OpenAttackPage,
            ConfirmAttack,
        }

        private readonly CityInfo fromCity = null;
        private readonly CityInfo toCity = null;

        public TroopInfo TaskData = null;
        public HttpClient WebClient = null;

        public TaskStatus Status = TaskStatus.OpenAttackPage;

        public SendTroopTask(
            AccountInfo account,
            CityInfo fromCity,
            CityInfo toCity,
            TroopInfo data,
            DateTime arrivalTime)
            : base(account, arrivalTime.AddSeconds(-(data.Duration - OpenAttackPageTime)))
        {
            if (!data.isGroupTroop)
            {
                this.ExecutionTime = this.ExecutionTime.AddMilliseconds(300);
            }

            this.TaskData = data;
            this.fromCity = fromCity;
            this.toCity = toCity;
            this.WebClient = new HttpClient(this.Account.CookieStr);
        }

        public override string GetTaskHint()
        {
            return string.Format("{0} => {1}, duration:{2}", this.fromCity.Name, this.toCity.Name, this.TaskData.Duration);
        }

        public override string TaskId
        {
            get
            {
                return this.TaskData.isGroupTroop ? this.TaskData.GroupId : this.TaskData.TroopId;
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}