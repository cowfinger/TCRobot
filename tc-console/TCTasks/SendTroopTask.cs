namespace TC
{
    using System;

    internal class SendTroopTask : TCTask
    {
        public enum TaskStatus
        {
            OpenAttackPage,

            ConfirmAttack
        }

        public const int OpenAttackPageTime = 5;

        private readonly CityInfo fromCity;

        private readonly CityInfo toCity;

        public TaskStatus Status = TaskStatus.OpenAttackPage;

        public TroopInfo TaskData;

        public HttpClient WebClient;

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

        public override string GetTaskHint()
        {
            return string.Format(
                "{0} => {1}, duration:{2}",
                this.fromCity.Name,
                this.toCity.Name,
                this.TaskData.Duration);
        }
    }
}