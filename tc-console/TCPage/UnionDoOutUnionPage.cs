using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class UnionDoOutUnionPage
    {
        public static UnionDoOutUnionPage Open(AccountInfo account)
        {
            var url = RequestAgent.BuildUrl(
                account.AccountType,
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.out_union);
            var webClient = new HttpClient(account.CookieStr);
            var rawPage = webClient.OpenUrl(url);
            return new UnionDoOutUnionPage(rawPage);
        }

        public UnionDoOutUnionPage(string page)
        {
        }
    }
}
