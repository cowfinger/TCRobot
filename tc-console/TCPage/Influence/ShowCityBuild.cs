﻿namespace TC.TCPage.Influence
{
    using System.Text.RegularExpressions;

    internal enum CityBuildId
    {
        Fortress = 1001,

        Wall = 1002,

        Road = 1003
    }

    internal class ShowCityBuild
    {
        public ShowCityBuild(string page)
        {
            const string CityNamePattern = "<div class=\"title\">(.+?)</div>";
            const string NodeIdPattern = @"node_id=(\d+)";
            const string BuildPattern =
                @".*?<span>耐久度：(?<duration>\d+)/(?<maxDuration>\d+)</span>"
                + @"&nbsp;&nbsp;&nbsp;<span>等级：.*?(?<level>\d+)/(?<maxLevel>\d+)</span>";
            const string FortressPattern = "fortress_descr" + BuildPattern;
            const string WallPattern = "wall_descr" + BuildPattern;
            const string RoadPattern = @"road_descr(.|\s)*?<span>等级：(?<level>\d+)/(?<maxLevel>\d+)</span>";

            var cityNameMatch = Regex.Match(page, CityNamePattern);
            this.CityName = cityNameMatch.Success ? cityNameMatch.Groups[1].Value : "";

            var nodeIdMatch = Regex.Match(page, NodeIdPattern);
            this.CityNodeId = nodeIdMatch.Success ? int.Parse(nodeIdMatch.Groups[1].Value) : 0;

            this.Fortress = ParseCityBuild(page, FortressPattern);
            this.Wall = ParseCityBuild(page, WallPattern);

            var roadMatch = Regex.Match(page, RoadPattern);
            if (roadMatch.Success)
            {
                this.Road = new CityBuild
                                {
                                    Duration = 0,
                                    MaxDuration = 0,
                                    Level = int.Parse(roadMatch.Groups["level"].Value),
                                    MaxLevel = int.Parse(roadMatch.Groups["maxLevel"].Value)
                                };
            }
        }

        public string CityName { get; private set; }

        public int CityNodeId { get; private set; }

        public CityBuild Fortress { get; private set; }

        public CityBuild Wall { get; private set; }

        public CityBuild Road { get; private set; }

        public static ShowCityBuild Open(RequestAgent agent, int cityNodeId)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.city_build,
                new TCRequestArgument(TCElement.node_id, cityNodeId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new ShowCityBuild(rawPage);
        }

        private static CityBuild ParseCityBuild(string page, string pattern)
        {
            var match = Regex.Match(page, pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                return new CityBuild
                           {
                               Duration = int.Parse(match.Groups["duration"].Value),
                               MaxDuration = int.Parse(match.Groups["maxDuration"].Value),
                               Level = int.Parse(match.Groups["level"].Value),
                               MaxLevel = int.Parse(match.Groups["maxLevel"].Value)
                           };
            }
            return null;
        }

        public class CityBuild
        {
            public int Duration { get; set; }

            public int MaxDuration { get; set; }

            public int Level { get; set; }

            public int MaxLevel { get; set; }
        }
    }
}