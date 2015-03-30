namespace TC.TCPage.WorldWar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ShowMoveArmy
    {
        public const string CityPattern = "<option value=\"(?<nodeId>\\d+)\"\\s*>(?<name>[^<]+)</option>";

        public ShowMoveArmy(string page)
        {
            this.RawPage = page;
            this.BrickNum = ParseBrickNumberFromMovePage(page);
            this.Army = ParseSoldierListFromMovePage(page);
            this.HeroList = ParseHeroInfoListFromMovePage(page);
        }

        public string RawPage { get; private set; }

        public int BrickNum { get; private set; }

        public IEnumerable<Soldier> Army { get; private set; }

        public IEnumerable<HeroInfo> HeroList { get; private set; }

        public int CityId { get; private set; }

        public IEnumerable<CityInfo> MoveTargetCityList
        {
            get
            {
                var contentParts = this.RawPage.Split(new[] { "目的地：" }, StringSplitOptions.RemoveEmptyEntries);
                if (contentParts.Count() < 2)
                {
                    yield break;
                }

                var toCityMatches = Regex.Matches(contentParts[1], CityPattern);
                foreach (Match toCityMatch in toCityMatches)
                {
                    var cityName = toCityMatch.Groups["name"].Value;
                    string nodeId;
                    if (!FormMain.CityList.TryGetValue(cityName, out nodeId))
                    {
                        nodeId = "0";
                    }

                    yield return
                        new CityInfo
                            {
                                CityId = int.Parse(toCityMatch.Groups["nodeId"].Value),
                                Name = cityName,
                                NodeId = int.Parse(nodeId)
                            };
                }
            }
        }

        public IEnumerable<CityInfo> CityList
        {
            get
            {
                return ParseCityListFromMoveTroopPage(this.RawPage);
            }
        }

        public static ShowMoveArmy Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.move_army);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowMoveArmy(rawPage);
        }

        public static ShowMoveArmy Open(RequestAgent agent, int fromCityId)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.move_army,
                new TCRequestArgument(TCElement.from_city_id, fromCityId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowMoveArmy(rawPage) { CityId = fromCityId };
        }

        private static int ParseBrickNumberFromMovePage(string page)
        {
            const string Pattern = "<span id=\"brick_num_max\">\\d+</span>/(\\d+)</span>\\)</span>";
            var match = Regex.Match(page, Pattern);
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private static IEnumerable<Soldier> ParseSoldierListFromMovePage(string page)
        {
            const string IdPattern = "max_num=(\\d+) name=\"s_(\\d+)\"";
            return from Match match in Regex.Matches(page, IdPattern)
                   select
                       new Soldier
                           {
                               Name = FormMain.KeyWordMap[string.Format("soldier_{0}", match.Groups[2].Value)],
                               SoldierType = int.Parse(match.Groups[2].Value),
                               SoldierNumber = int.Parse(match.Groups[1].Value)
                           };
        }

        private static IEnumerable<HeroInfo> ParseHeroInfoListFromMovePage(string page)
        {
            const string NamePattern =
                "<div class=\"name button1\"><a href=\"javascript:void\\(0\\)\"><span>(?<name>.+?)</span></a></div>";
            const string IdPattern = "hero_id=\"(\\d+)\" hero_status=\"(\\d+)\"";

            var nameMatches = Regex.Matches(page, NamePattern);
            var nameList = (from Match match in nameMatches select match.Groups[1].Value).ToList();

            var idMatches = Regex.Matches(page, IdPattern);
            var idList = (from Match match in idMatches select match.Groups[1].Value).ToList();
            var statusList = (from Match match in idMatches select match.Groups[2].Value).ToList();

            for (var i = 0; i < Math.Min(nameList.Count, idList.Count); ++i)
            {
                yield return new HeroInfo { Name = nameList[i], HeroId = idList[i], IsBusy = statusList[i] != "1" };
            }
        }

        private static IEnumerable<CityInfo> ParseCityListFromMoveTroopPage(string content)
        {
            const string CityPattern = "<option value=\"(?<nodeId>\\d+)\"\\s*>(?<name>[^<]+)</option>";
            const string SelectedCityPattern = "<option value=\"(?<nodeId>\\d+)\" selected\\s*>(?<name>[^<]+)</option>";

            var contentParts = content.Split(new[] { "目的地：" }, StringSplitOptions.RemoveEmptyEntries);
            var fromCityMatches = Regex.Matches(contentParts[0], CityPattern);
            var selectedCityMatch = Regex.Match(contentParts[0], SelectedCityPattern);

            if (selectedCityMatch.Success)
            {
                yield return
                    new CityInfo
                        {
                            Name = selectedCityMatch.Groups["name"].Value,
                            NodeId = int.Parse(selectedCityMatch.Groups["nodeId"].Value),
                            CityId = int.Parse(FormMain.CityList[selectedCityMatch.Groups["name"].Value])
                        };
            }

            foreach (Match cityMatch in fromCityMatches)
            {
                var cityName = cityMatch.Groups["name"].Value;
                var cityId = "0";
                if (!FormMain.CityList.TryGetValue(cityName, out cityId))
                {
                    cityId = "0";
                }

                yield return
                    new CityInfo
                        {
                            Name = cityName,
                            NodeId = int.Parse(cityMatch.Groups["nodeId"].Value),
                            CityId = int.Parse(cityId)
                        };
            }
        }
    }
}