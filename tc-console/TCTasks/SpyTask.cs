namespace TC.TCTasks
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    using TC.TCPage.Influence;
    using TC.TCPage.WorldWar;
    using TC.TCUtility;

    internal class SpyTask : TCTask
    {
        private const int CheckInterval = 2 * 1000;

        private readonly object enemyCityInfoListLock = new object();

        private readonly Random randGen = new Random();

        public int Counter = 0;

        private List<CityInfo> enemyCityInfoList = new List<CityInfo>();

        public Dictionary<string, CityMilitaryInfo> EnemyCityList = new Dictionary<string, CityMilitaryInfo>();

        private Func<IEnumerable<CityInfo>> targetCitysReader = null;

        private Action<CityMilitaryInfo, string> syncCityInfoToUi = null;

        public SpyTask(
            AccountInfo account,
            Func<IEnumerable<CityInfo>> targetCitysReader,
            Action<CityMilitaryInfo, string> syncCityInfoToUi)
            : base(account, CheckInterval)
        {
            this.targetCitysReader = targetCitysReader;
            this.syncCityInfoToUi = syncCityInfoToUi;
        }

        public List<CityInfo> EnemyCityInfoList
        {
            get
            {
                lock (this.enemyCityInfoListLock)
                {
                    return this.enemyCityInfoList;
                }
            }
            set
            {
                lock (this.enemyCityInfoListLock)
                {
                    this.enemyCityInfoList = value;
                }
            }
        }

        public override string TaskId
        {
            get
            {
                return "spy";
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override string GetTaskHint()
        {
            return "spy";
        }

        public override void TaskWorker()
        {
            var focusList = this.targetCitysReader();
            DoApplyInfluence.Open(this.Account.WebAgent, 9);

            List<CityInfo> cityInfoList;
            if (!this.EnemyCityInfoList.Any())
            {
                cityInfoList = QueryInfluenceCityList(this.Account).ToList();
                this.EnemyCityInfoList = cityInfoList;
            }
            else
            {
                cityInfoList = this.EnemyCityInfoList;
            }

            var toHandleList = focusList.Any()
                                   ? focusList
                                   : (from t in cityInfoList where this.randGen.NextDouble() > 0.2 select t)
                                         .ToList();

            var cityMilitaryInfoList = ParallelScanEnemyCityList(this.Account,
                toHandleList,
                this.EnemyCityList,
                this.Counter);

            ++this.Counter;

            DoCancelApplyInfluence.Open(this.Account.WebAgent, 9);

            foreach (var city in cityMilitaryInfoList)
            {
                var log = BuildCityMilitaryInfoString(city);
                this.syncCityInfoToUi(city, log);
                if (!string.IsNullOrEmpty(city.Name) && !this.EnemyCityList.ContainsKey(city.Name))
                {
                    this.EnemyCityList.Add(city.Name, city);
                }
            }
        }
        private static IEnumerable<CityMilitaryInfo> ParallelScanEnemyCityList(
            AccountInfo account,
            IEnumerable<CityInfo> cityIdList,
            IDictionary<string, CityMilitaryInfo> oldCityTable,
            int counter)
        {
            var cityMilitaryInfoList = new List<CityMilitaryInfo>();
            Parallel.Dispatch(
                cityIdList,
                city =>
                    {
                        if (string.IsNullOrEmpty(city.Name))
                        {
                            return;
                        }

                        CityMilitaryInfo oldCity;
                        CityMilitaryInfo newCity;
                        RequestAgent webAgent;
                        if (oldCityTable.TryGetValue(city.Name, out oldCity))
                        {
                            webAgent = oldCity.WebAgent;
                            var cityDetail = ShowInfluenceCityDetail.Open(webAgent, city.CityId);
                            oldCity.WallEndure = cityDetail.WallEndure;
                            oldCity.MaxWallEndure = cityDetail.MaxWallEndure;
                            oldCity.FortressEndure = cityDetail.FortressEndure;
                            oldCity.MaxFortressEndure = cityDetail.MaxFortressEndure;

                            if (oldCity.TotalArmy == 0 || oldCity.TotalHeroNum == 0 || counter - oldCity.Counter > 10)
                            {
                                var reserveArmyPage = ShowReserveArmyInfo.Open(webAgent, 1);
                                oldCity.TotalArmy = reserveArmyPage.ReserveArmyNum;
                                oldCity.TotalHeroNum = reserveArmyPage.ReserveHeroNum;
                            }

                            if (!oldCity.AttackTroops.Any() || counter - oldCity.Counter > 10)
                            {
                                var attackTeamPage = ShowTeam.Open(webAgent, 3);
                                oldCity.AttackTroops =
                                    attackTeamPage.TeamList.Select(
                                        team =>
                                        new CityTroopInfo
                                            {
                                                HeroNum = team.HeroNum,
                                                AttackPower = team.AttackPower,
                                                DefendPower = team.DefendPower
                                            }).ToList();
                            }

                            if (!oldCity.DefendTroops.Any() || counter - oldCity.Counter > 10)
                            {
                                var defendTeamPage = ShowTeam.Open(webAgent, 4);
                                oldCity.DefendTroops =
                                    defendTeamPage.TeamList.Select(
                                        team =>
                                        new CityTroopInfo
                                            {
                                                HeroNum = team.HeroNum,
                                                AttackPower = team.AttackPower,
                                                DefendPower = team.DefendPower
                                            }).ToList();
                            }
                            oldCity.Counter = counter;

                            newCity = oldCity;
                        }
                        else
                        {
                            webAgent = new RequestAgent(account);
                            var cityDetail = ShowInfluenceCityDetail.Open(webAgent, city.CityId);
                            var reserveArmyPage = ShowReserveArmyInfo.Open(webAgent, 1);
                            var attackTeamPage = ShowTeam.Open(webAgent, 3);
                            var defendTeamPage = ShowTeam.Open(webAgent, 4);

                            Logger.Verbose(
                                "Inspect{0}:Fortress:{1},Wall:{2};ReserveArmy:{3},ReserveHero:{4}",
                                cityDetail.CityName,
                                cityDetail.FortressEndure,
                                cityDetail.WallEndure,
                                reserveArmyPage.ReserveArmyNum,
                                reserveArmyPage.ReserveHeroNum);

                            var cityMilitaryInfo = new CityMilitaryInfo
                                                       {
                                                           Counter = counter,
                                                           RawData = city,
                                                           WebAgent = webAgent,
                                                           Name = cityDetail.CityName,
                                                           CityId = cityDetail.CityNodeId,
                                                           FortressEndure =
                                                               cityDetail.FortressEndure,
                                                           MaxFortressEndure =
                                                               cityDetail.MaxFortressEndure,
                                                           WallEndure = cityDetail.WallEndure,
                                                           MaxWallEndure =
                                                               cityDetail.MaxWallEndure,
                                                           TotalArmy =
                                                               reserveArmyPage.ReserveArmyNum,
                                                           TotalHeroNum =
                                                               reserveArmyPage.ReserveHeroNum,
                                                           AttackTroops =
                                                               attackTeamPage.TeamList.Select(
                                                                   team =>
                                                                   new CityTroopInfo
                                                                       {
                                                                           HeroNum
                                                                               =
                                                                               team
                                                                               .HeroNum,
                                                                           AttackPower
                                                                               =
                                                                               team
                                                                               .AttackPower,
                                                                           DefendPower
                                                                               =
                                                                               team
                                                                               .DefendPower
                                                                       })
                                                               .ToList(),
                                                           DefendTroops =
                                                               defendTeamPage.TeamList.Select(
                                                                   team =>
                                                                   new CityTroopInfo
                                                                       {
                                                                           HeroNum
                                                                               =
                                                                               team
                                                                               .HeroNum,
                                                                           AttackPower
                                                                               =
                                                                               team
                                                                               .AttackPower,
                                                                           DefendPower
                                                                               =
                                                                               team
                                                                               .DefendPower
                                                                       })
                                                               .ToList()
                                                       };
                            newCity = cityMilitaryInfo;
                        }

                        lock (cityMilitaryInfoList)
                        {
                            cityMilitaryInfoList.Add(newCity);
                        }
                    }).Wait();
            return cityMilitaryInfoList;
        }
        private static string BuildCityMilitaryInfoString(CityMilitaryInfo city)
        {
            var logBuilder = new StringBuilder();
            logBuilder.AppendFormat("要塞:{0}/{1}", city.FortressEndure, city.MaxFortressEndure);
            logBuilder.AppendFormat(",城墙:{0}/{1}", city.WallEndure, city.MaxWallEndure);
            logBuilder.AppendFormat(",后备部队:{0}", city.TotalArmy);
            logBuilder.AppendFormat(",后备将领:{0}", city.TotalHeroNum);
            logBuilder.AppendFormat(",攻击部队:{0}", city.AttackTroops.Count);
            logBuilder.AppendFormat(",防御部队:{0}", city.DefendTroops.Count);
            return logBuilder.ToString();
        }

        private static IEnumerable<CityInfo> QueryInfluenceCityList(AccountInfo account)
        {
            var agent = new RequestAgent(account);

            var cityPage = ShowInfluenceCity.Open(agent);
            if (!cityPage.Citys.Any())
            {
                return new List<CityInfo>();
            }

            ShowInfluenceCityDetail.Open(agent, cityPage.Citys.First());
            var moveArmyPage = ShowMoveArmy.Open(agent);
            return moveArmyPage.CityList;
        }

        public class CityTroopInfo
        {
            public int HeroNum { get; set; }

            public int AttackPower { get; set; }

            public int DefendPower { get; set; }
        }

        public class CityMilitaryInfo
        {
            public string Name { get; set; }

            public int CityId { get; set; }

            public int FortressEndure { get; set; }

            public int MaxFortressEndure { get; set; }

            public int WallEndure { get; set; }

            public int MaxWallEndure { get; set; }

            public int TotalArmy { get; set; }

            public int TotalHeroNum { get; set; }

            public List<CityTroopInfo> AttackTroops { get; set; }

            public List<CityTroopInfo> DefendTroops { get; set; }

            public RequestAgent WebAgent { get; set; }

            public ListViewItem UiItem { get; set; }

            public CityInfo RawData { get; set; }

            public int Counter { get; set; }
        }
    }
}