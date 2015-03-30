namespace TC.TCTasks
{
    using System;

    using TC.TCPage.Influence;
    using TC.TCPage.WorldWar;
    using TC.TCUtility;

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

        public DateTime ArrivalTime { get; private set; }

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

        public override string GetTaskHint()
        {
            return string.Format(
                "{0} => {1}, duration:{2}",
                this.fromCity.Name,
                this.toCity.Name,
                this.TaskData.Duration);
        }

        public override void TaskWorker()
        {
            switch (this.Status)
            {
                case TaskStatus.OpenAttackPage:
                    this.Status = TaskStatus.ConfirmAttack;
                    var requestPerfTimer = DateTime.Now;
                    ShowInfluenceCityDetail.Open(this.WebAgent, this.fromCity.CityId);
                    var cost = DateTime.Now - requestPerfTimer;
                    var attackTime = this.ArrivalTime.AddSeconds(-this.TaskData.Duration);
                    attackTime = attackTime.AddMilliseconds(-(cost.TotalMilliseconds / 2));
                    this.ExecutionTime = attackTime;

                    Logger.Verbose(
                        "Troop(Id={0},isGroup={1}) OpenCityPage(Elapse={2}ms), AttackTime={3}={4}-{5}.",
                        this.TaskData.isGroupTroop ? this.TaskData.GroupId : this.TaskData.TroopId,
                        this.TaskData.isGroupTroop,
                        cost.TotalMilliseconds,
                        attackTime,
                        this.ArrivalTime,
                        this.TaskData.Duration);
                    break;

                case TaskStatus.ConfirmAttack:
                    string result;
                    if (this.TaskData.isGroupTroop)
                    {
                        result =
                            DoJoinAttack.Open(
                                this.WebAgent,
                                int.Parse(this.TaskData.GroupId),
                                int.Parse(this.TaskData.ToCityNodeId)).RawPage;
                    }
                    else
                    {
                        result =
                            DoAttack.Open(
                                this.WebAgent,
                                int.Parse(this.TaskData.TroopId),
                                int.Parse(this.TaskData.ToCityNodeId)).RawPage;
                    }

                    Logger.Verbose(
                        "Troop(Id={0}, isGroup={1}) Sent, result={2}.",
                        this.TaskData.isGroupTroop ? this.TaskData.GroupId : this.TaskData.TroopId,
                        this.TaskData.isGroupTroop,
                        result);
                    this.IsCompleted = true;
                    break;
            }
        }
    }
}