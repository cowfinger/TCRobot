using System;

namespace TC.TCTasks
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using TC.TCPage.Influence;
    using TC.TCPage.WorldWar;

    internal class MoveTroopTask : TCTask
    {
        public int BrickNum;

        public List<string> HeroIdList;

        public List<string> Path = null;

        public int RetryCount = 0;

        public List<Soldier> SoldierList;

        public bool CanMove { get; private set; }

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

            this.CanMove = initialPath.Any();
            if (!this.CanMove)
            {
                this.Stop();
                return;
            }

            initialPath.Reverse();
            var nextCity = account.InfluenceCityList[initialPath.First().Name];

            this.CurrentCity = fromCity;
            this.NextCity = nextCity;
            this.TerminalCity = toCity;
            this.BrickNum = brickNum;
            this.SoldierList = soldierList;
            this.HeroIdList = heroList;
            this.Path = initialPath.Select(city => city.Name).ToList();
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

            this.Verbose("Troop Arrived: {0}.", this.NextCity.Name);

            if (this.NextCity == this.TerminalCity)
            {
                this.Stop();
                return;
            }

            var helper = new DijstraHelper(this.Account.InfluenceMap) { Account = this.Account };

            var path = helper.GetPath(
                this.NextCity.Name, this.TerminalCity.Name, this.SoldierList).ToList();

            path.Reverse();

            var distance = path.Sum(n => n.Distance);

            this.Path = path.Select(n => n.Name).ToList();
            this.CurrentCity = this.NextCity;
            this.NextCity = this.Account.InfluenceCityList[path.First().Name];

            this.Verbose("ETA: {0} mins.", distance);

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

                var thisMoveTasks =
                    newMoveArmyQueue.Items.Where(
                        taskItem => !moveArmyQueue.Items.Select(item => item.TaskId).Contains(taskItem.TaskId)).ToList();

                if (!thisMoveTasks.Any())
                {
                    continue;
                }

                var thisMoveTask = thisMoveTasks.First();
                this.TaskId = thisMoveTask.TaskId.ToString();
                this.ExecutionTime = thisMoveTask.Eta.AddSeconds(2);
                this.Verbose(
                    "Troop is moving: {0}=>{1}=>...=>{2}, ETA={3}.",
                    this.CurrentCity.Name,
                    this.NextCity.Name,
                    this.TerminalCity.Name,
                    thisMoveTask.Eta);
                break;
            }
        }

        private bool HasTroopArrived()
        {
            if (this.NextCity == null)
            {
                this.Stop();
                return false;
            }

            try
            {
                var cityNodeId = this.NextCity.CityId;
                var fromCityId = this.NextCity.NodeId;
                ShowInfluenceCityDetail.Open(this.Account.WebAgent, cityNodeId);
                var moveArmyPage = ShowMoveArmy.Open(this.Account.WebAgent, fromCityId);

                if (this.HeroIdList.Count > 0)
                {
                    var heroes = moveArmyPage.HeroList.Select(h => h.HeroId).ToList();
                    var heroMatchCount = this.HeroIdList.Sum(hero => heroes.Contains(int.Parse(hero)) ? 1 : 0);
                    if (heroMatchCount != this.HeroIdList.Count)
                    {
                        return false;
                    }
                }

                if (this.SoldierList.Count <= 0)
                {
                    return true;
                }
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

                return true;

            }
            catch (Exception e)
            {
                this.Verbose("HasTroopArrived Exception:{0}", e.Message);
                return false;
            }
        }
    }
}