using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class UnionDoApplyUnionPage
    {
        public static UnionDoApplyUnionPage Open(RequestAgent agent, int unionId)
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.apply_union,
                new TCRequestArgument(TCElement.union_id, unionId));
            var rawPage = agent.WebClient.OpenUrl(url);
            return new UnionDoApplyUnionPage(rawPage);
        }

        public UnionDoApplyUnionPage(string page)
        {
        }
    }
}
