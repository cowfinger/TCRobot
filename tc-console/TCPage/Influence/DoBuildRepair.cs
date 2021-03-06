﻿namespace TC.TCPage.Influence
{
    internal class DoBuildRepair : TCPage
    {
        public DoBuildRepair(string page) : base(page)
        {
        }

        public static DoBuildRepair Open(RequestAgent agent, int cityId, int buildId, int brickNum)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.build_repair,
                new TCRequestArgument(TCElement.build_id, buildId),
                new TCRequestArgument(TCElement.node_id, cityId),
                new TCRequestArgument(TCElement.brick_num, brickNum));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new DoBuildRepair(rawPage);
        }
    }
}