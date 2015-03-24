namespace TC
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    partial class FormMain
    {
        private string RefreshHomePage(string account)
        {
            var url = "http://" + this.hostname + "/index.php";
            return this.HTTPRequest(url, account);
        }

        private int GetAccountLevel(AccountInfo account)
        {
            const string pattern = @"<h4>提升等级至(\d+)：</h4>";
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.city,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.load_city,
                new TCRequestArgument(TCElement.cid, account.MainCity.NodeId),
                new TCRequestArgument(TCElement.mt, 1));
            var page = this.HTTPRequest(url, account.UserName);
            var match = Regex.Match(page, pattern);
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value) - 1;
            }

            return -1;
        }

        private void OpenAccountFirstCity(string account)
        {
            const string pattern = @"mod=influence/influence&op=show&func=influence_city_detail&node_id=(\d+)";

            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_city);

            var page = this.HTTPRequest(url, account);

            var match = Regex.Match(page, pattern);
            if (match.Success)
            {
                var cityUrl = RequestAgent.BuildUrl(this.hostname, match.Value);
                this.HTTPRequest(cityUrl, account);
            }
        }

        private string OpenMoveTroopPage(string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.move_army);
            return this.HTTPRequest(url, account);
        }

        private string QueryReliveQueueId(string tid, AccountInfo account)
        {
            var url = RequestAgent.BuildUrl(this.hostname, "mod=get_data&op=do");
            var body = string.Format("module=%7B%22task%22%3A%5B{0}%2C2%5D%7D", tid);
            var taskData = this.HTTPRequest(url, account.UserName, body);

            const string taskPattern = "\"tid\":(?<tid>\\d+)";
            var taskIdMatch = Regex.Match(taskData, taskPattern);

            return taskIdMatch.Success ? taskIdMatch.Groups["tid"].Value : string.Empty;
        }

        private void UserReliveItem(DepotItem item, string heroId, string queueId, string tid, AccountInfo account)
        {
            var callBack = string.Format("get_build_task_queue%28undefined%2C+{0}%2C+true%29", tid);
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.prop,
                TCSubMod.prop,
                TCOperation.Do,
                TCFunc.use_prop,
                new TCRequestArgument(TCElement.prop_id, item.PropertyID),
                new TCRequestArgument(TCElement.user_prop_id, item.UserPropertyID),
                new TCRequestArgument(TCElement.hero_id, heroId),
                new TCRequestArgument(TCElement.queue_id, queueId),
                new TCRequestArgument(TCElement.call_back, callBack));
            this.HTTPRequest(url, account.UserName);
        }

        private DepotItem QueryReliveItem(string reliveQueueId, string tid, AccountInfo account)
        {
            var callBack = string.Format("get_build_task_queue(undefined,%20{0},%20true)", tid);
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.prop,
                TCSubMod.prop,
                TCOperation.Show,
                TCFunc.allow_prop,
                new TCRequestArgument(TCElement.type, 29),
                new TCRequestArgument(TCElement.queue_id, reliveQueueId),
                new TCRequestArgument(TCElement.call_back, callBack));
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

        private void ReliveHero(string heroId, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.hero,
                TCSubMod.hero,
                TCOperation.Do,
                TCFunc.relive_hero,
                new TCRequestArgument(TCElement.hero_id, heroId));
            this.HTTPRequest(url, account);
        }

        private IEnumerable<AttackTask> QueryOnlineTroopList(string eventType, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.attack,
                TCOperation.Show,
                TCFunc.military_event_list,
                new TCRequestArgument(TCElement.type, eventType));
            var content = this.HTTPRequest(url, account);
            return this.ParseOnlineTroopList(content, account);
        }

        private string OpenHeroPage(string account)
        {
            var url = RequestAgent.BuildUrl(this.hostname, TCMod.hero, TCSubMod.hero, TCOperation.Show, TCFunc.my_heros);
            return this.HTTPRequest(url, account);
        }

        private void DismissGroup(string groupId, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.disband_group);
            var body = string.Format("group_id={0}&from_address=1", groupId);
            this.HTTPRequest(url, account, body);
        }

        private void UseDepotItems(string account, DepotItem item, int count)
        {
            const string callBack = "shop.reflash_res_depot(5,%201)";
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.prop,
                TCSubMod.prop,
                TCOperation.Do,
                TCFunc.use_prop,
                new TCRequestArgument(TCElement.p_type, 7),
                new TCRequestArgument(TCElement.prop_id, item.PropertyID),
                new TCRequestArgument(TCElement.prop_num, count),
                new TCRequestArgument(TCElement.user_prop_id, item.UserPropertyID),
                new TCRequestArgument(TCElement.from, 1),
                new TCRequestArgument(TCElement.call_back, callBack));
            this.HTTPRequest(url, account);
        }

        private string OpenAccountDepot(string account, int type, int page)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.depot,
                TCSubMod.depot,
                TCOperation.Show,
                TCFunc.my_depot,
                new TCRequestArgument(TCElement.goods_type, type),
                new TCRequestArgument(TCElement.page, page));
            return this.HTTPRequest(url, account);
        }

        private string OpenInfluenceSciencePage(string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.influence,
                TCSubMod.science,
                TCOperation.Show,
                TCFunc.science);
            return this.HTTPRequest(url, account);
        }

        private string DonateResource(string account, long val1, long val2, long val3, long val4)
        {
            var resValue = string.Format("{0}|{1}|{2}|{3}|0|0", val1, val2, val3, val4);
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.influence_donate,
                new TCRequestArgument(TCElement.res, resValue));
            return this.HTTPRequest(url, account);
        }

        private IEnumerable<long> GetAccountResources(string account)
        {
            const string pattern = "value=this\\.innerHTML;\">(\\d+)</span>\\)";

            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_donate);
            var page = this.HTTPRequest(url, account);

            var matches = Regex.Matches(page, pattern);
            foreach (Match match in matches)
            {
                yield return long.Parse(match.Groups[1].Value);
            }
        }

        private void DismissTeam(string teamId, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.disband_team);
            var body = string.Format("team_id={0}&from_address=1&detail_flag=0", teamId);
            this.HTTPRequest(url, account, body);
        }

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
            var url = RequestAgent.GetTimeUrl(this.hostname);
            var date = this.HTTPRequest(url, accountName);
            var utcStart = new DateTime(1970, 1, 1);
            var diff = utcStart - DateTime.MinValue;
            int seconds;
            return int.TryParse(date, out seconds)
                       ? new DateTime(seconds * TimeSpan.TicksPerSecond + diff.Ticks)
                       : DateTime.MinValue;
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

        private void JoinGroup(string groupTroopId, string subTroopId, string account)
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
            this.HTTPRequest(url1, account);

            var url2 = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.team,
                new TCRequestArgument(TCElement.tab_id, tabId),
                new TCRequestArgument(TCElement.user_nickname));

            var data = this.HTTPRequest(url2, account);

            var attackPowerList = ParseAttribute(data, @"<td>部队\d+</td>", @"<td>(\d+)</td>", 2).ToList();

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

        private string OpenCreateTeamPage(string cityId, string account)
        {
            this.OpenCityShowAttackPage(cityId, account);

            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.create_team,
                new TCRequestArgument(TCElement.team_type, 1),
                new TCRequestArgument(TCElement.from_address, 1));

            return this.HTTPRequest(url, account);
        }

        private static IEnumerable<Soldier> ParseSoldiersInCreateTeamPage(string content)
        {
            const string Pattern = "<input max_num=(\\d+) name=\"s_(\\d+)\" type=\"text\" />";
            var matches = Regex.Matches(content, Pattern);
            return from Match match in matches
                   let soldierNumber = int.Parse(match.Groups[1].Value)
                   let soldierId = int.Parse(match.Groups[2].Value)
                   select new Soldier { SoldierType = soldierId, SoldierNumber = soldierNumber };
        }

        private static IEnumerable<string> ParseHerosInCreateTeamPage(string content)
        {
            var pattern = new Regex(@"worldWarClass\.selectHero\('(\d+)',true\);");

            var matches = pattern.Matches(content);
            var tempHeroList = (from Match match in matches select match.Groups[1].Value).ToList();

            var statusPattern = new Regex("<p class=\"trans_70\">(.*)</p>");
            var statusMatches = statusPattern.Matches(content);
            var statusList = (from Match match in statusMatches select match.Groups[1].Value).ToList();

            return tempHeroList.Where((t, i) => statusList[i] == "空闲").ToList();
        }

        private void CreateTeam(string heroId, string subHeroes, string soldier, string teamType, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Do,
                TCFunc.create_team);

            var body =
                string.Format(
                    "team_type={0}&main_hero={1}&using_embattle_id=&sub_hero={2}&soldiers={3}&pk_hero_id=",
                    teamType,
                    heroId,
                    subHeroes,
                    soldier);

            this.HTTPRequest(url, account, body);
        }

        private string OpenCityShowAttackPage(string srccityid, string account)
        {
            var url0 = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.world,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.get_node);
            this.HTTPRequest(url0, account);

            var url1 = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_city_detail,
                new TCRequestArgument(TCElement.node_id, srccityid));
            this.HTTPRequest(url1, account);

            var url2 = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.attack,
                new TCRequestArgument(TCElement.team_id, 0));
            return this.HTTPRequest(url2, account);
        }

        private string OpenGroupTeamListPage(string srccityid, string account)
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
                new TCRequestArgument(TCElement.node_id, srccityid));
            this.HTTPRequest(url1, account);

            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.join_attack);
            return this.HTTPRequest(url, account);
        }

        private string OpenTeamDetailPage(string teamId, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.team_detail,
                new TCRequestArgument(TCElement.team_id, teamId),
                new TCRequestArgument(TCElement.from_address, 1));
            return this.HTTPRequest(url, account);
        }

        private string OpenGroupTeamDetailPage(string groupId, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.group_detail,
                new TCRequestArgument(TCElement.group_id, groupId));
            return this.HTTPRequest(url, account);
        }

        private string OpenGroupAttackPage(string groupId, string cityId, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.join_attack_confirm,
                new TCRequestArgument(TCElement.group_id, groupId),
                new TCRequestArgument(TCElement.to_city_id, cityId),
                new TCRequestArgument(TCElement.join_attack_type, 1));
            return this.HTTPRequest(url, account);
        }

        private string OpenTeamAttackPage(string teamid, string cityid, string account)
        {
            var url = RequestAgent.BuildUrl(
                this.hostname,
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.attack_confirm,
                new TCRequestArgument(TCElement.team_id, teamid),
                new TCRequestArgument(TCElement.to_city_id, cityid));
            return this.HTTPRequest(url, account);
        }
    }
}
