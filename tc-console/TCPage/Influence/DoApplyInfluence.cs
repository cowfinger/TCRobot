using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage.Influence
{
    class DoApplyInfluence
    {
        public bool Success { get; private set; }

        public static DoApplyInfluence Open(RequestAgent agent, int influenceId)
        {
            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.apply_influence,
                new TCRequestArgument(TCElement.influence_id, influenceId));
            var page = agent.WebClient.OpenUrl(url);
            return new DoApplyInfluence(page);
        }

        public DoApplyInfluence(string page)
        {
            this.Success = page.Contains("wee.lang('yes')");
        }
    }
}
