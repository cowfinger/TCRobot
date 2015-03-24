namespace TC.TCTasks
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

        public RequestAgent WebAgent;

        public DateTime ArrivalTime { get; private set; }

        private static DateTime CalcActualExecutionTime(TroopInfo troop, DateTime arrivalTime)
        {
            var result = arrivalTime.AddSeconds(-(troop.Duration + OpenAttackPageTime));
            if (!troop.isGroupTroop)
            {
                result = result.AddMilliseconds(300);
            }
            if (result <= FormMain.RemoteTime)
            {
                result = FormMain.RemoteTime.AddSeconds(10);
            }
            return result;
        }

        public SendTroopTask(
            AccountInfo account,
            CityInfo fromCity,
            CityInfo toCity,
            TroopInfo data,
            DateTime arrivalTime)
            : base(account, CalcActualExecutionTime(data, arrivalTime))
        {
            this.ArrivalTime = arrivalTime;
            this.TaskData = data;
            this.fromCity = fromCity;
            this.toCity = toCity;
            this.WebAgent = new RequestAgent(account);
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