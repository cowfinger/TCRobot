﻿namespace TC.TCPage.WorldWar
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ShowTeam
    {
        public const string TeamLinePattern =
            @"<tr>\s*" + @"<td>(?<accountName>.+?)</td>\s*" + @"<td>(?<heroNum>\d+)</td>\s*"
            + @"<td>(?<attackPower>\d+)</td>\s*" + @"<td>(?<defendPower>\d+)</td>\s*" + @"<td>(?<status>.+?)</td>\s*"
            + @"<td>\s*" + @"<div.*?worldWarClass\.showTeamDetail\((?<teamId>\d+),\d+\)";

        public ShowTeam(string page)
        {
            if (page.Contains("alert"))
            {
                this.TeamList = new List<TeamInfo>();
                return;
            }
            var teamLineMatches = Regex.Matches(page, TeamLinePattern, RegexOptions.Singleline);
            this.TeamList = from Match match in teamLineMatches
                            select
                                new TeamInfo
                                    {
                                        AccountName = match.Groups["accountName"].Value,
                                        TeamId = int.Parse(match.Groups["teamId"].Value),
                                        AttackPower = int.Parse(match.Groups["attackPower"].Value),
                                        DefendPower = int.Parse(match.Groups["defendPower"].Value),
                                        HeroNum = int.Parse(match.Groups["heroNum"].Value)
                                    };
        }

        public IEnumerable<TeamInfo> TeamList { get; private set; }

        public static ShowTeam Open(RequestAgent agent, int tabId)
        {
            var url = agent.BuildUrl(
                TCMod.military,
                TCSubMod.world_war,
                TCOperation.Show,
                TCFunc.team,
                new TCRequestArgument(TCElement.tab_id, tabId),
                new TCRequestArgument(TCElement.user_nickname));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowTeam(rawPage);
        }

        public class TeamInfo
        {
            public string AccountName { get; set; }

            public int HeroNum { get; set; }

            public int AttackPower { get; set; }

            public int DefendPower { get; set; }

            public string Status { get; set; }

            public int TeamId { get; set; }
        }
    }
}