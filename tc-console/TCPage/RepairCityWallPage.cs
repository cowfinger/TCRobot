using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage
{
    using System.Text.RegularExpressions;

    class RepairCityWallPage
    {
        public int BuildId { get; private set; }

        public int CityNodeId { get; private set; }

        public int WallDuration { get; private set; }

        public int WallMaxDuration { get; private set; }

        public int BrickNum { get; private set; }

        public int CompleteRepairNeeds { get; private set; }

        public RepairCityWallPage(string page)
        {
            const string DurationPattern = @"<th>耐久度</th>.*?<td>(?<duration>\d+)/(?<maxDuration>\d+)</td>";
            const string BrickNumPattern = @"<th>携带砖数</th>.*?<th>(\d+)</th>";
            const string MaxBrickPattern = @"if ( brick_num>(\d+) )";
            const string NodeIdPattern = @"build_id=(?<buildId>\d+)&node_id=(?<nodeId>\d+)";

            var matchDuration = Regex.Match(page, DurationPattern, RegexOptions.Singleline);
            var matchBrickNum = Regex.Match(page, BrickNumPattern, RegexOptions.Singleline);
            var matchMaxBrick = Regex.Match(page, MaxBrickPattern);
            var matchNodeId = Regex.Match(page, NodeIdPattern);

            this.CityNodeId = matchNodeId.Success ? int.Parse(matchNodeId.Groups["nodeId"].Value) : 0;
            this.BuildId = matchNodeId.Success ? int.Parse(matchNodeId.Groups["buildId"].Value) : 0;
            this.WallDuration = matchDuration.Success ? int.Parse(matchDuration.Groups["duration"].Value) : 0;
            this.WallMaxDuration = matchDuration.Success ? int.Parse(matchDuration.Groups["maxDuration"].Value) : 0;
            this.BrickNum = matchBrickNum.Success ? int.Parse(matchDuration.Groups[1].Value) : 0;
            this.CompleteRepairNeeds = matchMaxBrick.Success ? int.Parse(matchMaxBrick.Groups[1].Value) : 0;
        }
    }
}
