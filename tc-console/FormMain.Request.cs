using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC
{
    partial class FormMain
    {
        private IEnumerable<string> GetAccountInflunceCityNameListWithArmy(string account)
        {
            string url = string.Format("http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_army&r={1}",
                hostname, randGen.NextDouble());
            string resp = HTTPRequest(url, account);

            var pattern = new Regex("<td width=\"12%\">(.*)</td>");
            var matches = pattern.Matches(resp);
            foreach (Match match in matches)
            {
                yield return match.Groups[1].Value;
            }
        }

        private DateTime GetRemoteSysTime()
        {
            string rsp = RefreshHomePage(activeAccount);
            return ParseSysTimeFromHomePage(rsp);
        }

        private string GetTargetCityID(string srccityid, string dstcityname, string account)
        {
            string response = OpenCityPage(srccityid, account);
            return ParseTargetCityID(response, dstcityname);
        }

        private string CreateGroupHead(string cityId, string teamId, string account)
        {
            OpenCityPage(cityId, account);
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=create_group&r={1}",
                this.hostname, this.randGen.NextDouble()
                );

            string name = randGen.Next(100000, 999999).ToString();
            string body = string.Format("team_id={0}&group_name={1}", teamId, name);
            HTTPRequest(url, account, body);
            return name;
        }

        private void JoinGroup(string cityId, string groupTeamId, string subTeamId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=join_group&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            string body = string.Format("team_id={0}&group_id={1}&from_address=1", subTeamId, groupTeamId);
            HTTPRequest(url, account, body);
        }

        private IEnumerable<TeamInfo> GetAttackTeamsInfo(string srccityid, string dstcityid, string account)
        {
            string response = OpenCityPage(srccityid, account);
            return ParseAttackTeams(response, dstcityid, account);
        }

        private IEnumerable<TeamInfo> GetTeamsInfo(string srccityid, string account)
        {
            string cityPage = OpenCityPage(srccityid, account);
            return ParseTeams(cityPage, account);
        }

        private IEnumerable<TeamInfo> GetActiveTeamInfo(string cityId, string tabId, string account)
        {
            string url0 = string.Format("http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}", hostname, randGen.NextDouble());
            HTTPRequest(url0, account);

            string fmt1 = "http://" + hostname + "/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={0}&r=" + randGen.NextDouble().ToString();
            string url1 = string.Format(fmt1, cityId);
            HTTPRequest(url1, account);

            string url2 = string.Format(
                "http://{0}//index.php?mod=military/world_war&op=show&func=team&tab_id={1}&user_nickname=&r={2}",
                this.hostname, tabId, this.randGen.NextDouble()
            );
            string data = HTTPRequest(url2, account);

            var attackPowerList = ParseAttribute(data, @"<td>部队\d+</td>", @"<td>(\d+)</td>", 2).ToList();

            int i = 0;
            const string teamIDPattern = @"worldWarClass\.showTeamDetail\((\d+),\d+\)";
            var matches = Regex.Matches(data, teamIDPattern);
            foreach (Match match in matches)
            {
                yield return new TeamInfo()
                {
                    TeamId = match.Groups[1].Value,
                    AccountName = account,
                    PowerIndex = int.Parse(attackPowerList[i]),
                    isGroupTeam = false,
                    isDefendTeam = tabId == "2",
                };

                ++i;
            }
        }

        private IEnumerable<TeamInfo> GetGroupAttackTeamsInfo(string srccityid, string dstcityid, string account)
        {
            string response = OpenGroupTeamListPage(srccityid, account);
            return ParseGroupAttackTeams(response, dstcityid, account);
        }

        private IEnumerable<TeamInfo> GetGroupTeamsInfo(string srccityid, string account)
        {
            string response = OpenGroupTeamListPage(srccityid, account);
            return ParseGroupTeams(response, account);
        }

        private bool IsTeamInCity(string srcCityId, string account)
        {
            string url0 = string.Format("http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}", hostname, randGen.NextDouble());
            HTTPRequest(url0, account);

            string url1 = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                hostname,
                srcCityId,
                randGen.NextDouble());
            string resp = HTTPRequest(url1, account);

            var regex = new Regex(@"user_hero_\d+");
            return regex.Matches(resp).Count > 0;
        }

        private string OpenCreateTeamPage(string cityId, string account)
        {
            OpenCityPage(cityId, account);

            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=show&func=create_team&team_type=1&from_address=1&r={1}",
                hostname, randGen.NextDouble());

            return HTTPRequest(url, account);
        }

        private IEnumerable<Soldier> ParseSoldiersInCreateTeamPage(string content)
        {
            const string pattern = "<input max_num=(\\d+) name=\"s_(\\d+)\" type=\"text\" />";

            var matches = Regex.Matches(content, pattern);
            foreach (Match match in matches)
            {
                int soldierNumber = int.Parse(match.Groups[1].Value);
                int soldierId = int.Parse(match.Groups[2].Value);
                yield return new Soldier() { SoldierType = soldierId, SoldierNumber = soldierNumber, };
            }
        }

        private IEnumerable<string> ParseHerosInCreateTeamPage(string content)
        {
            var pattern = new Regex(@"worldWarClass\.selectHero\('(\d+)',true\);");
            var tempHeroList = new List<string>();

            var matches = pattern.Matches(content);
            foreach (Match match in matches)
            {
                tempHeroList.Add(match.Groups[1].Value);
            }

            var statusPattern = new Regex("<p class=\"trans_70\">(.*)</p>");
            var statusList = new List<string>();
            var statusMatches = statusPattern.Matches(content);
            foreach (Match match in statusMatches)
            {
                statusList.Add(match.Groups[1].Value);
            }

            var heroList = new List<string>();
            for (int i = 0; i < tempHeroList.Count; ++i)
            {
                if (statusList[i] == "空闲")
                {
                    heroList.Add(tempHeroList[i]);
                }
            }

            return heroList;
        }

        private void CreateTeam(string srcCityId, string heroId, string subHeroes, string soldier, string teamType, string account)
        {
            string url = string.Format("http://{0}/index.php?mod=military/world_war&func=create_team&op=do&r={1}", hostname, randGen.NextDouble());
            string body = string.Format(
                "team_type={0}&main_hero={1}&using_embattle_id=&sub_hero={2}&soldiers={3}&pk_hero_id=",
                teamType, heroId,  subHeroes, soldier
                );

            HTTPRequest(url, account, body);
        }

        private string OpenCityPage(string srccityid, string account)
        {
            string url0 = string.Format("http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}", hostname, randGen.NextDouble());
            HTTPRequest(url0, account);

            string fmt1 = "http://" + hostname + "/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={0}&r=" + randGen.NextDouble().ToString();
            string url1 = string.Format(fmt1, srccityid);
            HTTPRequest(url1, account);

            string url2 = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=attack&team_id=0&r={1}",
                hostname, randGen.NextDouble());
            return HTTPRequest(url2, account);
        }

        private string OpenGroupTeamListPage(string srccityid, string account)
        {
            string url0 = string.Format("http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}", hostname, randGen.NextDouble());
            HTTPRequest(url0, account);

            string fmt1 = "http://" + hostname + "/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={0}&r=" + randGen.NextDouble().ToString();
            string url1 = string.Format(fmt1, srccityid);
            HTTPRequest(url1, account);

            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=show&func=join_attack&r={1}",
                hostname, randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private string OpenTeamDetailPage(string teamId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=team_detail&team_id={1}&from_address=1&r={2}",
                hostname, teamId, randGen.NextDouble()
                );
            return HTTPRequest(url, account);
        }

        private string OpenGroupTeamDetailPage(string teamId, string account)
        {
            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=show&func=group_detail&group_id={1}&r={2}",
                hostname, teamId, randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private string OpenGroupTeamAttackPage(string teamId, string cityId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=join_attack_confirm&group_id={1}&to_city_id={2}&join_attack_type=1&r={3}",
                hostname, teamId, cityId, randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private IEnumerable<string> ParseAttribute(
            string content,
            string headPattern,
            string attributePattern,
            int attributeIndex
            )
        {
            // var pattern = new Regex(@"<td>(.*)</td>");
            // var headName = string.Format("<td>{0}</td>", attributeName);
            var lines = content.Split('\r', '\n');
            int status = 0;
            int count = 0;
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
                        Match match = Regex.Match(line, attributePattern);
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

        private List<string> OpenAttackPage(string cityId, string account)
        {
            string page = OpenCityPage(cityId, account);
            return ParseTargetCityList(page);
        }

        private string OpenAttackCityPage(string teamid, string cityid, string account)
        {
            string pattern = "http://" + hostname + "/index.php?mod=military/world_war&op=show&func=attack_confirm&team_id={0}&to_city_id={1}&r=" + randGen.NextDouble().ToString();
            string url = string.Format(pattern, teamid, cityid);
            return HTTPRequest(url, account);
        }

        private void AttackTarget(string team_id, string city_id, string account)
        {
            string url = "http://" + hostname + "/index.php?mod=military/world_war&op=do&func=attack&r=" + randGen.NextDouble().ToString();
            string bodyfmt = "team_id={0}&to_city_id={1}";
            string body = string.Format(bodyfmt, team_id, city_id);
            string ret = HTTPRequest(url, account, body);
        }

        private void GroupAttackTarget(string teamId, string cityId, string account)
        {
            string url = string.Format("/index.php?mod=military/world_war&op=do&func=join_attack&r=", randGen.NextDouble());
            string body = string.Format("group_id={0}&to_city_id={1}&join_attack_type=1", teamId, cityId);
            string ret = HTTPRequest(url, account, body);
        }

        private string RefreshHomePage(string account)
        {
            string url = "http://" + hostname + "/index.php";
            return HTTPRequest(url, account);
        }

        private DateTime ParseSysTimeFromHomePage(string content)
        {
            Regex re = new Regex("wee\\.timer\\.set_time\\( [0-9]+ \\);");

            MatchCollection ms = re.Matches(content);
            foreach (Match m in ms)
            {
                string timestr = m.Value.Split('(', ')')[1];
                DateTime ret = new DateTime(1970, 1, 1);
                int sec = Int32.Parse(timestr.Trim(' '));
                ret = ret.AddSeconds(sec);
                return ret.ToLocalTime();
            }
            return new DateTime();
        }

        private string ParseTargetCityID(string content, string keyword)
        {
            Regex re = new Regex("<option value=\\\"([0-9]+)\\\">([^<]*)</option>");

            MatchCollection ms = re.Matches(content);
            foreach (Match m in ms)
            {
                if (m.Groups[2].Value == keyword)
                    return m.Groups[1].Value;
            }
            return "";
        }

        private string ParseAttackDuration(string content)
        {
            Regex re = new Regex("[0-9][0-9]:[0-9][0-9]:[0-9][0-9]");

            MatchCollection ms = re.Matches(content);
            foreach (Match m in ms)
            {
                string[] strs = m.Value.Split(':');
                if (strs.Length == 3)
                {
                    return m.Value;
                }
            }
            return "";
        }

        private int TimeStr2Sec(string input)
        {
            string[] strs = input.Split(':');
            if (strs.Length == 3)
            {
                return Int32.Parse(strs[0]) * 3600 + Int32.Parse(strs[1]) * 60 + Int32.Parse(strs[2]);
            }
            return 0;
        }

        private IEnumerable<TeamInfo> ParseGroupTeams(string content, string account)
        {
            const string teamIdPattern = "worldWarClass\\.showTeamDetail\\((\\d+),\\d+\\)";
            const string groupIdPattern = "<a href='javascript:void\\(0\\)' onclick=\"worldWarClass\\.showGroupDetail\\((\\d+)\\)\" >(\\w+)</a>";
            var matches = Regex.Matches(content, groupIdPattern);
            foreach (Match match in matches)
            {
                string groupId = match.Groups[1].Value;
                string detailPage = OpenGroupTeamDetailPage(groupId, account);
                var teamIdMatch = Regex.Match(detailPage, teamIdPattern);

                yield return new TeamInfo()
                {
                    GroupId = match.Groups[1].Value,
                    TeamId = teamIdMatch.Success ? teamIdMatch.Groups[1].Value : "",
                    isGroupTeam = true,
                    isDefendTeam = false,
                    Name = match.Groups[2].Value,
                    AccountName = account,
                };
            }
        }

        private IEnumerable<TeamInfo> ParseGroupAttackTeams(string content, string destCityId, string account)
        {
            var groupIdPattern = new Regex("worldWarClass\\.showGroupDetail\\((\\d+)\\)");

            var matches = groupIdPattern.Matches(content);
            foreach (Match match in matches)
            {
                string teamId = match.Groups[1].Value;
                string teamPageContent = OpenGroupTeamDetailPage(teamId, account);
                string attackPage = OpenGroupTeamAttackPage(teamId, destCityId, account);
                string durationString = ParseAttackDuration(attackPage);

            // var pattern = new Regex(@"<td>(.*)</td>");
            // var headName = string.Format("<td>{0}</td>", attributeName);
                yield return new TeamInfo()
                {
                    TeamId = match.Groups[1].Value,
                    isGroupTeam = true,
                    isDefendTeam = false,
                    Name = ParseAttribute(teamPageContent, "<td>小队名称</td>", @"<td>(.*)</td>", 0).FirstOrDefault(),
                    Leader = ParseAttribute(teamPageContent, "<td>本小队队长</td>", @"<td>(.*)</td>", 0).FirstOrDefault(),
                    DurationString = durationString,
                    Duration = TimeStr2Sec(durationString),
                    AccountName = account,
                    TimeLeft = 0,
                };
            }
        }

        private IEnumerable<TeamInfo> ParseTeams(string content, string account)
        {
            Regex re = new Regex("worldWarClass\\.changeMyAttackTeam\\([0-9]+,[0-9]+,[0-9]+\\)");
            var heroPattern = new Regex("<img src=\"http://static\\.tc\\.9wee\\.com/hero/\\d+/\\d+\\.gif\" titleContent=\"(.*)\"/>");

            MatchCollection ms = re.Matches(content);
            foreach (Match m in ms)
            {
                TeamInfo team = new TeamInfo();
                team.TeamId = m.Value.Split('(', ',')[1];
                team.isGroupTeam = false;
                team.isDefendTeam = false;

                string teamDetailPage = OpenTeamDetailPage(team.TeamId, account);
                var match = heroPattern.Match(teamDetailPage);
                if (match.Success)
                {
                    team.Leader = match.Groups[1].Value;
                }

                team.AccountName = account;
                yield return team;
            }
        }

        private IEnumerable<TeamInfo> ParseAttackTeams(string content, string dstcityid, string account)
        {
            Regex re = new Regex("worldWarClass\\.changeMyAttackTeam\\([0-9]+,[0-9]+,[0-9]+\\)");

            MatchCollection ms = re.Matches(content);
            int team_no = 1;
            foreach (Match m in ms)
            {
                TeamInfo team = new TeamInfo();
                team.TeamId = m.Value.Split('(', ',')[1];
                team.isGroupTeam = false;
                team.isDefendTeam = false;
                team.Leader = team_no.ToString(); //temporary solution
                string rsp = OpenAttackCityPage(team.TeamId, dstcityid, account);
                team.DurationString = ParseAttackDuration(rsp);
                team.Duration = TimeStr2Sec(team.DurationString);

                team.AccountName = account;
                team.TimeLeft = 0;

                if (!string.IsNullOrEmpty(team.DurationString))
                {
                    yield return team;
                    team_no++;
                }
            }
        }

        void DismissTeam(string teamId, string account)
        {
            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=do&func=disband_team&r={1}", hostname, randGen.NextDouble());
            string body = string.Format("team_id={0}&from_address=1&detail_flag=0", teamId);
            HTTPRequest(url, account, body);
        }

        private List<string> GetMoveTargetCities(string sourceCidyId, string account)
        {
            OpenCityPage(sourceCidyId, account);
            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=show&func=move_army&r={1}", hostname, randGen);
            string content = HTTPRequest(url, account);

            return ParseTargetCityList(content);
        }

        private List<string> ParseTargetCityList(string content)
        {
            var targetBeginLine = "目的地";
            var targetPattern = new Regex("<option value=\"\\d+\">(.*)</option>");
            var lines = content.Split('\r');
            int status = 0;
            var cityList = new List<string>();
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
                            cityList.Add(match.Groups[1].Value);
                        }
                        break;
                }
            }
            return cityList;
        }

        private IEnumerable<KeyValuePair<long, long>> ParseInfluenceResource(string page)
        {
            const string pattern = "<div class=\"num\">(\\d+)/(\\d+)</div>";
            var matches = Regex.Matches(page, pattern);
            foreach (Match match in matches)
            {
                long Key = long.Parse(match.Groups[1].Value);
                long Value = long.Parse(match.Groups[2].Value);
                yield return new KeyValuePair<long, long>(Key, Value);
            }
        }

        private string OpenInfluenceSciencePage(string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=influence/science&op=show&func=science&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            return HTTPRequest(url, account);
        }

        private void DonateResource(string account, long val1, long val2, long val3, long val4)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=do&func=influence_donate&res={1}|{2}|{3}|{4}|0|0&r={5}",
                this.hostname, val1, val2, val3, val4, this.randGen.NextDouble()
                );
            HTTPRequest(url, account);
        }

        private IEnumerable<long> GetAccountResources(string account)
        {
            const string pattern = "value=this\\.innerHTML;\">(\\d+)</span>\\)";
            string url = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_donate&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            string page = HTTPRequest(url, account);

            var matches = Regex.Matches(page, pattern);
            foreach (Match match in matches)
            {
                yield return long.Parse(match.Groups[1].Value);
            }
        }

        private string OpenAccountDepot(string account, int type, int page)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=depot/depot&op=show&func=my_depot&goods_type={1}&page={2}&r={3}",
                this.hostname, type, page, this.randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private IEnumerable<DepotItem> ParseDepotItems(string page)
        {
            const string pattern = @"prop\.use_prop\((\d+), (\d+), (\d+), (\d+)\)";
            var matches = Regex.Matches(page, pattern);
            foreach (Match match in matches)
            {
                yield return new DepotItem()
                {
                    PropertyID = int.Parse(match.Groups[1].Value),
                    UserPropertyID = int.Parse(match.Groups[3].Value),
                    GoodsType = int.Parse(match.Groups[2].Value),
                };
            }
        }

        private int ParseMaxPageID(string page)
        {
            const string pattern = "<span class=\"page_on\">\\d+/(\\d+)</span>";
            var match = Regex.Match(page, pattern);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }

            return 0;
        }

        private IEnumerable<DepotItem> EnumDepotItems(string account)
        {
            int maxPage = 1;
            int curPage = 1;
            do
            {
                string page = OpenAccountDepot(account, 1, curPage);
                maxPage = ParseMaxPageID(page);

                var items = ParseDepotItems(page);
                foreach (var item in items)
                {
                    yield return item;
                }

                ++curPage;

            } while (curPage <= maxPage);
        }

        private void UseDepotItems(string account, DepotItem item, int count)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=prop/prop&op=do&func=use_prop&p_type=7&prop_id={1}&prop_num={2}&user_prop_id={3}&from=1&call_back=shop.reflash_res_depot(5,%201)&r={4}",
                this.hostname, item.PropertyID, count, item.UserPropertyID, this.randGen.NextDouble()
                );
            HTTPRequest(url, account);
        }

        private bool OpenResourceBox(string account)
        {
            var resBoxes = EnumDepotItems(account).Where(item => item.PropertyID == 207401);
            if (resBoxes.Any())
            {
                UseDepotItems(account, resBoxes.First(), 1);
                return true;
            }

            return false;
        }

        private void DismissGroup(string groupId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=disband_group&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            string body = string.Format("group_id={0}&from_address=1", groupId);
            HTTPRequest(url, account, body);
        }
    }
}
