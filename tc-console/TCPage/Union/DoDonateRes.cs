using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Union
{
    class DoDonateRes : TCPage
    {
        public bool Success
        {
            get
            {
                return this.RawPage.Contains("捐献资源成功");
            }
        }

        public static DoDonateRes Open(RequestAgent agent, IEnumerable<int> resTable)
        {
            var resStr = string.Join("|", resTable);
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.donate_res,
                new TCRequestArgument(TCElement.donate_res, resStr));
            return new DoDonateRes(agent.WebClient.OpenUrl(url));
        }

        protected DoDonateRes(string page)
            : base(page)
        {
        }
    }
}
