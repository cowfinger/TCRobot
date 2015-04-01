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

        public static DoInfluenceDonate Open(RequestAgent agent, IList<long> resList)
        {
            var toContriRes = "".PadRight(6).Zip(resList, (x, y) => y).ToList();
            var resValue = string.Join("|", toContriRes);
            var padValue = string.Join("|", "".PadRight(6 - toContriRes.Count, '0'));
            if (!string.IsNullOrEmpty(padValue))
            {
                resValue += padValue;
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
