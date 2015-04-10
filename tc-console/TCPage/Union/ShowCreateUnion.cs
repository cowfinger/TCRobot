using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Union
{
    class ShowCreateUnion : TCPage
    {
        public static ShowCreateUnion Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Show,
                TCFunc.create_union);
            return new ShowCreateUnion(agent.WebClient.OpenUrl(url));
        }

        protected ShowCreateUnion(string page) : base(page)
        {
        }
    }
}
