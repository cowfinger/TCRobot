using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class InfluenceDoBuildRepairPage
    {
        public static InfluenceDoBuildRepairPage Open(
            RequestAgent agent,
            int cityId,
            int buildId,
            int brickNum)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.build_repair,
                new TCRequestArgument(TCElement.build_id, cityId),
                new TCRequestArgument(TCElement.node_id, buildId),
                new TCRequestArgument(TCElement.brick_num, brickNum));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new InfluenceDoBuildRepairPage(rawPage);
        }

        public InfluenceDoBuildRepairPage(string page)
        {
        }
    }
}
