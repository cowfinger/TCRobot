using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class UnionDoOutUnionPage
    {
        public static UnionDoOutUnionPage Open(RequestAgent agent)
        {
            var url = agent.BuildUrl(
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.out_union);
            var rawPage = agent.WebClient.OpenUrl(url);
            return new UnionDoOutUnionPage(rawPage);
        }

        public UnionDoOutUnionPage(string page)
        {
        }
    }
}
