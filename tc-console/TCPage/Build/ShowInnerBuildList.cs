using System.Security.Policy;

namespace TC.TCPage.Build
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class ShowInnerBuildList
    {
        public const string BuildIdPattern =
            @"<h\d class=""build_name zb_color\d"">\[\[jslang\('build_(?<buildId>\d+)'\)\]\] "
            + @"\[\[jslang\('level', (?<buildLevel>\d+)\)\]\]</h\d>";

        public const string EndurePattern =
            @"<span class=""dt"">" + @"\[\[jslang\(""endure""\)\]\]:</span>"
            + @"<span class=""zb_color\d"">(?<endure>\d+)</span>" + @"<span class=""dt"">/(?<maxEndure>\d+)</span>";

        public const string ResourcePattern =
            @"<li class=""re_m""><span>(?<wood>\d+)</span></li>" + @"<li class=""re_s""><span>(?<mud>\d+)</span></li>"
            + @"<li class=""re_t""><span>(?<iron>\d+)</span></li>"
            + @"<li class=""re_l""><span>(?<food>\d+)</span></li>";

        public const string BuildUrl =
            @"index\.php\?mod=city/build&op=show&func=viewbuild&pid=(?<pid>\d+)&bt=(?<bt>\d+)&bid=\d+";

        public const string BuildPattern = BuildIdPattern + ".*?" + EndurePattern + ".*?" + ResourcePattern;

        public const string DisabledBuildIdPattern = @"<li class=""build_(?<buildId>\d+) disabled"">";

        public const string DisabledBuildEndPattern = @"<div class=""name"">[[jslang('build_\d+')]] 等级0</div>";

        public const string BuildPreCityPattern =
            @"<h4>[[jslang('up_pre')]]</h4>" + @"<ul>"
            + @"<li class=""red"">\[\[jslang\('city_lv'\)\]\] Lv\.(?<cityLevel>\d+)<li>";

        public const string BuildPreBuildPattern =
            @"<li>\[\[jslang\('build_(?<preBuildId>\d+)'\)\]\] "
            + @"\[\[jslang\('level',(?<prdBuildLevel>\d+)\)\]\]</li>" + @"</ul>";

        public const string DisabledBuildPattern = DisabledBuildIdPattern + "(?<content>.*?)" + DisabledBuildEndPattern;

        public const string CityLevelPattern = @"mod:city/city|func:update_level|op:do|now_level:(?<cityLevel>\d+)";

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
                return from Match match in matches
                       select
                           new Build
                               {
                                   Pid = int.Parse(match.Groups["pid"].Value),
                                   Bt = int.Parse(match.Groups["bt"].Value),
                                   BuildId = int.Parse(match.Groups["buildId"].Value),
                                   BuildLevel = int.Parse(match.Groups["buildLevel"].Value),
                                   Endure = int.Parse(match.Groups["endure"].Value),
                                   MaxEndure = int.Parse(match.Groups["maxEndure"].Value),
                                   UpgradeRequiredFood = int.Parse(match.Groups["food"].Value),
                                   UpgradeRequiredWood = int.Parse(match.Groups["wood"].Value),
                                   UpgradeRequiredIron = int.Parse(match.Groups["iron"].Value),
                                   UpgradeRequiredMud = int.Parse(match.Groups["mud"].Value)
                               };
            }
        }

        public IEnumerable<DisabledBuild> DisabledBuildList
        {
            get
            {
                var matches = Regex.Matches(this.RawPage, DisabledBuildIdPattern, RegexOptions.Singleline);
                var buildList = from Match match in matches
                                select new
                                {
                                    buildId = int.Parse(match.Groups["buildId"].Value),
                                    content = match.Groups["content"].Value
                                };

                return buildList.Select(item =>
                {
                    var cityPreMatch = Regex.Match(item.content, BuildPreCityPattern, RegexOptions.Singleline);
                    var buildPreMatches = Regex.Matches(item.content, BuildPreBuildPattern, RegexOptions.Singleline);
                    return new DisabledBuild
                        {
                            BuildId = item.buildId,
                            CityLevel = int.Parse(cityPreMatch.Groups["cityLevel"].Value),
                            PreBuilds = from Match match in buildPreMatches
                                        select new PreBuild
                                        {
                                            PreBuildId = int.Parse(match.Groups["preBuildId"].Value),
                                            PreBuildLevel = int.Parse(match.Groups["preBuildLevel"].Value)
                                        }
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

            public int Endure { get; set; }

            public int MaxEndure { get; set; }

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