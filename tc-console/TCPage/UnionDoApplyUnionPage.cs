using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC.TCPage
{
    class UnionDoApplyUnionPage
    {
        public static UnionDoApplyUnionPage Open(AccountInfo account, int unionId)
        {
            var url = RequestAgent.BuildUrl(
                account.AccountType,
                TCMod.union,
                TCSubMod.union,
                TCOperation.Do,
                TCFunc.apply_union,
                new TCRequestArgument(TCElement.union_id, unionId));
            var webClient = new HttpClient(account.CookieStr);
            var rawPage = webClient.OpenUrl(url);
            return new UnionDoApplyUnionPage(rawPage);
        }

        public UnionDoApplyUnionPage(string page)
        {
        }
    }
}
