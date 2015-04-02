using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TC.TCPage.Union
{
    class ShowUnionMember : TCPage
    {
        public static ShowUnionMember Open(RequestAgent agent, int unionId, string action = "new")
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Show,
                TCFunc.union_member,
                new TCRequestArgument(TCElement.union_id, unionId),
                new TCRequestArgument(TCElement.action, action));
            return new ShowUnionMember(agent.WebClient.OpenUrl(url));
        }

        protected ShowUnionMember(string page) : base(page)
        {
        }
    }
}
