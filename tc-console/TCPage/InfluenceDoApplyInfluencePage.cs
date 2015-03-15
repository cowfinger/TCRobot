﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class InfluenceDoApplyInfluencePage
    {
        public bool Success { get; private set; }

        public static InfluenceDoApplyInfluencePage Open(RequestAgent agent, int influenceId)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.apply_influence,
                new TCRequestArgument(TCElement.influence_id, influenceId));
            var page = agent.WebClient.OpenUrl(url);
            return new InfluenceDoApplyInfluencePage(page);
        }

        public InfluenceDoApplyInfluencePage(string page)
        {
            this.Success = page.Contains("wee.lang('yes')");
        }
    }
}
