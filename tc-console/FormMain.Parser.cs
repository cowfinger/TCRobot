using TC.TCPage.WorldWar;

namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    partial class FormMain
    {
        private static IEnumerable<string> ParseAttribute(
            string content,
            string headPattern,
            string attributePattern,
            int attributeIndex)
        {
            var lines = content.Split('\r', '\n');
            var status = 0;
            var count = 0;
            foreach (var line in lines)
            {
                switch (status)
                {
                    case 0:
                        if (Regex.IsMatch(line, headPattern))
                        {
                            status = 1;
                        }
                        break;

                    case 1:
                        var match = Regex.Match(line, attributePattern);
                        if (match.Success)
                        {
                            if (count < attributeIndex)
                            {
                                ++count;
                            }
                            else
                            {
                                count = 0;
                                status = 0;
                                yield return match.Groups[1].Value;
                            }
                        }
                        break;
                }
            }
        }

        private IEnumerable<string> OpenAttackPage(string cityId, string account)
        {
            var page = this.OpenCityShowAttackPage(cityId, account);
            return ParseTargetCityList(page);
        }

        private static string ParseTargetCityId(string content, string keyword)
        {
            var re = new Regex(@"<option value=\""([0-9]+)\"">([^<]*)</option>");

            var ms = re.Matches(content);
            foreach (Match m in ms)
            {
                if (m.Groups[2].Value == keyword)
                {
                    return m.Groups[1].Value;
                }
            }
            return "";
        }

        private static string ParseAttackDuration(string content)
        {
            const string durationPattern = "<td colspan='4'>需要 <span class=\"yell\">(\\d+):(\\d+):(\\d+)</span>";
            var match = Regex.Match(content, durationPattern);
            if (match.Success)
            {
                return match.Groups[1].Value + ":" + match.Groups[2].Value + ":" + match.Groups[3].Value;
            }

            return "";
        }

        private static int TimeStr2Sec(string input)
        {
            var strs = input.Split(':');
            if (strs.Length == 3)
            {
                return Int32.Parse(strs[0]) * 3600 + Int32.Parse(strs[1]) * 60 + Int32.Parse(strs[2]);
            }
            return 0;
        }

        private IEnumerable<TroopInfo> ParseGroupTeams(string content, string account, bool isHead)
        {
            const string teamIdPattern = @"worldWarClass\.showTeamDetail\((\d+),\d+\)";
            const string groupIdPattern = @"worldWarClass\.doDisbandGroup\((\d+),\d+\)";
            var matches = Regex.Matches(content, groupIdPattern);
            foreach (Match match in matches)
            {
                var groupId = match.Groups[1].Value;
                var teamId = "组成员";
                if (isHead)
                {
                    var detailPage = this.OpenGroupTeamDetailPage(groupId, account);
                    var teamIdMatch = Regex.Match(detailPage, teamIdPattern);
                    teamId = teamIdMatch.Success ? teamIdMatch.Groups[1].Value : "";
                }

                yield return
                    new TroopInfo
                        {
                            GroupId = match.Groups[1].Value,
                            TroopId = teamId,
                            isGroupTroop = true,
                            IsGroupHead = isHead,
                            isDefendTroop = false,
                            Name = match.Groups[2].Value,
                            AccountName = account
                        };
            }
        }

        private static IEnumerable<string> ParseTargetCityList(string content)
        {
            const string targetBeginLine = "目的地";
            var targetPattern = new Regex(@"<option value=""\d+"">(.*)</option>");
            var lines = content.Split('\r');
            var status = 0;
            foreach (var line in lines)
            {
                switch (status)
                {
                    case 0:
                        if (line.Contains(targetBeginLine))
                        {
                            status = 1;
                        }
                        break;

                    case 1:
                        var match = targetPattern.Match(line);
                        if (match.Success)
                        {
                            yield return match.Groups[1].Value;
                        }
                        break;
                }
            }
        }

        private static IEnumerable<KeyValuePair<long, long>> ParseInfluenceResource(string page)
        {
            const string pattern = @"<div class=""num\"">(\d+)/(\d+)</div>";
            var matches = Regex.Matches(page, pattern);
            foreach (Match match in matches)
            {
                var Key = long.Parse(match.Groups[1].Value);
                var Value = long.Parse(match.Groups[2].Value);
                yield return new KeyValuePair<long, long>(Key, Value);
            }
        }

        private static IEnumerable<DepotItem> ParseDepotItems(string page)
        {
            const string Pattern = @"prop\.use_prop\((\d+), (\d+), (\d+), (\d+)\)";
            var matches = Regex.Matches(page, Pattern);
            return from Match match in matches
                   select
                       new DepotItem
                           {
                               PropertyID = int.Parse(match.Groups[1].Value),
                               UserPropertyID = int.Parse(match.Groups[3].Value),
                               GoodsType = int.Parse(match.Groups[2].Value)
                           };
        }

        private static int ParseMaxPageId(string page)
        {
            const string pattern = @"<span class=""page_on"">\d+/(\d+)</span>";
            var match = Regex.Match(page, pattern);
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private IEnumerable<DepotItem> EnumDepotItems(string account)
        {
            var maxPage = 1;
            var curPage = 1;
            do
            {
                var page = this.OpenAccountDepot(account, 1, curPage);
                maxPage = ParseMaxPageId(page);

                var items = ParseDepotItems(page);
                foreach (var item in items)
                {
                    yield return item;
                }

                ++curPage;
            }
            while (curPage <= maxPage);
        }

        private bool OpenResourceBox(string account)
        {
            var resBoxes = this.EnumDepotItems(account).Where(item => item.PropertyID == 207401).ToList();
            if (!resBoxes.Any())
            {
                return false;
            }
            this.UseDepotItems(account, resBoxes.First(), 1);
            return true;
        }

        private IEnumerable<AttackTask> ParseOnlineTroopList(string page, string account)
        {
            const string linePattern = "<span id=\"event_(?<taskid>\\d+)\"></span>";
            const string etaPattern = @"将于&nbsp;&nbsp;(?<eta>\d\d\d\d\-\d\d\-\d\d \d\d:\d\d:\d\d)&nbsp;&nbsp;到达";
            const string fromCityPattern = "<div class=\"attack_infor\">(?<city>.*?)&nbsp;&nbsp;返回&nbsp;&nbsp;";
            const string cityPattern =
                "<a href='javascript:void\\(0\\)' onclick=\"military\\.show_attack\\('gj',\\d+,0\\);\">(?<city>.+?)\\(.*?</a><span class=\"button1\">";

            var lines = page.Split('\r');
            foreach (var line in lines)
            {
                var lineMatch = Regex.Match(line, linePattern);
                if (!lineMatch.Success)
                {
                    continue;
                }

                var etaMatch = Regex.Match(line, etaPattern);
                if (!etaMatch.Success)
                {
                    continue;
                }

                var cityMatches = Regex.Matches(line, cityPattern);
                if (cityMatches.Count == 0)
                {
                    continue;
                }

                var fromCity = "";
                var toCity = "";
                switch (cityMatches.Count)
                {
                    case 1:
                        toCity = cityMatches[0].Groups["city"].Value;

                        var fromCityMatch = Regex.Match(line, fromCityPattern);
                        if (fromCityMatch.Success)
                        {
                            fromCity = fromCityMatch.Groups["city"].Value;
                        }
                        break;
                    case 2:
                        fromCity = cityMatches[0].Groups["city"].Value;
                        toCity = cityMatches[1].Groups["city"].Value;
                        break;
                }

                var accountInfo = this.accountTable[account];
                var taskType = "返回";
                if (line.Contains("攻击"))
                {
                    if (accountInfo.InfluenceCityList == null)
                    {
                        var accountCityList = this.QueryInfluenceCityList(account).ToList();
                        accountInfo.InfluenceCityList = accountCityList.ToDictionary(city => city.Name);
                        accountInfo.InfluenceMap = this.BuildInfluenceCityMap(accountCityList, account);
                        accountInfo.MainCity = accountCityList.Single(cityInfo => cityInfo.CityId == 0);
                    }
                    taskType = accountInfo.InfluenceCityList.ContainsKey(toCity) ? "被攻击" : "攻击";
                }

                yield return
                    new AttackTask
                        {
                            AccountName = account,
                            TaskType = taskType,
                            TaskId = lineMatch.Groups["taskid"].Value,
                            FromCity = fromCity,
                            ToCity = toCity,
                            EndTime = DateTime.Parse(etaMatch.Groups["eta"].Value)
                        };
            }
        }

        private IEnumerable<HeroInfo> QueryHeroList(string account)
        {
            var content = this.OpenHeroPage(account);

            return ParseHeroList(content, account);
        }

        private static IEnumerable<HeroInfo> ParseHeroList(string content, string account)
        {
            const string pattern = "<li id=\"li_hero_my(?<heroid>\\d+)\" hname=\"(?<heroname>.+)\" die=(?<isdead>\\d)>";

            var matches = Regex.Matches(content, pattern);
            return from Match match in matches
                   select
                       new HeroInfo
                           {
                               AccountName = account,
                               HeroId = match.Groups["heroid"].Value,
                               Name = match.Groups["heroname"].Value,
                               IsDead = match.Groups["isdead"].Value == "1"
                           };
        }

        private static string GetTid(AccountInfo account)
        {
            var cookieMap = ParseCookieStr(account.CookieStr);

            string tmpId;
            return !cookieMap.TryGetValue("tmp_mid", out tmpId) ? string.Empty : tmpId;
        }

        private IEnumerable<CityInfo> QueryInfluenceCityList(string account)
        {
            this.OpenAccountFirstCity(account);
            var content = this.OpenMoveTroopPage(account);
            return this.ParseCityListFromMoveTroopPage(content);
        }

        private IEnumerable<CityInfo> ParseCityListFromMoveTroopPage(string content)
        {
            const string cityPattern = "<option value=\"(?<nodeId>\\d+)\"\\s*>(?<name>[^<]+)</option>";
            const string selectedCityPattern = "<option value=\"(?<nodeId>\\d+)\" selected\\s*>(?<name>[^<]+)</option>";

            var contentParts = content.Split(new[] { "目的地：" }, StringSplitOptions.RemoveEmptyEntries);
            var fromCityMatches = Regex.Matches(contentParts[0], cityPattern);
            var selectedCityMatch = Regex.Match(contentParts[0], selectedCityPattern);

            if (selectedCityMatch.Success)
            {
                yield return
                    new CityInfo
                        {
                            Name = selectedCityMatch.Groups["name"].Value,
                            NodeId = int.Parse(selectedCityMatch.Groups["nodeId"].Value),
                            CityId = int.Parse(this.cityList[selectedCityMatch.Groups["name"].Value])
                        };
            }

            foreach (Match cityMatch in fromCityMatches)
            {
                var cityName = cityMatch.Groups["name"].Value;
                var cityId = "0";
                if (!this.cityList.TryGetValue(cityName, out cityId))
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

        private Dictionary<string, HashSet<string>> BuildInfluenceCityMap(
            IEnumerable<CityInfo> influenceCityList,
            string account)
        {
            var accountInfo = this.accountTable[account];

            var map = new Dictionary<string, HashSet<string>>();
            foreach (var cityInfo in influenceCityList)
            {
                var moveArmyPage = ShowMoveArmy.Open(accountInfo.WebAgent, cityInfo.NodeId);
                var toSet = new HashSet<string>();
                foreach (var city in moveArmyPage.MoveTargetCityList)
                {
                    toSet.Add(city.Name);
                }

                map.Add(cityInfo.Name, toSet);
            }

            return map;
        }

        private static int ParseUnionIdFromMainPage(string page)
        {
            const string pattern = @"union_id=(?<unionId>\d+)";
            var match = Regex.Match(page, pattern);
            return match.Success ? int.Parse(match.Groups["unionId"].Value) : 0;
        }
    }
}