using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace TC.TCPage
{
    enum CityBuildId
    {
        Fortress = 1001,
        Wall = 1002,
        Road = 1003,
    }

    class InfluenceShowCityBuildPage
    {
        public class CityBuild
        {
            public int Duration { get; set; }

            public int MaxDuration { get; set; }

            public int Level { get; set; }

            public int MaxLevel { get; set; }
        }

        public string CityName { get; private set; }

        public int CityNodeId { get; private set; }

        public CityBuild Fortress { get; private set; }

        public CityBuild Wall { get; private set; }

        public CityBuild Road { get; private set; }

        public static InfluenceShowCityBuildPage Open(RequestAgent agent, int cityNodeId)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.city_build,
                new TCRequestArgument(TCElement.node_id, cityNodeId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new InfluenceShowCityBuildPage(rawPage);
        }

        public InfluenceShowCityBuildPage(string page)
        {
            const string CityNamePattern = "<div class=\"title\">(.+?)</div>";
            const string NodeIdPattern = @"node_id=(\d+)";
            const string BuildPattern =
                @".*?<span>耐久度：(?<duration>\d+)/(?<maxDuration>\d+)</span>" +
                @"&nbsp;&nbsp;&nbsp;<span>等级：.*?(?<level>\d+)/(?<maxLevel>\d+)</span>";
            const string FortressPattern = "fortress_descr" + BuildPattern;
            const string WallPattern = "wall_descr" + BuildPattern;
            const string RoadPattern = @"<span>等级：(?<level>\d+)/(?<maxLevel>\d+)</span>";

            var cityNameMatch = Regex.Match(page, CityNamePattern);
            this.CityName = cityNameMatch.Success ? cityNameMatch.Groups[1].Value : "";

            var nodeIdMatch = Regex.Match(page, NodeIdPattern);
            this.CityNodeId = nodeIdMatch.Success ? int.Parse(nodeIdMatch.Groups[1].Value) : 0;

            this.Fortress = ParseCityBuild(page, FortressPattern);
            this.Wall = ParseCityBuild(page, WallPattern);

            var roadMatch = Regex.Match(page, RoadPattern);
            if (roadMatch.Success)
            {
                this.Road = new CityBuild()
                {
                    Duration = 0,
                    MaxDuration = 0,
                    Level = int.Parse(roadMatch.Groups["level"].Value),
                    MaxLevel = int.Parse(roadMatch.Groups["maxLevel"].Value),
                };
            }
        }

        private CityBuild ParseCityBuild(string page, string pattern)
        {
            var match = Regex.Match(page, pattern, RegexOptions.Singleline);
            return new CityBuild()
            {
                Duration = int.Parse(match.Groups["duration"].Value),
                MaxDuration = int.Parse(match.Groups["maxDuration"].Value),
                Level = int.Parse(match.Groups["level"].Value),
                MaxLevel = int.Parse(match.Groups["maxLevel"].Value),
            };
        }
    }
}
