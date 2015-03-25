using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage
{
    using System.Security.Policy;
    using System.Text.RegularExpressions;

    class PoliticsShowDraftPage
    {
        public const string LeftTimesPattern = @"<tr>\s*<td>今日剩余次数</td>\s*<td>(\d+)</td>\s*</tr>";

        public const string HeroEfficencyPattern = "\"(?<heroId>\\d+)\":\\{\"army\":\\{\"\\d+\":(?<soldierNum>\\d+),";

        public const string SoldierIdPattern = @"civil_draft_soldier_radio_(\d+)";

        public int LeftTimes { get; private set; }

        public int EfficientHeroId { get; private set; }

        public HashSet<int> SoldierIdSet { get; private set; }

        public static PoliticsShowDraftPage Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(TCMod.civil, TCSubMod.politics, TCOperation.Show, TCFunc.draft);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new PoliticsShowDraftPage(rawPage);
        }

        public PoliticsShowDraftPage(string page)
        {
            var timesLeftMatch = Regex.Match(page, LeftTimesPattern, RegexOptions.Singleline);
            this.LeftTimes = timesLeftMatch.Success ? int.Parse(timesLeftMatch.Groups[1].Value) : 0;

            var heroEfficencyMatches = Regex.Matches(page, HeroEfficencyPattern, RegexOptions.Singleline);
            var heroEfficencyList = from Match match in heroEfficencyMatches
                                    let heroId = int.Parse(match.Groups["heroId"].Value)
                                    let soldierNum = int.Parse(match.Groups["soldierNum"].Value)
                                    select new { heroId, soldierNum };

            int maxSoldier = 0;
            foreach (var heroInfo in heroEfficencyList)
            {
                if (heroInfo.soldierNum > maxSoldier)
                {
                    this.EfficientHeroId = heroInfo.heroId;
                    maxSoldier = heroInfo.soldierNum;
                }
            }

            this.SoldierIdSet = new HashSet<int>();
            var soldierIdMatches = Regex.Matches(page, SoldierIdPattern, RegexOptions.Singleline);
            foreach (Match soldier in soldierIdMatches)
            {
                int soldierId = int.Parse(soldier.Groups[1].Value);
                if (!this.SoldierIdSet.Contains(soldierId))
                {
                    this.SoldierIdSet.Add(soldierId);
                }
            }
        }
    }
}
