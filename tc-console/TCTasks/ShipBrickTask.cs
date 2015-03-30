namespace TC.TCTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using TC.TCPage.Influence;
    using TC.TCPage.WorldWar;
    using TC.TCUtility;

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

        public override void TaskWorker()
        {
            var account = this.Account;
            var homeCity = account.InfluenceCityList.Values.First(city => city.CityId == 0);
            var targetCity = this.TargetCity;

            RepairCityWall(this.TargetCity.CityId, this.Account);

            var completedTasks = this.SubTasks.Where(t => t.IsCompleted).ToList();
            if (completedTasks.Any())
            {
                foreach (var subTask in completedTasks)
                {
                    this.SubTasks.Remove(subTask);
                }

                var moveTroopTasks = completedTasks.Where(t => t.TerminalCity.Name != targetCity.Name);
                var newMoveBrickTasks =
                    moveTroopTasks.Select(
                        t => this.ShipBrickCreateMoveBrickTask(account, t.TerminalCity, this.TargetCity))
                        .Where(t => t != null)
                        .ToList();

                this.SubTasks.AddRange(newMoveBrickTasks);

                var moveBrickTasks = completedTasks.Where(t => t.TerminalCity.Name == targetCity.Name);
                if (moveBrickTasks.Any())
                {
                    this.SubTasks.Add(this.ShipBrickCreateMoveTroopTask(account, targetCity, homeCity));
                }
            }
            else
            {
                // Search bricks and move troop.
                var newTasks = account.CityNameList.Select(
                    cityName =>
                    {
                        var cityInfo = account.InfluenceCityList[cityName];

                        if (cityInfo.Name == targetCity.Name)
                        {
                            return this.ShipBrickCreateMoveTroopTask(account, targetCity, homeCity);
                        }

                        if (cityInfo.Name == homeCity.Name)
                        {
                            return this.ShipBrickCreateMoveBrickTask(account, homeCity, targetCity);
                        }

                        var moveArmyPage = ShowMoveArmy.Open(account.WebAgent, cityInfo.NodeId);
                        var brickNum = moveArmyPage.BrickNum;

                        return brickNum == 0
                                   ? this.ShipBrickCreateMoveTroopTask(account, cityInfo, homeCity)
                                   : this.ShipBrickCreateMoveBrickTask(account, cityInfo, targetCity);
                    }).Where(t => t != null).ToList();
                this.SubTasks.AddRange(newTasks);
            }
        }
        private MoveTroopTask ShipBrickCreateMoveTroopTask(AccountInfo account, CityInfo fromCity, CityInfo toCity)
        {
            var moveArmyPage = ShowMoveArmy.Open(account.WebAgent, fromCity.NodeId);
            var troop = moveArmyPage.Army.ToList();

            return new MoveTroopTask(this.Account, fromCity, toCity, troop, new List<string>(), 0);
        }

        private static int CalcCarryBrickNum(IEnumerable<Soldier> army)
        {
            return army.Sum(a => FormMain.SoldierTable[a.SoldierType].Capacity * a.SoldierNumber) / 50000;
        }

        private IEnumerable<Soldier> CalcCarryTroop(IEnumerable<Soldier> army, int brickNum)
        {
            var soldierList = army.ToList();
            soldierList.Sort(
                (x, y) =>
                    {
                        var indexX = FormMain.SoldierTable[x.SoldierType].Capacity * FormMain.SoldierTable[x.SoldierType].Speed;
                        var indexY = FormMain.SoldierTable[y.SoldierType].Capacity * FormMain.SoldierTable[y.SoldierType].Speed;
                        return indexX.CompareTo(indexY);
                    });
            soldierList.Reverse();

            var totalCap = brickNum * 50000;
            var costCap = 0;
            var resultList = new List<Soldier>();
            foreach (var soldier in soldierList)
            {
                if (costCap >= totalCap)
                {
                    break;
                }

                var cap = soldier.SoldierNumber * FormMain.SoldierTable[soldier.SoldierType].Capacity;
                if (cap > 0)
                {
                    resultList.Add(soldier);
                    costCap += cap;
                }
            }

            return resultList;
        }

        private MoveTroopTask ShipBrickCreateMoveBrickTask(AccountInfo account, CityInfo fromCity, CityInfo toCity)
        {
            var moveArmyPage = ShowMoveArmy.Open(account.WebAgent, fromCity.NodeId);
            var soldiers = moveArmyPage.Army.ToList();
            var brickNum = moveArmyPage.BrickNum;
            var carryBrickNum = Math.Min(CalcCarryBrickNum(soldiers), brickNum);
            var troop = this.CalcCarryTroop(soldiers, carryBrickNum).ToList();
            if (carryBrickNum == 0)
            {
                Logger.Verbose("Move Brick {0}=>{1}: Canceld: No Brick.", fromCity.Name, toCity.Name);
                return null;
            }

            Logger.Verbose("Move Brick {0}=>{1}: Task Created {2} Bricks.", fromCity.Name, toCity.Name, carryBrickNum);
            return new MoveTroopTask(this.Account, fromCity, toCity, soldiers, new List<string>(), carryBrickNum);
        }

        private static bool RepairCityWall(int cityId, AccountInfo account)
        {
            ShowInfluenceCityDetail.Open(account.WebAgent, cityId);

            var cityBuildPage = ShowCityBuild.Open(account.WebAgent, cityId);

            var cityRepairWallPage = ShowInfluenceBuild.Open(
                account.WebAgent,
                cityId,
                (int)CityBuildId.Wall,
                cityBuildPage.Wall.Level);

            if (cityRepairWallPage.CompleteRepairNeeds == 0)
            {
                Logger.Verbose("Repair Wall at {0} canceled: No Need.", cityBuildPage.CityName);
                return false;
            }

            var brickNumToUse = Math.Min(cityRepairWallPage.BrickNum, cityRepairWallPage.CompleteRepairNeeds);

            if (brickNumToUse > 0)
            {
                DoBuildRepair.Open(
                    account.WebAgent,
                    cityRepairWallPage.CityNodeId,
                    cityRepairWallPage.BuildId,
                    brickNumToUse);
                Logger.Verbose("Repair Wall at {0} Ok", cityBuildPage.CityName);
            }
            else
            {
                Logger.Verbose("Repair Wall at {0} canceled: No Brick.", cityBuildPage.CityName);
            }

            return true;
        }
    }
}