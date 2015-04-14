namespace TC
{
    using System;
    using System.Collections.Generic;
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
            var page = TCPage.City.ShowUpdateCity.Open(account.WebAgent, account.Tid);
            return page.CurrentLevel;

            // const string pattern = @"<h4>提升等级至(\d+)：</h4>";
            // var url = RequestAgent.BuildUrl(
            //     this.hostname,
            //     TCMod.city,
            //     TCSubMod.influence,
            //     TCOperation.Show,
            //     TCFunc.load_city,
            //     new TCRequestArgument(TCElement.cid, account.Tid),
            //     new TCRequestArgument(TCElement.mt, 1));
            // var page = this.HTTPRequest(url, account.UserName);
            // var match = Regex.Match(page, pattern);
            // if (match.Success)
            // {
            //     return int.Parse(match.Groups[1].Value) - 1;
            // }

            // return -1;
        }

        private int QueryReliveQueueId(int tid, AccountInfo account)
        {
            var url = RequestAgent.BuildUrl(this.hostname, "mod=get_data&op=do");
            var body = string.Format("module=%7B%22task%22%3A%5B{0}%2C2%5D%7D", tid);
            var taskData = account.WebAgent.WebClient.OpenUrl(url, body);

            const string taskPattern = "\"tid\":(?<tid>\\d+)";
            var taskIdMatch = Regex.Match(taskData, taskPattern);

            return taskIdMatch.Success ? int.Parse(taskIdMatch.Groups["tid"].Value) : 0;
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
                            TroopId = int.Parse(match.Groups[1].Value),
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
            return ParseTargetCityList(response);
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

            var statusPattern = new Regex(@"<p class=""trans_70"">(.*)</p>");
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

        private string OpenGroupAttackPage(int groupId, string cityId, string account)
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

        private string OpenTeamAttackPage(int teamid, string cityid, string account)
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