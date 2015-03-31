namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    using TC.TCPage.Depot;
    using TC.TCPage.Prop;
    using TC.TCPage.WorldWar;

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

        public static int TimeStr2Sec(string input)
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

        private bool OpenResourceBox(string account)
        {
            var accountInfo = this.accountTable[account];
            var resBoxes =
                ShowMyDepot.EnumDepotItems(accountInfo.WebAgent)
                    .Where(prop => prop.PropertyId == (int)DepotItem.PropId.ResourceBox)
                    .ToList();
            if (!resBoxes.Any())
            {
                return false;
            }
            var firstBox = resBoxes.First();
            DoUseProp.Open(accountInfo.WebAgent, firstBox.PropertyId, firstBox.UserPropertyId, 1);
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
                        var accountCityList = QueryInfluenceCityList(accountInfo).ToList();
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
            if (account.WebAgent == null)
            {
                return "";
            }

            var url = new Uri(string.Format("http://{0}/", account.AccountType));
            var cookie = account.WebAgent.WebClient.Cookies.GetCookies(url)["tmp_mid"];
            return cookie != null ? cookie.Value : "";
        }

        private static IEnumerable<CityInfo> QueryInfluenceCityList(AccountInfo account)
        {
            var agent = new RequestAgent(account);

            var cityPage = TCPage.Influence.ShowInfluenceCity.Open(agent);
            if (!cityPage.Citys.Any())
            {
                return new List<CityInfo>();
            }

            TCPage.Influence.ShowInfluenceCityDetail.Open(agent, cityPage.Citys.First());
            var moveArmyPage = TCPage.WorldWar.ShowMoveArmy.Open(agent);
            return moveArmyPage.CityList;
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