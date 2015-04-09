using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Union
{
    class DoBuySoldierFromUnion : TCPage
    {
        public bool Success
        {
            get { return this.RawPage.Contains("购买成功"); }
        }

        public static DoBuySoldierFromUnion Open(
            RequestAgent agent,
            int soldierId,
            int soldierNum,
            int soldierLevel)
        {
            var soldierInfo = string.Format("{0}:{1}", soldierId, soldierNum);
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.buy_soldier_from_union,
                new TCRequestArgument(TCElement.soldier_info, soldierInfo),
                new TCRequestArgument(TCElement.soldier_level, soldierLevel));
            return new DoBuySoldierFromUnion(agent.WebClient.OpenUrl(url));
        }

        protected DoBuySoldierFromUnion(string page) : base(page)
        {
        }
    }
}
