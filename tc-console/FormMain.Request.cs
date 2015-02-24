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
            string url = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_army&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            string resp = HTTPRequest(url, account);

            var pattern = new Regex("<td width=\"12%\">(.*)</td>");
            var matches = pattern.Matches(resp);
            foreach (Match match in matches)
            {
                yield return match.Groups[1].Value;
            }
        }

        private DateTime QueryRemoteSysTime(string accountName)
        {
            string rsp = RefreshHomePage(accountName);
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

        private void JoinGroup(string cityId, string groupTroopId, string subTroopId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=join_group&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            string body = string.Format("team_id={0}&group_id={1}&from_address=1", subTroopId, groupTroopId);
            HTTPRequest(url, account, body);
        }

        private IEnumerable<TroopInfo> GetTeamsInfo(string srccityid, string account)
        {
            string cityPage = OpenCityPage(srccityid, account);
            return ParseTeams(cityPage, account);
        }

        private IEnumerable<TroopInfo> GetActiveTroopInfo(string cityId, string tabId, string account)
        {
            string url0 = string.Format(
                "http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}",
                hostname, randGen.NextDouble()
                );
            HTTPRequest(url0, account);

            string url1 = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r=",
                cityId, this.randGen.NextDouble()
                );
            HTTPRequest(url1, account);

            string url2 = string.Format(
                "http://{0}//index.php?mod=military/world_war&op=show&func=team&tab_id={1}&user_nickname=&r={2}",
                this.hostname, tabId, this.randGen.NextDouble()
            );
            string data = HTTPRequest(url2, account);

            var attackPowerList = ParseAttribute(data, @"<td>部队\d+</td>", @"<td>(\d+)</td>", 2).ToList();

            int i = 0;
            const string teamIDPattern = @"worldWarClass\.doDisbandTeam\((\d+),\d+,\d+\)";
            var matches = Regex.Matches(data, teamIDPattern);
            foreach (Match match in matches)
            {
                yield return new TroopInfo()
                {
                    TroopId = match.Groups[1].Value,
                    AccountName = account,
                    PowerIndex = int.Parse(attackPowerList[i]),
                    isGroupTroop = false,
                    isDefendTroop = tabId == "2",
                };

                ++i;
            }
        }

        private IEnumerable<TroopInfo> GetGroupTeamList(string srccityid, string account)
        {
            string response = OpenGroupTeamListPage(srccityid, account);
            var splitPoint = new string[] { "<h4>我加入的队伍</h4>" };
            var groupSplit = response.Split(splitPoint, StringSplitOptions.None);

            if (groupSplit.Count() != 2)
            {
                return new List<TroopInfo>();
            }
            else
            {
                return ParseGroupTeams(groupSplit[0], account, true);
            }
        }

        private IEnumerable<string> GetGroupAttackTargetCity(string cityId, string account)
        {
            string response = OpenGroupTeamListPage(cityId, account);
            return ParseTargetCityList(response);
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

            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=create_team&team_type=1&from_address=1&r={1}",
                hostname, randGen.NextDouble()
                );

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
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&func=create_team&op=do&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            string body = string.Format(
                "team_type={0}&main_hero={1}&using_embattle_id=&sub_hero={2}&soldiers={3}&pk_hero_id=",
                teamType, heroId,  subHeroes, soldier
                );

            HTTPRequest(url, account, body);
        }

        private void OpenCityPage(string cityId, ref HttpClient client)
        {
            // string url0 = string.Format(
            //     "http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}",
            //     this.hostname, this.randGen.NextDouble()
            //     );
            // client.OpenUrl(url0);

            string url1 = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                this.hostname, cityId, this.randGen.NextDouble()
                );
            client.OpenUrl(url1);

            // string url2 = string.Format(
            //     "http://{0}/index.php?mod=military/world_war&op=show&func=attack&team_id=0&r={1}",
            //     this.hostname, this.randGen.NextDouble()
            //     );
            // return client.OpenUrl(url2);
        }

        private string OpenCityPage(string srccityid, string account)
        {
            string url0 = string.Format(
                "http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            HTTPRequest(url0, account);

            string url1 = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                this.hostname, srccityid, this.randGen.NextDouble()
                );
            HTTPRequest(url1, account);

            string url2 = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=attack&team_id=0&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            return HTTPRequest(url2, account);
        }

        private string OpenGroupTeamListPage(string srccityid, string account)
        {
            string url0 = string.Format(
                "http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}",
                hostname, randGen.NextDouble()
                );
            HTTPRequest(url0, account);

            string url1 = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                this.hostname,
                srccityid,
                randGen.NextDouble()
                );
            HTTPRequest(url1, account);

            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=join_attack&r={1}",
                hostname, randGen.NextDouble()
                );
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
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=group_detail&group_id={1}&r={2}",
                hostname, teamId, randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private string OpenGroupAttackPage(string groupId, string cityId, ref HttpClient httpClient)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=join_attack_confirm&group_id={1}&to_city_id={2}&join_attack_type=1&r={3}",
                hostname, groupId, cityId, randGen.NextDouble());
            return httpClient.OpenUrl(url);
        }

        private string OpenGroupAttackPage(string groupId, string cityId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=join_attack_confirm&group_id={1}&to_city_id={2}&join_attack_type=1&r={3}",
                hostname, groupId, cityId, randGen.NextDouble());
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

        private IEnumerable<string> OpenAttackPage(string cityId, string account)
        {
            string page = OpenCityPage(cityId, account);
            return ParseTargetCityList(page);
        }

        private string OpenTeamAttackPage(string teamid, string cityid, ref HttpClient httpClient)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=attack_confirm&team_id={1}&to_city_id={2}&r={3}",
                this.hostname, teamid, cityid, this.randGen.NextDouble()
                );
            return httpClient.OpenUrl(url);
        }

        private string OpenTeamAttackPage(string teamid, string cityid, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=attack_confirm&team_id={1}&to_city_id={2}&r={3}",
                this.hostname, teamid, cityid, this.randGen.NextDouble()
                );
            return HTTPRequest(url, account);
        }

        private string TeamAttackTarget(string team_id, string city_id, ref HttpClient httpClient)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=attack&r={1}", 
                this.hostname, randGen.NextDouble()
                );
            string body = string.Format("team_id={0}&to_city_id={1}", team_id, city_id);
            return httpClient.OpenUrl(url, body);
        }

        private void TeamAttackTarget(string team_id, string city_id, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=attack&r={1}", 
                this.hostname,
                randGen.NextDouble()
                );
            string body = string.Format("team_id={0}&to_city_id={1}", team_id, city_id);
            HTTPRequest(url, account, body);
        }

        private void GroupAttackTarget(string groupId, string cityId, ref HttpClient httpClient)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=join_attack&r={1}", 
                this.hostname, this.randGen.NextDouble()
                );
            string body = string.Format("group_id={0}&to_city_id={1}&join_attack_type=1", groupId, cityId);
            httpClient.OpenUrl(url, body);
        }

        private void GroupAttackTarget(string groupId, string cityId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=join_attack&r={1}", 
                this.hostname, this.randGen.NextDouble()
                );
            string body = string.Format("group_id={0}&to_city_id={1}&join_attack_type=1", groupId, cityId);
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
            return DateTime.MinValue;
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
            const string durationPattern = "<td colspan='4'>需要 <span class=\"yell\">(\\d+):(\\d+):(\\d+)</span>";
            var match = Regex.Match(content, durationPattern);
            if (match.Success)
            {
                return match.Groups[1].Value + ":" + match.Groups[2].Value + ":" + match.Groups[3].Value;
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

        private IEnumerable<TroopInfo> ParseGroupTeams(string content, string account, bool isHead)
        {
            const string teamIdPattern = "worldWarClass\\.showTeamDetail\\((\\d+),\\d+\\)";
            const string groupIdPattern = @"worldWarClass\.doDisbandGroup\((\d+),\d+\)";
            var matches = Regex.Matches(content, groupIdPattern);
            foreach (Match match in matches)
            {
                string groupId = match.Groups[1].Value;
                string teamId = "组成员";
                if (isHead)
                {
                    string detailPage = OpenGroupTeamDetailPage(groupId, account);
                    var teamIdMatch = Regex.Match(detailPage, teamIdPattern);
                    teamId = teamIdMatch.Success ? teamIdMatch.Groups[1].Value : "";
                }

                yield return new TroopInfo()
                {
                    GroupId = match.Groups[1].Value,
                    TroopId = teamId,
                    isGroupTroop = true,
                    IsGroupHead = isHead,
                    isDefendTroop = false,
                    Name = match.Groups[2].Value,
                    AccountName = account,
                };
            }
        }

        private IEnumerable<TroopInfo> ParseTeams(string content, string account)
        {
            Regex re = new Regex("worldWarClass\\.changeMyAttackTeam\\([0-9]+,[0-9]+,[0-9]+\\)");
            var heroPattern = new Regex("<img src=\"http://static\\.tc\\.9wee\\.com/hero/\\d+/\\d+\\.gif\" titleContent=\"(.*)\"/>");

            MatchCollection ms = re.Matches(content);
            foreach (Match m in ms)
            {
                TroopInfo team = new TroopInfo();
                team.TroopId = m.Value.Split('(', ',')[1];
                team.isGroupTroop = false;
                team.isDefendTroop = false;

                string teamDetailPage = OpenTeamDetailPage(team.TroopId, account);
                var match = heroPattern.Match(teamDetailPage);
                if (match.Success)
                {
                    team.Leader = match.Groups[1].Value;
                }

                team.AccountName = account;
                yield return team;
            }
        }

        private IEnumerable<TroopInfo> ParseAttackTeams(string content, string dstcityid, string account)
        {
            Regex re = new Regex("worldWarClass\\.changeMyAttackTeam\\([0-9]+,[0-9]+,[0-9]+\\)");

            MatchCollection ms = re.Matches(content);
            int team_no = 1;
            foreach (Match m in ms)
            {
                TroopInfo team = new TroopInfo();
                team.TroopId = m.Value.Split('(', ',')[1];
                team.isGroupTroop = false;
                team.isDefendTroop = false;
                team.Leader = team_no.ToString(); //temporary solution
                string rsp = OpenTeamAttackPage(team.TroopId, dstcityid, account);
                team.Duration = TimeStr2Sec(ParseAttackDuration(rsp));

                team.AccountName = account;

                if (team.Duration > 0)
                {
                    yield return team;
                    team_no++;
                }
            }
        }

        void DismissTeam(string teamId, string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=disband_team&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            string body = string.Format("team_id={0}&from_address=1&detail_flag=0", teamId);
            HTTPRequest(url, account, body);
        }

        private IEnumerable<string> GetMoveTargetCities(string sourceCidyId, string account)
        {
            OpenCityPage(sourceCidyId, account);
            string url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=move_army&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            string content = HTTPRequest(url, account);

            return ParseTargetCityList(content);
        }

        private IEnumerable<string> ParseTargetCityList(string content)
        {
            var targetBeginLine = "目的地";
            var targetPattern = new Regex("<option value=\"\\d+\">(.*)</option>");
            var lines = content.Split('\r');
            int status = 0;
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
                this.hostname, type, page, this.randGen.NextDouble()
                );
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

        private IEnumerable<AttackTask> QueryOnlineTroopList(string eventType, string account)
        {
            const string linePattern = "<span id=\"event_(?<taskid>\\d+)\"></span>";
            const string etaPattern = @"将于&nbsp;&nbsp;(?<eta>\d\d\d\d\-\d\d\-\d\d \d\d:\d\d:\d\d)&nbsp;&nbsp;到达";
            const string cityPattern = "<a href='javascript:void\\(0\\)' onclick=\"military\\.show_attack\\('gj',\\d+,0\\);\">(?<city>[^<]+)</a><span class=\"button1\">";
            const string fromCityPattern = "<div class=\"attack_infor\">(?<city>[^&]*)&nbsp;&nbsp;返回&nbsp;&nbsp;";

            string url = string.Format(
                "http://{0}/index.php?mod=military/attack&op=show&func=military_event_list&type={1}&r={2}",
                this.hostname, eventType, this.randGen.NextDouble()
                );

            string content = HTTPRequest(url, account);

            var lines = content.Split('\r');
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

                string fromCity = "";
                string toCity = "";
                if (cityMatches.Count == 1)
                {
                    toCity = cityMatches[0].Groups["city"].Value;

                    var fromCityMatch = Regex.Match(line, fromCityPattern);
                    if (fromCityMatch.Success)
                    {
                        fromCity = fromCityMatch.Groups["city"].Value;
                    }
                }
                else if (cityMatches.Count == 2)
                {
                    fromCity = cityMatches[0].Groups["city"].Value;
                    toCity = cityMatches[1].Groups["city"].Value;
                }

                yield return new AttackTask()
                {
                    AccountName = account,
                    TaskType = line.Contains("攻击") ? "Attack" : "Return",
                    TaskId = lineMatch.Groups["taskid"].Value,
                    FromCity = fromCity,
                    ToCity = toCity,
                    EndTime = DateTime.Parse(etaMatch.Groups["eta"].Value),
                };
            }
        }

        private string OpenHeroPage(string account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=hero/hero&op=show&func=my_heros&r={1}",
                this.hostname, this.randGen.NextDouble()
                );
            return HTTPRequest(url, account);
        }

        private IEnumerable<HeroInfo> QueryDeadHeroList(string account)
        {
            var content = OpenHeroPage(account);

            return ParseHeroList(content, account);
        }

        private IEnumerable<HeroInfo> ParseHeroList(string content, string account)
        {
            const string pattern = "<li id=\"li_hero_my(?<heroid>\\d+)\" hname=\"(?<heroname>.+)\" die=(?<isdead>\\d)>";

            var matches = Regex.Matches(content, pattern);
            foreach (Match match in matches)
            {
                yield return new HeroInfo()
                {
                    AccountName = account,
                    HeroId = match.Groups["heroid"].Value,
                    Name = match.Groups["heroname"].Value,
                    IsDead = match.Groups["isdead"].Value == "1",
                };
            }
        }

        private void ReliveHero(string heroId, string account)
        {
            string url = string.Format(
                "http://{0}//index.php?mod=hero/hero&op=do&func=relive_hero&hero_id={1}&r={2}",
                this.hostname, heroId, this.randGen.NextDouble()
                );
            HTTPRequest(url, account);
        }

        private void QuickReliveHero(string heroId, AccountInfo account)
        {
            ReliveHero(heroId, account.UserName);
            var tid = GetTid(account);
            var reliveQueueId = QueryReliveQueueId(tid, account);
            var reliveItem = QueryReliveItem(reliveQueueId, tid, account);
            UserReliveItem(reliveItem, heroId, reliveQueueId, tid, account);
        }

        private void UserReliveItem(DepotItem item, string heroId, string queueId, string tid, AccountInfo account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=prop/prop&op=do&func=use_prop&prop_id={1}" + 
                "&user_prop_id={2}&hero_id={3}&pet_id=0&queue_id={4}&" + 
                "call_back=get_build_task_queue%28undefined%2C+{5}%2C+true%29&r={6}",
                this.hostname, item.PropertyID, item.UserPropertyID, heroId, queueId, tid, this.randGen.NextDouble()
                );
            HTTPRequest(url, account.UserName);
        }

        private DepotItem QueryReliveItem(string reliveQueueId, string tid, AccountInfo account)
        {
            string url = string.Format(
                "http://{0}/index.php?mod=prop/prop&op=show&func=allow_prop&type=29&queue_id={1}" + 
                "&call_back=get_build_task_queue(undefined,%20{2},%20true)&r={3}",
                this.hostname, reliveQueueId, tid, this.randGen.NextDouble()
                );
            string data = HTTPRequest(url, account.UserName);

            const string pattern = "<li onclick=\"shop\\.choose_prop\\((?<prop_id>\\d+), (?<user_prop_id>\\d+), 29\\);\">";
            var match = Regex.Match(data, pattern);
            if (!match.Success)
            {
                return null;
            }

            return new DepotItem()
            {
                GoodsType = 29,
                PropertyID = int.Parse(match.Groups["prop_id"].Value),
                UserPropertyID = int.Parse(match.Groups["user_prop_id"].Value),
            };
        }

        private string GetTid(AccountInfo account)
        {
            var cookieMap = ParseCookieStr(account.CookieStr);

            string tmp_id = string.Empty;
            if (!cookieMap.TryGetValue("tmp_id", out tmp_id))
            {
                return string.Empty;
            }

            return tmp_id;
        }

        private string QueryReliveQueueId(string tid, AccountInfo account)
        {
            string url0 = string.Format(
                "http://{0}/index.php?mod=get_data&op=do&r=P{1}",
                this.hostname, this.randGen.NextDouble()
                );

            string body0 = string.Format("module=%7B%22task%22%3A%5B{0}%2C2%5D%7D", tid);
            string taskData = HTTPRequest(url0, account.UserName, body0);

            const string taskPattern = "\"tid\":(?<tid>\\d+)";
            var taskIdMatch = Regex.Match(taskData, taskPattern);

            return taskIdMatch.Success ? taskIdMatch.Groups["tid"].Value : string.Empty;
        }
    }
}
