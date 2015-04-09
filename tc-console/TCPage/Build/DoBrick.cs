using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Build
{
    class DoBrick : TCPage
    {
        public static DoBrick Open(RequestAgent agent, int num)
        {
            var url = agent.BuildUrl(
                TCMod.city,
                TCSubMod.build,
                TCOperation.Do,
                TCFunc.brick,
                new TCRequestArgument(TCElement.n, num));
            return new DoBrick(agent.WebClient.OpenUrl(url));
        }

        protected DoBrick(string page) : base(page)
        {
        }
    }
}
