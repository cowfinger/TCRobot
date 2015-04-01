using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TC.TCPage.WorldWar;

namespace TC.TCDataType
{
    class InfluenceMap
    {

        public static Dictionary<string, HashSet<string>> GolalMap = new Dictionary<string, HashSet<string>>();

        public static IEnumerable<CityInfo> QueryCityList(AccountInfo account)
        {
            var cityPage = TCPage.Influence.ShowInfluenceCity.Open(account.WebAgent);
            if (!cityPage.Citys.Any())
            {
                return new List<CityInfo>();
            }

            TCPage.Influence.ShowInfluenceCityDetail.Open(account.WebAgent, cityPage.Citys.First());
            var moveArmyPage = ShowMoveArmy.Open(account.WebAgent);
            return moveArmyPage.CityList;
        }

        public static Dictionary<string, HashSet<string>> BuildMap(
            IEnumerable<CityInfo> influenceCityList,
            AccountInfo accountInfo)
        {
            var map = new Dictionary<string, HashSet<string>>();
            foreach (var cityInfo in influenceCityList)
            {
                HashSet<string> toSet;
                lock (GolalMap)
                {
                    if (!GolalMap.TryGetValue(cityInfo.Name, out toSet))
                    {
                        toSet = new HashSet<string>();
                        var moveArmyPage = ShowMoveArmy.Open(accountInfo.WebAgent, cityInfo.NodeId);
                        foreach (var city in moveArmyPage.MoveTargetCityList)
                        {
                            toSet.Add(city.Name);
                        }
                        GolalMap.Add(cityInfo.Name, toSet);
                    }
                }
                map.Add(cityInfo.Name, toSet);
            }

            return map;
        }
    }
}
