using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Influence
{
    class DoInfluenceDonate : TCPage
    {
        public bool Success
        {
            get { return this.RawPage.Contains("成功"); }
        }

        public static DoInfluenceDonate Open(RequestAgent agent, IEnumerable<long> resList)
        {
            var toContriRes = resList.ToList();
            var resValue = string.Join("|", toContriRes);
            for (var i = toContriRes.Count; i < 6; ++i)
            {
                resValue += "|0";
            }

            var url = agent.BuildUrl(
                TCMod.influence,
                TCSubMod.influence,
                TCOperation.Do,
                TCFunc.influence_donate,
                new TCRequestArgument(TCElement.res, resValue));
            return new DoInfluenceDonate(agent.WebClient.OpenUrl(url));
        }

        protected DoInfluenceDonate(string page) : base(page)
        {
        }
    }
}
