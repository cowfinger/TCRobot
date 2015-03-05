namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    partial class FormMain
    {
        private IEnumerable<string> GetAccountInflunceCityNameListWithArmy(string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_city_army);
            var resp = this.HTTPRequest(url, account);

            var pattern = new Regex("<td width=\"12%\">(.*)</td>");
            var matches = pattern.Matches(resp);
            return from Match match in matches select match.Groups[1].Value;
        }

        private DateTime QueryRemoteSysTime(string accountName)
        {
            var rsp = this.RefreshHomePage(accountName);
            return this.ParseSysTimeFromHomePage(rsp);
        }

        private string GetTargetCityId(string srccityid, string dstcityname, string account)
        {
            var response = this.OpenCityShowAttackPage(srccityid, account);
            return this.ParseTargetCityID(response, dstcityname);
        }

        private string CreateGroupHead(string cityId, string teamId, string account)
        {
            this.OpenCityShowAttackPage(cityId, account);
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.create_group);

            var name = this.randGen.Next(100000, 999999).ToString();
            var body = string.Format("team_id={0}&group_name={1}", teamId, name);
            this.HTTPRequest(url, account, body);
            return name;
        }

        private void JoinGroup(string cityId, string groupTroopId, string subTroopId, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.join_group);
            var body = string.Format("team_id={0}&group_id={1}&from_address=1", subTroopId, groupTroopId);
            this.HTTPRequest(url, account, body);
        }

        private IEnumerable<TroopInfo> GetTeamsInfo(string srccityid, string account)
        {
            var cityPage = this.OpenCityShowAttackPage(srccityid, account);
            return this.ParseTeams(cityPage, account);
        }

        private IEnumerable<TroopInfo> GetActiveTroopInfo(string cityId, string tabId, string account)
        {
            var url0 = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.world,
                TCSubMod.world,
                TCOperation.Show,
                TCFunc.get_node);

            this.HTTPRequest(url0, account);

            var url1 = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_city_detail,
                new TCRequestArgument(TCElement.node_id, int.Parse(cityId)));
            // var url1 =
            //     string.Format(
            //         "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r=",
            //         cityId,
            //         this.randGen.NextDouble());
            this.HTTPRequest(url1, account);

            var url2 =
                string.Format(
                    "http://{0}//index.php?mod=military/world_war&op=show&func=team&tab_id={1}&user_nickname=&r={2}",
                    this.hostname,
                    tabId,
                    this.randGen.NextDouble());
            var data = this.HTTPRequest(url2, account);

            var attackPowerList = this.ParseAttribute(data, @"<td>部队\d+</td>", @"<td>(\d+)</td>", 2).ToList();

            var i = 0;
            const string TeamIdPattern = @"worldWarClass\.doDisbandTeam\((\d+),\d+,\d+\)";
            var matches = Regex.Matches(data, TeamIdPattern);
            foreach (Match match in matches)
            {
                yield return
                    new TroopInfo
                        {
                            TroopId = match.Groups[1].Value,
                            AccountName = account,
                            PowerIndex = int.Parse(attackPowerList[i]),
                            isGroupTroop = false,
                            isDefendTroop = tabId == "2"
                        };

                ++i;
            }
        }

        private IEnumerable<TroopInfo> GetGroupTeamList(string srccityid, string account)
        {
            var response = this.OpenGroupTeamListPage(srccityid, account);
            var splitPoint = new[] { "<h4>我加入的队伍</h4>" };
            var groupSplit = response.Split(splitPoint, StringSplitOptions.None);

            if (groupSplit.Count() != 2)
            {
                return new List<TroopInfo>();
            }
            return this.ParseGroupTeams(groupSplit[0], account, true);
        }

        private IEnumerable<string> GetGroupAttackTargetCity(string cityId, string account)
        {
            var response = this.OpenGroupTeamListPage(cityId, account);
            return this.ParseTargetCityList(response);
        }

        private bool IsTeamInCity(string srcCityId, string account)
        {
            var url0 = string.Format(
                "http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            this.HTTPRequest(url0, account);

            var url1 =
                string.Format(
                    "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                    this.hostname,
                    srcCityId,
                    this.randGen.NextDouble());
            var resp = this.HTTPRequest(url1, account);

            var regex = new Regex(@"user_hero_\d+");
            return regex.Matches(resp).Count > 0;
        }

        private string OpenCreateTeamPage(string cityId, string account)
        {
            this.OpenCityShowAttackPage(cityId, account);

            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/world_war&op=show&func=create_team&team_type=1&from_address=1&r={1}",
                    this.hostname,
                    this.randGen.NextDouble());

            return this.HTTPRequest(url, account);
        }

        private IEnumerable<Soldier> ParseSoldiersInCreateTeamPage(string content)
        {
            const string pattern = "<input max_num=(\\d+) name=\"s_(\\d+)\" type=\"text\" />";

            var matches = Regex.Matches(content, pattern);
            foreach (Match match in matches)
            {
                var soldierNumber = int.Parse(match.Groups[1].Value);
                var soldierId = int.Parse(match.Groups[2].Value);
                yield return new Soldier { SoldierType = soldierId, SoldierNumber = soldierNumber };
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
            for (var i = 0; i < tempHeroList.Count; ++i)
            {
                if (statusList[i] == "空闲")
                {
                    heroList.Add(tempHeroList[i]);
                }
            }

            return heroList;
        }

        private void CreateTeam(
            string srcCityId,
            string heroId,
            string subHeroes,
            string soldier,
            string teamType,
            string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&func=create_team&op=do&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var body =
                string.Format(
                    "team_type={0}&main_hero={1}&using_embattle_id=&sub_hero={2}&soldiers={3}&pk_hero_id=",
                    teamType,
                    heroId,
                    subHeroes,
                    soldier);

            this.HTTPRequest(url, account, body);
        }

        private void OpenCityPage(string cityId, ref HttpClient client)
        {
            // string url0 = string.Format(
            //     "http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}",
            //     this.hostname, this.randGen.NextDouble()
            //     );
            // client.OpenUrl(url0);

            var url1 =
                string.Format(
                    "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                    this.hostname,
                    cityId,
                    this.randGen.NextDouble());
            client.OpenUrl(url1);

            // string url2 = string.Format(
            //     "http://{0}/index.php?mod=military/world_war&op=show&func=attack&team_id=0&r={1}",
            //     this.hostname, this.randGen.NextDouble()
            //     );
            // return client.OpenUrl(url2);
        }

        private string OpenCityBuildPage(string cityId, string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=city_build&node_id={1}&r={2}",
                this.hostname, cityId, this.randGen.NextDouble());
            return HTTPRequest(url, account);
        }

        private int ParseRoadLevelFromCityBuildPage(string page)
        {
            const string pattern = @"<span>等级：(\d+)/10</span>";
            var match = Regex.Match(page, pattern);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            else
            {
                return 6;
            }
        }

        private string OpenCityPage(string cityId, string account)
        {
            var url1 =
                string.Format(
                    "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                    this.hostname,
                    cityId,
                    this.randGen.NextDouble());
            return this.HTTPRequest(url1, account);
        }

        private string OpenCityShowAttackPage(string srccityid, string account)
        {
            var url0 = string.Format(
                "http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            this.HTTPRequest(url0, account);

            var url1 =
                string.Format(
                    "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                    this.hostname,
                    srccityid,
                    this.randGen.NextDouble());
            this.HTTPRequest(url1, account);

            var url2 = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=attack&team_id=0&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            return this.HTTPRequest(url2, account);
        }

        private string OpenGroupTeamListPage(string srccityid, string account)
        {
            var url0 = string.Format(
                "http://{0}/index.php?mod=world/world&op=show&func=get_node&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            this.HTTPRequest(url0, account);

            var url1 =
                string.Format(
                    "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city_detail&node_id={1}&r={2}",
                    this.hostname,
                    srccityid,
                    this.randGen.NextDouble());
            this.HTTPRequest(url1, account);

            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=join_attack&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private string OpenTeamDetailPage(string teamId, string account)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/world_war&op=show&func=team_detail&team_id={1}&from_address=1&r={2}",
                    this.hostname,
                    teamId,
                    this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private string OpenGroupTeamDetailPage(string teamId, string account)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/world_war&op=show&func=group_detail&group_id={1}&r={2}",
                    this.hostname,
                    teamId,
                    this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private string OpenGroupAttackPage(string groupId, string cityId, ref HttpClient httpClient)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/world_war&op=show&func=join_attack_confirm&group_id={1}&to_city_id={2}&join_attack_type=1&r={3}",
                    this.hostname,
                    groupId,
                    cityId,
                    this.randGen.NextDouble());
            return httpClient.OpenUrl(url);
        }

        private string OpenGroupAttackPage(string groupId, string cityId, string account)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/world_war&op=show&func=join_attack_confirm&group_id={1}&to_city_id={2}&join_attack_type=1&r={3}",
                    this.hostname,
                    groupId,
                    cityId,
                    this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private IEnumerable<string> ParseAttribute(
            string content,
            string headPattern,
            string attributePattern,
            int attributeIndex)
        {
            // var pattern = new Regex(@"<td>(.*)</td>");
            // var headName = string.Format("<td>{0}</td>", attributeName);
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
            return this.ParseTargetCityList(page);
        }

        private string OpenTeamAttackPage(string teamid, string cityid, ref HttpClient httpClient)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/world_war&op=show&func=attack_confirm&team_id={1}&to_city_id={2}&r={3}",
                    this.hostname,
                    teamid,
                    cityid,
                    this.randGen.NextDouble());
            return httpClient.OpenUrl(url);
        }

        private string OpenTeamAttackPage(string teamid, string cityid, string account)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/world_war&op=show&func=attack_confirm&team_id={1}&to_city_id={2}&r={3}",
                    this.hostname,
                    teamid,
                    cityid,
                    this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private string TeamAttackTarget(string team_id, string city_id, ref HttpClient httpClient)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=attack&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var body = string.Format("team_id={0}&to_city_id={1}", team_id, city_id);
            return httpClient.OpenUrl(url, body);
        }

        private void TeamAttackTarget(string team_id, string city_id, string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=attack&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var body = string.Format("team_id={0}&to_city_id={1}", team_id, city_id);
            this.HTTPRequest(url, account, body);
        }

        private void GroupAttackTarget(string groupId, string cityId, ref HttpClient httpClient)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=join_attack&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var body = string.Format("group_id={0}&to_city_id={1}&join_attack_type=1", groupId, cityId);
            httpClient.OpenUrl(url, body);
        }

        private void GroupAttackTarget(string groupId, string cityId, string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=join_attack&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var body = string.Format("group_id={0}&to_city_id={1}&join_attack_type=1", groupId, cityId);
            var ret = this.HTTPRequest(url, account, body);
        }

        private string RefreshHomePage(string account)
        {
            var url = "http://" + this.hostname + "/index.php";
            return this.HTTPRequest(url, account);
        }

        private DateTime ParseSysTimeFromHomePage(string content)
        {
            var re = new Regex("wee\\.timer\\.set_time\\( [0-9]+ \\);");

            var ms = re.Matches(content);
            foreach (Match m in ms)
            {
                var timestr = m.Value.Split('(', ')')[1];
                var ret = new DateTime(1970, 1, 1);
                var sec = Int32.Parse(timestr.Trim(' '));
                ret = ret.AddSeconds(sec);
                return ret.ToLocalTime();
            }
            return DateTime.MinValue;
        }

        private string ParseTargetCityID(string content, string keyword)
        {
            var re = new Regex("<option value=\\\"([0-9]+)\\\">([^<]*)</option>");

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
            var strs = input.Split(':');
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

        private IEnumerable<TroopInfo> ParseTeams(string content, string account)
        {
            var re = new Regex("worldWarClass\\.changeMyAttackTeam\\([0-9]+,[0-9]+,[0-9]+\\)");
            var heroPattern =
                new Regex("<img src=\"http://static\\.tc\\.9wee\\.com/hero/\\d+/\\d+\\.gif\" titleContent=\"(.*)\"/>");

            var ms = re.Matches(content);
            foreach (Match m in ms)
            {
                var team = new TroopInfo();
                team.TroopId = m.Value.Split('(', ',')[1];
                team.isGroupTroop = false;
                team.isDefendTroop = false;

                var teamDetailPage = this.OpenTeamDetailPage(team.TroopId, account);
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
            var re = new Regex("worldWarClass\\.changeMyAttackTeam\\([0-9]+,[0-9]+,[0-9]+\\)");

            var ms = re.Matches(content);
            var team_no = 1;
            foreach (Match m in ms)
            {
                var team = new TroopInfo();
                team.TroopId = m.Value.Split('(', ',')[1];
                team.isGroupTroop = false;
                team.isDefendTroop = false;
                team.Leader = team_no.ToString(); //temporary solution
                var rsp = this.OpenTeamAttackPage(team.TroopId, dstcityid, account);
                team.Duration = this.TimeStr2Sec(this.ParseAttackDuration(rsp));

                team.AccountName = account;

                if (team.Duration > 0)
                {
                    yield return team;
                    team_no++;
                }
            }
        }

        private void DismissTeam(string teamId, string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=disband_team&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var body = string.Format("team_id={0}&from_address=1&detail_flag=0", teamId);
            this.HTTPRequest(url, account, body);
        }

        private IEnumerable<string> GetMoveTargetCities(string sourceCidyId, string account)
        {
            this.OpenCityShowAttackPage(sourceCidyId, account);
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=move_army&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var content = this.HTTPRequest(url, account);

            return this.ParseTargetCityList(content);
        }

        private IEnumerable<string> ParseTargetCityList(string content)
        {
            var targetBeginLine = "目的地";
            var targetPattern = new Regex("<option value=\"\\d+\">(.*)</option>");
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

        private IEnumerable<KeyValuePair<long, long>> ParseInfluenceResource(string page)
        {
            const string pattern = "<div class=\"num\">(\\d+)/(\\d+)</div>";
            var matches = Regex.Matches(page, pattern);
            foreach (Match match in matches)
            {
                var Key = long.Parse(match.Groups[1].Value);
                var Value = long.Parse(match.Groups[2].Value);
                yield return new KeyValuePair<long, long>(Key, Value);
            }
        }

        private string OpenInfluenceSciencePage(string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=influence/science&op=show&func=science&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private void DonateResource(string account, long val1, long val2, long val3, long val4)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=influence/influence&op=do&func=influence_donate&res={1}|{2}|{3}|{4}|0|0&r={5}",
                    this.hostname,
                    val1,
                    val2,
                    val3,
                    val4,
                    this.randGen.NextDouble());
            this.HTTPRequest(url, account);
        }

        private IEnumerable<long> GetAccountResources(string account)
        {
            const string pattern = "value=this\\.innerHTML;\">(\\d+)</span>\\)";

            var url = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_donate&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var page = this.HTTPRequest(url, account);

            var matches = Regex.Matches(page, pattern);
            foreach (Match match in matches)
            {
                yield return long.Parse(match.Groups[1].Value);
            }
        }

        private string OpenAccountDepot(string account, int type, int page)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=depot/depot&op=show&func=my_depot&goods_type={1}&page={2}&r={3}",
                    this.hostname,
                    type,
                    page,
                    this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private IEnumerable<DepotItem> ParseDepotItems(string page)
        {
            const string Pattern = @"prop\.use_prop\((\d+), (\d+), (\d+), (\d+)\)";
            var matches = Regex.Matches(page, Pattern);
            return from Match match in matches
                   select new DepotItem
                              {
                                  PropertyID = int.Parse(match.Groups[1].Value),
                                  UserPropertyID = int.Parse(match.Groups[3].Value),
                                  GoodsType = int.Parse(match.Groups[2].Value)
                              };
        }

        private int ParseMaxPageID(string page)
        {
            const string Pattern = "<span class=\"page_on\">\\d+/(\\d+)</span>";
            var match = Regex.Match(page, Pattern);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }

            return 0;
        }

        private IEnumerable<DepotItem> EnumDepotItems(string account)
        {
            var maxPage = 1;
            var curPage = 1;
            do
            {
                var page = this.OpenAccountDepot(account, 1, curPage);
                maxPage = this.ParseMaxPageID(page);

                var items = this.ParseDepotItems(page);
                foreach (var item in items)
                {
                    yield return item;
                }

                ++curPage;
            }
            while (curPage <= maxPage);
        }

        private void UseDepotItems(string account, DepotItem item, int count)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=prop/prop&op=do&func=use_prop&p_type=7&prop_id={1}&prop_num={2}&user_prop_id={3}&from=1&call_back=shop.reflash_res_depot(5,%201)&r={4}",
                    this.hostname,
                    item.PropertyID,
                    count,
                    item.UserPropertyID,
                    this.randGen.NextDouble());
            this.HTTPRequest(url, account);
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

        private void DismissGroup(string groupId, string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=do&func=disband_group&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            var body = string.Format("group_id={0}&from_address=1", groupId);
            this.HTTPRequest(url, account, body);
        }

        private IEnumerable<AttackTask> QueryOnlineTroopList(string eventType, string account)
        {
            const string linePattern = "<span id=\"event_(?<taskid>\\d+)\"></span>";
            const string etaPattern = @"将于&nbsp;&nbsp;(?<eta>\d\d\d\d\-\d\d\-\d\d \d\d:\d\d:\d\d)&nbsp;&nbsp;到达";
            const string cityPattern =
                "<a href='javascript:void\\(0\\)' onclick=\"military\\.show_attack\\('gj',\\d+,0\\);\">(?<city>[^<]+)</a><span class=\"button1\">";
            const string fromCityPattern = "<div class=\"attack_infor\">(?<city>[^&]*)&nbsp;&nbsp;返回&nbsp;&nbsp;";

            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/attack&op=show&func=military_event_list&type={1}&r={2}",
                    this.hostname,
                    eventType,
                    this.randGen.NextDouble());

            var content = this.HTTPRequest(url, account);

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

                yield return
                    new AttackTask
                        {
                            AccountName = account,
                            TaskType = line.Contains("攻击") ? "Attack" : "Return",
                            TaskId = lineMatch.Groups["taskid"].Value,
                            FromCity = fromCity,
                            ToCity = toCity,
                            EndTime = DateTime.Parse(etaMatch.Groups["eta"].Value)
                        };
            }
        }

        private string OpenHeroPage(string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=hero/hero&op=show&func=my_heros&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private IEnumerable<HeroInfo> QueryHeroList(string account)
        {
            var content = this.OpenHeroPage(account);

            return this.ParseHeroList(content, account);
        }

        private IEnumerable<HeroInfo> ParseHeroList(string content, string account)
        {
            const string pattern = "<li id=\"li_hero_my(?<heroid>\\d+)\" hname=\"(?<heroname>.+)\" die=(?<isdead>\\d)>";

            var matches = Regex.Matches(content, pattern);
            return from Match match in matches
                   select new HeroInfo
                              {
                                  AccountName = account,
                                  HeroId = match.Groups["heroid"].Value,
                                  Name = match.Groups["heroname"].Value,
                                  IsDead = match.Groups["isdead"].Value == "1"
                              };
        }

        private void ReliveHero(string heroId, string account)
        {
            var url = string.Format(
                "http://{0}//index.php?mod=hero/hero&op=do&func=relive_hero&hero_id={1}&r={2}",
                this.hostname,
                heroId,
                this.randGen.NextDouble());
            this.HTTPRequest(url, account);
        }

        private void QuickReliveHero(string heroId, AccountInfo account)
        {
            this.ReliveHero(heroId, account.UserName);
            var tid = this.GetTid(account);
            var reliveQueueId = this.QueryReliveQueueId(tid, account);
            var reliveItem = this.QueryReliveItem(reliveQueueId, tid, account);
            this.UserReliveItem(reliveItem, heroId, reliveQueueId, tid, account);
        }

        private void UserReliveItem(DepotItem item, string heroId, string queueId, string tid, AccountInfo account)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=prop/prop&op=do&func=use_prop&prop_id={1}"
                    + "&user_prop_id={2}&hero_id={3}&pet_id=0&queue_id={4}&"
                    + "call_back=get_build_task_queue%28undefined%2C+{5}%2C+true%29&r={6}",
                    this.hostname,
                    item.PropertyID,
                    item.UserPropertyID,
                    heroId,
                    queueId,
                    tid,
                    this.randGen.NextDouble());
            this.HTTPRequest(url, account.UserName);
        }

        private DepotItem QueryReliveItem(string reliveQueueId, string tid, AccountInfo account)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=prop/prop&op=show&func=allow_prop&type=29&queue_id={1}"
                    + "&call_back=get_build_task_queue(undefined,%20{2},%20true)&r={3}",
                    this.hostname,
                    reliveQueueId,
                    tid,
                    this.randGen.NextDouble());
            var data = this.HTTPRequest(url, account.UserName);

            const string pattern =
                "<li onclick=\"shop\\.choose_prop\\((?<prop_id>\\d+), (?<user_prop_id>\\d+), 29\\);\">";
            var match = Regex.Match(data, pattern);
            if (!match.Success)
            {
                return null;
            }

            return new DepotItem
                       {
                           GoodsType = 29,
                           PropertyID = int.Parse(match.Groups["prop_id"].Value),
                           UserPropertyID = int.Parse(match.Groups["user_prop_id"].Value)
                       };
        }

        private string GetTid(AccountInfo account)
        {
            var cookieMap = this.ParseCookieStr(account.CookieStr);

            string tmp_id;
            return !cookieMap.TryGetValue("tmp_mid", out tmp_id) ? string.Empty : tmp_id;
        }

        private string QueryReliveQueueId(string tid, AccountInfo account)
        {
            var url0 = string.Format(
                "http://{0}/index.php?mod=get_data&op=do&r={1}",
                this.hostname,
                this.randGen.NextDouble());

            var body0 = string.Format("module=%7B%22task%22%3A%5B{0}%2C2%5D%7D", tid);
            var taskData = this.HTTPRequest(url0, account.UserName, body0);

            const string taskPattern = "\"tid\":(?<tid>\\d+)";
            var taskIdMatch = Regex.Match(taskData, taskPattern);

            return taskIdMatch.Success ? taskIdMatch.Groups["tid"].Value : string.Empty;
        }

        private string OpenMoveTroopPage(string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=move_army&r={1}",
                this.hostname,
                this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private string OpenMoveTaskQueue(string account)
        {
            var url = string.Format(
                "http://{0}/index.php?mod=military/world_war&op=show&func=move_army_queue&r={1}",
                this.hostname, this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private IEnumerable<MoveTask> ParseMoveTaskList(string page)
        {
            const string eventPattern = "id=\"event_(?<taskId>\\d+)\"";
            const string etaPattern = "预计于(?<eta>.+?)到达";

            var eventMatches = Regex.Matches(page, eventPattern);
            var etaMatches = Regex.Matches(page, etaPattern);

            var eventIdList = (from Match eventId in eventMatches
                              select eventId.Groups["taskId"].Value).ToList();

            var etaList = (from Match eta in etaMatches
                          select DateTime.Parse(eta.Groups["eta"].Value)).ToList();

            for (int i = 0; i < eventIdList.Count; ++i)
            {
                yield return new MoveTask()
                {
                    EndTime = etaList[i],
                    TaskId = eventIdList[i],
                };
            }
        }

        private IEnumerable<string> ParseHeroIdListFromCityPage(string page)
        {
            const string pattern = "<li id=\"user_hero_(?<heroId>\\d+)\" style=\"display:block;\">";
            var matches = Regex.Matches(page, pattern);
            return from Match match in matches
                   select match.Groups["heroId"].Value;
        }

        private IEnumerable<string> ParseHeroIdListFromMovePage(string page)
        {
            //const string pattern = "<li id=\"move_hero_(\\d+)\">";
            const string pattern = "hero_id=\"(\\d+)\" hero_status=\"1\"";
            var matches = Regex.Matches(page, pattern);
            return from Match match in matches select match.Groups[1].Value;
        }

        private int ParseBrickNumberFromMovePage(string page)
        {
            const string pattern = "<span id=\"brick_num_max\">\\d+</span>/(\\d+)</span>\\)</span>";
            var match = Regex.Match(page, pattern);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }
            return 0;
        }

        private string ConfirmMoveTroop(
            string fromCityId,
            string toCityId,
            string soldierString,
            string heroString,
            int brickCount,
            string account)
        {
            string url = string.Format("http://{0}/index.php?mod=military/world_war&op=do&func=move_army&r={1}",
                this.hostname, this.randGen.NextDouble());
            string body = string.Format("from_city_id={0}&to_city_id={1}&soldier={2}&hero={3}&brick_num={4}",
                fromCityId, toCityId, soldierString, heroString, brickCount > 0 ? brickCount.ToString() : "");
            return HTTPRequest(url, account, body);
        }

        private IEnumerable<CityInfo> QueryInfluenceCityList(string account)
        {
            this.OpenAccountFirstCity(account);
            var content = this.OpenMoveTroopPage(account);

            return ParseCityListFromMoveTroopPage(content);

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

        private string ChangeMoveFromCity(string account, string fromCityId)
        {
            var url =
                string.Format(
                    "http://{0}/index.php?mod=military/world_war&op=show&func=move_army&from_city_id={1}&r={2}",
                    this.hostname,
                    fromCityId,
                    this.randGen.NextDouble());
            return this.HTTPRequest(url, account);
        }

        private Dictionary<string, HashSet<string>> BuildInfluenceCityMap(
            IEnumerable<CityInfo> influenceCityList,
            string account)
        {
            const string cityPattern = "<option value=\"(?<nodeId>\\d+)\"\\s*>(?<name>[^<]+)</option>";

            var map = new Dictionary<string, HashSet<string>>();
            foreach (var cityInfo in influenceCityList)
            {
                var cityMovePage = this.ChangeMoveFromCity(account, cityInfo.NodeId.ToString());
                var contentParts = cityMovePage.Split(new[] { "目的地：" }, StringSplitOptions.RemoveEmptyEntries);
                if (contentParts.Count() < 2)
                {
                    continue;
                }

                var toCityMatches = Regex.Matches(contentParts[1], cityPattern);

                var toSet = new HashSet<string>();
                foreach (Match toCityMatch in toCityMatches)
                {
                    toSet.Add(toCityMatch.Groups["name"].Value);
                }

                map.Add(cityInfo.Name, toSet);
            }

            return map;
        }

        private void OpenAccountFirstCity(string account)
        {
            const string pattern =
                @"index\.php\?mod=influence/influence&op=show&func=influence_city_detail&node_id=(\d+)";
            var url = string.Format(
                "http://{0}/index.php?mod=influence/influence&op=show&func=influence_city&r={1}",
                this.hostname,
                this.randGen.NextDouble());

            var page = this.HTTPRequest(url, account);

            var match = Regex.Match(page, pattern);
            if (match.Success)
            {
                var cityUrl = string.Format(
                    "http://{0}/{1}&r={2}",
                    this.hostname,
                    match.Value,
                    this.randGen.NextDouble());
                this.HTTPRequest(cityUrl, account);
            }
        }

        private IEnumerable<Soldier> ParseSoldierInfoFromCityPage(string page)
        {
            const string pattern =
                "<li><img src=\"http://.+?/soldier/(?<typeId>\\d+)\\.gif\" titleContent=\"(?<name>.+?)\"/><span>(?<count>\\d+)</span></li>";
            var matches = Regex.Matches(page, pattern);
            return from Match match in matches
                   select new Soldier
                              {
                                  Name = match.Groups["name"].Value,
                                  SoldierType = int.Parse(match.Groups["typeId"].Value),
                                  SoldierNumber = int.Parse(match.Groups["count"].Value)
                              };
        }

        private IEnumerable<string> ParseHeroNameListFromCityPage(string page)
        {
            const string pattern = "<div class=\"name\">(?<heroName>.+?)</div>";
            var matches = Regex.Matches(page, pattern);
            return from Match match in matches select match.Groups["heroName"].Value;
        }
    }
}