namespace TC.TCPage.Build
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ShowBuild
    {
        public const string HeroBuildTimePattern = @"hero_select\.init\( \{(?<heriBuildTimePairs>.*?)\} \);";

        public const string HeroBuildTimePairPattern = @"""(?<heroId>\d+)"":""(?<elapse>[0-9:]+)""";


        public ShowBuild(string page)
        {
            this.RawPage = page;
        }

        public string RawPage { get; private set; }

        public ShowInnerBuildList.Build BuildDetail
        {
            get
            {
                var match = Regex.Match(this.RawPage, ShowInnerBuildList.ResourcePattern, RegexOptions.Singleline);
                return new ShowInnerBuildList.Build
                           {
                               UpgradeRequiredFood = int.Parse(match.Groups["food"].Value),
                               UpgradeRequiredWood = int.Parse(match.Groups["wood"].Value),
                               UpgradeRequiredIron = int.Parse(match.Groups["iron"].Value),
                               UpgradeRequiredMud = int.Parse(match.Groups["mud"].Value)
                           };
            }
        }

        public IEnumerable<HeroBuildTime> HeroBuildTimes
        {
            get
            {
                var listMatch = Regex.Match(this.RawPage, HeroBuildTimePattern, RegexOptions.Singleline);
                if (!listMatch.Success)
                {
                    return new List<HeroBuildTime>();
                }

                var pairMatches = Regex.Matches(listMatch.Groups[1].Value, HeroBuildTimePairPattern);
                return from Match match in pairMatches
                       select
                           new HeroBuildTime
                               {
                                   HeroId = int.Parse(match.Groups["heroId"].Value),
                                   BuildTimeInSeconds = FormMain.TimeStr2Sec(match.Groups["elapse"].Value)
                               };
            }
        }

        public static ShowBuild Open(RequestAgent agent, int pid, int bt, int bid)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.build,
                TCOperation.Show,
                TCFunc.build,
                new TCRequestArgument(TCElement.pid, pid),
                new TCRequestArgument(TCElement.bt, bt),
                new TCRequestArgument(TCElement.bid, bid));
            return new ShowBuild(agent.WebClient.OpenUrl(url));
        }

        public static ShowBuild Open(RequestAgent agent, int isRe, int pid, int bt, int bid, int tabId, int pedId)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.build,
                TCOperation.Show,
                TCFunc.build,
                new TCRequestArgument(TCElement.is_re, isRe),
                new TCRequestArgument(TCElement.pid, pid),
                new TCRequestArgument(TCElement.bt, bt),
                new TCRequestArgument(TCElement.bid, bid),
                new TCRequestArgument(TCElement.tab_id, tabId),
                new TCRequestArgument(TCElement.pet_user_id, pedId));
            return new ShowBuild(agent.WebClient.OpenUrl(url));
        }

        public class HeroBuildTime
        {
            public int HeroId { get; set; }

            public int BuildTimeInSeconds { get; set; }
        }
    }
}