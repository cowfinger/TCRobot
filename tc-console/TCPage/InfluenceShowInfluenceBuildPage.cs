
namespace TC.TCPage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    class InfluenceShowInfluenceBuildPage
    {
        public const string DurationPattern = @"<th>耐久度</th>.*?<td>(?<duration>\d+)/(?<maxDuration>\d+)</td>";
        public const string BrickNumPattern = @"<th>携带砖数</th>.*?<th>(\d+)</th>";
        public const string MaxBrickPattern = @"if \( brick_num>(\d+) \)";
        public const string NodeIdPattern = @"build_id=(?<buildId>\d+)&node_id=(?<nodeId>\d+)";

        public int BuildId { get; private set; }

        public int CityNodeId { get; private set; }

        public int WallDuration { get; private set; }

        public int WallMaxDuration { get; private set; }

        public int BrickNum { get; private set; }

        public int CompleteRepairNeeds { get; private set; }

        public static InfluenceShowInfluenceBuildPage Open(
            RequestAgent agent,
            int cityNodeId,
            int buildId,
            int buildLevel)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Show,
                TCFunc.influence_build,
                new TCRequestArgument(TCElement.build_id, buildId),
                new TCRequestArgument(TCElement.node_id, cityNodeId),
                new TCRequestArgument(TCElement.level, buildLevel),
                new TCRequestArgument(TCElement.action, "repair"));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new InfluenceShowInfluenceBuildPage(rawPage);
        }

        public InfluenceShowInfluenceBuildPage(string page)
        {
            var matchDuration = Regex.Match(page, DurationPattern, RegexOptions.Singleline);
            var matchBrickNum = Regex.Match(page, BrickNumPattern, RegexOptions.Singleline);
            var matchMaxBrick = Regex.Match(page, MaxBrickPattern);
            var matchNodeId = Regex.Match(page, NodeIdPattern);

            this.CityNodeId = matchNodeId.Success ? int.Parse(matchNodeId.Groups["nodeId"].Value) : 0;
            this.BuildId = matchNodeId.Success ? int.Parse(matchNodeId.Groups["buildId"].Value) : 0;
            this.WallDuration = matchDuration.Success ? int.Parse(matchDuration.Groups["duration"].Value) : 0;
            this.WallMaxDuration = matchDuration.Success ? int.Parse(matchDuration.Groups["maxDuration"].Value) : 0;
            this.BrickNum = matchBrickNum.Success ? int.Parse(matchBrickNum.Groups[1].Value) : 0;
            this.CompleteRepairNeeds = matchMaxBrick.Success ? int.Parse(matchMaxBrick.Groups[1].Value) : 0;
        }
    }
}
