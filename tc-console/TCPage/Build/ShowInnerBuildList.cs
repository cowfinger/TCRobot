using System.Security.Policy;
using System.Windows.Forms;

namespace TC.TCPage.Build
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ShowInnerBuildList
    {
        public const string AbledBuildIdPattern = @"<li class=""build_(?<buildId>\d+) able"">";

        public const string DisabledBuildIdPattern = @"<li class=""build_(?<buildId>\d+) disabled"">";

        public const string BuildIdPattern = @"<li class=""build_(?<buildId>\d+)"">";

        public const string BuildLevelPattern =
            @"\[\[jslang\('level', (?<buildLevel>\d+)\)\]\]";

        public const string EndurePattern =
            @"<span class=""dt"">" + @"\[\[jslang\(""endure""\)\]\]:</span>" +
            @"<span class=""zb_color\d"">(?<endure>\d+)</span>" +
            @"<span class=""dt"">/(?<maxEndure>\d+)</span>";

        public const string ResourcePattern =
            @"<li class=""re_m.*?""><span.*?>(?<wood>\d+)</span></li>" +
            @"<li class=""re_s.*?""><span.*?>(?<mud>\d+)</span></li>" +
            @"<li class=""re_t.*?""><span.*?>(?<iron>\d+)</span></li>" +
            @"<li class=""re_l.*?""><span.*?>(?<food>\d+)</span></li>";

        public const string BuildUrlPattern =
            @"index\.php\?mod=city/build&op=show&func=viewbuild&pid=(?<pid>\d+)&bt=(?<bt>\d+)&bid=\d+";

        public const string BuildPreCityPattern = @"\[\[jslang\('city_lv'\)\]\] Lv\.(?<cityLevel>\d+)<li>";

        public const string BuildPreBuildPattern =
            @"\[\[jslang\('build_(?<preBuildId>\d+)'\)\]\] \[\[jslang\('level',(?<preBuildLevel>\d+)\)\]\]";

        public const string CityLevelPattern = @"mod:city/city\|func:update_level\|op:do\|now_level:(?<cityLevel>\d+)";

        public const string BuildEndPattern = @"<div class=""name"">\[\[jslang\('build_\d+'\)\]\] 等级\d+</div>";

        public const string BuildPattern =
            BuildIdPattern + ".*?" +
            BuildLevelPattern + ".*?" +
            ResourcePattern + ".*?" +
            BuildUrlPattern;

        public const string AbleBuildPattern =
            AbledBuildIdPattern + ".*?" +
            BuildLevelPattern + ".*?" +
            ResourcePattern + "(?<content>.*?)" + BuildEndPattern;

        public const string DisabledBuildPattern =
            DisabledBuildIdPattern + "(?<content>.*?)" + BuildEndPattern;


        public ShowInnerBuildList(string page)
        {
            this.RawPage = page;
        }

        public string RawPage { get; private set; }

        public int CityLevel
        {
            get
            {
                var match = Regex.Match(this.RawPage, CityLevelPattern, RegexOptions.Singleline);
                return match.Success ? int.Parse(match.Groups["cityLevel"].Value) : -1;
            }
        }

        public IEnumerable<Build> BuildList
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, BuildPattern, RegexOptions.Singleline);
                var buildList = from Match match in matches
                                select
                                    new Build
                                        {
                                            Pid = int.Parse(match.Groups["pid"].Value),
                                            Bt = int.Parse(match.Groups["bt"].Value),
                                            BuildId = int.Parse(match.Groups["buildId"].Value),
                                            BuildLevel = int.Parse(match.Groups["buildLevel"].Value),
                                            UpgradeRequiredFood = int.Parse(match.Groups["food"].Value),
                                            UpgradeRequiredWood = int.Parse(match.Groups["wood"].Value),
                                            UpgradeRequiredIron = int.Parse(match.Groups["iron"].Value),
                                            UpgradeRequiredMud = int.Parse(match.Groups["mud"].Value)
                                        };
                return this.AbleBuildList.Concat(buildList);
            }
        }

        public IEnumerable<Build> AbleBuildList
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, AbleBuildPattern, RegexOptions.Singleline);
                var items = from Match match in matches
                            let urlMatch = Regex.Match(match.Groups["content"].Value, BuildUrlPattern, RegexOptions.Singleline)
                            select
                                new Build
                                    {
                                        Pid = urlMatch.Success ? int.Parse(urlMatch.Groups["pid"].Value) : 0,
                                        Bt = urlMatch.Success ? int.Parse(match.Groups["bt"].Value) : 2,
                                        BuildId = int.Parse(match.Groups["buildId"].Value),
                                        BuildLevel = int.Parse(match.Groups["buildLevel"].Value),
                                        UpgradeRequiredFood = int.Parse(match.Groups["food"].Value),
                                        UpgradeRequiredWood = int.Parse(match.Groups["wood"].Value),
                                        UpgradeRequiredIron = int.Parse(match.Groups["iron"].Value),
                                        UpgradeRequiredMud = int.Parse(match.Groups["mud"].Value)
                                    };

                return items;
            }
        }

        public IEnumerable<DisabledBuild> DisabledBuildList
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, DisabledBuildPattern, RegexOptions.Singleline);
                var buildList = from Match match in matches
                                select new
                                {
                                    buildId = int.Parse(match.Groups["buildId"].Value),
                                    content = match.Groups["content"].Value
                                };

                return buildList.Select(item =>
                {
                    var cityPreMatch = Regex.Match(item.content, BuildPreCityPattern, RegexOptions.Singleline);
                    var cityLevel = cityPreMatch.Success ? int.Parse(cityPreMatch.Groups["cityLevel"].Value) : 0;
                    var buildPreMatches = Regex.Matches(item.content, BuildPreBuildPattern, RegexOptions.Singleline);
                    var preBuilds = (from Match match in buildPreMatches
                                     select
                                         new PreBuild
                                             {
                                                 PreBuildId = int.Parse(match.Groups["preBuildId"].Value),
                                                 PreBuildLevel = int.Parse(match.Groups["preBuildLevel"].Value)
                                             }).ToList();

                    return new DisabledBuild
                        {
                            BuildId = item.buildId,
                            CityLevel = cityLevel,
                            PreBuilds = preBuilds
                        };
                });
            }
        }

        public static ShowInnerBuildList Open(RequestAgent agent, int pid = 0, int bt = 2)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.build,
                TCOperation.Show,
                TCFunc.inner_build_list,
                new TCRequestArgument(TCElement.pid, pid),
                new TCRequestArgument(TCElement.bt, bt));
            return new ShowInnerBuildList(agent.WebClient.OpenUrl(url));
        }

        public class Build
        {
            public int Pid { get; set; }

            public int Bt { get; set; }

            public int BuildId { get; set; }

            public int BuildLevel { get; set; }

            public int UpgradeRequiredWood { get; set; }

            public int UpgradeRequiredFood { get; set; }

            public int UpgradeRequiredIron { get; set; }

            public int UpgradeRequiredMud { get; set; }

            public IList<int> RequiredResourceTable
            {
                get
                {
                    return new List<int>()
                    {
                        this.UpgradeRequiredWood,
                        this.UpgradeRequiredMud,
                        this.UpgradeRequiredIron,
                        this.UpgradeRequiredFood
                    };
                }
            }
        }

        public class PreBuild
        {
            public int PreBuildId { get; set; }

            public int PreBuildLevel { get; set; }

        }

        public class DisabledBuild
        {
            public int BuildId { get; set; }

            public int CityLevel { get; set; }

            public IEnumerable<PreBuild> PreBuilds { get; set; }
        }
    }
}