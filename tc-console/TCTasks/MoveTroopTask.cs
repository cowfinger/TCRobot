namespace TC.TCTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using TC.TCPage.Influence;
    using TC.TCPage.WorldWar;
    using TC.TCUtility;

    internal class MoveTroopTask : TCTask
    {
        public int BrickNum;

        public List<string> HeroIdList;

        public List<string> Path = null;

        public int RetryCount = 0;

        public List<Soldier> SoldierList;

        public MoveTroopTask(
            AccountInfo account,
            CityInfo fromCity,
            CityInfo toCity,
            List<Soldier> soldierList,
            List<string> heroList,
            int brickNum)
            : base(account, FormMain.RemoteTime.AddMinutes(2))
        {
            var initialHelper = new DijstraHelper(account.InfluenceMap) { Account = account };

            var initialPath = initialHelper.GetPath(fromCity.Name, toCity.Name, soldierList).ToList();
            initialPath.Reverse();
            var nextCity = account.InfluenceCityList[initialPath.First()];

            this.CurrentCity = fromCity;
            this.NextCity = nextCity;
            this.TerminalCity = toCity;
            this.BrickNum = brickNum;
            this.SoldierList = soldierList;
            this.HeroIdList = heroList;
            this.Path = initialPath;
        }

        public CityInfo CurrentCity { get; set; }

        public CityInfo NextCity { get; set; }

        public CityInfo TerminalCity { get; set; }

        public override string TaskId { get; set; }

        public override string GetTaskHint()
        {
            var pathString = this.Path == null ? "" : string.Join("=>", this.Path.ToArray());
            return string.Format("Move Troop: {0}=>{1}", this.CurrentCity.Name, pathString);
        }

        public override void TaskWorker()
        {
            if (!this.HasTroopArrived())
            {
                ++this.RetryCount;
                if (this.RetryCount < 3)
                {
                    this.ExecutionTime = this.ExecutionTime.AddSeconds(10);
                }
                else
                {
                    this.IsCompleted = true;
                }
                return;
            }

            Logger.Verbose("Troop Arrived: {0}.", this.NextCity.Name);

            if (this.NextCity == this.TerminalCity)
            {
                this.IsCompleted = true;
                return;
            }

            var helper = new DijstraHelper(this.Account.InfluenceMap) { Account = this.Account};

            var path = helper.GetPath(this.NextCity.Name, this.TerminalCity.Name, this.SoldierList).ToList();
            path.Reverse();

            this.Path = path;
            this.CurrentCity = this.NextCity;
            this.NextCity = this.Account.InfluenceCityList[path.First()];

            this.MoveTroop();
        }

        public void MoveTroop()
        {
            var cityNodeId = this.CurrentCity.CityId;

            ShowInfluenceCityDetail.Open(this.Account.WebAgent, cityNodeId);
            var moveArmyQueue = ShowMoveArmyQueue.Open(this.Account.WebAgent);

            var heroString = string.Join("%7C", this.HeroIdList.ToArray());
            var soldiers =
                this.SoldierList.Where(s => s.SoldierNumber != 0)
                    .Select(soldier => string.Format("{0}%3A{1}", soldier.SoldierType, soldier.SoldierNumber))
                    .ToArray();
            var soldierString = string.Join("%7c", soldiers);

            DoMoveArmy.Open(
                this.Account.WebAgent,
                this.CurrentCity.NodeId,
                this.NextCity.NodeId,
                soldierString,
                heroString,
                this.BrickNum);

            for (var i = 0; i < 10; ++i)
            {
                Thread.Sleep(2000);
                ShowInfluenceCityDetail.Open(this.Account.WebAgent, this.NextCity.CityId);
                var newMoveArmyQueue = ShowMoveArmyQueue.Open(this.Account.WebAgent);

                if (!newMoveArmyQueue.Items.Any())
                {
                    continue;
                }

                var thisMoveTask =
                    newMoveArmyQueue.Items.Single(
                        taskItem => !moveArmyQueue.Items.Select(item => item.TaskId).Contains(taskItem.TaskId));
                this.TaskId = thisMoveTask.TaskId.ToString();
                this.ExecutionTime = thisMoveTask.Eta.AddSeconds(2);
                Logger.Verbose(
                    "Troop is moving: {0}=>{1}, ETA={2}.",
                    this.CurrentCity.Name,
                    this.NextCity.Name,
                    thisMoveTask.Eta);
                break;
            }
        }
        private bool HasTroopArrived()
        {
            var cityNodeId = this.NextCity.CityId;
            var fromCityId = this.NextCity.NodeId;
            ShowInfluenceCityDetail.Open(this.Account.WebAgent, cityNodeId);
            var moveArmyPage = ShowMoveArmy.Open(this.Account.WebAgent, fromCityId);

            if (this.HeroIdList.Count > 0)
            {
                var heroes = moveArmyPage.HeroList.Select(h => h.HeroId).ToList();
                var heroMatchCount = this.HeroIdList.Sum(hero => heroes.Contains(hero) ? 1 : 0);
                if (heroMatchCount != this.HeroIdList.Count)
                {
                    return false;
                }
            }

            if (this.SoldierList.Count > 0)
            {
                var troop = moveArmyPage.Army.ToList();
                foreach (var soldier in this.SoldierList)
                {
                    if (soldier.SoldierNumber == 0)
                    {
                        continue;
                    }

                    var inCitySoldier = troop.Single(s => s.SoldierType == soldier.SoldierType);
                    if (inCitySoldier.SoldierNumber < soldier.SoldierNumber)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}